namespace LegendPay.Models.WalletStation.Request
{
    public class DebitReversalRequest
    {
        public required string CustomerId { get; set; }
        public required string Description { get; set; }
        public required string TransactionId { get; set; }

    }
}
