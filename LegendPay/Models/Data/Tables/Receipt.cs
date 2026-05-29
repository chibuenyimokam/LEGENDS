using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Receipt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill Bill { get; set; }

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReceiptNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string BillerName { get; set; }

        [MaxLength(200)]
        public string? ConfirmationToken { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}