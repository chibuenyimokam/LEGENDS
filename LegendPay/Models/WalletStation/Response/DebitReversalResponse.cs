namespace LegendPay.Models.WalletStation.Response
{
    public class DebitReversalResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public required string Balance { get; set; } = string.Empty;
    }
}
