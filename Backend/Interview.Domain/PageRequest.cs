using System.ComponentModel.DataAnnotations;

namespace Interview.Domain;

public class PageRequest
{
    [Range(1, Constants.DefaultPageSize)]
    public int PageSize { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; }
}
