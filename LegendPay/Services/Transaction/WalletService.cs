using LegendPay.Interfaces.Transaction;
using LegendPay.Models.Data.Response_Table;
using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using static LegendPay.Models.WalletStation.Request.GetTransactionListRequest;

namespace LegendPay.Services.Transaction
{
    public class WalletService : IWalletService
    {
        private readonly HttpClient _httpClient;
        private readonly WalletTokenCache _tokenCache;
        private readonly ILogger<WalletService> _logger;
        private readonly string _username;
        private readonly string _password;

        public WalletService(HttpClient httpClient, IConfiguration config, WalletTokenCache tokenCache, ILogger<WalletService> logger)
        {
            _httpClient = httpClient;
            _tokenCache = tokenCache;
            _logger = logger;
            _username = config["WalletStation:Username"]!;
            _password = config["WalletStation:Password"]!;
        }

        
        private async Task<(string Token, DateTime Expiry)> FetchTokenFromApiAsync()
        {
            var payload = new AuthenticationRequest
            {
                Username = _username,
                Password = _password
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Token fetch failed ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful || result.Token == null)
                throw new InvalidOperationException(
                    $"CoralPay authentication rejected. Code: {result?.ResponseHeader?.ResponseCode}");

            return (result.Token, result.ExpiryDate);
        }

        private async Task<T?> PostAsync<T>(string endpoint, object payload, CancellationToken cancellationToken = default)
            where T : class
        {
            var token = await _tokenCache.GetOrRefreshAsync(FetchTokenFromApiAsync, cancellationToken);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Wallet API call to '{endpoint}' failed ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest walletRequest, CancellationToken cancellationToken = default)
        {
            var result = await PostAsync<CreateWalletResponse>("api/CreateAccount", walletRequest, cancellationToken);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result;
        }

        public async Task<CreditResponse?> CreditWalletAsync(CreditRequest creditRequest, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                Amount = creditRequest.Amount,
                CustomerId = creditRequest.CustomerId,
                Description = creditRequest.Description,
                TraceId = $"TIDL{Guid.NewGuid().ToString("N")[..8]}"
            };
            var result = await PostAsync<CreditResponse>("api/Credit", payload, cancellationToken);
            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result;
        }

        public async Task<DebitResponse?> DebitWalletAsync(DebitRequest debitRequest, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                Amount = debitRequest.Amount,
                CustomerId = debitRequest.CustomerId,
                Description = debitRequest.Description,
                TraceId = $"TIDL{Guid.NewGuid().ToString("N")[..8]}"
            };
            var result = await PostAsync<DebitResponse>("api/Debit", payload, cancellationToken);
            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result;
        }
        public async Task <DebitReversalResponse?> DebitReversalAsync(DebitReversalRequest debitReversalRequest, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                CustomerId = debitReversalRequest.CustomerId,
                Description = debitReversalRequest.Description,
                TransactionId = debitReversalRequest.TransactionId
        };
            var result = await PostAsync<DebitReversalResponse>("api/DebitReversal", payload, cancellationToken);
            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;
            return result;
        }



        public async Task<decimal?> GetBalanceAsync(string customerId, CancellationToken cancellationToken = default)
        {
            var payload = new { CustomerId = customerId };
            var result = await PostAsync<GetBalanceResponse>("api/GetBalance", payload, cancellationToken);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
                return null;

            return result.Balance;
        }

        public async Task<GetTransactionListResponse?> GetTransactionHistoryAsync(string customerId, int page = 1, int itemsPerPage = 10, CancellationToken cancellationToken = default)
        {
            var requestPayload = new GetTransactionListRequest
            {
                CustomerId = customerId,
                SearchDetails = new SearchDetails
                {
                    Page = page,
                    ItemsPerPage = itemsPerPage,
                    DateRange = new DateRange
                    {
                        Start = DateTime.UtcNow.AddDays(-30),
                        End = DateTime.UtcNow
                    }
                }
            };

            var response = await PostAsync<GetTransactionListResponse>("api/GetTransactionList", requestPayload);

            //if (response?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
            //{
            //    _logger.LogError("Failed to fetch wallet history for CustomerId {CustomerId}. Status: {Status}", customerId);
            //    return null;
            //}

            return response;
        }
    }
}