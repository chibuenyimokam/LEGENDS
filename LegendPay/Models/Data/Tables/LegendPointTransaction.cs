using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class LegendPointTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegendPointId { get; set; }

        [ForeignKey(nameof(LegendPointId))]
        public LegendPoint LegendPoint { get; set; }

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        public Guid? BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required]
        public LegendPointType Type { get; set; }

        [Required]
        public int Points { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BillAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CashbackValue { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}