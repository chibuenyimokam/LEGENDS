using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Response
{
    public class BillerGroupIdEnquiryResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("skipValidation")]
        public bool SkipValidation { get; set; }

        [JsonPropertyName("handleWithProductCode")]
        public bool HandleWithProductCode { get; set; }

        [JsonPropertyName("isRestricted")]
        public bool IsRestricted { get; set; }

        [JsonPropertyName("hideInstitution")]
        public bool HideInstitution { get; set; }
    }
}
