using ListenService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ListenService.Infrastrucure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<ListenRepo>();
        services.AddScoped<IListenRepo, CachedListenRepo>();
        services.AddSingleton<ListenCacheInvalidator>();
        return services;
    }
}
