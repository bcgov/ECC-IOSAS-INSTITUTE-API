using System;
using System.Text.Json.Serialization;
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

