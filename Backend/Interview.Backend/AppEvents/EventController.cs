using Interview.Backend.Common;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Events.Service;
using Interview.Domain.Events.Service.Create;
using Interview.Domain.Events.Service.FindPage;
using Interview.Domain.Events.Service.Update;
using Interview.Domain.Questions.Records.FindPage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.AppEvents;

[ApiController]
[Route("api/event")]
public class EventController : ControllerBase
{
    /// <summary>
    /// Getting a AppEvent page.
    /// </summary>
    /// <param name="request">Page Parameters.</param>
    /// <param name="service">Repository.</param>
    /// <returns>A page of questions with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<QuestionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<AppEventItem>> GetPage([FromQuery] PageRequest request, [FromServices] IAppEventService service)
    {
        return service.FindPageAsync(request.PageNumber, request.PageSize, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a Event by ID.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <param name="service">Service.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppEventItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromServices] IAppEventService service)
    {
        var room = await service.FindByIdAsync(id, HttpContext.RequestAborted);
        return room is null ? NotFound(new MessageResponse
        {
            Message = $"Not found event by id {id}",
        }) : Ok(room);
    }

    /// <summary>
    /// Getting a Event by Type.
    /// </summary>
    /// <param name="type">Id.</param>
    /// <param name="service">Service.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("{type}")]
    [ProducesResponseType(typeof(AppEventItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string type, [FromServices] IAppEventService service)
    {
        var room = await service.FindByTypeAsync(type, HttpContext.RequestAborted);
        return room is null ? NotFound(new MessageResponse
        {
            Message = $"Not found event by type {type}",
        }) : Ok(room);
    }

    /// <summary>
    /// Creating a new event.
    /// </summary>
    /// <param name="request">Create request.</param>
    /// <param name="service">Service.</param>
    /// <returns>Created room.</returns>
    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<CreatedResult> Create([FromBody] AppEventCreateRequest request, [FromServices] IAppEventService service)
    {
        var result = await service.CreateAsync(request, HttpContext.RequestAborted);
        return Created(string.Empty, result);
    }

    /// <summary>
    /// Update room.
    /// </summary>
    /// <param name="id">Event id.</param>
    /// <param name="request">Request.</param>
    /// <param name="service">Service.</param>
    /// <returns>Ok message.</returns>
    [Authorize]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public Task<AppEventItem> PatchUpdate(Guid id, [FromBody] AppEventUpdateRequest request, [FromServices] IAppEventService service)
    {
        return service.UpdateAsync(id, request, HttpContext.RequestAborted);
    }
}
