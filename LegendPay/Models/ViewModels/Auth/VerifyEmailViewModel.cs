using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter OTP code.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be 6 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "The OTP must contain numbers only.")]
        public string OtpCode { get; set; }
    }

}
