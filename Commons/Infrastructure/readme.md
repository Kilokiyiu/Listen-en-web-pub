# Infrastructure — EF Core 扩展

> Infrastructure目前封装了一些 EF Core 的扩展方法，为所有业务服务提供通用的数据查询和过滤能力。

---

## 一、项目概述

Infrastructure 目前是 EF Core 的扩展类库，依赖 `DomainCommonInterface` 项目。它通过 C# 扩展方法为 `DbContext` 和 `ModelBuilder` 增加额外能力，目前包含两个核心功能：

- **软删除全局过滤器**：自动过滤已标记删除的数据
- **只读查询扩展**：提供禁用变更跟踪的快速查询方法

---

## 二、核心类

### 2.1 EFCoreExtensions

#### `EnableSoftDeletionGlobalFilter` — 软删除全局过滤器

**实现原理：** 扫描所有实现了`ISoftDelete`接口的实体，再通过构建一个表达式树来筛选被栓删除的数据</BR>

```csharp
var entityTypeSoftDeletion = builder.Model.
    GetEntityTypes().Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDelete)));
```

**作用**：扫描所有实现 `ISoftDelete` 接口的实体并将实体赋值给entityTypeSoftDeletion。
``

```csharp
foreach (var entityType in entityTypeSoftDeletion)
{
    var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted));
    var parameter = Expression.Parameter(entityType.ClrType, "p");
    var filter = Expression.Lambda(
                Expression.Not(Expression.Property(parameter, isDeletedProperty.Name)),
                parameter);
    entityType.SetQueryFilter(filter);
}
```

**代码解释**：</br>
`var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted))`: 在筛选后的实体中找到IsDeleted这个字段</BR>
`var parameter = Expression.Parameter(entityType.ClrType, "p")`:创建一个表达式的参数p(相当于lambda的p)</BR>
`var filter = Expression.Lambda(Expression.Not(Expression.Property(parameter, isDeletedProperty.Name)),parameter)`:拼接条件，最后生成lambda表达式：`p => !p.IsDeleted`
`entityType.SetQueryFilter(filter)`：给当前的实体设置全局查询筛选器，以后任何查询都会自动带上这个条件

**原理**：
1. 遍历 `ModelBuilder` 中所有实体类型
2. 筛选出实现了 `ISoftDelete` 的实体
3. 为每个实体动态构建 Lambda 表达式：`p => !p.IsDeleted`
4. 通过 `SetQueryFilter` 设置为全局过滤器

**使用方式**：在 `DbContext.OnModelCreating` 中调用：

```csharp
public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.EnableSoftDeletionGlobalFilter();
    }
}
```

**效果**：
```csharp
// 执行查询时自动附加过滤条件
var users = await dbContext.Users.ToListAsync();
// 实际 SQL: SELECT ... FROM Users WHERE IsDeleted = false
```

> 注意：使用了EnableSoftDeletionGlobalFilter后，基本上大部分查询都会经过筛选器，但是目前有以下几种情况除外
> 1. FindAsync方法(如果数据在本地缓存的情况下)---标记为待解决
> ```csharp
> var user = await dbContext.Users.FindAsync(id);
> //原因：FindAsync会优先查询本地缓存，而本地的缓存不会直接查询数据库，
> //如果有本地没有被标记为删除，而数据库被标记为删除的的数据，则会直接读取本地没有被标记为删除的数据
> ```
> 
> 2. 显式忽略筛选器
> ```csharp
> var allUsers = await dbContext.Users 
>   .IgnoreQueryFilters()  // ← 忽略筛选器
>   .ToListAsync();
> ```
> 
> 3. 原始SQL语句
> ```csharp
> var users = await dbContext.Users
>   .FromSqlRaw("SELECT * FROM Users") // ← 直接写sql语句会忽略筛选器，需要添加索引
>   .ToListAsync();
> ```

---

#### `Query<T>` — 只读查询

```csharp
public static IQueryable<T> Query<T>(this DbContext ctx) where T : class, IEntity
```

**作用**：提供禁用变更跟踪（`AsNoTracking`）的快捷查询方法。

**约束条件**：`T` 必须实现 `IEntity` 接口。

**使用方式**：

```csharp
// 需要实体实现 IEntity
public class Album : IEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}

// 调用扩展方法
var albums = dbContext.Query<Album>()
    .Where(a => a.Title.Contains("CET4"))
    .ToList();
```

**与原生查询的区别**：

| 方式 | 代码 | 变更跟踪 |
|------|------|----------|
| 原生 | `dbContext.Set<Album>().Where(...)` | ✅ 启用 |
| 扩展 | `dbContext.Query<Album>().Where(...)` | ❌ 禁用 |

> 只读查询场景（如列表展示、搜索）使用 `Query<T>()` 可提升性能，因为 EF Core 不需要构建变更跟踪快照。

---

## 四、后续扩展方向

当前 `EFCoreExtensions` 只有两个方法，后续可按需添加更多通用查询方法：

```csharp
// 按 Id 查询
public static async Task<T?> FindByIdAsync<T>(this DbContext ctx, Guid id) 
    where T : class, IEntity
{
    return await ctx.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
}

// 判断是否存在
public static async Task<bool> ExistsAsync<T>(this DbContext ctx, Guid id) 
    where T : class, IEntity
{
    return await ctx.Set<T>().AnyAsync(e => e.Id == id);
}
```

---

## 五、后续待实现内容

| 能力 | Listen-en-web | YouZack-VNext |
|------|---------------|---------------|
| 软删除过滤器 | ✅ `EnableSoftDeletionGlobalFilter` | ✅ `EnableSoftDeletionGlobalFilter` |
| 只读查询 | ✅ `Query<T>()` | ✅ `Query<T>()` |
| BaseDbContext | ❌ 未实现 | ✅ 领域事件 + 缓存清理 |
| Mediator 扩展 | ❌ 未实现 | ✅ `DispatchDomainEventsAsync` |