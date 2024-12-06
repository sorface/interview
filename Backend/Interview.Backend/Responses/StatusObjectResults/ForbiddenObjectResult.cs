using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Interview.Backend.Responses.StatusObjectResults;

public class ForbiddenObjectResult : ObjectResult
{
    private const int DefaultStatusCode = StatusCodes.Status403Forbidden;

    public ForbiddenObjectResult([ActionResultObjectValue] object? value)
        : base(value)
    {
        StatusCode = DefaultStatusCode;
    }
}
