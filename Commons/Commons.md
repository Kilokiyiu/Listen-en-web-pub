# Commons 使用说明

> Commons 是 Listen-en-web 项目的公共基础设施层，为所有业务服务提供通用能力。
> 采用"公共能力下沉"原则，将跨服务复用的代码提取为独立类库，避免重复实现。

---

## 一、概述

### 1.1 设计原则

| 原则 | 说明                             |
|------|--------------------------------|
| **公共能力下沉** | 主要用于将多个服务都需要的功能提取到Commons中统一维护 |
| **零业务依赖** | Commons 不依赖任何业务层，只依赖框架和基础库     |
| **可独立引用** | 各业务服务按需引用，不需要全部引用              |

### 1.2 项目结构

```
Commons/
├── CommonInit/                  # 用来给所有微服务启动时提供配置模块公共初始化工具
├── DomainCommonInterface/       # 封装了领域层的公共接口（实体、软删除、审计字段）
├── Infrastructure/              # 封装了一些对于EF Core 扩展方法（全局过滤器、只读查询）
├── MyCommons/                   # 封装了服务需要的通用工具
├── MyEventController/           # 事件总线基类（事件处理器抽象）
└── MyJWT/                       # 封装了JWT认证（包括配置、生成、中间件扩展）
```

### 1.3 引用关系

```
MyJWT ──→ Microsoft.AspNetCore.Authentication.JwtBearer
Infrastructure ──→ DomainCommons ──→ EF Core
CommonInit ──→ EF Core.SqlServer
MyEventController ──→ System.Text.Json
MyCommons ──→ System.Text.Json
```

---

## 二、CommonInit — 初始化工具

### 2.1 定位

给所有微服务启动时提供配置模块公共初始化工具。CommonInit的具体使用文档请查看：xxx

### 2.2 核心类

### WebAppBuilderExtensions

**`ConfigureDbConfiguration`**

```csharp
待补充
```

**作用**：配置数据库的连接字符串，并将配置存储在数据库中，支持运行时热启动


**`ConfigureExtraServices`**

**作用**：一键注册所有通用服务

| 注册项 | 说明 |
|--------|------|
| 模块初始化器 | 通过反射扫描所有程序集，自动执行 `IModuleInitializer` |
| EF Core DbContext | 自动注册所有 DbContext，统一使用 SQL Server |
| JWT 认证授权 | 注册 `AddAuthorization` + `AddAuthentication` + `AddJWTAuthentication` |
| CORS | 从配置文件读取允许的源 |
| FluentValidation | 自动扫描程序集注册验证器 |

---

## 三、DomainCommons — 领域层公共接口

### 3.1 定位

定义DDD领域层的基础接口契约，所有业务实体均可实现这些接口获得通用能力。

### 3.2 核心接口

#### `BaseEntity` — 实体标识

```csharp
public interface IEntity
{
    public Guid Id { get; }
}
```

**说明**：所有实体的基类。

#### `ISoftDelete` — 软删除

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; }
    void SoftDelete();
}
```

**说明**：
- 实现此接口的实体不会被物理删除
- `SoftDelete()` 方法将 `IsDeleted` 设为 `true`
- EF Core 全局过滤器会自动排除 `IsDeleted = true` 的数据

#### `ICreationTime` / `IDeletionTime` — 审计字段

```csharp
public interface ICreationTime { DateTime CreationTime { get; } }
public interface IDeletionTime { DateTime? DeletionTime { get; } }
```

---

## 四、Infrastructure — EF Core 扩展

### 4.1 定位

为 EF Core 提供全局过滤器和查询扩展方法。

### 4.2 核心类

#### `EFCoreExtensions`

**方法 1：软删除全局过滤器**

```csharp
public static void EnableSoftDeletionGlobalFilter(this ModelBuilder builder)
```

**作用**：扫描所有实现 `ISoftDelete` 的实体，自动添加 `WHERE IsDeleted = false` 查询条件。

**使用方式**：在 `OnModelCreating` 中调用：
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.EnableSoftDeletionGlobalFilter();
}
```

**方法 2：只读查询**

```csharp
public static IQueryable<T> Query<T>(this DbContext ctx) where T : class, IEntity
```

**作用**：返回禁用变更跟踪的查询，提升查询性能。

**使用方式**：
```csharp
var users = dbContext.Query<User>().Where(u => u.UserName == "admin").ToList();
```

---

## 五、MyCommons — 通用工具

### 5.1 定位

提供 JSON 处理、服务初始化等通用工具。

### 5.2 核心接口/类

#### `IServiceInit` — 服务初始化契约

```csharp
public interface IServiceInit
{
    void ServiceInit(IServiceCollection services);
}
```

