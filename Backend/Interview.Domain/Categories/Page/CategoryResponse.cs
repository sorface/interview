using Interview.Domain.Repository;

namespace Interview.Domain.Categories.Page;

public class CategoryResponse
{
    public static readonly Mapper<Category, CategoryResponse> Mapper =
        new(e => new CategoryResponse { Id = e.Id, Name = e.Name, ParentId = e.ParentId });

    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid? ParentId { get; init; }
}
