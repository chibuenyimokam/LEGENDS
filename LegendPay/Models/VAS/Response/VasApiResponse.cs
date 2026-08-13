using System.Text.Json.Serialization;

namespace LegendPay.Models.Vas
{
    // Generic envelope every CoralPay VAS endpoint responds with.
    public class VasApiResponse<T>
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("responseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("responseData")]
        public T? ResponseData { get; set; }

        public bool IsSuccess => !Error && ResponseCode == "00";
    }
}