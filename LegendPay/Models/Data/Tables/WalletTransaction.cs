using LegendPay.Models.Enums;
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
        public Wallet? Wallet { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public WalletTransactionType Type { get; set; }

        [Required]
        public WalletTransactionStatus Status { get; set; }

        public WalletTransactionSource? Source { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? ExternalReference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}