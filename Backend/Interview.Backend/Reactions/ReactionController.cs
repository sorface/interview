using Interview.Domain;
using Interview.Domain.Reactions.Records;
using Interview.Domain.Reactions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Reactions;

[ApiController]
[Route("api/reactions")]
public class ReactionController(IReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Getting a list of reactions.
    /// </summary>
    /// <param name="request">Page Parameters.</param>
    /// <returns>List of reactions.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IPagedList<ReactionDetail>), StatusCodes.Status200OK)]
    public Task<IPagedList<ReactionDetail>> GetPage([FromQuery] PageRequest request) =>
        reactionService.FindPageAsync(request.PageNumber, request.PageSize, HttpContext.RequestAborted);
}
