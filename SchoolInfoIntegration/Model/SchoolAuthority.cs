using System.Text.Json.Serialization;
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
    public partial class SchoolAuthority
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
            result["iosas_authorityno"] = this.AuthorityNumber;
            result["iosas_authorityno"] = this.DisplayName;
            result["iosas_phone"] = this.PhoneNumber;
            result["iosas_fax"] = this.FaxNumber;
            result["iosas_email"] = this.Email;
            result["iosas_authoritystatus"] = this.AuthorityStatus; // TODO: Need default value
            result["iosas_authoritytype"] = this.AuthorityTypeCode;
            result["iosas_opendate"] = this.OpenedDate;
            result["iosas_closedate"] = this.ClosedDate;
            // Address Mapping
            if (this.Addresses?.Length > 0)
            {
                var address = this.Addresses[0];
                result["iosas_addressline1"] = address.AddressLine1;
                result["iosas_addressline2"] = address.AddressLine2;
                result["iosas_addresscity"] = address.City;
                result["iosas_addresspostalcode"] = address.Postal;
                result["iosas_addressprovince"] = address.ProvinceCode;
                result["iosas_addresscountry"] = address.CountryCode;
            }
            // Contact Mapping
            const string AuthTypeCode = "INDAUTHREP";
            var authContacts = (Contact[]?)(this.Contacts?.Where(contact => (contact.AuthorityContactTypeCode ?? $"") == AuthTypeCode));
            var sortedAuthContacts = authContacts?.OrderBy((contact) => contact.CreateDate).ToArray();
            if (sortedAuthContacts?.Length > 0)
            {
                var contact = sortedAuthContacts[0];
                result["iosas_firstname"] = contact.FirstName;
                result["iosas_lastname"] = contact.LastName;
                result["iosas_maincontact"] = contact.AuthorityContactId;
            }
            return result;
        }
    }

    

}

