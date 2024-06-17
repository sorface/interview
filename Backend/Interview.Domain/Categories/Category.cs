using Interview.Domain.Database;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.ServiceResults.Errors;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Categories;

public class Category : Entity
{
    public required string Name { get; set; }

    public Guid? ParentId { get; set; }

    public Category? Parent { get; set; }

    public List<Room> Rooms { get; set; } = new();

    public static async Task<ServiceError?> ValidateCategoryAsync(AppDbContext db, Guid? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return null;
        }

        var hasCategory = await db.Categories.AnyAsync(e => e.Id == id, cancellationToken);
        if (!hasCategory)
        {
            return ServiceError.NotFound($"Not found category by id '{id}'");
        }

        return null;
    }

    public static async Task<ServiceError?> ValidateParentAsync(AppDbContext db, Guid? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return null;
        }

        var hasParent = await db.Categories.AnyAsync(e => e.Id == id, cancellationToken);
        if (!hasParent)
        {
            return ServiceError.NotFound($"Not found parent for category by id '{id}'");
        }

        return null;
    }
}
