using LegendPay.Interfaces.Auth;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LegendPay.Services.Account
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                { 
                _logger.LogError("SendGrid API key is not configred.");
                throw new InvalidOperationException("SendGrid API key is not configured");
                }
            if (!apiKey.StartsWith("SG."))
            {
                _logger.LogWarning("Configured SendGrid API key does not start with 'SG.'; it may be invalid or truncated. Length={Length}", apiKey.Length);
            }

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(
                    _config["SendGrid:FromEmail"],
                    _config["SendGrid:FromName"]),
                Subject = "LegendPay - Verify your Email",
                HtmlContent = $@"
                    <h2>Verify your email</h2>
                    <p>Your OTP code is: </p>
                    <h1 style='color:blue;'>{otp}</h1>
                    <p>This code will expire in 10 minutes.
                    If you did not request this, please ignore this email.</p>"
            };
            msg.AddTo(new EmailAddress(toEmail));
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send OTP email to {Email}. Status: {StatusCode}. Response: {ResponseBody}", toEmail, response.StatusCode, body);
                throw new InvalidOperationException($"Failed to send OTP email. Status: {response.StatusCode}. Response: {body}");
            }
        }
    }
}
