using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Roadmaps;
using Interview.Domain.Roadmaps.RoadmapById;
using Interview.Domain.Roadmaps.RoadmapPage;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.Tags.Records.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Roadmap;

[ApiController]
[Route("api/roadmaps")]
public class RoadmapController(IRoadmapService roadmapService) : ControllerBase
{
    /// <summary>
    /// Getting a available roadmap page.
    /// </summary>
    /// <param name="request">Page Parameters.</param>
    /// <returns>A page of roadmaps with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<TagItem>), StatusCodes.Status200OK)]
    public Task<IPagedList<RoadmapPageResponse>> FindPageAsync([FromQuery] FilteredRequest<RoadmapPageRequestFilter> request)
    {
        return roadmapService.FindPageAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting roadmap detail.
    /// </summary>
    /// <param name="id">Roadmap id.</param>
    /// <returns>Detailed information about roadmap.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RoadmapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<RoadmapResponse> FindPageAsync(Guid id)
    {
        return roadmapService.GetByIdAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Upsert roadmap.
    /// </summary>
    /// <param name="request">Roadmap upsert request.</param>
    /// <returns>Updated question object.</returns>
    [Authorize]
    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<ActionResult<Guid>> UpsertAsync([FromBody] UpsertRoadmapRequest request)
    {
        return roadmapService.UpsertAsync(request, HttpContext.RequestAborted).ToActionResultAsync();
    }
}
