using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.RoomQuestions;

[ApiController]
[Route("api/room-questions")]
public class RoomQuestionController : ControllerBase
{
    private readonly IRoomQuestionService _roomQuestionService;

    public RoomQuestionController(IRoomQuestionService roomQuestionService)
    {
        _roomQuestionService = roomQuestionService;
    }

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
        return _roomQuestionService.ChangeActiveQuestionAsync(request, HttpContext.RequestAborted);
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
        return _roomQuestionService.FindQuestionsAsync(request);
    }
}
