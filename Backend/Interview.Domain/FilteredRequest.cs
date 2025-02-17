using System.ComponentModel.DataAnnotations;

namespace Interview.Domain;

public class FilteredRequest<TFilter>
{
    [Required]
    public required PageRequest Page { get; set; }

    public TFilter? Filter { get; set; }
}

public class RequiredFilteredRequest<TFilter>
{
    [Required]
    public required PageRequest Page { get; set; }

    [Required]
    public required TFilter Filter { get; set; }
}
