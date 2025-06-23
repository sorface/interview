using Interview.Backend.Responses;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services;
using Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomQuestions;

[ApiController]
[Route("api/room-questions")]
public class RoomQuestionController(IRoomQuestionService roomQuestionService) : ControllerBase
{
    /// <summary>
    /// Changing the current question.
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <returns>Data of the current question, room, status.</returns>
    [Authorize]
    [HttpPut("active-question")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RoomQuestionDetail), StatusCodes.Status200OK)]
    public Task<RoomQuestionDetail> ChangeActiveQuestion(RoomQuestionChangeActiveRequest request)
    {
        return roomQuestionService.ChangeActiveQuestionAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a page with room questions.
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <returns>The page with the questions of the room.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<RoomQuestionResponse>), StatusCodes.Status200OK)]
    public Task<List<RoomQuestionResponse>> FindRoomQuestions([FromQuery] RoomQuestionsRequest request)
    {
        return roomQuestionService.FindQuestionsAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting question answer detail.
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <returns>The page with the questions of the room.</returns>
    [Authorize]
    [HttpGet("answer")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RoomQuestionAnswerDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<RoomQuestionAnswerDetailResponse> GetAnswerQuestionDetailsAsync([FromQuery] RoomQuestionAnswerDetailRequest request)
    {
        return roomQuestionService.GetAnswerDetailsAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Get analytics for closed questions.
    /// </summary>
    /// <param name="roomId">Room ID.</param>
    /// <returns>Analytics for closed questions.</returns>
    [Authorize]
    [HttpGet("closed-analytics/{roomId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RoomQuestionClosedAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<RoomQuestionClosedAnalytics> GetClosedQuestionsAnalyticsAsync(Guid roomId)
    {
        return roomQuestionService.GetClosedQuestionsAnalyticsAsync(roomId, HttpContext.RequestAborted);
    }
}
