using LegendPay.Models.Data.Tables;

namespace LegendPay.Models.ViewModels
{
    public class AdminSupportInboxViewModel
    {
        public List<SupportChat> Chats { get; set; } = new();
        public string? ActiveStatusFilter { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class AdminSupportChatDetailViewModel
    {
        public SupportChat Chat { get; set; } = null!;
        public List<SupportMessage> Messages { get; set; } = new();
        public string? ReplyText { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}