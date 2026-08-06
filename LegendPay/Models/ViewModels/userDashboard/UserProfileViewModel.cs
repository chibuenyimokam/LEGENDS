namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? CustomerId { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime MemberSince { get; set; }
    }
}
