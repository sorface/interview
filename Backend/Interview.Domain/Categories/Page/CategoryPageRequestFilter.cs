namespace Interview.Domain.Categories.Page;

public class CategoryPageRequestFilter
{
    public required Guid? ParentId { get; init; }

    public required string? Name { get; init; }

    public bool ShowOnlyWithoutParent { get; init; }
}
