using System.Text.Json.Serialization;
using static LegendPay.Models.VAS.Response.CustomerEnquiryResponse;
using static LegendPay.Models.VAS.Response.VendValueResponse;

namespace LegendPay.Models.VAS.Response
{
    public class VendTransactionEnquiryResponse
    {
        public VasResponseHeader? ResponseHeader { get; set; }
        public ResponseData? ResponseData { get; set; }
    }
        public class ResponseData 
        {
            [JsonPropertyName("billerName")]
            public string? BillerName { get; set; }

            [JsonPropertyName("customer")]
            public VasCustomerInfo? Customer { get; set; }

            [JsonPropertyName("tokenData")]
            public VasTokenData? TokenData { get; set; }

            [JsonPropertyName("paid")]
            public bool Paid { get; set; }

            [JsonPropertyName("paymentReference")]
            public string? PaymentReference { get; set; }

            [JsonPropertyName("transactionId")]
            public string? TransactionId { get; set; }

            [JsonPropertyName("vendStatus")]
            public string? VendStatus { get; set; }

            [JsonPropertyName("narration")]
            public string? Narration { get; set; }

            [JsonPropertyName("statusCode")]
            public string? StatusCode { get; set; }

            [JsonPropertyName("amount")]
            public decimal? Amount { get; set; }

            [JsonPropertyName("customerMessage")]
            public string? CustomerMessage { get; set; }

            [JsonPropertyName("orderId")]
            public string? OrderId { get; set; }

            [JsonPropertyName("date")]
            public string? Date { get; set; }

            [JsonPropertyName("confirmationTime")]
            public string? ConfirmationTime { get; set; }
        }
    
}

