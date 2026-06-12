namespace MyEventController;

public interface IEventBus
{
    void Publish(string eventName, object? eventData);
}
