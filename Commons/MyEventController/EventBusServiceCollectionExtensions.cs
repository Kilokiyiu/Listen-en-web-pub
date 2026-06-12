using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyEventController;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        string queueName,
        params Assembly[] assemblies)
    {
        services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));

        var rabbitOptions = configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>() ?? new RabbitMQOptions();
        if (!rabbitOptions.Enabled)
        {
            services.AddSingleton<IEventBus, NullEventBus>();
            return services;
        }

        var subscriptionManager = new EventBusSubscriptionManager();
        foreach (var assembly in assemblies)
        {
            RegisterHandlersFromAssembly(services, subscriptionManager, assembly);
        }

        services.AddSingleton(subscriptionManager);
        services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<EventBusSubscriptionManager>(),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMQOptions>>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMQEventBus>>(),
            queueName));
        services.AddHostedService(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return services;
    }

    private static void RegisterHandlersFromAssembly(
        IServiceCollection services,
        EventBusSubscriptionManager subscriptionManager,
        Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IIntegrationEventHandler).IsAssignableFrom(t));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);

            foreach (var attribute in handlerType.GetCustomAttributes<EventNameAttribute>())
            {
                subscriptionManager.AddSubscription(attribute.Name, handlerType);
            }
        }
    }
}
