using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter the code sent to your email.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be 6 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "The code must contain numbers only.")]
        public string OtpCode { get; set; }

        [Required(ErrorMessage = "Please enter a new password.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
