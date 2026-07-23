using LegendPay.Interfaces.Transaction;
using LegendPay.Models.BillerOne.Request;
using LegendPay.Models.BillerOne.Response;
using LegendPay.Models.Data.Response_Table;
using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace LegendPay.Services.Transaction
{
    public class BillerOneService : IBillerOneService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BillerOneService> _logger;
        private readonly string _apiToken;
        private readonly string _billerUrl;
        private readonly string _username;
        private readonly string _password;
        //private readonly BillerOneTokenCache _tokenCache;


        public BillerOneService(HttpClient httpClient, IConfiguration configuration, ILogger<BillerOneService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            //_apiToken = configuration["BillerOne:ApiToken"];
            //_tokenCache = tokenCache;
            _billerUrl = configuration["BillerOne:BillerOneBaseUrl"];
            _username = configuration["BillerOne:username"];
            _password = configuration["BillerOne:password"];
        }

        private async Task<string> GetBillerApiTokenAsync()
        {
            var payload = new BillerOneLoginRequest
            {
                Username = _username,
                Password = _password
            };
            var response = await _httpClient.PostAsJsonAsync($"{_billerUrl}/api/guest/Login", payload);
             if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"BillerOne token fetch failed ({response.StatusCode}): {error}");
            }
             var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BillerOneLoginResponse>(json);

            if (result?.ResponseHeader?.ResponseCode != ResponseCode.Successful || result.Token == null)
                throw new InvalidOperationException(
                    $"BillerOne Login credentials invalid. Code: {result?.ResponseHeader?.ResponseCode}");
            return (result.Token);
        }

        private async Task<T?> PostAsync<T>(string endpoint, object payload, CancellationToken cancellationToken = default)
            where T : class
        {
            var token = await GetBillerApiTokenAsync();
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
                    $"BillerOne Login API call to '{endpoint}' failed ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<T>(json);
        }
        public async Task<GetCategoriesResponse?> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await GetBillerApiTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_billerUrl}/api/GetBillerCategories")
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(new { }),
                    Encoding.UTF8,
                    "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Biller API call to '{_billerUrl}/api/GetBillerCategories' failed ({response.StatusCode}): {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                //_logger.LogInformation("GetCategoriesAsync response: {Response}", json);
                return JsonConvert.DeserializeObject<GetCategoriesResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Biller categories");
                return null;
            }
        }

        public async Task<GetBillersResponse?> GetBillersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await GetBillerApiTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_billerUrl}/api/GetAllBillers")
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(new { }),
                    Encoding.UTF8,
                    "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Biller API call to '{_billerUrl}/api/GetBillers' failed ({response.StatusCode}): {errorContent}");
                }
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                //_logger.LogInformation("BillerOne GetBillers response: {Response}", json);
                return JsonConvert.DeserializeObject<GetBillersResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Biller list");
                return null;
            }
        }

        public async Task<GetBillerPackagesResponse?> GetBillerPackagesAsync(string billerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await GetBillerApiTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_billerUrl}/api/GetBillerPackages?Billerid={billerId}")
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(new { }),
                    Encoding.UTF8,
                    "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Biller API call to '{_billerUrl}/api/GetBillerPackages' failed ({response.StatusCode}): {errorContent}");
                }
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                //_logger.LogInformation("BillerOne GetBillerPackages response: {Response}", json);
                return JsonConvert.DeserializeObject<GetBillerPackagesResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Biller packages for {BillerId}", billerId);
                return null;
            }
        }
    }
}
