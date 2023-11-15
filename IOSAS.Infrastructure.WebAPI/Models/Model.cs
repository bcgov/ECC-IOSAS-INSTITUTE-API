using System.Text.Json.Serialization;

namespace IOSAS.Infrastructure.WebAPI.Models
{
    public class Document
    {
        [JsonPropertyName("regardingId")]
        public string? RegardingId { get; set; }

        [JsonPropertyName("documentName")]
        public string? DocumentName { get; set; }

        [JsonPropertyName("documentCategory")]
        public long? DocumentCategory { get; set; }

        [JsonPropertyName("documentType")]
        public long? DocumentType { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("regardingType")]
        public string? RegardingType { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

    }
}
