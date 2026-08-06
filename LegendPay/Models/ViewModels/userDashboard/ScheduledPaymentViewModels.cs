using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class ScheduledPaymentsViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public decimal AvailableBalance { get; set; }
        public List<ScheduledPaymentItem> Payments { get; set; } = new();
        public CreateScheduledPaymentViewModel Form { get; set; } = new();
        public List<BillerCategoryViewModel> Categories { get; set; } = new();
    }

    public class ScheduledPaymentItem
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string BillerCategory { get; set; } = string.Empty;
        public string AccountReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateScheduledPaymentViewModel
    {
        [Required(ErrorMessage = "Select a biller category.")]
        [MaxLength(50)]
        public string BillerCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the biller name.")]
        [MaxLength(100)]
        public string BillerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the account, meter or phone number.")]
        [MaxLength(100)]
        public string AccountReference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter an amount.")]
        [Range(50, 1000000, ErrorMessage = "Amount must be between ₦50 and ₦1,000,000.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Choose a date.")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }
    }
}
