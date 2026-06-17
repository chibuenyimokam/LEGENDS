namespace LegendPay.Models.WalletStation.Response
{
    public class GetBalanceResponse
    {
        public ResponseHeader? ResponseHeader { get; set; }
        public decimal Balance { get; set; }
    }
}
