using System.Linq.Expressions;
using System.Reflection;
using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Domain;

public static class QueryableExt
{
    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);

    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IEnumerable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);

    public static async Task<HashSet<Guid>> GetAllChildrenAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        Guid parentId,
        Expression<Func<TEntity, Guid?>> parentIdSelector,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
    {
        // Анализируем выражение parentIdSelector, чтобы получить доступ к свойству ParentId
        if (parentIdSelector.Body is not MemberExpression memberExpression ||
            memberExpression.Member is not PropertyInfo parentIdProperty)
        {
            throw new ArgumentException("The parentIdSelector must be a simple property access expression.", nameof(parentIdSelector));
        }

        // Создаем параметр для выражения
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        // Выражение для Id
        var idExpression = Expression.Property(parameter, nameof(Entity.Id));

        // Выражение для ParentId
        var parentIdExpression = Expression.Property(parameter, parentIdProperty.Name);

        // Выражение для начального условия: ParentId == parentId
        var initialCondition = Expression.Equal(parentIdExpression, Expression.Constant(parentId, typeof(Guid?)));
        var initialWhere = Expression.Lambda<Func<TEntity, bool>>(initialCondition, parameter);

        // Получаем начальный список дочерних элементов
        var childrenList = await dbSet
            .AsNoTracking()
            .Where(initialWhere)
            .Select(Expression.Lambda<Func<TEntity, Guid>>(idExpression, parameter))
            .ToListAsync(cancellationToken);

        var children = childrenList.ToHashSet();
        var actualParent = children;

        // Рекурсивно получаем все дочерние элементы
        while (actualParent.Count > 0)
        {
            var parent = actualParent;
            var parentIds = parent.Select(id => (Guid?)id).ToList();

            // Выражение для условия: ParentId != null && parent.Contains(ParentId.Value)
            var parentIdNotNull = Expression.NotEqual(parentIdExpression, Expression.Constant(null, typeof(Guid?)));
            var parentIdInList = Expression.Call(
                typeof(Enumerable),
                "Contains",
                new[] { typeof(Guid?) },
                Expression.Constant(parentIds),
                parentIdExpression);

            var combinedCondition = Expression.AndAlso(parentIdNotNull, parentIdInList);
            var whereExpression = Expression.Lambda<Func<TEntity, bool>>(combinedCondition, parameter);

            // Получаем дочерние элементы следующего уровня
            var childrenNLevel = await dbSet
                .AsNoTracking()
                .Where(whereExpression)
                .Select(Expression.Lambda<Func<TEntity, Guid>>(idExpression, parameter))
                .ToListAsync(cancellationToken);

            actualParent = new HashSet<Guid>();
            foreach (var id in childrenNLevel)
            {
                if (children.Add(id))
                {
                    actualParent.Add(id);
                }
            }
        }

        return children;
    }
}