**说明**：各业务服务实现此接口，在 WebAPI 的 `Program.cs` 中统一调用 `ServiceInit()` 完成依赖注册。

#### `JsonExtensions` — JSON 扩展

提供 `System.Text.Json` 的常用扩展方法。

#### `JsonConverters` — 自定义转换器

提供日期、枚举等类型的自定义 JSON 转换器。

#### `Validators` — 验证器工具

提供通用的 FluentValidation 验证规则。

---

## 六、MyEventController — 事件总线基类

### 6.1 定位

为微服务间的事件驱动通信提供处理器抽象基类。

### 6.2 核心接口/类

#### `IIntegrationEventHandler` — 事件处理器接口

```csharp
public interface IIntegrationEventHandler
{
    // 因为消息可能会重复发送，因此 Handle 内的实现需要是幂等的
    Task Handle(string eventName, string eventData);
}
```

**说明**：
- 所有事件处理器必须实现此接口
- 实现必须是**幂等的**（同一事件重复消费不能产生副作用）

#### `JsonIntegrationEventHandler<T>` — 泛型抽象基类

```csharp
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

**作用**：自动完成 JSON 反序列化，子类只需关心业务逻辑。

**使用方式**：
```csharp
public class UserCreatedEventHandler : JsonIntegrationEventHandler<UserCreatedEvent>
{
    public override Task HandleJson(string eventName, UserCreatedEvent? eventData)
    {
        // 处理业务逻辑
        return Task.CompletedTask;
    }
}
```

#### `EventNameAttribute` — 事件名称标记

用于标记事件类的名称，便于事件路由。

---

## 七、MyJWT — JWT 认证

### 7.1 定位

提供 JWT Token 的生成、配置和 ASP.NET Core 认证中间件集成。

### 7.2 核心类

#### `JWTOptions` — 配置模型

```csharp
public class JWTOptions
{
    public string Issuer { get; set; }      // 签发者
    public string Audience { get; set; }    // 接收者
    public string Key { get; set; }         // 密钥（至少 32 字符）
    public int ExpireSeconds { get; set; }  // 过期时间（秒）
}
```

**配置示例**（appsettings.json）：
```json
{
  "JWT": {
    "Issuer": "Listen-en-web",
    "Audience": "Listen-en-web",
    "Key": "your-32-char-secret-key-here!!!!",
    "ExpireSeconds": 86400
  }
}
```

#### `IGenerateToken` / `GenerateToken` — Token 生成

```csharp
public interface IGenerateToken
{
    string BuildToken(IEnumerable<Claim> claims, JWTOptions jwtOptions);
}
```

**说明**：使用 `SymmetricSecurityKey` + `HmacSha256` 签名生成 JWT。

#### `WebApplicationBuilderExtensions` — 一键配置

```csharp
public static class WebApplicationBuilderExtensions
{
    public static void ConfigureInfrastructureServices(this WebApplicationBuilder builder);
}
```

**作用**：
- 从 `appsettings.json` 读取 JWT 配置
- 注册 `JWTOptions` 到 DI 容器
- 配置 JWT Bearer 认证中间件
- 注册 `IGenerateToken` 服务

**使用方式**（Program.cs）：
```csharp
builder.ConfigureInfrastructureServices();
```

---

## 八、使用指南

### 8.1 如何在业务服务中引用

以 IdentityService 为例：

```xml
<!-- IdentityService.WebAPI.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\Commons\MyJWT\MyJWT.csproj" />
  <ProjectReference Include="..\..\Commons\DomainCommons\DomainCommons.csproj" />
</ItemGroup>
```

### 8.2 完整启动流程

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. JWT 配置（MyJWT）
builder.ConfigureInfrastructureServices();

// 2. EF Core（使用 CommonInit 的工厂）
builder.Services.AddDbContext<MyDbContext>(options => {
    var connStr = builder.Configuration.GetConnectionString("DatabaseConnStr");
    options.UseSqlServer(connStr);
});

// 3. 业务服务初始化（MyCommons IServiceInit）
builder.Services.ServiceInit();

// 4. 认证/授权管道
app.UseAuthentication();
app.UseAuthorization();
```

---

## 九、后续扩展内容

| 扩展方向 | 说明 |
|----------|------|
| 统一 API 响应包装 | 添加 `ApiResponse<T>` 和中间件，统一 `{code, message, data}` 格式 |
| 全局异常处理 | 添加 `ExceptionHandlingMiddleware`，统一错误响应 |
| 缓存抽象 | 添加 `ICacheService` 接口，支持 MemoryCache / Redis 切换 |
| 日志扩展 | 添加结构化日志配置，统一日志格式 |
| 健康检查 | 添加 `HealthChecks` 基类配置 |