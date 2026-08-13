using System.Text.Json.Serialization;

namespace LegendPay.Models.VAS.Response
{
    public class PackagesEnquiryResponse
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("billerId")]
        public int BillerId { get; set; }

        [JsonPropertyName("sequenceNumber")]
        public int SequenceNumber { get; set; }
    }
}
