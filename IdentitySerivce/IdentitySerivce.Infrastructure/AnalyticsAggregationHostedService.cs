using IdentitySerivce.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentitySerivce.Infrastructure;

public sealed class AnalyticsAggregationHostedService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(6);
    private const int RetentionDays = 90;

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AnalyticsAggregationHostedService> logger;

    public AnalyticsAggregationHostedService(
        IServiceProvider serviceProvider,
        ILogger<AnalyticsAggregationHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                await analytics.AggregateYesterdayAsync(stoppingToken);
                await analytics.PurgeOldEventsAsync(RetentionDays, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Analytics aggregation failed");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }
}
