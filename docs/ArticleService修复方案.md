# ArticleService 后端修复详细方案

> 基于当前代码实际状态整理，按文件逐一列出问题 + 修复方式。
> 前端 API 约定（来自 `api/DailyArticle.js`）：
> - `GET  /DailyArticle/GetByDate?date=yyyy-MM-dd` → 按日期获取短文
> - `POST /DailyArticle/MarkAsRead` → 标记已读
> - `POST /DailyArticle/ToggleFavorite` → 切换收藏
> - `GET  /DailyArticle/GetReadHistory?page=&pageSize=` → 已读历史（分页）

---

## 一、问题总览

| # | 文件 | 问题 |
|---|---|---|
| 1 | `DailyArticle.cs` | `CreationTime` 被赋值两次（第21行 `date.Date` / 第28行 `DateTime.Now`），且缺少 `Date` 字段 |
| 2 | `DailyArticle.cs` | `PublicTime` 属性声明有多余空格（非致命，建议清理） |
| 3 | `IArticleRepo.cs` | `GetPublishedArticlesAsync` 参数名 `IsPublished` 首字母大写不规范；缺少分页参数 |
| 4 | `ArticleRepo.cs` | `FindAsync(date)` 按主键（Guid）查找，传入 DateTime 会报错 |
| 5 | `ArticleRepo.cs` | `GetPublishedArticlesAsync` 硬编码 `== true`，忽略入参 |
| 6 | `ArticleRepo.cs` | `MarkAsReadAsync` / `ToggleFavoriteAsync` 抛 `NotImplementedException` |
| 7 | `DailyArticleConfig.cs` | `HasDefaultValue(DateTime.Now)` 使用 C# 值而非 SQL 端默认值 |
| 8 | `ArticleController.cs` | `[HttpPost]` 应为 `[HttpGet]`（`GetByDate` 是查询操作） |
| 9 | `ArticleController.cs` | `date = DateTime.Now` 覆盖入参，导致无论传什么日期都返回当天 |
| 10 | `ArticleController.cs` | 直接依赖 `ArticleRepo` 而非 `IArticleRepo` |
| 11 | `ArticleController.cs` | 缺少 `MarkAsRead` / `ToggleFavorite` / `GetReadHistory` 三个端点 |
| 12 | `Program.cs` | 未注册 `ArticleDbContext`（数据库连接完全未配置） |
| 13 | `Program.cs` | 未注册 `IArticleRepo` DI |
| 14 | `Program.cs` | 未配置 CORS |
| 15 | `Program.cs` | 未配置 JWT 认证（前端会带 Token 请求） |
| 16 | `ArticleRespons.cs` | DTO 为空类，无属性 |
| 17 | 缺 | 无 `UserArticleStatus` 实体（`MarkAsRead` / `ToggleFavorite` 需要用户×文章的关联表） |

---

## 二、逐文件修复方案

### 1. `DailyArticle.cs`（Domain 实体）

**修复后代码：**

```csharp
using DomainCommons;

namespace ArticleService.Domain.Entity;

public class DailyArticle : ICreationTime
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }          // 新增：文章对应日期（用于按日期查询）
    public MultilingualString Title { get; private set; }
    public DateTime CreationTime { get; private set; }  // 由 ICreationTime 要求，记录创建时间
    public DateTime PublicTime { get; private set; }
    public string EnglishText { get; private set; }
    public string ChineseText { get; private set; }
    public string? ArticleUrl { get; private set; }     // 改为可空，与构造函数一致
    public bool IsPublished { get; private set; }

    public DailyArticle(DateTime date, MultilingualString title, DateTime publicTime,
        string englishText, string chineseText, string? articleUrl = null)
    {
        Id = Guid.NewGuid();
        Date = date.Date;               // 文章所属日期
        CreationTime = DateTime.Now;     // 记录创建时间戳（只保留这一处）
        Title = title;
        EnglishText = englishText;
        ChineseText = chineseText;
        PublicTime = publicTime;
        ArticleUrl = articleUrl;
        IsPublished = false;
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;
}
```

**要点：**
- 删除原第28行 `CreationTime = DateTime.Now`（保留第21行并改为 `DateTime.Now`，或移到构造函数末尾）
- 新增 `Date` 属性（用于按日期查询）
- `ArticleUrl` 改为可空 `string?`（与构造函数参数一致）

---

