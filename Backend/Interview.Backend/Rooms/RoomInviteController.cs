using Interview.Backend.Responses;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.Rooms;

[ApiController]
[Route("api/rooms/invites")]
public class RoomInviteController(IRoomService roomService) : ControllerBase
{
    /// <summary>
    /// Getting a Room invites by room id.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <returns>Room Invites response.</returns>
    [Authorize]
    [HttpGet("{roomId:guid}")]
    [ProducesResponseType(typeof(RoomInviteResponse), StatusCodes.Status200OK)]
    public Task<List<RoomInviteResponse>> GetRoomInvites(Guid roomId)
    {
        return roomService.GetInvitesAsync(roomId, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Apply Room Invitations.
    /// </summary>
    /// <param name="roomId">room id.</param>
    /// <param name="inviteId">invite id.</param>
    /// <returns>Information on the invitation.</returns>
    [Authorize]
    [HttpPost("{roomId:guid}/apply")]
    [ProducesResponseType(typeof(RoomInviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public Task<RoomInviteResponse> ApplyInvite(
        [FromRoute(Name = "roomId")] Guid roomId,
        [FromQuery(Name = "inviteId")] Guid inviteId)
    {
        return roomService.ApplyInvite(roomId, inviteId, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Generating room invitations for all types of room participants.
    /// </summary>
    /// <param name="id">room id.</param>
    /// <returns>Information on the invitations.</returns>
    [Authorize]
    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RoomInviteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public Task<List<RoomInviteResponse>> GenerateInvites([FromRoute(Name = "id")] Guid id)
    {
        return roomService.GenerateInvitesAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Generating room invitation for only one type of room participants.
    /// </summary>
    /// <param name="request">Request to regenerate the invitation.</param>
    /// <returns>Information on the invitation.</returns>
    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(RoomInviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public Task<RoomInviteResponse> GenerateInvite(RoomInviteGeneratedRequest request)
    {
        return roomService.GenerateInviteAsync(request, HttpContext.RequestAborted);
    }
}
