using MyEventController;

namespace ListenService.WebAPI.Events;

[EventName(IntegrationEventNames.IdentityUserCreated)]
public class UserCreatedEventHandler : JsonIntegrationEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        this.logger = logger;
    }

    public override Task HandleJson(string eventName, UserCreatedEvent? eventData)
    {
        if (eventData == null)
        {
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "Received user registration event: {UserId}, {UserName}, {Email}",
            eventData.UserId,
            eventData.UserName,
            eventData.Email);

        return Task.CompletedTask;
    }
}
