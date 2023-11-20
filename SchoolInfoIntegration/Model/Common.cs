﻿using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ECC.Institute.CRM.IntegrationAPI.Model
{
    public class AddressType
    {
        private AddressType(string input) { this.Value = input; }
        public string Value { get; private set; }
        public static AddressType Mailing { get { return new AddressType("MAILING"); } }
        public static AddressType Address { get { return new AddressType("ADDRESS"); } }
        public override string ToString()
        {
            return Value;
        }
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

        public static Address? getAddressWithType(string type, Address[] addresses)
        {
            return Array.Find(addresses, (adress) => adress.AddressTypeCode == type);
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
}