### 2. 新增 `UserArticleStatus.cs`（Domain 实体）

`MarkAsRead` 和 `ToggleFavorite` 是**每用户每文章**的状态，需要独立的关联表。

```csharp
namespace ArticleService.Domain.Entity;

/// <summary>
/// 用户对短文的操作状态（已读、收藏）
/// </summary>
public class UserArticleStatus
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ArticleId { get; private set; }
    public bool IsRead { get; private set; }
    public bool IsFavorited { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FavoritedAt { get; private set; }

    // 导航属性（可选，方便 EF Core 查询）
    public DailyArticle Article { get; private set; } = null!;

    private UserArticleStatus() { } // EF Core 需要

    public UserArticleStatus(Guid userId, Guid articleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ArticleId = articleId;
        IsRead = false;
        IsFavorited = false;
        CreatedAt = DateTime.Now;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void ToggleFavorite()
    {
        IsFavorited = !IsFavorited;
        FavoritedAt = IsFavorited ? DateTime.Now : null;
    }
}
```

---

### 3. `IArticleRepo.cs`（Domain 接口）

```csharp
using ArticleService.Domain.Entity;

namespace ArticleService.Domain;

public interface IArticleRepo
{
    /// <summary>
    /// 按日期获取已发布的短文
    /// </summary>
    Task<DailyArticle?> GetByDateAsync(DateTime date);

    /// <summary>
    /// 获取已发布的短文列表（分页）
    /// </summary>
    Task<DailyArticle[]> GetPublishedArticlesAsync(int page, int pageSize);

    /// <summary>
    /// 标记已读（如不存在则创建记录）
    /// </summary>
    Task MarkAsReadAsync(Guid userId, Guid articleId);

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    Task ToggleFavoriteAsync(Guid userId, Guid articleId);

    /// <summary>
    /// 获取用户的已读历史（分页，按时间倒序）
    /// </summary>
    Task<UserArticleStatus[]> GetReadHistoryAsync(Guid userId, int page, int pageSize);
}
```

**要点：**
- 移除 `bool IsPublished` 参数（`GetPublishedArticlesAsync` 始终只查已发布的）
- 新增 `GetReadHistoryAsync` 用于已读历史

---

### 4. `ArticleRepo.cs`（Infrastructure 实现）

```csharp
using ArticleService.Domain;
using ArticleService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure;

public class ArticleRepo : IArticleRepo
{
    private readonly ArticleDbContext dbContext;

    public ArticleRepo(ArticleDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<DailyArticle?> GetByDateAsync(DateTime date)
    {
        // 按 Date 字段查询，且只返回已发布的
        return dbContext.DailyArticles
            .FirstOrDefaultAsync(a => a.Date == date.Date && a.IsPublished);
    }

    public Task<DailyArticle[]> GetPublishedArticlesAsync(int page, int pageSize)
    {
        return dbContext.DailyArticles
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.Date)   // 按日期倒序
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
    }

    public async Task MarkAsReadAsync(Guid userId, Guid articleId)
    {
        var status = await dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ArticleId == articleId);

        if (status == null)
        {
            status = new UserArticleStatus(userId, articleId);
            status.MarkAsRead();
            dbContext.UserArticleStatuses.Add(status);
        }
        else if (!status.IsRead)
        {
            status.MarkAsRead();
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task ToggleFavoriteAsync(Guid userId, Guid articleId)
    {
        var status = await dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ArticleId == articleId);

        if (status == null)
        {
            status = new UserArticleStatus(userId, articleId);
            status.ToggleFavorite();
            dbContext.UserArticleStatuses.Add(status);
        }
        else
        {
            status.ToggleFavorite();
        }

        await dbContext.SaveChangesAsync();
    }

    public Task<UserArticleStatus[]> GetReadHistoryAsync(Guid userId, int page, int pageSize)
    {
        return dbContext.UserArticleStatuses
            .Where(s => s.UserId == userId && s.IsRead)
            .Include(s => s.Article)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
    }
}
```

**要点：**
- `FindAsync(date)` → 改为 `FirstOrDefaultAsync(a => a.Date == date.Date && a.IsPublished)`
- `MarkAsReadAsync` / `ToggleFavoriteAsync` 完整实现
- 需要新增 `UserArticleStatuses` DbSet（见下文 `ArticleDbContext`）

---

### 5. `ArticleDbContext.cs`（Infrastructure）

