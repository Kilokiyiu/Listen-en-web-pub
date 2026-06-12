using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyJWT;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureInfrastructureServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfiguration configuration = builder.Configuration;

        JWTOptions jwtOptions = configuration.GetSection("JWT").Get<JWTOptions>() ?? throw new InvalidOperationException("JWT options not found");

        // 支持通过环境变量覆盖 JWT Key（生产环境必须）
        var jwtKeyFromEnv = Environment.GetEnvironmentVariable("JWT_KEY");
        if (!string.IsNullOrEmpty(jwtKeyFromEnv))
        {
            jwtOptions.Key = jwtKeyFromEnv;
        }

        if (string.IsNullOrEmpty(jwtOptions.Key))
        {
            throw new InvalidOperationException("JWT Key is not configured. Set the 'JWT:Key' in appsettings.json or the 'JWT_KEY' environment variable.");
        }

        services.Configure<JWTOptions>(options =>
        {
            options.Issuer = jwtOptions.Issuer;
            options.Audience = jwtOptions.Audience;
            options.Key = jwtOptions.Key;
            options.ExpireSeconds = jwtOptions.ExpireSeconds;
        });
        services.AddJwtAuthentication(jwtOptions);
        services.AddAuthentication();
        services.AddScoped<IGenerateToken, GenerateToken>();
    }
}