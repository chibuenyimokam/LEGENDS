namespace LegendPay.Models.WalletStation.Response
{
    public class CreditResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public decimal? Amount { get; set; }
        public required decimal Balance { get; set; }
        public string? Description { get; set; }
        public required string TransactionId { get; set; }
        public string? TraceId { get; set; }
    }
}
