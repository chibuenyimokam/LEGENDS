using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(256, ErrorMessage = "Password cannot exceed 256 characters.")]
        public string Password { get; set; }

        public string? TwoFactorCode { get; set; }
    }
}