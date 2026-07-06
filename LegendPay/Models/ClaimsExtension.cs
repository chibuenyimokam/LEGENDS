using System.Security.Claims;

namespace LegendPay.Models
{
    public static class ClaimsExtension
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;

        }

        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        public static string? GetFirstName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.GivenName)?.Value;
        }
        public static string? GetLastName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Surname)?.Value;
        }
        public static string? GetFullName(this ClaimsPrincipal user)
        {
            var first = user.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            var last = user.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            return $"{first} {last}".Trim();
        }

        public static bool IsUser(this ClaimsPrincipal user)
        {
            return user.IsInRole("User");
        }
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

    }
}
