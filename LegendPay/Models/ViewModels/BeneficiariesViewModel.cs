namespace LegendPay.Models.ViewModels
{
    public class BeneficiariesViewModel
    {
        public List<BeneficiaryDisplayItem> Beneficiaries { get; set; } = new();

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

