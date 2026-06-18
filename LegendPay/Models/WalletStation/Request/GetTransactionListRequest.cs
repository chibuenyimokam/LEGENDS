namespace LegendPay.Models.WalletStation.Request
{
    public class GetTransactionListRequest
    {
        public required string CustomerId { get; set; }

        public class SearchDetails {
            public int Page { get; set; }
            public int ItemsPerPage { get; set; }
        } 
        public class DateRange
        {
            public string Start { get; set; }
            public string End { get; set; }
        }
    }
}
