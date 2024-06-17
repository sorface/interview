namespace Interview.Domain.Categories.Page;

public class CategoryResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid? ParentId { get; init; }
}
