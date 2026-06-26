using LegendPay.Interfaces.Admin;
using LegendPay.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LegendPay.Services.Admin
{
    public class AdminEmailService : IAdminEmailService
    {
        private readonly IConfiguration _config;

        public AdminEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ServiceResponse<string>> SendTwoFactorCodeAsync(string toEmail, string code)
        {
            try
            {
                var apiKey = _config["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(
                        _config["SendGrid:FromEmail"],
                        _config["SendGrid:FromName"]),
                    Subject = "LegendPay Admin - 2FA Code",
                    HtmlContent = $@"
                        <h2>Admin Login Verification</h2>
                        <p>Your 2FA code is:</p>
                        <h1 style='color:#001B44;'>{code}</h1>
                        <p>This code will expire in 10 minutes.
                        If you did not request this, please ignore this email.</p>"
                };
                msg.AddTo(new EmailAddress(toEmail));
                await client.SendEmailAsync(msg);

                return ServiceResponse<string>.SuccessResponse("", "2FA code sent successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"Failed to send email: {ex.Message}");
            }
        }
    }
}