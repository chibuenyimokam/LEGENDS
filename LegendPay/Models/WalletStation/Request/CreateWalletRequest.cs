using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.WalletStation.Request
{
    public class CreateWalletRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string CustomerAlias { get; set; }
        public string? BVN { get; set; }
        public string? Otp { get; set; }

    }
}
