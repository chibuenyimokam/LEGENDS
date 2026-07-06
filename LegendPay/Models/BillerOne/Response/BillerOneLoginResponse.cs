using LegendPay.Models.WalletStation.Response;

namespace LegendPay.Models.BillerOne.Response
{
    public class BillerOneLoginResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public string? Token { get; set; }
    }
}
