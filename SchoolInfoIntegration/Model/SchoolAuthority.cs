﻿using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Newtonsoft.Json.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;

/**
    * DataModel: 
    */
namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public class SchoolAuthorityImport: D365ImportBase
    {
        public string AuthorityId;
        public long AuthorityNumber;
        public string Name;
        public string OpenDate;
        public string CloseDate;

        public override JObject ToIOSAS()
        {
            return new JObject
            {
                new JProperty("edu_authority_no", AuthorityNumber.ToString($"D3")),
                new JProperty("iosas_externalid", AuthorityId)
            };
        }
        public override JObject ToISFS()
        {
            return new()
            {
                new JProperty("isfs_authorityno", AuthorityNumber.ToString($"D3")),
                new JProperty("edu_externalid", AuthorityId)
            };
        }
    }
    public class AuthorityMapper: ClassMap<SchoolAuthorityImport>
    {
        public AuthorityMapper()
        {
            Map(authority => authority.AuthorityId).Name("independent_authority_id");
            Map(authority => authority.AuthorityNumber).Name("authority_number");
            Map(authority => authority.Name).Name("display_name");
            Map(authority => authority.OpenDate).Name("opened_date");
            Map(authority => authority.CloseDate).Name("closed_date");
        }
    }
    public partial class SchoolAuthority: D365Model
    {
       
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonPropertyName("createDate")]
        public DateTimeOffset? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTimeOffset? UpdateDate { get; set; }

        [JsonPropertyName("independentAuthorityId")]
        [Required]
        public string IndependentAuthorityId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("authorityNumber")]
        [Required]
        public long AuthorityNumber { get; set; }

        [JsonPropertyName("faxNumber")]
        public string? FaxNumber { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("displayName")]
        [Required]
        public string? DisplayName { get; set; }

        [JsonPropertyName("authorityTypeCode")]
        [Required]
        public string? AuthorityTypeCode { get; set; }

        [JsonPropertyName("authorityStatus")]
        // TODO: Missing
        public string? AuthorityStatus { get; set; }

        [JsonPropertyName("openedDate")]
        [Required]
        public DateTimeOffset OpenedDate { get; set; }

        [JsonPropertyName("closedDate")]
        public DateTimeOffset? ClosedDate { get; set; }

        [JsonPropertyName("contacts")]
        public Contact[]? Contacts { get; set; }

        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }


        public string UpsertQuery()
        {
            return $"edu_schoolauthorities(edu_authority_no='{this.AuthorityNumber}')";
        }

        public string GetQuery()
        {
            return $"edu_schoolauthorities?$filter=edu_name eq '{this.DisplayName}'";
        }

        public string IdQuery(string id)
        {
            return "edu_schoolauthorities({id})";
        }

        public string EntityName()
        {
            return "edu_schoolauthorities";
        }
        public string Key()
        {
            return "";
        }
        public string KeyValue()
        {
            return $"{this.AuthorityNumber}";
        }
        public string KeyDisplay()
        {
            return $"ed_schoolAuthorityNo={this.AuthorityNumber}";
        }
        public string ExternalId()
        {
            return IndependentAuthorityId;
        }

        public bool VerifyExisting(D365ModelMetdaData meta, JObject data, ILogger? logger)
        {
            string buisnessValue = data[meta.businessKey]?.ToString() ?? "";
            string? externalId = data[meta.externalIdKey]?.ToString();
            return buisnessValue == this.AuthorityNumber.ToString() && (externalId == null || externalId == "" || externalId == ExternalId());
        }

        public JObject ToIOSAS(JObject lookups)
        {
            var result = new JObject();
            // Direct data mapping
            result["edu_authority_no"] = $"{this.AuthorityNumber}";
            result["edu_name"] = this.DisplayName;
            result["edu_phone"] = this.PhoneNumber;
            result["edu_fax"] = this.FaxNumber;
            result["edu_email"] = this.Email;
            result["iosas_authoritystatus"] = AuthorityStatus != null && AuthorityStatus == "OPEN" ? true: false ; // TODO: Need default value | Missing
            result["edu_authority_type"] = this.AuthorityTypeCode == "INDEPENDNT" ? 757500000 : 757500001; // 757500000 (IND) 757500001 OFFSORE
            result["edu_opendate"] = this.OpenedDate.ToString("yyyy-MM-dd"); // Format: yyyy-mm-dd
            result["iosas_externalid"] = this.IndependentAuthorityId; 
            if (this.ClosedDate != null)
            {
                result["edu_closedate"] = this.ClosedDate?.ToString("yyyy-MM-dd");// Format: yyyy-mm-dd 
            }
            // Physical Address Mapping
            if (Address.getAddressWithType(AddressType.Physical.Value, this.Addresses) is var address && address != null)
            {
                result["edu_address_street1"] = address.AddressLine1;
                result["edu_address_street2"] = address.AddressLine2;
                result["edu_address_city"] = address.City;
                result["edu_address_postalcode"] = address.Postal;
                result["edu_address_province"] = address.ProvinceCode;
                result["edu_address_country"] = address.CountryCode;
            } else if (Address.getAddressWithType(AddressType.Mailing.Value, this.Addresses) is var mailing && mailing != null)
            {
                result["edu_address_street1"] = mailing.AddressLine1;
                result["edu_address_street2"] = mailing.AddressLine2;
                result["edu_address_city"] = mailing.City;
                result["edu_address_postalcode"] = mailing.Postal;
                result["edu_address_province"] = mailing.ProvinceCode;
                result["edu_address_country"] = mailing.CountryCode;
            }
            // Contact Mapping: Finding oldest Auth type contact
            const string AuthTypeCode = "INDAUTHREP";
            var authContacts = this.Contacts?.Where(contact => (contact.AuthorityContactTypeCode ?? $"") == AuthTypeCode).ToArray();
            var sortedAuthContacts = authContacts?.OrderBy((contact) => contact.CreateDate).ToArray();
            if (sortedAuthContacts?.Length > 0)
            {
                var contact = sortedAuthContacts[0];
                result["iosas_firstname"] = contact.FirstName;
                result["iosas_lastname"] = contact.LastName;
                result["iosas_maincontact"] = contact.Email;
            }
            return result;
        }
        public JObject ISFS(JObject lookups)
        {
            JObject result = new();
            result["isfs_authorityname"] = DisplayName;
            result["isfs_authoritystatus"] = AuthorityStatus == "OPEN" ? true : false;
            result["edu_externalid"] = this.IndependentAuthorityId;
            result["isfs_email"] = Email;
            result["isfs_authorityno"] = AuthorityNumber.ToString($"D3");
            result["isfs_authoritytype"] = this.AuthorityTypeCode == "INDEPENDNT" ? 746910000 : 746910001; // 746910000:Independent 746910001: Offshore
            result["isfs_email"] = Email;
            result["isfs_fax"] = FaxNumber;
            result["isfs_opendate"] = this.OpenedDate.ToString("yyyy-MM-dd");
            if (this.ClosedDate != null)
            {
                result["isfs_closedate"] = this.ClosedDate?.ToString("yyyy-MM-dd");// Format: yyyy-mm-dd 
            }
            // Physical Address Mapping
            if (Address.getAddressWithType(AddressType.Physical.Value, this.Addresses) is var address && address != null)
            {
                result["isfs_addressline1"] = address.AddressLine1;
                result["isfs_addressline2"] = address.AddressLine2;
                result["isfs_addresscity"] = address.City;
                result["isfs_addresspostalcode"] = address.Postal;
                result["isfs_addressprovince"] = address.ProvinceCode;
                result["isfs_addresscountry"] = address.CountryCode;
            }
            else if (Address.getAddressWithType(AddressType.Mailing.Value, this.Addresses) is var mailing && mailing != null)
            {
                result["isfs_addressline1"] = mailing.AddressLine1;
                result["isfs_addressline2"] = mailing.AddressLine2;
                result["isfs_addresscity"] = mailing.City;
                result["isfs_addresspostalcode"] = mailing.Postal;
                result["isfs_addressprovince"] = mailing.ProvinceCode;
                result["isfs_addresscountry"] = mailing.CountryCode;
            }
            // Contact Mapping: Finding oldest Auth type contact
            const string AuthTypeCode = "INDAUTHREP";
            var authContacts = this.Contacts?.Where(contact => (contact.AuthorityContactTypeCode ?? $"") == AuthTypeCode).ToArray();
            var sortedAuthContacts = authContacts?.OrderBy((contact) => contact.CreateDate).ToArray();
            if (sortedAuthContacts?.Length > 0)
            {
                var contact = sortedAuthContacts[0];
                result["isfs_maincontact"] = $"{contact.FirstName} {contact.LastName}";
            }
            return result;

        }
        public JObject ToISFS(JObject lookups)
        {
            JObject result = ToIOSAS(lookups);
            result["statecode"] = AuthorityStatus == "OPEN" ? 1 : 0;
            result["edu_externalid"] = this.IndependentAuthorityId;
            result.Remove("iosas_authoritystatus");
            result.Remove("iosas_firstname");
            result.Remove("iosas_lastname");
            result.Remove("iosas_maincontact");
            result.Remove("iosas_externalid");
            // edu_contact_first_name
            // edu_contact_last_name
            // edu_contact_email
            // Contact Mapping: Finding oldest Auth type contact
            const string AuthTypeCode = "INDAUTHREP";
            var authContacts = this.Contacts?.Where(contact => (contact.AuthorityContactTypeCode ?? $"") == AuthTypeCode).ToArray();
            var sortedAuthContacts = authContacts?.OrderBy((contact) => contact.CreateDate).ToArray();
            if (sortedAuthContacts?.Length > 0)
            {
                var contact = sortedAuthContacts[0];
                result["edu_contactfirstname"] = contact.FirstName;
                result["edu_contactlastname"] = contact.LastName;
                result["edu_contactemail"] = contact.Email;
            }
            return result;
        }

    }

}