```csharp
using ArticleService.Domain.Entity;
using Infrastructure.EFCORE;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure;

public class ArticleDbContext : DbContext
{
    public DbSet<DailyArticle> DailyArticles { get; set; }
    public DbSet<UserArticleStatus> UserArticleStatuses { get; set; }  // 新增

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}
```

---

### 6. 新增 `UserArticleStatusConfig.cs`（Infrastructure 配置）

```csharp
using ArticleService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleService.Infrastructure.EntityConfig;

public class UserArticleStatusConfig : IEntityTypeConfiguration<UserArticleStatus>
{
    public void Configure(EntityTypeBuilder<UserArticleStatus> builder)
    {
        builder.ToTable("T_UserArticleStatus");
        builder.HasIndex(s => new { s.UserId, s.ArticleId }).IsUnique(); // 防止重复记录
        builder.HasOne(s => s.Article)
            .WithMany()
            .HasForeignKey(s => s.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

### 7. `DailyArticleConfig.cs`（Infrastructure 配置）

```csharp
using ArticleService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleService.Infrastructure.EntityConfig;

public class DailyArticleConfig : IEntityTypeConfiguration<DailyArticle>
{
    public void Configure(EntityTypeBuilder<DailyArticle> builder)
    {
        builder.ToTable("T_DailyArticle");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreationTime).HasDefaultValueSql("GETDATE()"); // 修复：SQL 端默认值
        builder.Property(e => e.IsPublished).HasDefaultValue(false);
        builder.HasIndex(e => e.Date).IsUnique(); // 每日期望只有一篇文章
    }
}
```

**要点：**
- `HasDefaultValue(DateTime.Now)` → 改为 `HasDefaultValueSql("GETDATE()")`（SQL Server）
- 建议给 `Date` 加唯一索引（每天一篇）

---

### 8. `ArticleController.cs`（WebAPI 控制器）

```csharp
using ArticleService.Domain;
using ArticleService.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]   // 需要认证（获取用户 ID）
public class ArticleController : ControllerBase
{
    private readonly IArticleRepo repo;

    public ArticleController(IArticleRepo repo)
    {
        this.repo = repo;
    }

    /// <summary>
    /// 按日期获取短文（前端：/DailyArticle/GetByDate?date=yyyy-MM-dd）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DailyArticleRespons>> GetByDate(DateTime date)
    {
        var article = await repo.GetByDateAsync(date);
        if (article == null)
        {
            return NotFound("该日期没有已发布的短文");
        }

        // 映射为 DTO，不直接返回实体（避免暴露内部字段）
        var dto = new DailyArticleRespons
        {
            Id = article.Id,
            Date = article.Date,
            TitleChinese = article.Title.Chinese,
            TitleEnglish = article.Title.English,
            EnglishText = article.EnglishText,
            ChineseText = article.ChineseText,
            ArticleUrl = article.ArticleUrl,
            PublicTime = article.PublicTime
        };

        return Ok(dto);
    }

    /// <summary>
    /// 标记已读（前端：POST /DailyArticle/MarkAsRead）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> MarkAsRead([FromBody] ArticleIdRequest request)
    {
        var userId = GetCurrentUserId(); // 从 JWT 获取当前用户 ID
        await repo.MarkAsReadAsync(userId, request.ArticleId);
        return Ok();
    }

    /// <summary>
    /// 切换收藏状态（前端：POST /DailyArticle/ToggleFavorite）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> ToggleFavorite([FromBody] ArticleIdRequest request)
    {
        var userId = GetCurrentUserId();
        await repo.ToggleFavoriteAsync(userId, request.ArticleId);
        return Ok();
    }

    /// <summary>
    /// 获取已读历史（前端：GET /DailyArticle/GetReadHistory?page=&pageSize=）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ReadHistoryRespons[]>> GetReadHistory(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var history = await repo.GetReadHistoryAsync(userId, page, pageSize);

        var dto = history.Select(h => new ReadHistoryRespons
        {
            ArticleId = h.Article.Id,
            TitleChinese = h.Article.Title.Chinese,
            TitleEnglish = h.Article.Title.English,
            Date = h.Article.Date,
            IsFavorited = h.IsFavorited,
            CreatedAt = h.CreatedAt
        }).ToArray();

