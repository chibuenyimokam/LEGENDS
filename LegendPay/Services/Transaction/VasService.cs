using System.Net.Http.Json;
using System.Text.Json;
using LegendPay.Models.VAS;
using Microsoft.Extensions.Logging;

namespace LegendPay.Services.Transaction
{
    /// <summary>
    /// Client for CoralPay's VAS interface (biller-groups, billers, packages,
    /// customer-lookup, process-payment, payment-lookup). Registered via
    /// AddHttpClient("VasClient", ...) — see DI notes at the bottom of this file.
    /// </summary>
    public class VasService : IVasService
    {
        private readonly HttpClient _httpClient;
        private readonly VasSignatureService _signatureService;
        private readonly ILogger<VasService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public VasService(
            IHttpClientFactory httpClientFactory,
            VasSignatureService signatureService,
            ILogger<VasService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("VasClient");
            _signatureService = signatureService;
            _logger = logger;
        }

        public async Task<VasApiResponse<List<BillerGroup>>> GetBillerGroupsAsync(CancellationToken ct = default)
        {
            return await GetAsync<List<BillerGroup>>("/api/biller-groups", ct);
        }

        public async Task<VasApiResponse<List<Biller>>> GetBillersByGroupIdAsync(int billerGroupId, CancellationToken ct = default)
        {
            return await GetAsync<List<Biller>>($"/api/billers/group/{billerGroupId}", ct);
        }

        public async Task<VasApiResponse<List<Biller>>> GetBillersByGroupSlugAsync(string billerGroupSlug, CancellationToken ct = default)
        {
            return await GetAsync<List<Biller>>($"/api/billers/group/slug/{billerGroupSlug}", ct);
        }

        public async Task<VasApiResponse<List<VasPackage>>> GetPackagesByBillerIdAsync(int billerId, CancellationToken ct = default)
        {
            return await GetAsync<List<VasPackage>>($"/api/packages/biller/{billerId}", ct);
        }

        public async Task<VasApiResponse<List<VasPackage>>> GetPackagesByBillerSlugAsync(string billerSlug, CancellationToken ct = default)
        {
            return await GetAsync<List<VasPackage>>($"/api/packages/biller/slug/{billerSlug}", ct);
        }

        public async Task<VasApiResponse<CustomerLookupResponseData>> CustomerLookupAsync(
            CustomerLookupRequest request, string billerId, CancellationToken ct = default)
        {
            var signature = _signatureService.GenerateCustomerLookupSignature(
                request.CustomerId ?? string.Empty, billerId);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/transactions/customer-lookup")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Signature", signature);

            return await SendAsync<CustomerLookupResponseData>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendValueResponseData>> VendValueAsync(
            VendValueRequest request, string billerId, CancellationToken ct = default)
        {
            var amountString = request.Amount?.ToString("F2") ?? string.Empty;
            var signature = _signatureService.GenerateVendValueSignature(
                request.PaymentReference, request.CustomerId ?? string.Empty, amountString, billerId);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/transactions/process-payment")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Signature", signature);

            return await SendAsync<VendValueResponseData>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendTransactionResponseData>> GetTransactionByPaymentReferenceAsync(
            string paymentReference, CancellationToken ct = default)
        {
            var path = $"/api/transactions/payment-lookup/?paymentReference={Uri.EscapeDataString(paymentReference)}";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
            return await SendAsync<VendTransactionResponseData>(httpRequest, ct);
        }

        public async Task<VasApiResponse<VendTransactionResponseData>> GetTransactionByTransactionIdAsync(
            string transactionId, CancellationToken ct = default)
        {
            var path = $"/api/transactions/payment-lookup/?transactionId={Uri.EscapeDataString(transactionId)}";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
            return await SendAsync<VendTransactionResponseData>(httpRequest, ct);
        }

        // ---------------- helpers ----------------

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

    // ------------------------------------------------------------------
    // DI registration — add to Program.cs (replaces your old BillerOne
    // HttpClient registration):
    //
    //   builder.Services.Configure<VasSettings>(
    //       builder.Configuration.GetSection(VasSettings.SectionName));
    //
    //   builder.Services.AddSingleton<VasSignatureService>();
    //
    //   builder.Services.AddHttpClient("VasClient", (sp, client) =>
    //   {
    //       var settings = sp.GetRequiredService<IOptions<VasSettings>>().Value;
    //       client.BaseAddress = new Uri(settings.VasBaseUrl);
    //
    //       var basicAuth = Convert.ToBase64String(
    //           Encoding.UTF8.GetBytes($"{settings.Username}:{settings.Password}"));
    //       client.DefaultRequestHeaders.Authorization =
    //           new AuthenticationHeaderValue("Basic", basicAuth);
    //   });
    //
    //   builder.Services.AddScoped<IVasService, VasService>();
    //
    // appsettings.json:
    //   "Vas": {
    //     "VasBaseUrl": "https://sandbox1.coralpay.com/coralpay-vas",
    //     "Username": "vfdmfb",
    //     "Password": "<from secrets, not committed>",
    //     "InstitutionId": "<confirm with CoralPay>",
    //     "PrivateKeyPem": "<from secrets/Key Vault, not committed>"
    //   }
    // ------------------------------------------------------------------
}