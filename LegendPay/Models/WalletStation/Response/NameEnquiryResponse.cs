using Newtonsoft.Json;

namespace LegendPay.Models.WalletStation.Response
{
    public class NameEnquiryResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
    
        public required string AccountNumber { get; set; }

        [JsonProperty("CustomerId")]
        public required string CustomerId { get; set; }
        public required string CustomerAlias { get; set; }
        public required string BankName { get; set; }
        public required string BankCode { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Bvn { get; set; }
    }
}
