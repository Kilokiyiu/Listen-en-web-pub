using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordService.Domain.Entity;

namespace WordService.Infrastructure;

public class WordDbContextInitService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WordDbContextInitService> _logger;

    public WordDbContextInitService(IServiceProvider serviceProvider, ILogger<WordDbContextInitService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WordDbContext>();

        // 执行数据库迁移
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migration completed");

        // 检查是否已有词根数据
        if (!await context.WordRoots.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Seeding word roots data...");
            var roots = SeedData.GetWordRoots();
            foreach (var root in roots)
            {
                context.WordRoots.Add(root);
            }
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} word roots", roots.Length);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static class WordDbContextInitExtensions
{
    public static IServiceCollection AddWordDbContextInit(this IServiceCollection services)
    {
        services.AddHostedService<WordDbContextInitService>();
        return services;
    }
}
