using LegendPay.Interfaces.Transaction;
using LegendPay.Models.BillerOne.Response;
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

        public BillerOneService(HttpClient httpClient, IConfiguration configuration, ILogger<BillerOneService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiToken = configuration["BillerOne:ApiToken"];
            _billerUrl = configuration["BillerOne:BillerOneBaseUrl"];
        }

        public async Task<GetCategoriesResponse?> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_billerUrl}/api/GetBillerCategories")
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(new { }),
                    Encoding.UTF8,
                    "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Biller API call to '{_billerUrl}/api/GetBillerCategories' failed ({response.StatusCode}): {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("GetCategoriesAsync response: {Response}", json);
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
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_billerUrl}/api/GetAllBillers")
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(new { }),
                    Encoding.UTF8,
                    "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Biller API call to '{_billerUrl}/api/GetBillers' failed ({response.StatusCode}): {errorContent}");
                }
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("BillerOne GetBillers response: {Response}", json);
                return JsonConvert.DeserializeObject<GetBillersResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Biller list");
                return null;
            }
        }
    }
}
