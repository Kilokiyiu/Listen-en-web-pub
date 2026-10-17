using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonInit;

/// <summary>
/// 每隔两分钟对数据库执行一次 SELECT 1，避免连接池在闲置过夜后被清空。
/// 第二天第一次打开首页时，接口就不用重新做一次完整的 SQL 登录。
/// </summary>
public static class SqlConnectionKeepAliveExtensions
{
    public static IServiceCollection AddSqlConnectionKeepAlive(this IServiceCollection services)
    {
        services.AddHostedService<SqlConnectionKeepAliveService>();
        return services;
    }
}

public sealed class SqlConnectionKeepAliveService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    private readonly IConfiguration configuration;
    private readonly ILogger<SqlConnectionKeepAliveService> logger;

    public SqlConnectionKeepAliveService(
        IConfiguration configuration,
        ILogger<SqlConnectionKeepAliveService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var connStr = Environment.GetEnvironmentVariable("DatabaseConnStr")
                          ?? configuration.GetConnectionString("DatabaseConnStr");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    await using var conn = new SqlConnection(connStr);
                    await conn.OpenAsync(stoppingToken);
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1";
                    await cmd.ExecuteScalarAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogDebug(ex, "SQL keepalive failed");
                }
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
