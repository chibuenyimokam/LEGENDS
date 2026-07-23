using LegendPay.Models.WalletStation.Response;
using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Response
{
    public class GetBillerPackagesResponse
    {
        [JsonProperty("responseHeader")]
        public ResponseHeader ResponseHeader { get; set; }

        [JsonProperty("packages")]
        public List<BillerPackage> Packages { get; set; }
    }

    public class BillerPackage
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("billerItemName")]
        public string BillerItemName { get; set; }

        [JsonProperty("billerItemId")]
        public string BillerItemId { get; set; }
    }
}