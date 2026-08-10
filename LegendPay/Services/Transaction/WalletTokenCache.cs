namespace LegendPay.Services.Transaction
{
    
    public class WalletTokenCache
    {
        private readonly SemaphoreSlim _lock = new(1, 1);

        private string? _token;
        private DateTime _expiry = DateTime.MinValue;

        private bool IsValid =>
            _token != null && DateTime.Now < _expiry.AddDays(-1);

        
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