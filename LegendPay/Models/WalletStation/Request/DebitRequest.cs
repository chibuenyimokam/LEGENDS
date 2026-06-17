namespace LegendPay.Models.WalletStation.Request
{
    public class DebitRequest
    {
        public required decimal Amount { get; set; }
        public required string CustomerId { get; set; }
        public required string Description { get; set; }
        public required string TraceId { get; set; }
    }
}
