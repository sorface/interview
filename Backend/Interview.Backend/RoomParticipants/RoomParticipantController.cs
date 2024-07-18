using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Records.Response;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomParticipants;

[ApiController]
[Route("api/room-participants")]
public class RoomParticipantController : ControllerBase
{
    private readonly IRoomParticipantService _roomParticipantService;

    public RoomParticipantController(IRoomParticipantService roomParticipantService)
    {
        _roomParticipantService = roomParticipantService;
    }

    /// <summary>
    /// Getting a list of room participants.
    /// </summary>
    /// <param name="request">Page Parameters.</param>
    /// <returns>List of room participants.</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(RoomParticipantDetail), StatusCodes.Status200OK)]
    public Task<RoomParticipantDetail> FindByRoomIdAndUserId([FromQuery] RoomParticipantGetRequest request)
    {
        return _roomParticipantService.FindByRoomIdAndUserIdAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Adding a transaction participant to a room.
    /// </summary>
    /// <param name="request">Data for adding a participant to a room.</param>
    /// <returns>Data about the added participant to the room.</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(RoomParticipantDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomParticipantDetail>> Create([FromBody] RoomParticipantCreateRequest request)
    {
        var roomParticipant = await _roomParticipantService.CreateAsync(request, HttpContext.RequestAborted);

        return ServiceResult.Created(roomParticipant).ToActionResult();
    }

    /// <summary>
    /// Changing the status of a room participant.
    /// </summary>
    /// <param name="request">Data changes in the status of a room participant.</param>
    /// <returns>Information about the participant of the room.</returns>
    [Authorize]
    [HttpPatch]
    [ProducesResponseType(typeof(RoomParticipantDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<RoomParticipantDetail> ChangeStatus([FromBody] RoomParticipantChangeStatusRequest request)
    {
        return _roomParticipantService.ChangeStatusAsync(request, HttpContext.RequestAborted);
    }
}
