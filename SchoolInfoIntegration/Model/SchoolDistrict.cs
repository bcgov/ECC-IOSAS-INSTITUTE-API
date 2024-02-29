using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string DistrictId { get; set; }

        [JsonPropertyName("districtNumber")]
        [Required]
        public long DistrictNumber { get; set; }

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
        public string DisplayName { get; set; }

        [JsonPropertyName("districtRegionCode")]
        [Required]
        public string? DistrictRegionCode { get; set; }

        [JsonPropertyName("districtStatusCode")]
        [Required]
        public string? DistrictStatusCode { get; set; }

        [JsonPropertyName("contacts")]
        public Contact[]? Contacts { get; set; }

        [JsonPropertyName("addresses")]
        public Address[]? Addresses { get; set; }

        public JObject ToIOSAS(JObject lookups)
        {
            var result = new JObject();
            result["edu_number"] = $"SD{this.KeyValue()}";
            result["edu_internalcode"] = this.KeyValue();
            result["edu_name"] = this.DisplayName;
            result["edu_districtstatus"] = this.DistrictStatusCode == "ACTIVE" ? true : false; // ACTIVE : 1, INACTIVE: 0
            // result["edu_region"] = this.DistrictRegionCode; // TODO: Have to find a mapping logic
            // { "iosas_edu_Year@odata.bind", $"/edu_years({value._iosas_edu_year_value})" }
            result["edu_fax"] = this.FaxNumber;
            result["edu_website"] = this.Website;
            result["iosas_externalid"] = this.DistrictId;
            result["iosas_email"] = this.Email;
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
            // Edu region: Lookup
            JObject[]? regions = lookups.GetValue("edu_regions")?
                   .ToArray()
                   .Select(token => (JObject)token)
                   .ToArray();
            var isRegionsAvailable = regions != null && regions.Length > 0;
            if (isRegionsAvailable && this.DistrictRegionCode != null && RegionMapper.Shared().GetValueForRegion(DistrictRegionCode ?? "") is var regionNumber && regionNumber != null)
            {
                if (regions != null && regions.Length > 0 &&
                    Array.Find(regions, (region) => region.GetValue("edu_regionnumber")?.ToString() == regionNumber) is var selectedRegion &&
                    selectedRegion != null &&
                    selectedRegion.GetValue("edu_regionid")?.ToString() is var regionId &&
                    regionId != null
                    )
                {
                    result["edu_Region@odata.bind"] = $"edu_regions({regionId})";

                }
            } else if (isRegionsAvailable && regions != null &&
                Array.Find(regions, (region) => region.GetValue("edu_name")?.ToString().ToLower().Contains("other") ?? false) is var otherRegion &&
                otherRegion != null &&
                otherRegion.GetValue("edu_regionid")?.ToString() is var regionId &&
                    regionId != null
                )
            {
                // Assign other region
                result["edu_Region@odata.bind"] = $"edu_regions({regionId})";

            }
            return result;
        }
        public JObject ToISFS(JObject lookups)
        {
            var result = ToIOSAS(lookups);
            result["edu_externalid"] = this.DistrictId;
            result.Remove("iosas_externalid");
            result.Remove("iosas_email");
            result.Remove("edu_Region@odata.bind");
            // Edu region: Lookup
            JObject[]? regions = lookups.GetValue("edu_regions")?
                   .ToArray()
                   .Select(token => (JObject)token)
                   .ToArray();
            var isRegionsAvailable = regions != null && regions.Length > 0;
            if (isRegionsAvailable && this.DistrictRegionCode != null && RegionMapper.Shared().GetValueForRegion(DistrictRegionCode ?? "") is var regionNumber && regionNumber != null)
            {
                if (regions != null && regions.Length > 0 &&
                    Array.Find(regions, (region) => region.GetValue("edu_internalcode")?.ToString() == regionNumber) is var selectedRegion &&
                    selectedRegion != null &&
                    selectedRegion.GetValue("edu_regionid")?.ToString() is var regionId &&
                    regionId != null
                    )
                {
                    result["edu_Region@odata.bind"] = $"edu_regions({regionId})";

                }
            }
            else if (isRegionsAvailable && regions != null &&
                Array.Find(regions, (region) => region.GetValue("edu_name")?.ToString().ToLower().Contains("other") ?? false) is var otherRegion &&
                otherRegion != null &&
                otherRegion.GetValue("edu_regionid")?.ToString() is var regionId &&
                    regionId != null
                )
            {
                // Assign other region
                result["edu_Region@odata.bind"] = $"edu_regions({regionId})";

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
            var value = this.DistrictNumber.ToString($"D3") ?? "";
            return value;
        }
        public string KeyDisplay()
        {
            return $"schoolDistrictNo={this.KeyValue()}";
        }

        public string ExternalId()
        {
            return DistrictId;
        }

        public bool VerifyExisting(D365ModelMetdaData meta, JObject data, ILogger? logger)
        {
            string buisnessValue = data[meta.businessKey]?.ToString() ?? "";
            string? externalId = data[meta.externalIdKey]?.ToString();
            return buisnessValue == this.DistrictNumber.ToString($"D3") && (externalId == null || externalId == "" || externalId == ExternalId()); ;
        }
    }
}

