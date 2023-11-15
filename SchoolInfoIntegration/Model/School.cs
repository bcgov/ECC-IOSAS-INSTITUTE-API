using System.Text.Json.Serialization;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public partial class School
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
        public object? IndependentAuthorityId { get; set; }

        [JsonPropertyName("schoolNumber")]
        public string? SchoolNumber { get; set; }

        [JsonPropertyName("faxNumber")]
        public string? FaxNumber { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("website")]
        public object? Website { get; set; }

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
    }
}

