using X.PagedList;

namespace Interview.Domain;

public static class PagedListExt
{
    public static IPagedList<TRes> ConvertAll<T, TRes>(this IPagedList<T> self, Func<T, TRes> mapper)
    {
        return new StaticPagedList<TRes>(self.Select(mapper), self);
    }
}
