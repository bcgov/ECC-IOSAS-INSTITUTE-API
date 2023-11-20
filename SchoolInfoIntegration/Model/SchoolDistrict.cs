using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public class SchoolDistrict
    {
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonPropertyName("createDate")]
        public string? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public string? UpdateDate { get; set; }

        [JsonPropertyName("districtId")]
        public string? DistrictId { get; set; }

        [JsonPropertyName("districtNumber")]
        public string? DistrictNumber { get; set; }

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

        [JsonPropertyName("districtRegionCode")]
        public string? DistrictRegionCode { get; set; }

        [JsonPropertyName("districtStatusCode")]
        public string? DistrictStatusCode { get; set; }

        [JsonPropertyName("contacts")]
        public Contact[]? Contacts { get; set; }

        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }

        [JsonPropertyName("notes")]
        public Note[]? Notes { get; set; }

        public JObject ToD365EntityModel()
        {
            var result = new JObject();
            result["DistrictNo"] = this.DistrictNumber;
            result["edu_name"] = this.DisplayName;
            result["edu_districtstatus"] = this.DistrictStatusCode;
            result["Region_edu_internalcode"] = this.DistrictRegionCode;
            result["edu_fax"] = this.FaxNumber;
            result["edu_email"] = this.Email;
            result["edu_website"] = this.Website;
            var empty = Array.Empty<Address>();
            // Mail Address Mapping
            if (this.Addresses?.Length > 0 && Address.getAddressWithType(AddressType.Mailing.Value, (this.Addresses ?? empty)) is var mailingAddress && mailingAddress != null)
            {
                result["edu_mailaddressline1"] = mailingAddress.AddressLine1;
                result["edu_mailaddressline2"] = mailingAddress.AddressLine2;
                result["edu_mailcity"] = mailingAddress.City;
                result["edu_mailpostalcode"] = mailingAddress.Postal;
                result["edu_mailprovince"] = mailingAddress.ProvinceCode;
                result["edu_mailcountry"] = mailingAddress.CountryCode;
            }
            // Address Mapping
            if (this.Addresses?.Length > 0 && Address.getAddressWithType(AddressType.Address.Value, (this.Addresses ?? empty)) is var address  && address != null)
            {

                result["edu_addressline1"] = address.AddressLine1;
                result["edu_ddressline2"] = address.AddressLine2;
                result["edu_city"] = address.City;
                result["edu_postalcode"] = address.Postal;
                result["edu_province"] = address.ProvinceCode;
                result["edu_country"] = address.CountryCode;
            }
            return result;
        }
    }

    public partial class Note
    {
        [JsonPropertyName("createUser")]
        public string? CreateUser { get; set; }

        [JsonPropertyName("updateUser")]
        public string? UpdateUser { get; set; }

        [JsonPropertyName("createDate")]
        public string? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public string? UpdateDate { get; set; }

        [JsonPropertyName("noteId")]
        public string? NoteId { get; set; }

        [JsonPropertyName("schoolId")]
        public string? SchoolId { get; set; }

        [JsonPropertyName("districtId")]
        public string? DistrictId { get; set; }

        [JsonPropertyName("independentAuthorityId")]
        public string? IndependentAuthorityId { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

