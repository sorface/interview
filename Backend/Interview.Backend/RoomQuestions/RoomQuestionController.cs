using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services;
using Interview.Domain.Rooms.RoomQuestions.Services.Update;
using Interview.Domain.ServiceResults.Success;
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
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<RoomQuestionDetail> ChangeActiveQuestion(RoomQuestionChangeActiveRequest request)
    {
        return _roomQuestionService.ChangeActiveQuestionAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a question in a room.
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <returns>Data of the current question, room, status.</returns>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RoomQuestionDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoomQuestionDetail>> Create(RoomQuestionCreateRequest request)
    {
        var roomQuestionDetail = await _roomQuestionService.CreateAsync(request, HttpContext.RequestAborted);
        return ServiceResult.Created(roomQuestionDetail).ToActionResult();
    }

    /// <summary>
    /// Update questions in a room.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="request">User Request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Authorize]
    [HttpPut("{roomId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(Guid roomId, [FromBody] List<RoomQuestionUpdateRequest> request)
    {
        var roomQuestionDetail = await _roomQuestionService.UpdateAsync(roomId, request, HttpContext.RequestAborted);
        return roomQuestionDetail.ToActionResult();
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
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<List<RoomQuestionResponse>> FindRoomQuestions([FromQuery] RoomQuestionsRequest request)
    {
        return _roomQuestionService.FindQuestionsAsync(request);
    }
}
