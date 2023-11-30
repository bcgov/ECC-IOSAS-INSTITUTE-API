using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public class SchoolDistrict: D365Model
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

        public JObject ToD365EntityModel()
        {
            var result = new JObject();
            result["edu_number"] = $"SD{this.KeyValue()}";
            result["edu_internalcode"] = this.KeyValue();
            result["edu_name"] = this.DisplayName;
            result["edu_districtstatus"] = this.DistrictStatusCode == "ACTIVE" ? true: false; // Active : 1, Inactiave: 0
            // result["edu_region"] = this.DistrictRegionCode; // TODO: Have to find a mapping logic
            // { "iosas_edu_Year@odata.bind", $"/edu_years({value._iosas_edu_year_value})" }
            result["edu_fax"] = this.FaxNumber;
            result["edu_email"] = this.Email;
            result["edu_website"] = this.Website;
            result["iosas_externalid"] = this.DistrictId;
            var empty = Array.Empty<Address>();
            // Mail Address Mapping
            if (Address.getAddressWithType(AddressType.Mailing.Value, this.Addresses) is var mailingAddress && mailingAddress != null)
            {
                result["edu_mailaddressline1"] = mailingAddress.AddressLine1;
                result["edu_mailaddressline2"] = mailingAddress.AddressLine2;
                result["edu_mailcity"] = mailingAddress.City;
                result["edu_mailpostalcode"] = mailingAddress.Postal;
                result["edu_mailprovince"] = mailingAddress.ProvinceCode;
                result["edu_mailcountry"] = mailingAddress.CountryCode;
            }
            // Physical Address Mapping
            if (Address.getAddressWithType(AddressType.Physical.Value, this.Addresses) is var address && address != null)
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

        public string UpsertQuery()
        {

            return $"";
        }

        public string GetQuery()
        {
            return $"edu_schooldistricts?#select=edu_name,edu_schooldistrictid";
        }
        public string IdQuery(string id)
        {
            return "";
        }
        public string EntityName()
        {
            return "edu_schooldistricts";
        }
        public string Key()
        {
            return "edu_number";
        }
        public string KeyValue()
        {
            var value = this.DistrictNumber ?? "";
            return value;
        }
        public string KeyDisplay()
        {
            return $"schoolDistrictNo={this.KeyValue()}";
        }
    }
}

