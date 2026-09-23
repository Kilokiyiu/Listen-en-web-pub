using IdentitySerivce.Domain;
using IdentitySerivce.Infrastructure.Options;
using IdentitySerivce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentitySerivce.Infrastructure;

public static class InitService
{
    public static IServiceCollection ServiceInit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.Configure<CloudEmailSettings>(configuration.GetSection("CloudEmail"));
        services.Configure<SmtpEmailSettings>(configuration.GetSection("SmtpEmail"));
        services.Configure<FeedbackSettings>(configuration.GetSection("Feedback"));

        services.AddScoped<IdentityDomainService>();
        services.AddScoped<IIdentityRepo, IdentityRepo>();

        var smtpHost = configuration.GetSection("SmtpEmail")["Host"];
        var smtpUser = configuration.GetSection("SmtpEmail")["UserName"];
        var smtpPassword = configuration.GetSection("SmtpEmail")["Password"];
        var cloudEmailUser = configuration.GetSection("CloudEmail")["ApiUser"];

        // 优先 SMTP（送达 Outlook 更稳），其次 SendCloud，最后 Mock
        if (!string.IsNullOrWhiteSpace(smtpHost)
            && !string.IsNullOrWhiteSpace(smtpUser)
            && !string.IsNullOrWhiteSpace(smtpPassword))
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else if (!string.IsNullOrWhiteSpace(cloudEmailUser))
        {
            services.AddScoped<IEmailSender, CloudEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, MockEmailSender>();
        }

        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IStudyService, StudyService>();
        services.AddHostedService<AnalyticsAggregationHostedService>();
        return services;
    }
}
