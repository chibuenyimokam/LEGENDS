using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
