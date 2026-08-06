namespace LegendPay.Models.ViewModels
{
    public class AdminAuditViewModel
    {
        public List<AuditEntry> Entries { get; set; } = new();
    }

    public class AuditEntry
    {
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Icon { get; set; } = "history";
        public DateTime Timestamp { get; set; }
    }
}
