namespace LegendPay.Models.ViewModels
{
    public class AdminSettlementViewModel
    {
        public decimal TotalFloat { get; set; }
        public int FundedAccounts { get; set; }
        public int TotalCustomers { get; set; }
        public List<BankFloatItem> BankBreakdown { get; set; } = new();
        public List<SettlementTransactionItem> RecentTransactions { get; set; } = new();
    }

    public class BankFloatItem
    {
        public string BankName { get; set; } = string.Empty;
        public int Accounts { get; set; }
        public decimal Float { get; set; }
    }

    public class SettlementTransactionItem
    {
        public string Reference { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
