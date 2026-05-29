using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    // Only ONE float account for the entire platform
    // Funded manually by admin, used to pay out cashback to users
    public class FloatAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FloatTransaction>? Transactions { get; set; }
    }
}