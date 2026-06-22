namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class WalletDashboardViewModel
    {
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public decimal Balance { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // Transactions
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
    }

    public class RecentTransactionViewModel
    {
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }          // "Credit" or "Debit"
        public DateTime Date { get; set; }
        public string Status { get; set; }        // "Successful", "Pending", "Failed"
    }
}