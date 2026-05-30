using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class SpendingRecord
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
        public int Month { get; set; } 

        [Required]
        public int Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; }

        public int TransactionCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PredictedNextMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BudgetLimit { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}