using LegendPay.Interfaces.Transaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LegendPay.Services.Background
{
    // it only executes payments while the app is running(i think this is how it's done)
    public class ScheduledPaymentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledPaymentWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

        public ScheduledPaymentWorker(IServiceScopeFactory scopeFactory, ILogger<ScheduledPaymentWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled payment worker started, polling every {Interval}.", PollInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduledPaymentService = scope.ServiceProvider.GetRequiredService<IScheduledPaymentService>();

                    var processed = await scheduledPaymentService.ProcessDuePaymentsAsync();
                    if (processed > 0)
                    {
                        _logger.LogInformation("Scheduled payment worker processed {Count} due payment(s).", processed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled payment worker failed while processing due payments.");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // expected during shutdown
                }
            }
        }
    }
}