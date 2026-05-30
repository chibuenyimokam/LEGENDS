using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class WalletTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WalletId { get; set; }

        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Type { get; set; } // "Credit" or "Debit"

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } // "Pending", "Success", "Failed"

        [MaxLength(50)]
        public string? Source { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? ExternalReference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}