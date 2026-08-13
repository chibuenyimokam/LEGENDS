namespace LegendPay.Configuration
{
    public class VasSettings
    {
        public const string SectionName = "Vas";

        public string VasBaseUrl { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

}