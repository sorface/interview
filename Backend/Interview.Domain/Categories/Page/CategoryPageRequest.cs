namespace Interview.Domain.Categories.Page;

public class CategoryPageRequest
{
    public required CategoryPageRequestFilter? Filter { get; init; }

    public required PageRequest Page { get; init; }
}
