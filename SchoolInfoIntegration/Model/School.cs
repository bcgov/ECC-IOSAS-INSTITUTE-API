using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
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

    public partial class School: D365Model
    {
        [JsonPropertyName("createDate")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTimeOffset UpdateDate { get; set; }

        [JsonPropertyName("schoolId")]
        public Guid SchoolId { get; set; }

        [JsonPropertyName("districtNumber")]
        public Guid DistrictNumber { get; set; }

        [JsonPropertyName("schoolAuthorityNumber")]
        public string SchoolAuthorityNumber { get; set; }

        [JsonPropertyName("mincode")]
        public string? Mincode { get; set; }

        // TODO: Missing
        [JsonPropertyName("certficateGrade")]
        public string? CertficateGrade { get; set; }

        // TODO: Missing
        [JsonPropertyName("schoolGrades")]
        public string? SchoolGrades { get; set; }

        // TODO: Missing
        [JsonPropertyName("schoolStatus")]
        public string? SchoolStatus { get; set; }

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

        [JsonPropertyName("facilityTypeNumber")]
        public string? FacilityTypeNumber { get; set; }

        [JsonPropertyName("openedDate")]
        public DateTimeOffset OpenedDate { get; set; }

        [JsonPropertyName("closedDate")]
        public DateTimeOffset ClosedDate { get; set; }

        [JsonPropertyName("schoolFundingGroup")]
        public string? SchoolFundingGroupName { get; set; }

        // teamOwnerId
        [JsonPropertyName("schoolTeamOwnerId")]
        public string? SchoolTeamOwnerId { get; set; }

        // TODO: Missing
        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }

        // TODO: Missing
        [JsonPropertyName("principal")]
        public Principal? Principal { get; set; }


        private SchoolCategory Category()
        {
            switch (this.SchoolCategoryCode)
            {
                case "OFFSHORE":
                    return SchoolCategory.Offsore;
                case "PUBLIC":
                    return SchoolCategory.Public;
                default:
                    return SchoolCategory.Independent;
            }
        }
        public JObject ToD365EntityModel()
        {
            var result = new JObject();
            // result["edu_schooldistrict"] = this.DistrictId; // TODO: Need mapping for district > find the code write
            result["edu_name"] = this.DisplayName;
            result["edu_schoolcode"] = this.SchoolNumber;
            result["edu_mincode"] = this.Mincode;
            //result["iosas_authority"] = this.IndependentAuthorityId; // TODO: Not getting Authority Number
            result["edu_fax"] = this.FaxNumber;
            result["iosas_email"] = this.Email;
            result["edu_phone"] = this.PhoneNumber;
            result["edu_opendate"] = this.OpenedDate.ToString("yyyy-mm-dd");
            result["edu_closedate"] = this.ClosedDate.ToString("yyyy-mm-dd");
            result["edu_closedate"] = this.Website;
            result["edu_facilitytype"] = this.FacilityTypeCode == "STANDARD" ? 757500000: 757500008; // Mapping: STANDARD: 757500000 | ONLINE 757500008
            result["edu_schoolcategory"] = (int) this.Category(); // TODOD: OFFSHORE | PUBLIC |
            // Required fields 

            // Mail Address Mapping
            /*if (this.Addresses?.Length > 0)
            {
                var address = this.Addresses[0];
                result["edu_address1_city"] = address.AddressLine1;
                result["edu_mailaddressline2"] = address.AddressLine2;
                result["edu_address1_city"] = address.City;
                result["edu_mailpostalcode"] = address.Postal;
                result["edu_mailprovince"] = address.ProvinceCode;
                result["edu_address1_country"] = address.CountryCode;
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
            return this.Mincode ?? "";
        }
        public string KeyDisplay()
        {
            return $"schoolMinCode={this.Mincode}";
        }
    }
}

