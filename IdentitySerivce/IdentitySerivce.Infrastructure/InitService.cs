using IdentitySerivce.Domain;
using IdentitySerivce.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentitySerivce.Infrastructure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<IdentityDomainService>();
        services.AddScoped<IIdentityRepo, IdentityRepo>();
        services.AddScoped<IEmailSender, MockEmailSender>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddHostedService<AnalyticsAggregationHostedService>();
        return services;
    }
}