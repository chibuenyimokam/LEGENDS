namespace LegendPay.Services.Transaction
{
    // Registered as a singleton so the token persists across requests.
    // WalletService is transient (created per-request by HttpClientFactory),
    // so the cache cannot live on WalletService itself.
    public class WalletTokenCache
    {
        private readonly SemaphoreSlim _lock = new(1, 1);

        public string? Token { get; private set; }
        public DateTime Expiry { get; private set; } = DateTime.MinValue;

        public bool IsValid =>
            Token != null && DateTime.UtcNow < Expiry.AddMinutes(-5);

        public async Task SetAsync(string token, DateTime expiry)
        {
            await _lock.WaitAsync();
            try
            {
                Token = token;
                Expiry = expiry;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<string?> GetAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return IsValid ? Token : null;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}