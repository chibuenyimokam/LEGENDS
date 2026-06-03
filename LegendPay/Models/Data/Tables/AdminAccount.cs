using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.Data.Tables
{
    [Index(nameof(Email), IsUnique = true)]
    public class AdminAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(256, ErrorMessage = "Password cannot exceed 256 characters.")]
        public string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Admin"; 

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorExpiration { get; set; }
    }
}