namespace LegendPay.Models.ViewModels
{
    public class AdminTransactionsViewModel
    {
        public decimal TodayVolume { get; set; }
        public double SuccessRate { get; set; }

        public List<AdminTxnRegistryRow> Transactions { get; set; } = new();
        public List<string> Billers { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int ShownCount => Transactions.Count;

        public string? Status { get; set; }
        public string? Biller { get; set; }
        public string? Method { get; set; }
    }

    public class AdminTxnRegistryRow
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Reference => "TX-" + Id.ToString("N")[..8].ToUpperInvariant();
        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }
}
