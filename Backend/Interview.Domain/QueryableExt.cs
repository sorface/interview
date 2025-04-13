using System.Linq.Expressions;
using System.Reflection;
using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Domain;

public static class QueryableExt
{
    public static IOrderedQueryable<T> OrderBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, EVSortOrder sortOrder)
        => sortOrder == EVSortOrder.Asc ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

    public static IOrderedQueryable<T> ThenBy<T, TKey>(this IOrderedQueryable<T> query, Expression<Func<T, TKey>> keySelector, EVSortOrder sortOrder)
        => sortOrder == EVSortOrder.Asc ? query.ThenBy(keySelector) : query.ThenByDescending(keySelector);

    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);

    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IEnumerable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);

    public static async Task<HashSet<Guid>> GetAllChildrenAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        Guid parentId,
        Expression<Func<TEntity, Guid?>> parentIdSelector,
        bool includeParent = false,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
    {
        // Проверяем, что передано корректное выражение
        if (parentIdSelector.Body is not MemberExpression { Member: PropertyInfo parentIdProperty })
        {
            throw new ArgumentException("The parentIdSelector must be a simple property access expression.", nameof(parentIdSelector));
        }

        var result = new HashSet<Guid>();
        if (includeParent)
        {
            result.Add(parentId);
        }

        var queue = new Queue<Guid>();
        queue.Enqueue(parentId);

        // Параметр для выражения
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var parentIdExpression = Expression.Property(parameter, parentIdProperty.Name);
        var idExpression = Expression.Property(parameter, nameof(Entity.Id));

        var containsMethod = typeof(List<Guid?>).GetMethod(nameof(List<Guid?>.Contains), new[] { typeof(Guid?) }) ?? throw new InvalidOperationException("Not found contains method");

        while (queue.Count > 0)
        {
            var currentParents = queue.ToList();
            queue.Clear();

            // Создаём динамическое условие WHERE e.ParentId IN (currentParents)
            var parentIdListExpression = Expression.Constant(currentParents.Cast<Guid?>().ToList());

            var condition = Expression.Call(parentIdListExpression, containsMethod, parentIdExpression);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);

            // Выполняем запрос к БД
            var children = await dbSet
                .AsNoTracking()
                .Where(lambda)
                .Select(Expression.Lambda<Func<TEntity, Guid>>(idExpression, parameter))
                .ToListAsync(cancellationToken);

            foreach (var childId in children)
            {
                // Добавляем в результат только новые узлы
                if (result.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return result;
    }
}
