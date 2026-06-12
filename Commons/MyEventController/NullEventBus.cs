namespace MyEventController;

public sealed class NullEventBus : IEventBus
{
    public void Publish(string eventName, object? eventData)
    {
    }
}
