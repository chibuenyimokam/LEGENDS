using LegendPay.Interfaces.Transaction;
using LegendPay.Models.Data.Response_Table;
using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LegendPay.Services.Transaction
{
    public class WalletService : IWalletService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _username;
        private readonly string _password;
        private readonly string _walletBaseUrl;

        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public WalletService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _username = _config["CoralPay:Username"]!;
            _password = _config["CoralPay:Password"]!;
            _walletBaseUrl = _config["CoralPay:WalletBaseUrl"]!;
        }

        private async Task<string?> GetTokenAsync()
        {
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
                return _cachedToken;

            try
            {
                var url = $"{_walletBaseUrl}/api/Auth";
                var payload = new AuthenticationRequest
                {
                    Username = _username,
                    Password = _password
                };

                var response = await _httpClient.PostAsJsonAsync(url, payload);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json);

                if (result?.responseHeader?.ResponseCode != ResponseCode.Successful || result.Token == null)
                    return null;

                _cachedToken = result.Token;
                _tokenExpiry = result.ExpiryDate;
                return _cachedToken;
            }
            catch
            {
                return null;
            }
        }

        // this is a shared POST helper that attaches bearer token and deserializes
        private async Task<T?> PostAsync<T>(string endpoint, object payload) where T : class
        {
            var token = await GetTokenAsync();
            if (token == null) return null;

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_walletBaseUrl}/{endpoint}")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        
        public async Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest walletRequest)
        {
            try
            {
                var result = await PostAsync<CreateWalletResponse>("api/CreateWallet", walletRequest);

                if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                    return null;

                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<decimal?> GetBalanceAsync(string customerId)
        {
            try
            {
                var payload = new { CustomerId = customerId };
                var result = await PostAsync<GetBalanceResponse>("api/GetBalance", payload);

                if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                    return null;

                return result.Balance;
            }
            catch
            {
                return null;
            }
        }
    }
}