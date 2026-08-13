using LegendPay.Models.BillerOne.Response;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models.VAS.Response;
using LegendPay.Models.VAS.Request;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using LegendPay.Models.Vas;

namespace LegendPay.Services.Vas
{
  
    public class VasService : IVasService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VasService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public VasService(
            IHttpClientFactory httpClientFactory,
            ILogger<VasService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("VasClient");
            _logger = logger;
        }

        public async Task<VasApiResponse<List<BillerGroupEnquiry>>> GetBillerGroupsAsync(CancellationToken ct = default)
        {
            return await GetAsync<List<BillerGroupEnquiry>>("api/biller-groups", ct);
        }

        public async Task<VasApiResponse<List<Biller>>> GetBillersByGroupIdAsync(int billerGroupId, CancellationToken ct = default)
        {
            return await GetAsync<List<Biller>>($"api/billers/group/{billerGroupId}", ct);
        }

        public async Task<VasApiResponse<List<BillerGroupSlugEnquiryResponse>>> GetBillersByGroupSlugAsync(string billerGroupSlug, CancellationToken ct = default)
        {
            return await GetAsync<List<BillerGroupSlugEnquiryResponse>>($"api/billers/group/slug/{billerGroupSlug}", ct);
        }

        public async Task<VasApiResponse<List<PackagesEnquiryResponse>>> GetPackagesByBillerIdAsync(int billerId, CancellationToken ct = default)
        {
            return await GetAsync<List<PackagesEnquiryResponse>>($"api/packages/biller/{billerId}", ct);
        }

        public async Task<VasApiResponse<List<PackagesEnquirySlugResponse>>> GetPackagesByBillerSlugAsync(string billerSlug, CancellationToken ct = default)
        {
            return await GetAsync<List<PackagesEnquirySlugResponse>>($"api/packages/biller/slug/{billerSlug}", ct);
        }

        public async Task<VasApiResponse<CustomerEnquiryResponse>> CustomerLookupAsync(
            CustomerEnquiryRequest request, CancellationToken ct = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/transactions/customer-lookup")
            {
                Content = JsonContent.Create(request)
            };

            return await SendAsync<CustomerEnquiryResponse>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendValueResponse>> VendValueAsync(
            VendValueRequest request, CancellationToken ct = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/transactions/process-payment")
            {
                Content = JsonContent.Create(request)
            };

            return await SendAsync<VendValueResponse>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendTransactionEnquiryResponse>> GetTransactionByPaymentReferenceAsync(
            string paymentReference, CancellationToken ct = default)
        {
            var path = $"api/transactions/payment-lookup/?paymentReference={Uri.EscapeDataString(paymentReference)}";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
            return await SendAsync<VendTransactionEnquiryResponse>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendTransactionEnquiryResponse>> GetTransactionByTransactionIdAsync(
            string transactionId, CancellationToken ct = default)
        {
            var path = $"api/transactions/payment-lookup/?transactionId={Uri.EscapeDataString(transactionId)}";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
            return await SendAsync<VendTransactionEnquiryResponse>(httpRequest, ct);
        }

        // helpers

        private async Task<VasApiResponse<T>> GetAsync<T>(string path, CancellationToken ct)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            return await SendAsync<T>(httpRequest, ct);
        }

        private async Task<VasApiResponse<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "VAS request to {Path} failed with {StatusCode}: {Body}",
                        request.RequestUri, response.StatusCode, body);
                }

                var parsed = JsonSerializer.Deserialize<VasApiResponse<T>>(body, JsonOptions);

                return parsed ?? new VasApiResponse<T>
                {
                    Error = true,
                    Message = "Empty or unparseable response from VAS interface",
                    ResponseCode = "96"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling VAS endpoint {Path}", request.RequestUri);
                return new VasApiResponse<T>
                {
                    Error = true,
                    Message = "System malfunction while contacting VAS interface",
                    ResponseCode = "96"
                };
            }
        }
    }

    
}