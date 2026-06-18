namespace LegendPay.Models.WalletStation.Response
{
    public class DebitResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public required decimal Amount { get; set; }
        public required decimal Balance { get; set; }
        public required string Description { get; set; }
        public required string TransactionId { get; set; }
        public required string TraceId { get; set; }
    }
}
