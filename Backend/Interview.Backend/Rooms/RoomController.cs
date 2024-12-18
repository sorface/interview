using System.Net.Mime;
using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.Service;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Rooms;

[ApiController]
[Route("api/rooms")]
public class RoomController(IRoomService roomService) : ControllerBase
{
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
    public Task<IPagedList<RoomPageDetail>> GetPage(
        [FromQuery] PageRequest request,
        [FromQuery] RoomPageDetailRequestFilter? filter)
    {
        return roomService.FindPageAsync(
            filter ?? new RoomPageDetailRequestFilter(),
            request.PageNumber,
            request.PageSize,
            HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a calendar rooms
    /// </summary>
    /// <param name="request">FromQuery filter</param>
    /// <returns>Page.</returns>
    [Authorize]
    [HttpGet("calendar")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<RoomCalendarItem>), StatusCodes.Status200OK)]
    public Task<List<RoomCalendarItem>> GetCalendar([FromQuery] RoomCalendarRequest request)
    {
        return roomService.GetCalendarAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a Room by ID.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <returns>Room.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<RoomDetail> GetById(Guid id)
    {
        return roomService.FindByIdAsync(id, HttpContext.RequestAborted);
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
        return roomService.GetActualStateAsync(id);
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
        await roomService.UpsertRoomStateAsync(id, type, request.Payload, HttpContext.RequestAborted);
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
        await roomService.DeleteRoomStateAsync(id, type, HttpContext.RequestAborted);
        return MessageResponse.Ok;
    }

    /// <summary>
    /// Creating a new room.
    /// </summary>
    /// <param name="request">Room.</param>
    /// <returns>Created room.</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(RoomPageDetail), StatusCodes.Status201Created)]
    public async Task<ActionResult<RoomPageDetail>> Create([FromBody] RoomCreateApiRequest request)
    {
        var domainRequest = new RoomCreateRequest
        {
            Name = request.Name,
            AccessType = SERoomAccessType.FromName(request.AccessType),
            Questions = request.Questions ?? [],
            Experts = request.Experts,
            Examinees = request.Examinees,
            Tags = request.Tags,
            DurationSec = request.Duration,
            ScheduleStartTime = request.ScheduleStartTime,
        };

        var room = await roomService.CreateAsync(domainRequest, HttpContext.RequestAborted);
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
    public Task<RoomItem> PatchUpdate(Guid id, [FromBody] RoomUpdateRequest request)
    {
        return roomService.UpdateAsync(id, request);
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
        return roomService.GetAnalyticsAsync(new RoomAnalyticsRequest(id), HttpContext.RequestAborted);
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

        return roomService.GetAnalyticsSummaryAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Closing the room.
    /// </summary>
    /// <param name="id">room id.</param>
    /// <returns>result operation.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/close")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseRoom(Guid id)
    {
        await roomService.CloseAsync(id, HttpContext.RequestAborted);

        return Ok();
    }

    /// <summary>
    /// Moving the room to the review stage.
    /// </summary>
    /// <param name="id">room id.</param>
    /// <returns>result operation.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/startReview")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> StartReviewRoom(Guid id)
    {
        await roomService.StartReviewAsync(id, HttpContext.RequestAborted);

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
    public async Task<ActionResult> SendEvent(RoomEventApiRequest request)
    {
        var user = User.ToUser();
        if (user == null)
        {
            return Unauthorized();
        }

        var sendRequest = request.ToDomainRequest(user.Id);
        await roomService.SendEventRequestAsync(sendRequest, HttpContext.RequestAborted);
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
        var request = new TranscriptionRequest { RoomId = roomId, UserId = currentUserAccessor.GetUserIdOrThrow(), TranscriptionTypeMap = options, };
        return roomService.GetTranscriptionAsync(request, HttpContext.RequestAborted);
    }
}
