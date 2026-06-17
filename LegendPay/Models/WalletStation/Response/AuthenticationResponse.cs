using Azure.Core;

namespace LegendPay.Models.WalletStation.Response
{
    public class AuthenticationResponse
    {
        public ResponseHeader? responseHeader { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
