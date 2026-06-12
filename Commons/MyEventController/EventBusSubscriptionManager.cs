namespace MyEventController;

public class EventBusSubscriptionManager
{
    private readonly Dictionary<string, List<Type>> handlers = new(StringComparer.Ordinal);

    public void AddSubscription(string eventName, Type handlerType)
    {
        if (!handlers.TryGetValue(eventName, out var list))
        {
            list = [];
            handlers[eventName] = list;
        }

        if (!list.Contains(handlerType))
        {
            list.Add(handlerType);
        }
    }

    public IReadOnlyDictionary<string, List<Type>> Handlers => handlers;
}
