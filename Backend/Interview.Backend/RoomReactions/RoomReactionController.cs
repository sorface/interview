using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Rooms.RoomQuestionReactions.Records;
using Interview.Domain.Rooms.RoomQuestionReactions.Records.Response;
using Interview.Domain.Rooms.RoomQuestionReactions.Services;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomReactions;

[ApiController]
[Route("api/room-reactions")]
public class RoomReactionController(IRoomQuestionReactionService roomQuestionReactionService) : ControllerBase
{
    /// <summary>
    ///  Creates a reaction from the user to an active question in the room.
    /// </summary>
    /// <param name="request">The user request.</param>
    /// <returns>Data about the new bundle (reaction, room, question).</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(RoomQuestionReactionDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomQuestionReactionDetail>> CreateInRoom(
        [FromBody] RoomQuestionReactionCreateRequest request)
    {
        var user = User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("User is not unauthorized");
        }

        var roomQuestionReaction =
            await roomQuestionReactionService.CreateAsync(request, user.Id, HttpContext.RequestAborted);

        return ServiceResult.Created(roomQuestionReaction).ToActionResult();
    }
}
