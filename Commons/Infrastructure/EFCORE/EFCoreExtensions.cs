using System.Linq.Expressions;
using DomainCommons;
using Microsoft.EntityFrameworkCore;

/*
 * 这是EFCore的扩展方法，添加了两个新功能
 */
namespace Infrastructure.EFCORE;

/// <summary>
/// 过滤掉逻辑删除的数据
/// </summary>
public static class EFCoreExtensions
{
    public static void EnableSoftDeletionGlobalFilter(this ModelBuilder builder)
    {
        var entityTypeSoftDeletion =
            builder.Model.GetEntityTypes().Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDelete)));

        foreach (var entityType in entityTypeSoftDeletion)
        {
            var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted));
            var parameter = Expression.Parameter(entityType.ClrType, "p");
            var filter = Expression.Lambda(
                Expression.Not(Expression.Property(parameter, isDeletedProperty.Name)),
                parameter);
            entityType.SetQueryFilter(filter);
        }
    }

    /// <summary>
    /// 提供一个只读查询的快捷方法，自动禁用变更跟踪。
    /// </summary>
    /// <param name="ctx"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Query<T>(this DbContext ctx) where T : class, IEntity
    {
        return ctx.Set<T>().AsNoTracking();
    }
}