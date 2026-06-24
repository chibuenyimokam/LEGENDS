namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class BillHistoryViewModel
    {
        public List<BillHistoryItemViewModel> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int ShownCount => Transactions.Count;

        public List<string> Billers { get; set; } = new();

        public string? Range { get; set; }
        public string? Biller { get; set; }
        public string? Amount { get; set; }
    }

    public class BillHistoryItemViewModel
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string BillerCategory { get; set; } = string.Empty;
        public string AccountReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Icon => BillerIcon.From(BillerCategory, BillerName);
        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }

    public static class BillerIcon
    {
        public static string From(string? category, string? name)
        {
            var c = $"{category} {name}".ToLowerInvariant();
            if (c.Contains("electric") || c.Contains("ikedc") || c.Contains("eedc") || c.Contains("power") || c.Contains("disco")) return "bolt";
            if (c.Contains("airtime") || c.Contains("mtn") || c.Contains("airtel") || c.Contains("glo") || c.Contains("9mobile") || c.Contains("mobile")) return "cell_tower";
            if (c.Contains("tv") || c.Contains("dstv") || c.Contains("gotv") || c.Contains("multichoice") || c.Contains("startimes") || c.Contains("cable")) return "tv";
            if (c.Contains("internet") || c.Contains("data") || c.Contains("wifi") || c.Contains("broadband") || c.Contains("smile") || c.Contains("spectranet")) return "wifi";
            return "receipt_long";
        }
    }
}
