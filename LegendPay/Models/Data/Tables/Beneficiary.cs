using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Beneficiary
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required(ErrorMessage = "Nickname is required.")]
        [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters.")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "Biller category is required.")]
        [MaxLength(50, ErrorMessage = "Biller category cannot exceed 50 characters.")]
        public string BillerCategory { get; set; }

        [Required(ErrorMessage = "Biller name is required.")]
        [MaxLength(100, ErrorMessage = "Biller name cannot exceed 100 characters.")]
        public string BillerName { get; set; }

        [Required(ErrorMessage = "Account reference is required.")]
        [MaxLength(100, ErrorMessage = "Account reference cannot exceed 100 characters.")]
        public string AccountReference { get; set; }

        public bool IsFavourite { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}