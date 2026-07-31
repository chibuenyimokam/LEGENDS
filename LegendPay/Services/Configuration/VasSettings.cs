namespace LegendPay.Services.Configuration
{
    public class VasSettings
    {
        public const string SectionName = "Vas";

        public string VasBaseUrl { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        /// Required by CoralPay for X-Signature generation. Not present in the Postman collection —
        /// confirm this value with CoralPay's VAS onboarding team.
        public string InstitutionId { get; set; } = string.Empty;

        /// PEM-formatted RSA private key (including -----BEGIN PRIVATE KEY----- / -----END PRIVATE KEY-----)
        /// issued by CoralPay for signing X-Signature headers. Store this in User Secrets / Key Vault,
        /// never commit it (you already learned this lesson with the SendGrid key).
        public string PrivateKeyPem { get; set; } = string.Empty;
    }
}