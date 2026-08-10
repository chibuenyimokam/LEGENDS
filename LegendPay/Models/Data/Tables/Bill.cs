using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{

    // this is for admin portal
    public class Bill
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        [MaxLength(50)]
        public string BillerCategory { get; set; }

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
        [MaxLength(20)]
        public string PaymentMethod { get; set; } // "Wallet" or "Verge"

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // "Pending", "Success", "Failed"

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