using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Response
{
    public class CustomerEnquiryResponse
    {

        [JsonPropertyName("billerName")]
        public string? BillerName { get; set; }

        [JsonPropertyName("customer")]
        public VasCustomerInfo? Customer { get; set; }

        [JsonPropertyName("paid")]
        public bool Paid { get; set; }

        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; set; }

        [JsonPropertyName("minPayableAmount")]
        public decimal? MinPayableAmount { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }
        public class VasCustomerInfo
        {
            [JsonPropertyName("firstName")]
            public string? FirstName { get; set; }

            [JsonPropertyName("lastName")]
            public string? LastName { get; set; }

            [JsonPropertyName("customerName")]
            public string? CustomerName { get; set; }

            [JsonPropertyName("accountNumber")]
            public string? AccountNumber { get; set; }

            [JsonPropertyName("canVend")]
            public bool CanVend { get; set; }

            [JsonPropertyName("address")]
            public string? Address { get; set; }

            [JsonPropertyName("meterSerial")]
            public string? MeterSerial { get; set; }

            [JsonPropertyName("meterNumber")]
            public string? MeterNumber { get; set; }

            [JsonPropertyName("tariffDescription")]
            public string? TariffDescription { get; set; }

            [JsonPropertyName("accountBalance")]
            public decimal? AccountBalance { get; set; }

            [JsonPropertyName("indicatorPrePostAccount")]
            public int? IndicatorPrePostAccount { get; set; }
        }
    }
}
