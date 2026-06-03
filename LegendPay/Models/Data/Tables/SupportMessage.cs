using LegendPay.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegendPay.Models.Data.Tables
{
    public class SupportMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SupportChatId { get; set; }

        [ForeignKey(nameof(SupportChatId))]
        public SupportChat? SupportChat { get; set; }

        [Required]
        public SenderType? Sender { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        public string? MessageText { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}