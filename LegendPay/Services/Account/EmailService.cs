using LegendPay.Interfaces.Auth;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LegendPay.Services.Account
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, HttpClient httpClient, ILogger<EmailService> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var apiKey = _config["Brevo:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Brevo API key is not configured.");
                throw new InvalidOperationException("Brevo API key is not configured");
            }

            var payload = new
            {
                sender = new
                {
                    name = _config["Brevo:FromName"],
                    email = _config["Brevo:FromEmail"]
                },
                to = new[] { new { email = toEmail } },
                subject = "LegendPay - Verify your Email",
                htmlContent = $@"
                    <h2>Verify your email</h2>
                    <p>Your OTP code is: </p>
                    <h1 style='color:blue;'>{otp}</h1>
                    <p>This code will expire in 10 minutes.
                    If you did not request this, please ignore this email.</p>"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send OTP email to {Email}. Status: {StatusCode}. Response: {ResponseBody}", toEmail, response.StatusCode, body);
                throw new InvalidOperationException($"Failed to send OTP email. Status: {response.StatusCode}. Response: {body}");
            }
        }
    }
}