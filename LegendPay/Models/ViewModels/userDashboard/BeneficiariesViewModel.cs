namespace LegendPay.Models.ViewModels.userDashboard
{
    public class BeneficiariesViewModel
    {
        public List<BeneficiaryDisplayItem> Beneficiaries { get; set; } = new();
        public string? Search { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }

    }
     public class BeneficiaryDisplayItem
     {
        public string BenefId { get; set; } = string.Empty;
        public string BenefName { get; set; } = string.Empty;
        public string BenefRefId { get; set; } = string.Empty;
        public string Biller { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
     }
}

