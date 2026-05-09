using System.Text.Json;
using System.Threading.Tasks;

namespace MyEventController
{
    /// <summary>
    /// 事件处理器的抽象基类，作用是帮子类自动完成 JSON 反序列化，让子类只关心业务逻辑。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JsonIntegrationEventHandler<T> : IIntegrationEventHandler
    {
        public Task Handle(string eventName, string json)
        {
            T? eventData = JsonSerializer.Deserialize<T>(json);
            return HandleJson(eventName, eventData);
        }

        public abstract Task HandleJson(string eventName, T? eventData);
    }
}