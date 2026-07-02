//namespace LegendPay.Services.Transaction
//{
//    // Registered as a singleton so the token persists across requests.
//    // WalletService is transient (created per-request by HttpClientFactory),
//    // so the cache cannot live on WalletService itself.
//    public class BillerOneTokenCache
//    {
//        private readonly SemaphoreSlim _lock = new(1, 1);

//        private string? _token;
//        //private DateTime _expiry = DateTime.Now.AddMinutes(5);

//        private bool IsValid =>
//            _token != null;

//        /// Returns the cached token if still valid, otherwise acquires the lock,
//        /// double-checks, and calls<paramref name="fetchTokenFactory"/> exactly once
//        /// even when many concurrent requests arrive simultaneously (prevents cache stampede).
//        public async Task<string> GetOrRefreshBillerOneTokenAsync(
//            Func<Task<string>> fetchTokenFactory,
//            CancellationToken cancellationToken = default)
//        {
//            if (IsValid) return _token!;

//            await _lock.WaitAsync(cancellationToken);
//            try
//            {
//                if (IsValid) return _token!;

//                var newToken = await fetchTokenFactory();

//                _token = newToken;

//                return _token;
//            }
//            catch
//            {
//                _token = null;
//                throw;
//            }
//            finally
//            {
//                _lock.Release();
//            }
//        }
//    }
//}