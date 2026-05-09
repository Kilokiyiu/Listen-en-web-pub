# MyEventController — 事件总线基类

> 定义事件驱动架构的基础抽象，为微服务间的集成事件通信提供处理器契约和反序列化支持。

---

## 一、项目概述

MyEventController 是事件总线的基础抽象层，当前只包含事件处理器的接口和基类，**尚未接入具体的消息队列实现**（如 RabbitMQ）。

后续启用事件总线，需要在此项目或新项目中补充：
- `IEventBus` 接口（Publish/Subscribe/Unsubscribe）
- `RabbitMQEventBus` 实现（或 Kafka、Azure Service Bus 等）
- `ServicesCollectionExtensions`（自动扫描注册事件处理器）

---

## 二、核心接口/类

### 2.1 IIntegrationEventHandler — 事件处理器接口

```csharp
namespace MyEventController;

public interface IIntegrationEventHandler
{
    // 因为消息可能会重复发送，因此 Handle 内的实现需要是幂等的
    Task Handle(string eventName, string eventData);
}
```

**设计要点**：
- 接收的是 `string eventData`（JSON 字符串），而非强类型对象
- 原因：集成事件是跨服务的，不同服务不应该共享同一个事件类定义
- 处理器实现必须是**幂等的**：同一事件重复消费不能产生副作用

---

### 2.2 JsonIntegrationEventHandler<T> — 泛型抽象基类

```csharp
namespace MyEventController;

public abstract class JsonIntegrationEventHandler<T> : IIntegrationEventHandler
{
    public Task Handle(string eventName, string json)
    {
        T? eventData = JsonSerializer.Deserialize<T>(json);
        return HandleJson(eventName, eventData);
    }

    public abstract Task HandleJson(string eventName, T? eventData);
}
```

**作用**：帮子类自动完成 JSON 反序列化，子类只需关心业务逻辑。

**使用示例**（以 Listening 服务订阅用户创建事件为例）：

```csharp
// 定义本地的事件数据结构（只在当前服务内使用）
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
}

// 事件处理器
[EventName("Identity.UserCreated")]
public class UserCreatedEventHandler : JsonIntegrationEventHandler<UserCreatedEvent>
{
    public override Task HandleJson(string eventName, UserCreatedEvent? eventData)
    {
        if (eventData == null) return Task.CompletedTask;
        
        // 业务逻辑：为新用户初始化学习记录等
        Console.WriteLine($"用户 {eventData.UserName} 已创建");
        return Task.CompletedTask;
    }
}
```

---

### 2.3 EventNameAttribute — 事件名称标记

```csharp
namespace MyEventController;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EventNameAttribute : Attribute
{
    public EventNameAttribute(string name)
    {
        this.Name = name;
    }
    public string Name { get; init; }
}
```

**作用**：标注事件处理器监听的事件名称，支持一个处理器监听多个事件（`AllowMultiple = true`）。

**命名规范**：建议采用 `"服务名.事件名"` 格式，如：
- `Identity.UserCreated`
- `Listening.EpisodeCreated`
- `FileService.FileUploaded`

---

## 三、未实现内容

| 能力 | Listen-en-web | YouZack-VNext |
|------|---------------|---------------|
| 事件处理器接口 | ✅ `IIntegrationEventHandler` | ✅ `IIntegrationEventHandler` |
| 泛型抽象基类 | ✅ `JsonIntegrationEventHandler<T>` | ✅ `JsonIntegrationEventHandler<T>` |
| 事件名称特性 | ✅ `EventNameAttribute` | ✅ `EventNameAttribute` |
| 事件总线接口 | ❌ 未实现 | ✅ `IEventBus` |
| RabbitMQ 实现 | ❌ 未实现 | ✅ `RabbitMQEventBus` |
| 自动扫描注册 | ❌ 未实现 | ✅ `AddEventBus` 扩展方法 |
| 消息持久化 | ❌ 未实现 | ✅ `DeliveryMode = 2` |
| 手动 ACK | ❌ 未实现 | ✅ `BasicAck` |

---

## 四、后续待扩展内容

如需启用事件总线，建议按以下顺序实现：

### 阶段 1：定义事件总线接口

```csharp
public interface IEventBus
{
    void Publish(string eventName, object? eventData);
    void Subscribe(string eventName, Type handlerType);
    void Unsubscribe(string eventName, Type handlerType);
}
```

### 阶段 2：接入 RabbitMQ 实现

参考 YouZack-VNext 的 `RabbitMQEventBus`，核心设计：
- 使用 Direct Exchange，以事件名作为 RoutingKey
- Singleton 服务中通过 `IServiceScopeFactory` 创建 Scope
- 手动 ACK，确保消息不丢失
- 消息持久化（`DeliveryMode = 2`）

### 阶段 3：自动扫描注册

```csharp
public static IServiceCollection AddEventBus(this IServiceCollection services, 
    string queueName, params Assembly[] assemblies)
{
    // 自动扫描所有 IIntegrationEventHandler 实现类
    // 通过 EventNameAttribute 获取监听的事件名
    // 批量注册到 EventBus
}
```

### 阶段 4：在业务中发布/订阅事件

```csharp
// 发布事件
_eventBus.Publish("Identity.UserCreated", new { UserId = user.Id, UserName = user.UserName });

// 订阅事件（通过特性标注）
[EventName("Identity.UserCreated")]
public class UserCreatedHandler : JsonIntegrationEventHandler<UserCreatedEvent> { ... }
```

---

## 五、事件命名规范建议

| 事件名 | 说明 |
|--------|------|
| `Identity.UserCreated` | 用户注册成功 |
| `Identity.UserPasswordChanged` | 用户修改密码 |
| `Listening.AlbumCreated` | 专辑创建 |
| `Listening.EpisodeCreated` | 听力材料创建 |
| `FileService.FileUploaded` | 文件上传完成 |
| `FileService.FileDeleted` | 文件删除 |