using LegendPay.Interfaces.Admin;
using LegendPay.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LegendPay.Services.Admin
{
    public class AdminEmailService : IAdminEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminEmailService> _logger;

        public AdminEmailService(IConfiguration config, HttpClient httpClient, ILogger<AdminEmailService> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ServiceResponse<string>> SendTwoFactorCodeAsync(string toEmail, string code)
        {
            try
            {
                var apiKey = _config["Brevo:ApiKey"];
                var fromEmail = _config["Brevo:FromEmail"];
                var fromName = _config["Brevo:FromName"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("Brevo settings are missing in configuration.");
                    return ServiceResponse<string>.FailureResponse("Email configuration error.");
                }

                var payload = new
                {
                    sender = new { name = fromName, email = fromEmail },
                    to = new[] { new { email = toEmail } },
                    subject = "LegendPay Admin - 2FA Code",
                    htmlContent = $@"
                        <h2>Admin Login Verification</h2>
                        <p>Your 2FA code is:</p>
                        <h1 style='color:#001B44;'>{code}</h1>
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
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Brevo dispatch failed with status {Status}: {Body}", response.StatusCode, errorBody);
                    return ServiceResponse<string>.FailureResponse($"Failed to send 2FA email ({response.StatusCode}).");
                }

                return ServiceResponse<string>.SuccessResponse("", "2FA code sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 2FA email to {Email}", toEmail);
                return ServiceResponse<string>.FailureResponse($"Failed to send email: {ex.Message}");
            }
        }
    }
}