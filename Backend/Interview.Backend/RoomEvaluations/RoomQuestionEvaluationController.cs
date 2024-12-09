using System.Net.Mime;
using Interview.Backend.Auth;
using Interview.Backend.RoomEvaluations.Records;
using Interview.Domain;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomEvaluations;

[ApiController]
[Route("api/room-evaluations")]
public class RoomQuestionEvaluationController(IRoomQuestionEvaluationService roomQuestionEvaluationService) : ControllerBase
{
    /// <summary>
    /// Getting a current user room evaluations.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="currentUserAccessor">Current user accessor.</param>
    /// <returns>Page.</returns>
    [Authorize]
    [HttpGet("{roomId:guid}/my")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<RoomQuestionEvaluationResponse>), StatusCodes.Status200OK)]
    public Task<List<RoomQuestionEvaluationResponse>> GetMyEvaluations(
        Guid roomId,
        [FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetUserIdOrThrow();
        var request = new UserRoomQuestionEvaluationsRequest { UserId = userId, RoomId = roomId, };
        return roomQuestionEvaluationService.GetUserRoomQuestionEvaluationsAsync(request, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(QuestionEvaluationDetail), StatusCodes.Status200OK)]
    public async Task<QuestionEvaluationDetail?> FindByRoomIdAndQuestionId([FromQuery] QuestionEvaluationGetRequestApi request)
    {
        var user = HttpContext.User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("Current user not found");
        }

        var questionEvaluationGetRequest = new QuestionEvaluationGetRequest { UserId = user.Id, RoomId = request.RoomId, QuestionId = request.QuestionId, };

        return await roomQuestionEvaluationService.FindByRoomIdAndQuestionIdAsync(questionEvaluationGetRequest, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpPost("merge")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(QuestionEvaluationDetail), StatusCodes.Status200OK)]
    public async Task<QuestionEvaluationDetail> Merge(QuestionEvaluationMergeRequestApi request)
    {
        var user = HttpContext.User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("Current user not found");
        }

        var questionEvaluationMergeRequest = new QuestionEvaluationMergeRequest
        {
            UserId = user.Id,
            RoomId = request.RoomId,
            QuestionId = request.QuestionId,
            Mark = request.Mark,
            Review = request.Review,
        };

        return await roomQuestionEvaluationService.MergeAsync(questionEvaluationMergeRequest, HttpContext.RequestAborted);
    }
}
