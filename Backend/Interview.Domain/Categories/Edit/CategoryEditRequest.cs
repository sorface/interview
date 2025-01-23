namespace Interview.Domain.Categories.Edit;

public class CategoryEditRequest
{
    public string? Name { get; set; }

    public int Order { get; set; }

    public Guid? ParentId { get; set; }
}
