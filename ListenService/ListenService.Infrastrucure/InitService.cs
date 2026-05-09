using ListenService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ListenService.Infrastrucure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<IListenRepo, ListenRepo>();
        return services;
    }
}