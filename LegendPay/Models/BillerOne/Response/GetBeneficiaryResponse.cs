using LegendPay.Models.WalletStation.Response;
using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Response
{
    public class BeneficiaryItem
    {
        [JsonProperty("benefId")]
        public string BenefId { get; set; } = string.Empty;

        [JsonProperty("benefName")]
        public string BenefName { get; set; } = string.Empty;

        [JsonProperty("benefRefId")]
        public string BenefRefId { get; set; } = string.Empty;

        [JsonProperty("biller")]
        public string Biller { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;
    }

    public class GetBeneficiaryResponse
    {
        [JsonProperty("beneficiaryList")]
        public List<BeneficiaryItem>? BeneficiaryList { get; set; }
        public ResponseHeader? ResponseHeader { get; set; }

    }
}
