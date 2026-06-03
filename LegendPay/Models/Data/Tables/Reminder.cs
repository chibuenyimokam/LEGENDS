using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class Reminder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SubscriptionId { get; set; }

        [ForeignKey(nameof(SubscriptionId))]
        public Subscription Subscription { get; set; }

        [Required]
        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        public ReminderType ReminderType { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        [Required]
        public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    }
}