namespace Interview.Domain.Categories.Page;

public class CategoryPageRequestFilter
{
    public Guid? ParentId { get; init; }

    public string? Name { get; init; }

    public bool ShowOnlyWithoutParent { get; init; }

    public Guid? EditingCategoryId { get; init; }
}
