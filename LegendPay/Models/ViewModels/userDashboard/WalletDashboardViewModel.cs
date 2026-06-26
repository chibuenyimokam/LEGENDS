//namespace LegendPay.Models.ViewModels.UserDashboard
//{
//    public class WalletDashboardViewModel
//    {
//        public string CustomerId { get; set; }
//        public string AccountNumber { get; set; }
//        public string BankName { get; set; }
//        public decimal Balance { get; set; }

//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        public string FullName => $"{FirstName} {LastName}";

//        // Transactions
//        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
//    }

//    public class RecentTransactionViewModel
//    {
//        public string TransactionId { get; set; }
//        public string Description { get; set; }
//        public decimal Amount { get; set; }
//        public string Type { get; set; }          // "Credit" or "Debit"
//        public DateTime Date { get; set; }
//        public string Status { get; set; }        // "Successful", "Pending", "Failed"
//    }
//}


namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class WalletDashboardViewModel
    {
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public decimal WalletBalance { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public string KycTier { get; set; } = "Tier 2 Verified";
        public string WeeklyChangeLabel { get; set; } = "+12.5% this week";

        // Formatted account number  "803 123 4567"
        public string AccountNumberFormatted =>
            string.IsNullOrWhiteSpace(AccountNumber)
                ? AccountNumber
                : System.Text.RegularExpressions.Regex.Replace(AccountNumber, @"(\d{3})(\d{3})(\d{4})", "$1 $2 $3");

        // Transactions
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
    }

    public class RecentTransactionViewModel
    {
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }   // "Credit" or "Debit"
        public DateTime Date { get; set; }
        public string Status { get; set; }  // "Successful", "Pending", "Failed"

        public bool IsCredit => string.Equals(Type, "Credit", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }
}