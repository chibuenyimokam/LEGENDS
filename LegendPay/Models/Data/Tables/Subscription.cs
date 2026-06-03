using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required(ErrorMessage = "Biller category is required.")]
        public BillerCategory BillerCategory { get; set; }

        [Required(ErrorMessage = "Biller name is required.")]
        [MaxLength(100, ErrorMessage = "Biller name cannot exceed 100 characters.")]
        public string BillerName { get; set; }

        [Required(ErrorMessage = "Account reference is required.")]
        [MaxLength(100, ErrorMessage = "Account reference cannot exceed 100 characters.")]
        public string AccountReference { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Next due date is required.")]
        public DateTime NextDueDate { get; set; }

        [Required(ErrorMessage = "Renewal interval is required.")]
        public int RenewalIntervalDays { get; set; }

        public bool IsAutoPayEnabled { get; set; } = false;

        [Required]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Wallet;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Reminder>? Reminders { get; set; }
    }
}