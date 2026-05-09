using Microsoft.Extensions.DependencyInjection;

namespace IdentitySerivce.Infrastructure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<IdentityDomainService>();
        services.AddScoped<IIdentityRepo, IdentityRepo>();
        return services;
    }
}