namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class ReceiptViewModel
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string BillerCategory { get; set; } = string.Empty;
        public string AccountReference { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Icon => BillerIcon.From(BillerCategory, BillerName);
        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
    }
}
