using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Request
{
    public class CreateBeneficiaryRequest
    {
        [JsonProperty("benefName")]
        public string BenefName { get; set; } = string.Empty;

        [JsonProperty("benefRefId")]
        public string BenefRefId { get; set; } = string.Empty;

        [JsonProperty("biller")]
        public string Biller { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;
    }
}
