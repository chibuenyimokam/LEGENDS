using LegendPay.Models.WalletStation.Response;
using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Response
{
    public class GetCategoriesResponse
    {
        [JsonProperty("responseHeader")]
        public ResponseHeader ResponseHeader { get; set; }

        [JsonProperty("categoryList")]
        public List<BillerCategory> CategoryList   { get; set; }
    }

    public class BillerCategory
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("logoUrl")]
        public string? LogoUrl { get; set; }
    }
}
