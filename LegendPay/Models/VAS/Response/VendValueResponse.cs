using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Response
{  
    public class VendValueResponse
    {
        public VasResponseHeader? VasResponseHeader { get; set; }

        [JsonPropertyName("packageName")]
        public string? PackageName { get; set; }

        [JsonPropertyName("tokenData")]
        public VasTokenData? TokenData { get; set; }

        [JsonPropertyName("paymentDate")]
        public string? PaymentDate { get; set; }

        [JsonPropertyName("paid")]
        public bool Paid { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("paymentReference")]
        public string? PaymentReference { get; set; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("accountBalance")]
        public decimal? AccountBalance { get; set; }

        /// <summary>Pending | Failed | Confirmed | Awaiting_Service_Provider</summary>
        [JsonPropertyName("vendStatus")]
        public string? VendStatus { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("narration")]
        public string? Narration { get; set; }

        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; set; }

        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("debtPayment")]
        public decimal? DebtPayment { get; set; }

        [JsonPropertyName("convenienceFee")]
        public decimal? ConvenienceFee { get; set; }

        [JsonPropertyName("customerMessage")]
        public string? CustomerMessage { get; set; }

        [JsonPropertyName("meterNumber")]
        public string? MeterNumber { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

       public class VasTokenData
        {
          [JsonPropertyName("stdToken")]
          public StdToken? StdToken { get; set; }
        }
        public class StdToken
        {
            [JsonPropertyName("amount")]
            public string? Amount { get; set; }

            [JsonPropertyName("receiptNumber")]
            public string? ReceiptNumber { get; set; }

            [JsonPropertyName("units")]
            public string? Units { get; set; }

            [JsonPropertyName("unitsType")]
            public string? UnitsType { get; set; }

            [JsonPropertyName("value")]
            public string? Value { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("tariff")]
            public string? Tariff { get; set; }

            [JsonPropertyName("tax")]
            public string? Tax { get; set; }
        }

        
    }
}
