using X.PagedList;

namespace Interview.Backend.Common;

/// <summary>
/// Serves as a class for initializing a new instances of the <see cref="PagedListResponse{T}"/> class.
/// </summary>
public static class PagedListExt
{
    /// <summary>
    /// Asynchronously creates a new instance of the <see cref="PagedListResponse{T}"/> class.
    /// </summary>
    /// <param name="self">An asynchronous operation that returns a list of items for the page as a value.</param>
    /// <typeparam name="T">Page element type.</typeparam>
    /// <returns>New <see cref="PagedListResponse{T}"/> object.</returns>
    public static async Task<PagedListResponse<T>> ToPagedListResponseAsync<T>(this Task<IPagedList<T>> self)
    {
        var res = await self;
        return new PagedListResponse<T>(res);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PagedListResponse{T}"/> class.
    /// </summary>
    /// <param name="self">A list of items for the page.</param>
    /// <typeparam name="T">Page element type.</typeparam>
    /// <returns>New <see cref="PagedListResponse{T}"/> object.</returns>
    public static PagedListResponse<T> ToPagedListResponse<T>(this IPagedList<T> self)
    {
        return new PagedListResponse<T>(self);
    }
}
