using Interview.Backend.Common;
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
    /// Getting a available category page.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>A page of category with metadata about the pages.</returns>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedListResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<PagedListResponse<CategoryResponse>> FindPage([FromQuery] CategoryPageRequest request)
    {
        return _categoryService.FindPageAsync(request, HttpContext.RequestAborted).ToPagedListResponseAsync();
    }

    /// <summary>
    /// Getting a available archived category page.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>A page of category with metadata about the pages.</returns>
    [Authorize]
    [HttpGet("archive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedListResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<PagedListResponse<CategoryResponse>> FindArchivePage([FromQuery] CategoryPageRequest request)
    {
        return _categoryService.FindArchivePageAsync(request, HttpContext.RequestAborted).ToPagedListResponseAsync();
    }

    /// <summary>
    /// Archive category.
    /// </summary>
    /// <param name="id">Id category.</param>
    /// <returns>Archived category.</returns>
    [Authorize]
    [HttpPost("archive/{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<CategoryResponse> Archive(Guid id)
    {
        return _categoryService.ArchiveAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Unarchive category.
    /// </summary>
    /// <param name="id">Id category.</param>
    /// <returns>Unarchived category.</returns>
    [Authorize]
    [HttpPost("unarchive/{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<CategoryResponse> Unarchive(Guid id)
    {
        return _categoryService.UnarchiveAsync(id, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a new category.
    /// </summary>
    /// <param name="request">category edit request.</param>
    /// <returns>The object of the new category.</returns>
    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CategoryResponse>> Create(CategoryEditRequest request)
    {
        return _categoryService.CreateAsync(request, HttpContext.RequestAborted).ToResponseAsync();
    }

    /// <summary>
    /// Updating the category by ID.
    /// </summary>
    /// <param name="id">ID of the of category.</param>
    /// <param name="request">The object with the category data for which you need to update.</param>
    /// <returns>Updated category object.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CategoryResponse>> Update(Guid id, CategoryEditRequest request)
    {
        return _categoryService.UpdateAsync(id, request, HttpContext.RequestAborted).ToResponseAsync();
    }
}
