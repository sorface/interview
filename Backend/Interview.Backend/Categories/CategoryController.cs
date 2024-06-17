using Interview.Backend.Responses;
using Interview.Domain.Categories;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Categories;

[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Getting a available tags page.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>A page of questions with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<CategoryResponse>> GetTagPage([FromQuery] CategoryPageRequest request)
    {
        return _categoryService.FindPageAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a new tag.
    /// </summary>
    /// <param name="request">Tag edit request.</param>
    /// <returns>The object of the new question.</returns>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CategoryResponse>> CreateTag(CategoryEditRequest request)
    {
        return _categoryService.CreateTagAsync(request, HttpContext.RequestAborted).ToResponseAsync();
    }

    /// <summary>
    /// Updating the tag by ID.
    /// </summary>
    /// <param name="id">ID of the of tag.</param>
    /// <param name="request">The object with the tag data for which you need to update.</param>
    /// <returns>Updated question object.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CategoryResponse>> UpdateTag(Guid id, CategoryEditRequest request)
    {
        return _categoryService.UpdateTagAsync(id, request, HttpContext.RequestAborted).ToResponseAsync();
    }
}
