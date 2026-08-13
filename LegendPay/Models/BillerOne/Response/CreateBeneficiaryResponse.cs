using Newtonsoft.Json;

namespace LegendPay.Models.BillerOne.Response
{
    public class CreateBeneficiaryResponse
    {
        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; } = string.Empty;

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; } = string.Empty;
    }
}
