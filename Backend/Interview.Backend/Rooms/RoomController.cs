using System.Net.Mime;
using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Events.Storage;
using Interview.Domain.RoomReviews.Records;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Rooms.Service.Records.Response;
using Interview.Domain.Rooms.Service.Records.Response.Detail;
using Interview.Domain.Rooms.Service.Records.Response.Page;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Rooms;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Getting a Room page.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="filter">Search filter.</param>
    /// <returns>Page.</returns>
    [Authorize]
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoomReviewDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<RoomPageDetail>> GetPage([FromQuery] PageRequest request, [FromQuery] RoomPageDetailRequestFilter? filter)
    {
        return _roomService.FindPageAsync(filter ?? new RoomPageDetailRequestFilter(), request.PageNumber, request.PageSize, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a Room by ID.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<RoomDetail> GetById(Guid id)
    {
        return _roomService.FindByIdAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a Room state by id.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <returns>Room state.</returns>
    [Authorize]
    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(ActualRoomStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<ActualRoomStateResponse> GetRoomState(Guid id)
    {
        return _roomService.GetActualStateAsync(id);
    }

    /// <summary>
    /// Upsert Room state.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <param name="type">State type.</param>
    /// <param name="request">State payload.</param>
    /// <returns>Upsert result.</returns>
    [Authorize]
    [HttpPut("{id:guid}/state/{type}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<MessageResponse> UpsertRoomState(Guid id, string type, [FromBody] UpsertRoomRequestApi request)
    {
        await _roomService.UpsertRoomStateAsync(id, type, request.Payload, HttpContext.RequestAborted);
        return MessageResponse.Ok;
    }

    /// <summary>
    /// Delete Room state.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <param name="type">State type.</param>
    /// <returns>Delete result.</returns>
    [Authorize]
    [HttpDelete("{id:guid}/state/{type}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<MessageResponse> DeleteRoomState(Guid id, string type)
    {
        await _roomService.DeleteRoomStateAsync(id, type, HttpContext.RequestAborted);
        return MessageResponse.Ok;
    }

    /// <summary>
    /// Creating a new room.
    /// </summary>
    /// <param name="request">Room.</param>
    /// <returns>Created room.</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Room>> Create([FromBody] RoomCreateRequest request)
    {
        var room = await _roomService.CreateAsync(request, HttpContext.RequestAborted);

        return ServiceResult.Created(room).ToActionResult();
    }

    /// <summary>
    /// Update room.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <param name="request">Request.</param>
    /// <returns>Ok message.</returns>
    [Authorize]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public Task<RoomItem> PatchUpdate(Guid id, [FromBody] RoomUpdateRequest request)
    {
        return _roomService.UpdateAsync(id, request);
    }

    /// <summary>
    /// Get analytics by room.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <returns>Analytics.</returns>
    [Authorize]
    [HttpGet("{id:guid}/analytics")]
    [ProducesResponseType(typeof(Analytics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<Analytics> GetAnalytics(Guid id)
    {
        return _roomService.GetAnalyticsAsync(new RoomAnalyticsRequest(id), HttpContext.RequestAborted);
    }

    /// <summary>
    /// Get analytics  by room.
    /// </summary>
    /// <param name="id">Room id.</param>
    /// <returns>Analytics.</returns>
    [Authorize]
    [HttpGet("{id:guid}/analytics/summary")]
    [ProducesResponseType(typeof(AnalyticsSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<AnalyticsSummary> GetAnalyticsSummary(Guid id)
    {
        var user = User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("User is unauthorized");
        }

        var request = new RoomAnalyticsRequest(id);

        return _roomService.GetAnalyticsSummaryAsync(request, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpPatch("{id:guid}/close")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CloseRoom(Guid id)
    {
        await _roomService.CloseAsync(id, HttpContext.RequestAborted);

        return Ok();
    }

    [Authorize]
    [HttpPatch("{id:guid}/startReview")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> StartReviewRoom(Guid id)
    {
        await _roomService.StartReviewAsync(id, HttpContext.RequestAborted);

        return Ok();
    }

    /// <summary>
    /// Sending event to room.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>Ok message.</returns>
    [Authorize]
    [HttpPost("event")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendEvent(RoomEventApiRequest request)
    {
        var user = User.ToUser();
        if (user == null)
        {
            return Unauthorized();
        }

        var sendRequest = request.ToDomainRequest(user.Id);
        await _roomService.SendEventRequestAsync(sendRequest, HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    /// Get transcription by room.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="options">Options. Key = transcription type, value = response options.</param>
    /// <param name="currentUserAccessor">Current user accessor.</param>
    /// <returns>Analytics.</returns>
    [Authorize]
    [HttpPost("{roomId:guid}/transcription/search")]
    [ProducesResponseType(typeof(Dictionary<string, IReadOnlyCollection<IStorageEvent>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<Dictionary<string, List<IStorageEvent>>> GetTranscription(
        Guid roomId,
        [FromBody] Dictionary<string, TranscriptionRequestOption> options,
        [FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        var request = new TranscriptionRequest
        {
            RoomId = roomId,
            UserId = currentUserAccessor.UserId ?? throw new AccessDeniedException("User is unauthorized"),
            TranscriptionTypeMap = options,
        };
        return _roomService.GetTranscriptionAsync(request, HttpContext.RequestAborted);
    }
}
