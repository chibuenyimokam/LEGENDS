namespace LegendPay.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int TransactionsToday { get; set; }
        public decimal TotalValueToday { get; set; }
        public int FailedToday { get; set; }

        public List<AdminTransactionRow> RecentTransactions { get; set; } = new();
    }

    public class AdminTransactionRow
    {
        public string Reference { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }
}
