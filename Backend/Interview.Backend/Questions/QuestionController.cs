using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Questions;

[ApiController]
[Route("questions")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Getting a Question page.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <returns>A page of questions with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<QuestionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<QuestionItem>> GetPage([FromQuery] FindPageRequest request)
    {
        return _questionService.FindPageAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a list of archived questions.
    /// </summary>
    /// <param name="pageRequest">Page params.</param>
    /// <returns>Deleted question object.</returns>
    [Authorize]
    [HttpGet("archived")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<QuestionItem>> Unarchive([FromQuery] PageRequest pageRequest)
    {
        return _questionService.FindPageArchiveAsync(pageRequest.PageNumber, pageRequest.PageSize, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a question by ID.
    /// </summary>
    /// <param name="id">Question ID.</param>
    /// <returns>The found object of the question.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<QuestionItem> GetById(Guid id)
    {
        return _questionService.FindByIdAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a new question.
    /// </summary>
    /// <param name="request">The object with the question data for which you need to create.</param>
    /// <returns>The object of the new question.</returns>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionItem>> Create(QuestionCreateRequest request)
    {
        var question = await _questionService.CreateAsync(request, HttpContext.RequestAborted);

        return ServiceResult.Created(question).ToActionResult();
    }

    /// <summary>
    /// Updating the question by ID.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <param name="request">The object with the question data for which you need to update.</param>
    /// <returns>Updated question object.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<QuestionItem> Update(Guid id, QuestionEditRequest request)
    {
        return _questionService.UpdateAsync(id, request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Transfer to the archive of the question.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <returns>Archived question object.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/archive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<QuestionItem> ArchiveAsync(Guid id)
    {
        return _questionService.ArchiveAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Permanently deleting a question by ID.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <returns>Deleted question object.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/unarchive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<QuestionItem> Unarchive(Guid id)
    {
        return _questionService.UnarchiveAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Permanently deleting a question by ID.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <returns>Deleted question object.</returns>
    [Authorize]
    [HttpDelete("{id:guid}/permanently")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<QuestionItem> DeletePermanently(Guid id)
    {
        return _questionService.DeletePermanentlyAsync(id, HttpContext.RequestAborted);
    }
}
