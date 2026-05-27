using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Phone number or email is required.")]
        [MaxLength(100, ErrorMessage = "Max number of characters is 100")]
        [Display(Name = "Phone number or email")]
        //[RegularExpression(@)]
        //[DataType(DataType.EmailAddress)]
        public string PhoneNumberOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MaxLength(20, ErrorMessage = "Password cannot exceed 20 characters.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
        public string Password { get; set; }
    }
}
