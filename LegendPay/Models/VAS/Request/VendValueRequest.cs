using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Request
{
    public class VendValueRequest
    {
        [JsonPropertyName("paymentReference")]
        public string PaymentReference { get; set; } = string.Empty;

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("packageSlug")]
        public string? PackageSlug { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = "WEB";

        /// <summary>Not required for DSTV/GOTV — fixed package amounts.</summary>
        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }
    }
}
