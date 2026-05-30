using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class ScheduledPayment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required(ErrorMessage = "Biller category is required.")]
        [MaxLength(50, ErrorMessage = "Biller category cannot exceed 50 characters.")]
        public string BillerCategory { get; set; }

        [Required(ErrorMessage = "Biller name is required.")]
        [MaxLength(100, ErrorMessage = "Biller name cannot exceed 100 characters.")]
        public string BillerName { get; set; }

        [Required(ErrorMessage = "Account reference is required.")]
        [MaxLength(100, ErrorMessage = "Account reference cannot exceed 100 characters.")]
        public string AccountReference { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Scheduled date is required.")]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Wallet";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Processed", "Cancelled", "Failed"

        public Guid? BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}