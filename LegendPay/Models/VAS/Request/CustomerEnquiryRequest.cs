using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Request
{
    public class CustomerEnquiryRequest
    {
        [JsonPropertyName("customerId")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("billerSlug")]
        public string BillerSlug { get; set; } = string.Empty;

        /// Typically the package slug. Required for all Discos
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }
    }
}
