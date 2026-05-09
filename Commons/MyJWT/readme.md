# MyJWT — JWT 认证

> 封装 JWT 令牌的生成、配置和 ASP.NET Core 认证中间件集成，为所有业务服务提供统一的认证能力。

---

## 一、项目概述

MyJWT 是 JWT 认证的基础设施库，负责：

- 从 `appsettings.json` 读取 JWT 配置
- 提供 Token 生成服务（`IGenerateToken`）
- 注册 JWT Bearer 认证中间件
- 支持 HS256 对称加密签名

当前使用 **对称加密**（`SymmetricSecurityKey`），所有服务共享同一个 Key。如需更高级的安全场景，可后续升级为非对称加密（RSA）。

---

## 二、核心类

### 3.1 JWTOptions — 配置模型

```csharp
namespace MyJWT;

public class JWTOptions
{
    public string Issuer { get; set; } = string.Empty;      // 签发者
    public string Audience { get; set; } = string.Empty;    // 接收者
    public string Key { get; set; } = string.Empty;         // 密钥（至少 32 字符）
    public int ExpireSeconds { get; set; }                  // 过期时间（秒）
}
```

**配置示例**（`appsettings.json`）：

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

> **重要**：`Key` 必须至少 32 个字符（256 位），否则 HS256 签名会报 `IDX10720` 错误。

---

### 3.2 IGenerateToken / GenerateToken — Token 生成

```csharp
namespace MyJWT;

public interface IGenerateToken
{
    string BuildToken(IEnumerable<Claim> claims, JWTOptions jwtOptions);
}

public class GenerateToken : IGenerateToken
{
    public string BuildToken(IEnumerable<Claim> claims, JWTOptions jwtOptions)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: DateTime.Now.AddSeconds(jwtOptions.ExpireSeconds),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**使用方式**（以 IdentityService 登录为例）：

```csharp
public class IdentityDomainService
{
    private readonly IGenerateToken tokenService;
    private readonly IOptions<JWTOptions> jwtOptions;

    public async Task<(SignInResult, string?)> LoginByEmailAndPwdAsync(string email, string password)
    {
        // ... 验证用户名密码 ...

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        string token = tokenService.BuildToken(claims, jwtOptions.Value);
        return (SignInResult.Success, token);
    }
}
```

---

### 3.3 AuthenticationToken — 认证中间件注册

```csharp
namespace MyJWT;

public static class AuthenticationToken
{
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, JWTOptions jwtoptions)
    {
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtoptions.Issuer,
                ValidAudience = jwtoptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtoptions.Key)),
            };
        });
    }
}
```

**作用**：注册 JWT Bearer 认证方案，配置 Token 验证参数。

**验证规则**：
- `ValidateIssuer`：验证签发者是否匹配
- `ValidateAudience`：验证接收者是否匹配
- `ValidateLifetime`：验证 Token 是否过期
- `ValidateIssuerSigningKey`：验证签名密钥是否有效

---

### 3.4 WebApplicationBuilderExtensions — 一键配置

```csharp
namespace MyJWT;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureInfrastructureServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfiguration configuration = builder.Configuration;
        
        JWTOptions jwtOptions = configuration.GetSection("JWT").Get<JWTOptions>() 
            ?? throw new InvalidOperationException("JWT options not found");
        
        services.Configure<JWTOptions>(configuration.GetSection("JWT"));
        services.AddJwtAuthentication(jwtOptions);
        services.AddAuthentication();
        services.AddScoped<IGenerateToken, GenerateToken>();
    }
}
```

**作用**：从 `appsettings.json` 读取 JWT 配置，一键完成所有 JWT 相关的服务注册。

**使用方式**（`Program.cs`）：

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureInfrastructureServices();  // ← 一键配置 JWT

var app = builder.Build();
app.UseAuthentication();  // ← 启用认证中间件
app.UseAuthorization();   // ← 启用授权中间件
```
---

## 三、常见问题

### Q1：登录时报 `IDX10720: key has '240' bits`

**原因**：`JWT.Key` 少于 32 个字符，HS256 需要至少 256 位（32 字节）。

**解决**：确保 Key ≥ 32 个字符：
```json
"Key": "your-32-char-secret-key-here!!!!"  // 32 个字符
```

### Q2：接口返回 401，但 Token 已正确携带

**检查项**：
1. `Program.cs` 中是否调用了 `app.UseAuthentication()` 和 `app.UseAuthorization()`
2. 控制器或 Action 是否标记了 `[Authorize]`
3. Token 是否过期（`ExpireSeconds`）
4. `Issuer` / `Audience` / `Key` 是否与生成时一致

### Q3：如何调整 Token 过期时间？

修改 `appsettings.json`：
```json
"JWT": {
  "ExpireSeconds": 3600  // 1 小时
}
```

---

## 四、后续待扩展内容

| 扩展方向 | 说明 |
|----------|------|
| Refresh Token | 支持刷新令牌机制，避免频繁重新登录 |
| 非对称加密 | 使用 RSA 密钥对，提升安全性 |
| Token 黑名单 | 支持主动注销 Token（需配合 Redis） |