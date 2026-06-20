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
        private readonly WalletTokenCache _tokenCache;
        private readonly string _username;
        private readonly string _password;

        public WalletService(HttpClient httpClient, IConfiguration config, WalletTokenCache tokenCache)
        {
            _httpClient = httpClient;
            _config = config;
            _tokenCache = tokenCache;
            _username = _config["WalletStation:Username"]!;
            _password = _config["WalletStation:Password"]!;
        }

        private async Task<string?> GetTokenAsync()
        {
            var cachedToken = await _tokenCache.GetAsync();
            if (cachedToken != null) return cachedToken;

            var payload = new AuthenticationRequest
            {
                Username = _username,
                Password = _password
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth", payload);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful || result.Token == null)
                return null;

            await _tokenCache.SetAsync(result.Token, result.ExpiryDate);
            return result.Token;
        }

        private async Task<T?> PostAsync<T>(string endpoint, object payload) where T : class
        {
            var token = await GetTokenAsync();
            if (token == null)
            {
                throw new InvalidOperationException("Failed to acquire authentication token from the upstream Wallet Engine provider.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Upstream API wallet call to '{endpoint}' failed with status code {response.StatusCode}. Details: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RAW WALLET RESPONSE: {json}");
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest walletRequest)
        {
            
            var result = await PostAsync<CreateWalletResponse>("api/CreateAccount", walletRequest);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result;
        }

        public async Task<decimal?> GetBalanceAsync(string customerId)
        {
            var payload = new { CustomerId = customerId };
            var result = await PostAsync<GetBalanceResponse>("api/GetBalance", payload);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result.Balance;
        }
    }
}