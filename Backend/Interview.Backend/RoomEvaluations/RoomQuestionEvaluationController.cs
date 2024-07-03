using System.Net.Mime;
using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Backend.RoomEvaluations.Records;
using Interview.Domain;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Services;
using Interview.Domain.Rooms.RoomReviews.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomEvaluations;

[ApiController]
[Route("api/room-evaluations")]
public class RoomQuestionEvaluationController : ControllerBase
{
    private readonly IRoomQuestionEvaluationService _roomQuestionEvaluationService;

    public RoomQuestionEvaluationController(IRoomQuestionEvaluationService roomQuestionEvaluationService)
    {
        _roomQuestionEvaluationService = roomQuestionEvaluationService;
    }

    [Authorize]
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(QuestionEvaluationDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<QuestionEvaluationDetail?> FindByRoomIdAndQuestionId([FromQuery] QuestionEvaluationGetRequestApi request)
    {
        var user = HttpContext.User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("Current user not found");
        }

        var questionEvaluationGetRequest = new QuestionEvaluationGetRequest { UserId = user.Id, RoomId = request.RoomId, QuestionId = request.QuestionId, };

        return await _roomQuestionEvaluationService.FindByRoomIdAndQuestionId(questionEvaluationGetRequest, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpPost("merge")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(QuestionEvaluationDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
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

        return await _roomQuestionEvaluationService.MergeAsync(questionEvaluationMergeRequest, HttpContext.RequestAborted);
    }
}
