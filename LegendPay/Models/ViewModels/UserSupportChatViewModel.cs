using LegendPay.Models.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class UserSupportChatViewModel
    {
        public Guid ChatId { get; set; }
        public string UserName { get; set; }
        public SupportChat? ActiveChat { get; set; }
        public List<SupportChat> AllChats { get; set; } = new List<SupportChat>();
        public List<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
        public string? ErrorMessage { get; set; }

        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string? NewMessage { get; set; }
    }
}