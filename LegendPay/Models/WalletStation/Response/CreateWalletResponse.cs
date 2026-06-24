using Newtonsoft.Json;

namespace LegendPay.Models.WalletStation.Response
{
    public class CreateWalletResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public AccountDetails AccountDetails { get; set; }
    }
    public class AccountDetails
    {
        public required string AccountNumber { get; set; }
        public required string BankCode { get; set; }
        public required string BankName { get; set; }

        [JsonProperty("CustomerId")]
        public required string CustomerId { get; set; }
        public required string CustomerAlias { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Bvn { get; set; }
    }
}
