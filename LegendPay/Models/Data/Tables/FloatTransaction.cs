using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class FloatTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FloatAccountId { get; set; }

        [ForeignKey(nameof(FloatAccountId))]
        public FloatAccount? FloatAccount { get; set; }

        [Required]
        public FloatTransactionType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionReason TransactionReason { get; set; }

        public Guid? UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount? UserAccount { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}