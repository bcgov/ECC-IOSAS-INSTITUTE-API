using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public class SchoolImport: D365ImportBase
    {
        public string Mincode;
        public string SchoolId;

        public override JObject ToIOSAS()
        {
            SchoolIOSAS meta = new();
            return new JObject
            {
                new JProperty(meta.businessKey, Mincode),
                new JProperty(meta.externalIdKey, SchoolId)
            };
        }
        public override JObject ToISFS()
        {
            SchoolIOSAS meta = new();
            return new JObject
            {
                new JProperty(meta.businessKey, Mincode),
                new JProperty(meta.externalIdKey, SchoolId)
            };
        }
    }
    public class SchoolMapper : ClassMap<SchoolImport>
    {
        public SchoolMapper()
        {
            Map(school => school.SchoolId).Name("school_id");
            Map(school => school.Mincode).Name("mincode");
        }
    }

    public class Principal
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("middleName")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public enum SchoolCategory
    {
        Public = 103,
        Offsore = 102,
        Independent = 101

    }

    public enum SchoolFacilityCode
    {
        Standard = 757500000, // 757500000
        Online = 757500008,
        Standalone = 757500009,
        Host = 757500010
    }

    public partial class SchoolContact
    {
        
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonPropertyName("createDate")]
        public DateTimeOffset? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTimeOffset? UpdateDate { get; set; }

        
        [JsonPropertyName("schoolContactId")]
        public Guid? SchoolContactId { get; set; }

        
        [JsonPropertyName("schoolId")]
        public Guid? SchoolId { get; set; }

        
        [JsonPropertyName("schoolContactTypeCode")]
        public string? SchoolContactTypeCode { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phoneExtension")]
        public string? PhoneExtension { get; set; }

        [JsonPropertyName("alternatePhoneNumber")]
        public string? AlternatePhoneNumber { get; set; }

        [JsonPropertyName("alternatePhoneExtension")]
        public string? AlternatePhoneExtension { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        
        [JsonPropertyName("effectiveDate")]
        public DateTimeOffset? EffectiveDate { get; set; }

        [JsonPropertyName("expiryDate")]
        public string? ExpiryDate { get; set; }
    }

    public partial class School: D365Model
    {
        [JsonPropertyName("createDate")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTimeOffset UpdateDate { get; set; }

        [JsonPropertyName("schoolId")]
        [Required]
        public string SchoolId { get; set; }

        [JsonPropertyName("districtNumber")]
        public string? DistrictNumber { get; set; }

        [JsonPropertyName("schoolAuthorityNumber")]
        public string? SchoolAuthorityNumber { get; set; }

        [JsonPropertyName("independentAuthorityId")]
        [Required]
        public string IndependentAuthorityId { get; set; }

        [JsonPropertyName("mincode")]
        [Required]
        public string? Mincode { get; set; }

        [JsonPropertyName("districtId")]
        [Required]
        public string DistrictId { get; set; }

        // TODO: Missing
        [JsonPropertyName("certficateGrade")]
        public string? CertficateGrade { get; set; }

        // TODO: Missing
        [JsonPropertyName("schoolGrades")]
        public string? SchoolGrades { get; set; }

        // TODO: Missing
        [JsonPropertyName("schoolStatus")]
        public string? SchoolStatus { get; set; }


        [JsonPropertyName("schoolNumber")]
        [Required]
        public string? SchoolNumber { get; set; }

        [JsonPropertyName("faxNumber")]
        public string? FaxNumber { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("displayName")]
        [Required]
        public string? DisplayName { get; set; }


        [JsonPropertyName("schoolReportingRequirementCode")]
        public string? SchoolReportingRequirementCode { get; set; }

        [JsonPropertyName("schoolOrganizationCode")]
        public string? SchoolOrganizationCode { get; set; }

        [JsonPropertyName("schoolCategoryCode")]
        [Required]
        public string? SchoolCategoryCode { get; set; }

        [JsonPropertyName("facilityTypeCode")]
        [Required]
        public string? FacilityTypeCode { get; set; }

        [JsonPropertyName("facilityTypeNumber")]
        public string? FacilityTypeNumber { get; set; }

        [JsonPropertyName("openedDate")]
        public DateTimeOffset OpenedDate { get; set; }

        [JsonPropertyName("closedDate")]
        public DateTimeOffset? ClosedDate { get; set; }

        [JsonPropertyName("schoolFundingGroup")]
        public string? SchoolFundingGroup { get; set; }


        [JsonPropertyName("schoolFundingGroupCode")]
        public string? SchoolFundingCode { get; set; }
       
        [JsonPropertyName("schoolTeamOwnerOperatorNumber")]
        public string? SchoolTeamOwnerOperatorNumber { get; set; }

        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }

        [JsonPropertyName("contacts")]
        public SchoolContact[]? Contacts { get; set; }


        private SchoolCategory Category()
        {
            switch (this.SchoolCategoryCode)
            {
                case "OFFSHORE":
                    return SchoolCategory.Offsore;
                case "PUBLIC":
                    return SchoolCategory.Offsore;
                default:
                    return SchoolCategory.Independent;
            }
        }

        private SchoolFacilityCode Facility()
        {
            switch (this.FacilityTypeCode)
            {
                case "ONLINE":
                    return SchoolFacilityCode.Online;
                case "STANDALONE":
                    return SchoolFacilityCode.Standalone;
                case "HOST":
                    return SchoolFacilityCode.Host;
                default:
                    return SchoolFacilityCode.Standalone;
            }
        }

        static public string[] SchoolAuthorityIds(School[] schools)
        {
            return schools.Where(school => school.IndependentAuthorityId != null).Select(school => school.IndependentAuthorityId ?? "").ToArray();
        }
        static public string[] SchoolDistrictIds(School[] schools)
        {
            return schools.Select(school => school.DistrictId).ToArray();
        }

        private SchoolContact? GetPrincipal()
        {
            SchoolContact[]? principals = this.Contacts?.Where(contact => contact.SchoolContactTypeCode == "PRINCIPAL" && contact.ExpiryDate == null).ToArray();
            return principals != null && principals.Length > 0 ? principals[0] : null;
        }

        public JObject ToIOSAS(JObject lookups)
        {
            var result = new JObject();
            string? authorityNumebr = GetAuthorityNumber(lookups);
            // result["edu_schooldistrict"] = this.DistrictId; // TODO: Need mapping for district > find the code write
            result["edu_name"] = this.DisplayName;
            result["edu_schoolcode"] = this.SchoolNumber;
            result["edu_mincode"] = this.Mincode;
            //result["iosas_authority"] = this.IndependentAuthorityId; // TODO: Not getting Authority Number
            result["edu_fax"] = this.FaxNumber;
            result["iosas_email"] = this.Email;
            result["edu_phone"] = this.PhoneNumber;
            result["edu_opendate"] = this.OpenedDate.ToString("yyyy-MM-dd");
            result["edu_closedate"] = this.ClosedDate?.ToString("yyyy-MM-dd");
            result["edu_website"] = this.Website;
            result["edu_facilitytype"] = this.FacilityTypeCode == "STANDARD" ? 757500000 : 757500008; // Mapping: STANDARD: 757500000 | ONLINE 757500008
            result["edu_schoolcategory"] = (int)this.Category(); // TODOD: OFFSHORE | PUBLIC |
            result["iosas_facilitytypeoffshore"] = (int)this.Facility();
            result["iosas_externalid"] = this.SchoolId;
            result["iosas_authoritynumber"] = authorityNumebr;
            // Required fields 

            // Mail Address Mapping
            if (this.Addresses?.Length > 0)
            {
                var address = this.Addresses[0];
                result["edu_address1_line1"] = address.AddressLine1;
                result["edu_address1_line2"] = address.AddressLine2;
                result["edu_address1_city"] = address.City;
                result["edu_address1_postalcode"] = address.Postal;
                result["edu_address1_province"] = address.ProvinceCode;
                result["edu_address1_country"] = address.CountryCode;
            }
            // Address Mapping
            if (this.Addresses?.Length > 1)
            {
                var address = this.Addresses[1];
                result["edu_address2_line1"] = address.AddressLine1;
                result["edu_address2_line2"] = address.AddressLine2;
                result["edu_address2_city"] = address.City;
                result["edu_address2_postalcode"] = address.Postal;
                result["edu_address2_province"] = address.ProvinceCode;
                result["edu_address2_country"] = address.CountryCode;
            }
            // Authority
            SchoolAuthorityIOSAS schoolAuthMeta = new();
            AssignLookupKeyUsingExternalId(schoolAuthMeta, lookups, IndependentAuthorityId, "iosas_authority", result);
            // School District
            SchoolDistrictIOSAS schoolDistrictMeta = new();
            AssignLookupKeyUsingExternalId(schoolDistrictMeta, lookups, DistrictId, "edu_SchoolDistrict", result);

            // Handling Lookups
            // (School group)  => iosas_inspectionfundinggroup.iosas_facilitycode == 8
            // Funding Group => iosas_fundinggroups.iosas_name == concat("Group " + data.SchoolFundingGroup)
            // Owner filter owners iosas_owneroperators.teamtype == 0 and
            // (iosas_owneroperators.name [In Independent Schools Branch, Offshore School Program] => iosas_owneroperators. */

            // Owner operator if Non offsore school
            //  check DL school facility type DIST_LEARN/8 => 
            // Funding Group is not available
            // Owner operator id : fetch and save id for independent school user selected owner operator id
            // Rules to select owner operator id:
            //  1. For offsore school select operator for iosas_owneroperators.iosas_owneroperatornumber == data.AuthorityNumber for Offsore school
            // bind key: iosas_owneroperators
            
            IOSASOwnerOperator operatorMeta = new();
            if (authorityNumebr != null &&
                GetLookValue(lookups, operatorMeta.entityName, "iosas_owneroperatornumber", authorityNumebr, operatorMeta.primaryKey) is var operatorId && operatorId != null)
            {
                result["iosas_owneroperators@data.bind"] = $"{operatorMeta.entityName}({operatorId})";
            }

            // Update Principal
            if (GetPrincipal() is var pricipal && pricipal != null)
            {
                string fullName = pricipal.LastName != null ? $"{pricipal.FirstName ?? ""} ${pricipal.LastName}" : $"{pricipal.FirstName ?? ""}";
                result["iosas_firstname"] = pricipal.FirstName;
                result["iosas_lastname"] = pricipal.LastName;
                result["iosas_primarycontactname"] = fullName != "" ? fullName : null;
                result["iosas_principal"] = fullName != "" ? fullName : null;
            }
            // Funding grpup
            if (GetFundingGroup(lookups, false) is var group && group != null)
            {
                result.Add(group);
            }

            return result;
        }
        public JObject ToISFS(JObject lookups)
        {
            /*var result = ToIOSAS(lookups);
            result["edu_email"] = this.Email;
            result.Remove("iosas_externalid");
            result.Remove("iosas_email");
            result.Remove("iosas_facilitytypeoffshore");
            return result;*/
            var result = new JObject();
            string? authorityNumebr = GetAuthorityNumber(lookups);
            // result["edu_schooldistrict"] = this.DistrictId; // TODO: Need mapping for district > find the code write
            result["edu_name"] = this.DisplayName;
            result["edu_schoolcode"] = this.SchoolNumber;
            result["edu_mincode"] = this.Mincode;
            //result["iosas_authority"] = this.IndependentAuthorityId; // TODO: Not getting Authority Number
            result["edu_fax"] = this.FaxNumber;
            result["edu_email"] = this.Email;
            result["edu_phone"] = this.PhoneNumber;
            result["edu_opendate"] = this.OpenedDate.ToString("yyyy-MM-dd");
            result["edu_closedate"] = this.ClosedDate?.ToString("yyyy-MM-dd");
            result["edu_website"] = this.Website;
            result["edu_facilitytype"] = this.FacilityTypeCode == "STANDARD" ? 757500000 : 757500008; // Mapping: STANDARD: 757500000,  | ONLINE 757500008 |
            result["edu_schoolcategory"] = 757500002;
            result["edu_externalid"] = this.SchoolId;
            result["isfs_authoritynumber"] = authorityNumebr;
            // Required fields 

            // Mail Address Mapping
            if (this.Addresses?.Length > 0)
            {
                var address = this.Addresses[0];
                result["edu_address1_line1"] = address.AddressLine1;
                result["edu_address1_line2"] = address.AddressLine2;
                result["edu_address1_city"] = address.City;
                result["edu_address1_postalcode"] = address.Postal;
                result["edu_address1_province"] = address.ProvinceCode;
                result["edu_address1_country"] = address.CountryCode;
            }
            // Address Mapping
            if (this.Addresses?.Length > 1)
            {
                var address = this.Addresses[1];
                result["edu_address2_line1"] = address.AddressLine1;
                result["edu_address2_line2"] = address.AddressLine2;
                result["edu_address2_city"] = address.City;
                result["edu_address2_postalcode"] = address.Postal;
                result["edu_address2_province"] = address.ProvinceCode;
                result["edu_address2_country"] = address.CountryCode;
            }
            // Authority
            AuthorityISFS schoolAuthMeta = new();
            AssignLookupKeyUsingExternalId(schoolAuthMeta, lookups, IndependentAuthorityId, "isfs_Authority", result);
            // School District
            SchoolDistrictISFS schoolDistrictMeta = new();
            AssignLookupKeyUsingExternalId(schoolDistrictMeta, lookups, DistrictId, "edu_SchoolDistrict", result);

            // Handling Lookups
            // (School group)  => iosas_inspectionfundinggroup.iosas_facilitycode == 8
            // Funding Group => iosas_fundinggroups.iosas_name == concat("Group " + data.SchoolFundingGroup)
            // Owner filter owners iosas_owneroperators.teamtype == 0 and
            // (iosas_owneroperators.name [In Independent Schools Branch, Offshore School Program] => iosas_owneroperators. */

            // Owner operator if Non offsore school
            //  check DL school facility type DIST_LEARN/8 => 
            // Funding Group is not available
            // Owner operator id : fetch and save id for independent school user selected owner operator id
            // Rules to select owner operator id:
            //  1. For offsore school select operator for iosas_owneroperators.iosas_owneroperatornumber == data.AuthorityNumber for Offsore school
            // bind key: iosas_owneroperators

            // Update Principal
            if (GetPrincipal() is var pricipal && pricipal != null)
            {
                string fullName = pricipal.LastName != null ? $"{pricipal.FirstName ?? ""} {pricipal.LastName}" : $"{pricipal.FirstName ?? ""}";
                result["isfs_schoolprincipal"] = fullName != "" ? fullName : null;
            }

            // Funding grpup
            if (GetFundingGroup(lookups) is var group && group != null)
            {
                result.Add(group);
            }

            return result;
        }
        private string? GetAuthorityNumber(JObject lookups)
        {
            SchoolAuthorityIOSAS meta = new();
            JObject[]? authorities = lookups
                .GetValue(meta.entityName)?
                .ToArray()
                .Where(authority => ((JObject)authority).GetValue(meta.externalIdKey)?.ToString() == ExternalId())
                .Select(authority => (JObject)authority)
                .ToArray();
            return authorities?.Length > 0 ? authorities[0]?.GetValue(meta.businessKey)?.ToString() : null;
        }
        private JProperty? GetFundingGroup(JObject lookup, Boolean isIOSAS = true)
        {
            if (this.SchoolFundingGroup == null)
            {
                return null;
            }
            string groupName = $"Group {SchoolFundingGroup}";
            D365ModelMetdaData meta = isIOSAS ? new IOSASFundingGroup() : new ISFSFundingGroup();
            System.Console.WriteLine($"GetFundingGroup => {lookup[meta.entityName]?.ToString()}");
            JObject[]? fundingGroups = lookup[meta.entityName]?
                .ToArray()
                .Select(token => (JObject)token)
                .Where(item => item.GetValue(meta.businessKey)?.ToString() == groupName)
                .ToArray();
            if (fundingGroups != null &&
                fundingGroups.Length > 0 &&
                fundingGroups[0] is var fundingGroup &&
                fundingGroup.GetValue(meta.primaryKey) is var idKey &&
                idKey != null
                )
            {
                return isIOSAS ? new JProperty("iosas_fundinggroup@odata.bind", $"{meta.entityName}({idKey})") :
                    new JProperty("isfs_FundingGroup@odata.bind", $"{meta.entityName}({idKey})");
            }
            return null;

        }
        private void AssignLookupKeyUsingExternalId(D365ModelMetdaData meta, JObject lookups,string matchingValue, string assignmentKey, JObject result)
        {
            if (GetLookValue(lookups, meta.entityName, meta.externalIdKey, matchingValue, meta.primaryKey) is var id && id != null)
            {
                result[$"{assignmentKey}@odata.bind"] = $"{meta.entityName}({id})";
            }
        }
        private static void AssignLookupMatchingValue(D365ModelMetdaData meta, JObject lookups, string assignmentKey, string matchingValue, JObject result)
        {
            if (GetLookValue(lookups, meta.entityName, meta.externalIdKey, matchingValue, meta.primaryKey) is var id && id != null)
            {
                result[$"{assignmentKey}@odata.bind"] = $"{meta.entityName}({id})";
            }
        }
        private static string? GetLookValue(JObject lookup, string lookupKey, string matchingKey ,string matchingValue, string idKey)
        {
            // System.Console.WriteLine($" {lookupKey} {matchingKey} {matchingValue} {idKey} {lookup.GetValue(lookupKey)}");
            JObject[]? items = lookup.GetValue(lookupKey)?
                   .ToArray()
                   .Select(token => (JObject)token)?
                   .Where(item => item.GetValue(matchingKey)?.ToString() == matchingValue)?
                   .ToArray();
            return items?.Length > 0 ? items[0]?.GetValue(idKey)?.ToString() : null;
        }
        public string UpsertQuery()
        {

            return $"";
        }
        public string GetQuery()
        {
            return WebUtility.UrlEncode($"");
        }
        public string IdQuery(string id)
        {
            return "";
        }
        public string EntityName()
        {
            return "";
        }
        public string Key()
        {
            return "";
        }
        public string KeyValue()
        {
            return this.Mincode ?? "";
        }
        public string KeyDisplay()
        {
            return $"schoolMinCode={this.Mincode}";
        }
        public string ExternalId()
        {
            return SchoolId;
        }
        public bool VerifyExisting(D365ModelMetdaData meta, JObject data, ILogger? logger)
        {
            string buisnessValue = data[meta.businessKey]?.ToString() ?? "";
            string? externalId = data[meta.externalIdKey]?.ToString();
            return buisnessValue == this.Mincode && (externalId == null || externalId == "" || externalId == ExternalId()); ;
        }
    }
}