        return Ok(dto);
    }

    /// <summary>
    /// 从 JWT Token 中获取当前用户 ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("无法获取用户 ID");
        return Guid.Parse(userIdClaim.Value);
    }
}
```

**还需要两个请求/响应 DTO（新建在 DTO 文件夹）：**

```csharp
// DTO/ArticleIdRequest.cs
namespace ArticleService.WebAPI.Controllers.DTO;

public class ArticleIdRequest
{
    public Guid ArticleId { get; set; }
}
```

```csharp
// DTO/DailyArticleRespons.cs（替换原来的 ArticleRespons.cs）
namespace ArticleService.WebAPI.Controllers.DTO;

public class DailyArticleRespons
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string ChineseText { get; set; } = "";
    public string? ArticleUrl { get; set; }
    public DateTime PublicTime { get; set; }
}
```

```csharp
// DTO/ReadHistoryRespons.cs
namespace ArticleService.WebAPI.Controllers.DTO;

public class ReadHistoryRespons
{
    public Guid ArticleId { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    public DateTime Date { get; set; }
    public bool IsFavorited { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### 9. `Program.cs`（WebAPI 启动配置）

参考 ListenService.WebAPI 的配置模式：

```csharp
using ArticleService.Domain;
using ArticleService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 添加 DbContext（SQL Server）
builder.Services.AddDbContext<ArticleDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("Default");
    options.UseSqlServer(connStr);
});

// 2. 注册 Repository（DI）
builder.Services.AddScoped<IArticleRepo, ArticleRepo>();

// 3. JWT 认证（与 IdentityService 使用相同的密钥和配置）
// TODO：从 appsettings.json 读取 JwtSettings
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // 与 IdentityService 一致的配置
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // TODO: 填入与 IdentityService 相同的 Issuer/Audience/Key
        };
    });

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();              // 启用 CORS
app.UseAuthentication();    // 启用认证
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**同时在 `appsettings.json` 中添加连接字符串：**

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=ListenEnArticle;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Issuer": "ListenEn",
    "Audience": "ListenEnClient",
    "Key": "YOUR_SECRET_KEY_HERE"   // 与 IdentityService 相同
  }
}
```

---

## 三、数据库迁移步骤

```bash
# 在 ArticleService.WebAPI 目录下执行
dotnet ef migrations add InitDailyArticle -p ../ArticleService.Infrastructure -s .
dotnet ef database update -p ../ArticleService.Infrastructure -s .
```

将生成两张表：
- `T_DailyArticle`（短文主表）
- `T_UserArticleStatus`（用户操作状态表）

---

## 四、前端 API 对接注意事项

前端 `api/DailyArticle.js` 中的接口地址需要与实际控制器路由匹配：

| 前端调用 | 实际路由 | 说明 |
|---|---|---|
| `get('/DailyArticle/GetByDate', { params: { date } })` | `GET /Article/GetByDate?date=` | 控制器路由 `[Route("[controller]/[action]")]` |
| `post('/DailyArticle/MarkAsRead', { articleId })` | `POST /Article/MarkAsRead` | |
| `post('/DailyArticle/ToggleFavorite', { articleId })` | `POST /Article/ToggleFavorite` | |
| `get('/DailyArticle/GetReadHistory', { params: { page, pageSize } })` | `GET /Article/GetReadHistory?page=&pageSize=` | |

前端 `baseURL` 需指向 ArticleService 的端口（与 ListenService 不同端口，或统一走 Nginx 反向代理）。

---

## 五、修复优先级建议

| 优先级 | 内容 |
|---|---|
| P0 | `Program.cs` 注册 DbContext + 数据库连接（否则任何接口都报错） |
| P0 | `ArticleRepo.GetByDateAsync` 修复查询逻辑（`FindAsync` → `FirstOrDefaultAsync`） |
| P0 | `DailyArticle.cs` 新增 `Date` 字段 + 清理 `CreationTime` 重复赋值 |
| P1 | `ArticleController` 修复 HTTP Method + 删除 `date = DateTime.Now` |
| P1 | 实现 `MarkAsReadAsync` / `ToggleFavoriteAsync`（需新增 `UserArticleStatus` 实体） |
| P1 | `Program.cs` 配置 JWT 认证（否则 `[Authorize]` 会导致所有请求 401） |
| P2 | `DailyArticleConfig`：`HasDefaultValueSql("GETDATE()")` |
| P2 | CORS 配置 |
| P2 | DTO 规范化（不把实体直接返回前端） |
