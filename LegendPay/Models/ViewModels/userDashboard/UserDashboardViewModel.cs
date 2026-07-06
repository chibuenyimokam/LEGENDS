namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class UserDashboardViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string AccountType { get; set; } = "Personal Account";

        public decimal Balance { get; set; }
        public int LegendPoints { get; set; }

        public int PendingBillsCount { get; set; }
        public decimal PendingBillsTotal { get; set; }

        public decimal TotalSpending { get; set; }

        public List<DashboardBillViewModel> UpcomingBills { get; set; } = new();
        public List<SpendingSliceViewModel> SpendingBreakdown { get; set; } = new();
        public List<RenewalViewModel> UpcomingRenewals { get; set; } = new();

        public string Initials
        {
            get
            {
                var first = string.IsNullOrWhiteSpace(FirstName) ? string.Empty : char.ToUpperInvariant(FirstName[0]).ToString();
                var last = string.IsNullOrWhiteSpace(LastName) ? string.Empty : char.ToUpperInvariant(LastName[0]).ToString();
                var combined = first + last;
                return string.IsNullOrEmpty(combined) ? "?" : combined;
            }
        }
    }

    public class DashboardBillViewModel
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Badge => BillerBadge.From(BillerName);
    }

    public class SpendingSliceViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class RenewalViewModel
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public DateTime NextDueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsAutoPayEnabled { get; set; }
        public string Badge => BillerBadge.From(BillerName);
    }

    public static class BillerBadge
    {
        public static string From(string billerName)
        {
            if (string.IsNullOrWhiteSpace(billerName)) return "?";

            var parts = billerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
            }

            var single = parts[0];
            return single.Length >= 2
                ? single[..2].ToUpperInvariant()
                : single.ToUpperInvariant();
        }
    }
}
