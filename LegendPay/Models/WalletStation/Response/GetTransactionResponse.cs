namespace LegendPay.Models.WalletStation.Response
{
    public class GetTransactionResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public required string TransactionDetails { get; set; }
        public required string TransactionType { get; set; }
        public required decimal Amount { get; set; }
        public required string Description { get; set; }
        public required string TransactionId { get; set; }
        public required string SessionId { get; set; }
        public required string BankCode { get; set; }
        public required string BankName { get; set; }
    }
}
