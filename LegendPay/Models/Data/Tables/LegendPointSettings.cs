using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    // Only ONE record in this table - admin configures it from the admin portal
    public class LegendPointSettings
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Points earned for bills below ₦1,000 (default 10%)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal EarnRateBelowThousand { get; set; } = 0.10m;

        // Points earned for bills above ₦1,000 (default 5%)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal EarnRateAboveThousand { get; set; } = 0.05m;

        // Cashback rate on redemption (default 5%)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal RedemptionRate { get; set; } = 0.05m;

        // Minimum points required before a user can redeem (default 10,000)
        [Required]
        public int MinimumRedemptionPoints { get; set; } = 10000;

        public Guid? LastUpdatedByAdminId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}