using KaoyanService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace KaoyanService.Infrastructure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<IKaoyanRepo, KaoyanRepo>();
        return services;
    }
}
