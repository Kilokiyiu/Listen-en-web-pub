# MyCommonTools — 通用工具库

> 封装服务需要的通用工具，包括服务初始化契约、JSON 处理、自定义转换器和验证规则扩展。

---

## 一、项目概述

MyCommonTools 是通用工具类库，不依赖任何业务层，为所有服务提供基础工具能力。目前当前包含四大类功能：

| 功能 | 说明 |
|------|------|
| **服务初始化** | `IServiceInit` 接口，定义模块自注册契约 |
| **JSON 扩展** | 序列化/反序列化扩展方法，统一日期格式 |
| **JSON 转换器** | `DateTimeJsonConverter`，控制 DateTime 的 JSON 输出格式 |
| **验证规则扩展** | FluentValidation 的集合验证扩展（去重、包含、不包含） |

---

## 二、核心类

### 3.1 IServiceInit — 服务初始化契约

```csharp
namespace MyCommons;

public interface IServiceInit
{
    public void ServiceInit(IServiceCollection services);
}
```

**作用**：各业务模块实现此接口，在 `Program.cs` 中统一调用，完成依赖注入注册。

**使用方式**（以 IdentityService.Infrastructure 为例）：

```csharp
// Infrastructure 层
public class ModuleInitializer : IServiceInit
{
    public void ServiceInit(IServiceCollection services)
    {
        services.AddScoped<IIdentityRepo, IdRepository>();
    }
}

// WebAPI 层 Program.cs
var moduleInit = new ModuleInitializer();
moduleInit.ServiceInit(builder.Services);
```

> 参考项目 YouZack-VNext 使用 `IModuleInitializer` + 反射自动扫描，不需要在 Program.cs 中显式实例化。当前项目是手动调用。

---

### 3.2 JsonExtensions — JSON 序列化扩展

```csharp
public static class JsonExtensions
{
    // 创建统一配置的 JsonSerializerOptions
    public static JsonSerializerOptions CreateJsonSerializerOptions(bool camelCase = false);
    
    // 对象转 JSON 字符串
    public static string ToJsonString(this object value, bool camelCase = false);
    
    // JSON 字符串转对象
    public static T? ParseJson<T>(this string value);
}
```

**特点**：
- 统一日期格式：`yyyy-MM-dd HH:mm:ss`
- 支持中文编码：`UnicodeRanges.All`
- 可选驼峰命名：`camelCase` 参数控制

**使用示例**：

```csharp
var user = new { Name = "admin", CreateTime = DateTime.Now };

// 序列化（默认 PascalCase）
string json = user.ToJsonString();
// 输出：{"Name":"admin","CreateTime":"2026-05-01 14:30:00"}

// 序列化（驼峰命名）
string jsonCamel = user.ToJsonString(camelCase: true);
// 输出：{"name":"admin","createTime":"2026-05-01 14:30:00"}

// 反序列化
var obj = json.ParseJson<User>();
```

---

### 3.3 DateTimeJsonConverter — 日期时间转换器

```csharp
namespace Zack.Commons.JsonConverters;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public DateTimeJsonConverter(string dateFormatString = "yyyy-MM-dd HH:mm:ss");
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options);
}
```

**作用**：自定义 `System.Text.Json` 对 `DateTime` 的序列化/反序列化行为，固定输出格式为 `yyyy-MM-dd HH:mm:ss`。

**特点**：
- 序列化时固定使用服务器时区
- 反序列化时支持标准日期字符串解析
- 前端如需适配用户时区，需自行调整

**使用方式**：

```csharp
// 在 JsonSerializerOptions 中注册
var options = new JsonSerializerOptions();
options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));

// 或在 ASP.NET Core 中全局配置
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
});
```

---

### 3.4 EnumerableValidators — 集合验证扩展

```csharp
namespace FluentValidation;

public static class EnumerableValidators
{
    // 集合中没有重复元素
    public static IRuleBuilderOptions<T, IEnumerable<TItem>> NotDuplicated<T, TItem>(...);
    
    // 集合中不包含指定值
    public static IRuleBuilderOptions<T, IEnumerable<TItem>> NotContains<T, TItem>(..., TItem comparedValue);
    
    // 集合中包含指定值
    public static IRuleBuilderOptions<T, IEnumerable<TItem>> Contains<T, TItem>(..., TItem comparedValue);
}
```

**作用**：扩展 FluentValidation，为集合类型提供常用的验证规则。

**使用示例**：

```csharp
public class CreateAlbumRequestValidator : AbstractValidator<CreateAlbumRequest>
{
    public CreateAlbumRequestValidator()
    {
        RuleFor(x => x.Tags).NotDuplicated()           // 标签不能重复
                            .NotContains("invalid");   // 不能包含非法标签
        
        RuleFor(x => x.Categories).Contains("default"); // 必须包含默认分类
    }
}
```

---

## 四、未实现内容

| 能力 | Listen-en-web | YouZack-VNext |
|------|---------------|---------------|
| 服务初始化 | `IServiceInit`（手动调用） | `IModuleInitializer`（反射自动扫描） |
| JSON 工具 | `JsonExtensions` + `DateTimeJsonConverter` | 同左 + `JsonExtentions` 更多方法 |
| 反射帮助 | ❌ 未实现 | ✅ `ReflectionHelper`（获取所有引用程序集） |
| 字符串/集合扩展 | ❌ 未实现 | ✅ `StringExtensions`、`EnumerableExtensions` |
| 哈希/随机数 | ❌ 未实现 | ✅ `HashHelper`、`RandomExtensions` |
| IO/HTTP 帮助 | ❌ 未实现 | ✅ `IOHelper`、`HttpHelper` |
| 日志扩展 | ❌ 未实现 | ✅ `LoggerExtensions` |

---

## 五、后续待扩展内容

| 扩展方向 | 说明 |
|----------|------|
| `ReflectionHelper` | 获取所有引用程序集，支持模块自动扫描 |
| `StringExtensions` | 字符串常用扩展（如 `IsNullOrEmpty`、`Truncate`） |
| `HashHelper` | SHA256 等散列计算 |
| `IOHelper` | 文件下载、路径处理 |
| `HttpHelper` | HTTP 请求封装 |