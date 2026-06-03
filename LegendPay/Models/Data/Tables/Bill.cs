using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Bill
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        public BillerCategory BillerCategory { get; set; }

        [Required]
        [MaxLength(100)]
        public string BillerName { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountReference { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public BillStatus Status { get; set; } = BillStatus.Pending;

        [MaxLength(200)]
        public string? ConfirmationToken { get; set; }

        [MaxLength(200)]
        public string? VergeRefrence { get; set; }

        [MaxLength(200)]
        public string? BilleroneRefrence { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Receipt? Receipt { get; set; }
        public LegendPointTransaction? LegendPointTransaction { get; set; }
    }
}