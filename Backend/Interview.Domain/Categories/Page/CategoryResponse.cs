using Interview.Domain.Repository;

namespace Interview.Domain.Categories.Page;

public class CategoryResponse
{
    public static readonly Mapper<Category, CategoryResponse> Mapper =
        new(e => new CategoryResponse
        {
            Id = e.Id,
            Name = e.Name,
            Order = e.Order,
            Parent = e.Parent == null
                ? null
                : new CategoryParentResponse
                {
                    Id = e.Parent.Id,
                    Name = e.Parent.Name,
                },
        });

    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required int Order { get; init; }

    public required CategoryParentResponse? Parent { get; init; }
}

public class CategoryParentResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}
