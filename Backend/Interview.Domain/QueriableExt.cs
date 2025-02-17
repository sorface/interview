using X.PagedList;

namespace Interview.Domain;

public static class QueryableExt
{
    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);

    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IEnumerable<T> query, PageRequest page, CancellationToken cancellationToken)
        => query.ToPagedListAsync(page.PageNumber, page.PageSize, cancellationToken);
}
