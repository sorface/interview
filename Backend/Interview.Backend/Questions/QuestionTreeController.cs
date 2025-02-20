using Interview.Backend.Common;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Questions.QuestionTreeById;
using Interview.Domain.Questions.QuestionTreePage;
using Interview.Domain.Questions.Services;
using Interview.Domain.Questions.UpsertQuestionTree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.Questions;

[ApiController]
[Route("api/questions/tree")]

public class QuestionTreeController(IQuestionService questionService) : ControllerBase
{
    /// <summary>
    /// Getting a Question tree page.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <returns>A page of question trees with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedListResponse<QuestionTreePageRequest>), StatusCodes.Status200OK)]
    public Task<PagedListResponse<QuestionTreePageResponse>> FindQuestionTreePageAsync([FromQuery] QuestionTreePageRequest request)
    {
        return questionService.FindQuestionTreePageAsync(request, HttpContext.RequestAborted).ToPagedListResponseAsync();
    }

    /// <summary>
    /// Getting a Question tree by id.
    /// </summary>
    /// <param name="id">Question tree id.</param>
    /// <param name="archive">Archive.</param>
    /// <returns>A question tree detail.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(QuestionTreeByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    public Task<QuestionTreeByIdResponse> GetQuestionTreeByIdAsync(Guid id, [FromQuery] bool archive)
    {
        return questionService.GetQuestionTreeByIdAsync(id, archive, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Upsert question tree.
    /// </summary>
    /// <param name="request">Upsert question tree request.</param>
    /// <returns>Question tree id.</returns>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionResult<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ActionResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    public Task<ActionResult<Guid>> UpsertQuestionTreeAsync([FromBody] UpsertQuestionTreeRequest request)
    {
        return questionService.UpsertQuestionTreeAsync(request, HttpContext.RequestAborted).ToActionResultAsync();
    }

    /// <summary>
    /// Transfer to the archive of the question.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <returns>Archived question object.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/archive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ArchiveAsync(Guid id)
    {
        await questionService.ArchiveQuestionTreeAsync(id, HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    /// Permanently deleting a question by ID.
    /// </summary>
    /// <param name="id">ID of the of question.</param>
    /// <returns>Deleted question object.</returns>
    [Authorize]
    [HttpPatch("{id:guid}/unarchive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Unarchive(Guid id)
    {
        await questionService.UnarchiveQuestionTreeAsync(id, HttpContext.RequestAborted);
        return Ok();
    }
}
