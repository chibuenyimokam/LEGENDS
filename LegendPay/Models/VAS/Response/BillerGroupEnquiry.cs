using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Response
{
    public class BillerGroupEnquiry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("addedBy")]
        public int AddedBy { get; set; }

        [JsonPropertyName("dateAdded")]
        public string? DateAdded { get; set; }

        [JsonPropertyName("dateUpdated")]
        public string? DateUpdated { get; set; }
    }
}
