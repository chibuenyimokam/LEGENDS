using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.WalletStation.Request
{
    public class AuthenticationRequest
    {
        
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
