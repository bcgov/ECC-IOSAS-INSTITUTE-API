﻿using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Newtonsoft.Json.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;

/**
 * DataModel: 
 */
namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public partial class SchoolAuthority: D365Model
    {
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("createDate")]
        public DateTimeOffset? CreateDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("updateDate")]
        public DateTimeOffset? UpdateDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("independentAuthorityId")]
        [Required]
        public Guid? IndependentAuthorityId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("authorityNumber")]
        [Required]
        public long? AuthorityNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("faxNumber")]
        [Required]
        public string? FaxNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("phoneNumber")]
        [Required]
        public string? PhoneNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("email")]
        [Required]
        public string? Email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("displayName")]
        [Required]
        public string? DisplayName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("authorityTypeCode")]
        [Required]
        public string? AuthorityTypeCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("authorityStatus")]
        public string? AuthorityStatus { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("openedDate")]
        [Required]
        public DateTimeOffset OpenedDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("closedDate")]
        [Required]
        public DateTimeOffset ClosedDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("contacts")]
        public Contact[]? Contacts { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }

        [JsonPropertyName("notes")]
        public Note[]? Notes { get; set; }

        public JObject ToD365EntityModel()
        {
            var result = new JObject();
            // Direct data mapping
            result["edu_authority_no"] = $"{this.AuthorityNumber}";
            result["edu_name"] = this.DisplayName;
            result["edu_phone"] = this.PhoneNumber;
            result["edu_fax"] = this.FaxNumber;
            result["edu_email"] = this.Email;
            result["iosas_authoritystatus"] = this.AuthorityStatus; // TODO: Need default value
            result["edu_authority_type"] = this.AuthorityTypeCode == "INDEPENDNT" ? 757500000 : 757500001; // 757500000 (IND) 757500001 OFFSORE
            result["edu_opendate"] = "2023-04-07";//this.OpenedDate; // TODO: Fix date format 
            result["edu_closedate"] = "2023-04-07";//this.ClosedDate;
            // Physical Address Mapping
            if (Address.getAddressWithType(AddressType.Physical.Value, this.Addresses) is var address && address != null)
            {
                result["edu_address_street1"] = address.AddressLine1;
                result["edu_address_street2"] = address.AddressLine2;
                result["edu_address_city"] = address.City;
                result["edu_address_postalcode"] = address.Postal;
                result["edu_address_province"] = address.ProvinceCode;
                result["edu_address_country"] = address.CountryCode;
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
            return "";
        }

    }

}

