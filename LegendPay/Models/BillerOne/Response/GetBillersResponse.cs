using LegendPay.Models.WalletStation.Response;
using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Response
{
    public class GetBillersResponse
    {
        [JsonProperty("responseHeader")]
        public ResponseHeader ResponseHeader { get; set; }

        [JsonProperty("billers")]
        public List<Biller> Billers { get; set; }
    }

    public class Biller
    {
        [JsonProperty("cartegory")]  // api typo it's "cartegory", in response payload
        public string Category { get; set; }

        [JsonProperty("biller")]
        public string BillerName { get; set; }

        [JsonProperty("billerId")]
        public string BillerId { get; set; }

        [JsonProperty("logoPath")]
        public string? LogoPath { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("amountInVerification")]
        public bool AmountInVerification { get; set; }

        [JsonProperty("referenceIdVerifable")]
        public bool ReferenceIdVerifiable { get; set; }
    }
}
