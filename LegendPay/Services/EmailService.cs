using SendGrid;
using SendGrid.Helpers.Mail;

namespace LegendPay.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var apiKey = _config["SendGrid:ApiKey"];
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
            await client.SendEmailAsync(msg);
        }
    }
}
