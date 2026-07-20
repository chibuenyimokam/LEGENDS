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
        [MaxLength(50)]
        public string Type { get; set; } // "WalletFunded", "BillPaid", "Reminder", "AutoPay", "CashbackCredited", "LowBalance"

        [Required]
        [MaxLength(300)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public Guid? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}