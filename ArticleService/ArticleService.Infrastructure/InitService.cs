using ArticleService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace ArticleService.Infrastructure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services)
    {
        services.AddScoped<IArticleRepo, ArticleRepo>();
        return services;
    }
}