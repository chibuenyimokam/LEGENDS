using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class AdminLegendPointsViewModel
    {
        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100.")]
        [Display(Name = "Earn rate below ₦1,000")]
        public decimal EarnRateBelowThousandPct { get; set; }

        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100.")]
        [Display(Name = "Earn rate above ₦1,000")]
        public decimal EarnRateAboveThousandPct { get; set; }

        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100.")]
        [Display(Name = "Redemption cashback rate")]
        public decimal RedemptionRatePct { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Enter a positive number.")]
        [Display(Name = "Minimum redemption points")]
        public int MinimumRedemptionPoints { get; set; }

        public DateTime UpdatedAt { get; set; }

        public long TotalPointsIssued { get; set; }
        public long TotalRedeemedPoints { get; set; }
        public int ActiveUsers { get; set; }
    }
}
