using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class SupportChat
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        public Guid? BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [MaxLength(100, ErrorMessage = "Subject cannot exceed 100 characters.")]
        public string Subject { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // "Open", "InProgress", "Resolved", "Closed"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SupportMessage>? Messages { get; set; }
    }
}