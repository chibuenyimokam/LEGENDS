namespace LegendPay.Models.WalletStation.Request
{
    public class GetTransactionListRequest
    {
        public required string CustomerId { get; set; }
        public SearchDetails SearchDetails { get; set; }
    }

    public class SearchDetails
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public DateRange DateRange { get; set; } = new();
    }
    public class DateRange
    {
        public DateTime Start { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime End { get; set; } = DateTime.UtcNow;
    }
}