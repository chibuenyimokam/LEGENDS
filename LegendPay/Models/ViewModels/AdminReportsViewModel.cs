namespace LegendPay.Models.ViewModels
{
    public class AdminReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AvgTransactionValue { get; set; }
        public double SuccessRate { get; set; }

        public List<BillerPerformanceRow> TopBillers { get; set; } = new();

        public decimal TopRevenue => TopBillers.Count > 0 ? TopBillers.Max(b => b.Revenue) : 0m;
    }

    public class BillerPerformanceRow
    {
        public string Name { get; set; } = string.Empty;
        public int Volume { get; set; }
        public decimal Revenue { get; set; }
        public double SuccessRate { get; set; }
    }
}
