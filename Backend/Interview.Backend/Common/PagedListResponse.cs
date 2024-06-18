using X.PagedList;

namespace Interview.Backend.Common;

/// <summary>
/// Serves as a class for getting data to display as a page.
/// </summary>
/// <typeparam name="T">Page element type.</typeparam>
public class PagedListResponse<T>
{
    private readonly IPagedList<T> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedListResponse{T}"/> class.
    /// </summary>
    /// <param name="items">List of all elements.</param>
    public PagedListResponse(IPagedList<T> items)
    {
        _items = items;
    }

    /// <summary>
    /// Gets current page number.
    /// </summary>
    public int PageNumber => _items.PageNumber;

    /// <summary>
    /// Gets the number of items on the page.
    /// </summary>
    public int PageSize => _items.PageSize;

    /// <summary>
    /// Gets the amount of all pages.
    /// </summary>
    public int PageCount => _items.PageCount;

    /// <summary>
    /// Gets the number of all list items.
    /// </summary>
    public int TotalItemCount => _items.TotalItemCount;

    /// <summary>
    /// Gets a list of all items.
    /// </summary>
    public IReadOnlyCollection<T> Data => _items;
}
