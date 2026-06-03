using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(300)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}