using System.Collections.Generic;

namespace LegendPay.Models.ViewModels
{
    public class PayBillsViewModel
    {
        // For the header / layout display
        public string UserName { get; set; }
        public string WalletId { get; set; }
        public decimal AvailableBalance { get; set; }

        // For the Pay Bills page specific content
        public List<RecentBillerViewModel> RecentFavorites { get; set; } = new();
        public List<BillCategoryViewModel> Categories { get; set; } = new();
    }

    public class RecentBillerViewModel
    {
        public string BillerName { get; set; }
        public string AccountNumber { get; set; }
        public string LastPaidLabel { get; set; }
        public string Tag { get; set; }
    }

    public class BillCategoryViewModel
    {
        public string Name { get; set; }
        public string SvgIcon { get; set; }   // raw SVG string rendered with @Html.Raw()
        public string ColorClass { get; set; }   // e.g. "cat-pink"
    }
}