using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public partial class School: D365Model
    {
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonPropertyName("createDate")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTimeOffset UpdateDate { get; set; }

        [JsonPropertyName("schoolId")]
        public Guid SchoolId { get; set; }

        [JsonPropertyName("districtId")]
        public Guid DistrictId { get; set; }

        [JsonPropertyName("mincode")]
        public string? Mincode { get; set; }

        [JsonPropertyName("independentAuthorityId")]
        public string? IndependentAuthorityId { get; set; }

        [JsonPropertyName("schoolNumber")]
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
        public string? DisplayName { get; set; }

        [JsonPropertyName("displayNameNoSpecialChars")]
        public string? DisplayNameNoSpecialChars { get; set; }

        [JsonPropertyName("schoolReportingRequirementCode")]
        public string? SchoolReportingRequirementCode { get; set; }

        [JsonPropertyName("schoolOrganizationCode")]
        public string? SchoolOrganizationCode { get; set; }

        [JsonPropertyName("schoolCategoryCode")]
        public string? SchoolCategoryCode { get; set; }

        [JsonPropertyName("facilityTypeCode")]
        public string? FacilityTypeCode { get; set; }

        [JsonPropertyName("openedDate")]
        public DateTimeOffset OpenedDate { get; set; }

        [JsonPropertyName("closedDate")]
        public DateTimeOffset ClosedDate { get; set; }

        public JObject ToD365EntityModel()
        {
            var result = new JObject();
            result["schoolDistrictNo"] = this.DistrictId; // TODO: Not getting Distraic Number
            result["schoolName"] = this.DisplayName;
            result["schoolCode"] = this.SchoolNumber;
            result["minCode"] = this.Mincode;
            result["authorityNumber"] = this.IndependentAuthorityId; // TODO: Not getting Authority Number
            result["fax"] = this.FaxNumber;
            result["email"] = this.Email;
            result["phone"] = this.PhoneNumber;
            result["opendate"] = this.OpenedDate;
            result["closedate"] = this.ClosedDate;
            result["website"] = this.Website;
            result["facilityType"] = this.FacilityTypeCode;
            result["schoolCategory"] = this.SchoolCategoryCode;
            result["teamOwnerId"] = $"OWNER";
            // Required fields 
            // schoolStatus => schoolStatus
            // schoolType => schoolGrades
            // Principal 
                // principal_FirstName
                // principal_LastName
                // principal_MiddleName
                // principal_FullName
                // principal_Prefix
            // Address
            // Mail Address Mapping
            /*if (this.Addresses?.Length > 0)
            {
                var address = this.Addresses[0];
                result["edu_mailaddressline1"] = address.AddressLine1;
                result["edu_mailaddressline2"] = address.AddressLine2;
                result["edu_mailcity"] = address.City;
                result["edu_mailpostalcode"] = address.Postal;
                result["edu_mailprovince"] = address.ProvinceCode;
                result["edu_mailcountry"] = address.CountryCode;
            }
            // Address Mapping
            if (this.Addresses?.Length > 1)
            {
                var address = this.Addresses[1];
                result["edu_addressline1"] = address.AddressLine1;
                result["edu_ddressline2"] = address.AddressLine2;
                result["edu_city"] = address.City;
                result["edu_postalcode"] = address.Postal;
                result["edu_province"] = address.ProvinceCode;
                result["edu_country"] = address.CountryCode;
            }*/
            return result;
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
            return "";
        }
    }
}

