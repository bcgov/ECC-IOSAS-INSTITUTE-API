using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public interface D365Model
    {
        public JObject ToIOSAS(JObject lookups);
        public JObject ToISFS(JObject lookups);
        public string UpsertQuery();
        public string GetQuery();
        public string IdQuery(string id);
        public string EntityName();
        public string Key();
        public string KeyValue();
        public string KeyDisplay();
        public string ExternalId();
        public bool VerifyExisting(D365ModelMetdaData meta, JObject data, ILogger? logger);
    }

    public interface ID365ImportModel
    {
        public JObject ToIOSAS();
        public JObject ToISFS();
    }

    public class D365ImportBase: ID365ImportModel
    {
        public virtual JObject ToIOSAS()
        {
            return new();
        }
        public virtual JObject ToISFS()
        {
            return new();
        }
    }

    public class D365ModelUtility
    {
        /*public static JObject[] ToJSONArray(D365Model[] items)
        {
            return items.Select(item => item.ToD365EntityModel()).ToArray() ;
        }*/
        public static string ResponseDescription(HttpResponseMessage message)
        {
            return $"Resp: URI: [{message.RequestMessage?.RequestUri}] | Status: {message.StatusCode}, ${message.ReasonPhrase} | Content: {message.Content.ReadAsStringAsync().Result}";
        }
    }

    public class AddressType
    {
        private AddressType(string input) { this.Value = input; }
        public string Value { get; private set; }
        public static AddressType Mailing { get { return new AddressType("MAILING"); } }
        public static AddressType Physical { get { return new AddressType("PHYSICAL"); } } // Physical 
        public override string ToString()
        {
            return Value;
        }
    }

    public class DynamicEntityName
    {
        public string Value { get; private set; }
        private DynamicEntityName(string entityName) { this.Value = entityName; }
        public static DynamicEntityName SchoolAuthority { get { return new DynamicEntityName("edu_SchoolAuthority"); } }
        public static DynamicEntityName SchoolDistrict { get { return new DynamicEntityName("edu_SchoolDistrict"); } }
    }

    
    public partial class Address
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
        [JsonPropertyName("addressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("addressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("postal")]
        public string? Postal { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("addressTypeCode")]
        public string? AddressTypeCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("provinceCode")]
        public string? ProvinceCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("independentAuthorityAddressId")]
        public Guid? IndependentAuthorityAddressId { get; set; }

        [JsonPropertyName("independentAuthorityId")]
        public string? IndependentAuthorityId { get; set; }

        public static Address? getAddressWithType(string type, Address[]? addresses)
        {
            if (addresses?.Length > 0)
            {
                return Array.Find(addresses, (adress) => adress.AddressTypeCode == type);
            } else
            {
                return null;
            }
            
        }
    }

    public partial class Contact
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
        [JsonPropertyName("authorityContactId")]
        public Guid? AuthorityContactId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("independentAuthorityId")]
        public Guid? IndependentAuthorityId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("authorityContactTypeCode")]
        public string? AuthorityContactTypeCode { get; set; }

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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("effectiveDate")]
        public DateTimeOffset? EffectiveDate { get; set; }

        [JsonPropertyName("expiryDate")]
        public string? ExpiryDate { get; set; }
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

