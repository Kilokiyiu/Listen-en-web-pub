using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyEventController;

public sealed class RabbitMQEventBus : IEventBus, IHostedService, IDisposable
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly EventBusSubscriptionManager subscriptionManager;
    private readonly RabbitMQOptions options;
    private readonly ILogger<RabbitMQEventBus> logger;
    private readonly string queueName;
    private readonly object connectLock = new();
    private readonly object publishLock = new();

    private IConnection? connection;
    private IModel? publishChannel;
    private IModel? consumeChannel;
    private bool consumerStarted;

    public RabbitMQEventBus(
        IServiceScopeFactory scopeFactory,
        EventBusSubscriptionManager subscriptionManager,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQEventBus> logger,
        string queueName)
    {
        this.scopeFactory = scopeFactory;
        this.subscriptionManager = subscriptionManager;
        this.options = options.Value;
        this.logger = logger;
        this.queueName = queueName;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = ConnectWithRetryAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Publish(string eventName, object? eventData)
    {
        try
        {
            EnsureConnected();

            if (publishChannel == null || !publishChannel.IsOpen)
            {
                logger.LogWarning("Skip publish {EventName}: RabbitMQ channel unavailable", eventName);
                return;
            }

            var json = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(json);

            lock (publishLock)
            {
                var properties = publishChannel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                publishChannel.BasicPublish(options.ExchangeName, eventName, properties, body);
            }

            logger.LogInformation("Published event {EventName} to exchange {Exchange}", eventName, options.ExchangeName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish event {EventName}", eventName);
        }
    }

    public void Dispose()
    {
        try { publishChannel?.Close(); } catch { /* ignore shutdown errors */ }
        try { consumeChannel?.Close(); } catch { /* ignore shutdown errors */ }
        try { connection?.Close(); } catch { /* ignore shutdown errors */ }

        publishChannel?.Dispose();
        consumeChannel?.Dispose();
        connection?.Dispose();

        publishChannel = null;
        consumeChannel = null;
        connection = null;
        consumerStarted = false;
    }

    private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= 30 && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                EnsureConnected();
                logger.LogInformation("RabbitMQ event bus connected on attempt {Attempt}", attempt);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ connect attempt {Attempt} failed", attempt);
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30));
            }
        }

        logger.LogError("RabbitMQ unavailable after retries; API will continue without message queue");
    }

    private void EnsureConnected()
    {
        lock (connectLock)
        {
            if (connection is { IsOpen: true } && publishChannel is { IsOpen: true })
            {
                return;
            }

            DisposeChannelsOnly();

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            connection = factory.CreateConnection();
            publishChannel = connection.CreateModel();
            consumeChannel = connection.CreateModel();

            publishChannel.ExchangeDeclare(options.ExchangeName, ExchangeType.Direct, durable: true);
            consumeChannel.ExchangeDeclare(options.ExchangeName, ExchangeType.Direct, durable: true);
            consumeChannel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            foreach (var (eventName, _) in subscriptionManager.Handlers)
            {
                consumeChannel.QueueBind(queueName, options.ExchangeName, eventName);
            }

            if (!consumerStarted)
            {
                var consumer = new AsyncEventingBasicConsumer(consumeChannel);
                consumer.Received += OnMessageReceivedAsync;
                consumeChannel.BasicConsume(queueName, autoAck: false, consumer);
                consumerStarted = true;
            }

            logger.LogInformation(
                "RabbitMQ event bus ready. Exchange={Exchange}, Queue={Queue}",
                options.ExchangeName,
                queueName);
        }
    }

    private void DisposeChannelsOnly()
    {
        try { publishChannel?.Close(); } catch { }
        try { consumeChannel?.Close(); } catch { }
        try { connection?.Close(); } catch { }

        publishChannel?.Dispose();
        consumeChannel?.Dispose();
        connection?.Dispose();

        publishChannel = null;
        consumeChannel = null;
        connection = null;
        consumerStarted = false;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var eventName = ea.RoutingKey;
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

        if (!subscriptionManager.Handlers.TryGetValue(eventName, out var handlerTypes))
        {
            consumeChannel!.BasicAck(ea.DeliveryTag, multiple: false);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            foreach (var handlerType in handlerTypes)
            {
                var handler = (IIntegrationEventHandler)scope.ServiceProvider.GetRequiredService(handlerType);
                await handler.Handle(eventName, json);
            }

            consumeChannel!.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle event {EventName}", eventName);
            consumeChannel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }
}
