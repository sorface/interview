using System.ComponentModel.DataAnnotations;

namespace Interview.Domain.Rooms.BusinessAnalytic;

public class BusinessAnalyticRequest
{
    [Required]
    public required BusinessAnalyticRequestFilter Filter { get; set; }

    public required EVSortOrder DateSort { get; set; }
}
