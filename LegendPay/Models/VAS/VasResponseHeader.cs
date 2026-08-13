using Newtonsoft.Json;

namespace LegendPay.Models.VAS
{
    public class VasResponseHeader
    {
        [JsonProperty("error")]

        public bool Error { get; set; }

        [JsonProperty("status")]

        public string Status { get; set; }

        [JsonProperty("message")]

        public string Message { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }
    }
}
