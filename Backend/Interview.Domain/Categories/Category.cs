using Interview.Domain.Repository;
using Interview.Domain.Rooms;

namespace Interview.Domain.Categories;

public class Category : Entity
{
    public required string Name { get; set; }

    public Guid? ParentId { get; set; }

    public Category? Parent { get; set; }

    public List<Room> Rooms { get; set; } = new();
}
