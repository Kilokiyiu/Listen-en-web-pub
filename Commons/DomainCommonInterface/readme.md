# DomainCommons — 领域层公共接口

> 定义 DDD 领域层的基础接口契约，所有业务实体均可实现这些接口获得通用能力。
> 位置：`Commons/DomainCommons/`

---

## 一、项目概述

DomainCommons 是领域层的公共基础库，不包含任何业务逻辑，只定义领域实体需要遵循的契约接口。业务服务中的实体通过实现这些接口，获得以下通用能力：

- 统一的主键标识（`IEntity`）
- 软删除支持（`ISoftDelete`）
- 审计字段（创建时间、删除时间）

---

## 二、接口清单

### 2.1 IEntity — 实体标识

```csharp
namespace DomainCommons;

public interface IEntity
{
    public Guid Id { get; }
}
```

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `Guid` | 实体的全局唯一标识 |

**设计意图**：EF Core自身有查询数据的方法，如果需要使用Commons中额外的查询方法，就需要实现IEntity接口，例如Infrastructure中的EFCoreExtensions就是Commons提供的方法

**使用示例**：
```csharp
public class User : IEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserName { get; set; }
}
```

---

### 2.2 ISoftDelete — 软删除

```csharp
namespace DomainCommons

public interface ISoftDelete
{
    bool IsDeleted { get; }
    void SoftDelete();
}
```

| 成员 | 类型 | 说明 |
|------|------|------|
| `IsDeleted` | `bool` | 标记是否已被软删除 |
| `SoftDelete()` | `void` | 执行软删除操作（将 IsDeleted 设为 true） |

**设计意图**：
- 实现此接口的实体不会被物理删除，而是通过 `IsDeleted = true` 标记为已删除
- EF Core 全局过滤器会自动排除 `IsDeleted = true` 的数据
- `IsDeleted` 不能写成 `get; protected set;`，否则在实现类中不能是 public

**使用示例**：
```csharp
public class User : IEntity, ISoftDelete
{
    public Guid Id { get; private set; }
    public string UserName { get; set; }
    public bool IsDeleted { get; private set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

**与 Infrastructure 的配合**：
在 `DbContext.OnModelCreating` 中启用全局过滤器：
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.EnableSoftDeletionGlobalFilter();
}
```

---

### 2.3 ICreationTime — 创建时间审计

```csharp
namespace DomainCommons;

public interface ICreationTime
{
    DateTime CreationTime { get; }
}
```

| 属性 | 类型 | 说明 |
|------|------|------|
| `CreationTime` | `DateTime` | 实体创建时间 |

---

### 2.4 IDeletionTime — 删除时间审计

```csharp
namespace DomainCommons;

public interface IDeletionTime
{
    DateTime? DeletionTime { get; }
}
```

| 属性 | 类型 | 说明 |
|------|------|------|
| `DeletionTime` | `DateTime?` | 实体删除时间（软删除时填充） |

---

## 三、后续待实现内容

当前 DomainCommons 只包含 4 个基础接口，后续可按需扩展：

| 扩展接口 | 说明 |
|----------|------|
| `IModificationTime` | 最后修改时间审计 |
| `IAggregateRoot` | 聚合根标记接口（空接口） |
| `IDomainEvents` | 领域事件接口，`AddDomainEvent` / `ClearDomainEvents` |
| `BaseEntity` | 基类，将接口聚合为可直接继承的实体基类 |
| `AggregateRootEntity` | 基类，将接口聚合为可直接继承的实体基类 |

