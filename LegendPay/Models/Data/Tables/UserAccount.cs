using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.Data.Tables
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class UserAccount
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
        //[DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        //[Required(ErrorMessage = "Username is required.")]
        //[MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters.")]
        //public string UserName { get; set; }

        /* [Required(ErrorMessage = "Password is required.")]
         //[DataType(DataType.Password)]
         [MaxLength(20, ErrorMessage = "Password cannot exceed 20 characters.")]
         [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
         public string Password { get; set; }
        */
        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(256, ErrorMessage = "Password cannot exceed 256 characters.")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string PhoneNumber { get; set; }

        public string? OtpCode { get; set; }
        public DateTime? OtpExpiration { get; set; }
        public bool IsEmailVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? WalletId { get; set; }      
        public string? CustomerId { get; set; }    //might not be needed
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }

        public decimal Balance { get; set; } = 0.00m;

        // Navigation Properties
        public Wallet Wallet { get; set; }
        public LegendPoint LegendPoint { get; set; }
        public ICollection<Bill>? Bills { get; set; }
        public ICollection<Receipt>? Receipts { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<Subscription>? Subscriptions { get; set; }
        public ICollection<Beneficiary>? Beneficiaries { get; set; }
        public ICollection<ScheduledPayment>? ScheduledPayments { get; set; }
        public ICollection<SupportChat>? SupportChats { get; set; }

    }
}
