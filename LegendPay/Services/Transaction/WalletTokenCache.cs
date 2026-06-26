namespace LegendPay.Services.Transaction
{
    // Registered as a singleton so the token persists across requests.
    // WalletService is transient (created per-request by HttpClientFactory),
    // so the cache cannot live on WalletService itself.
    public class WalletTokenCache
    {
        private readonly SemaphoreSlim _lock = new(1, 1);

        private string? _token;
        private DateTime _expiry = DateTime.MinValue;

        private bool IsValid =>
            _token != null && DateTime.Now < _expiry.AddDays(-1);

        /// Returns the cached token if still valid, otherwise acquires the lock,
        /// double-checks, and calls <paramref name="fetchTokenFactory"/> exactly once
        /// even when many concurrent requests arrive simultaneously (prevents cache stampede).
        public async Task<string> GetOrRefreshAsync(
            Func<Task<(string Token, DateTime Expiry)>> fetchTokenFactory,
            CancellationToken cancellationToken = default)
        {
            if (IsValid) return _token!;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (IsValid) return _token!;

                var (newToken, newExpiry) = await fetchTokenFactory();

                _token = newToken;
                _expiry = newExpiry.Kind == DateTimeKind.Utc
                    ? newExpiry
                    : newExpiry.ToUniversalTime();

                return _token;
            }
            catch
            {
                _token = null;
                _expiry = DateTime.MinValue;
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}