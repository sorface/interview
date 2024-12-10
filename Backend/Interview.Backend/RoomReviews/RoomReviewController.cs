using System.Net.Mime;
using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.RoomReviews;

[Route("api/room-reviews")]
public class RoomReviewController(IRoomReviewService roomReviewService) : ControllerBase
{
    /// <summary>
    /// Getting a room reviews page.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>Page.</returns>
    [Authorize]
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<RoomReviewDetail>), StatusCodes.Status200OK)]
    public Task<IPagedList<RoomReviewPageDetail>> FindPage([FromQuery] RoomReviewPageRequest request)
    {
        return roomReviewService.FindPageAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Getting a current user room reviews.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="currentUserAccessor">Current user accessor.</param>
    /// <returns>Page.</returns>
    [Authorize]
    [HttpGet("{roomId:guid}/my")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserRoomReviewResponse), StatusCodes.Status200OK)]
    public Task<UserRoomReviewResponse?> GetMyReview(
        Guid roomId,
        [FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetUserIdOrThrow();
        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };
        return roomReviewService.GetUserRoomReviewAsync(request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a review for a room
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <returns>Review details.</returns>
    [Authorize]
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoomReviewDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RoomReviewDetail), StatusCodes.Status200OK)]
    [Obsolete("It is deprecated, please use /api/room-reviews/upsert instead.")]
    public async Task<ActionResult<RoomReviewDetail>> CreateAsync([FromBody] RoomReviewCreateRequest request)
    {
        var user = HttpContext.User.ToUser();

        if (user is null)
        {
            throw new AccessDeniedException("Current user not found");
        }

        var roomReviewDetail = await roomReviewService.CreateAsync(request, user.Id, HttpContext.RequestAborted);

        return ServiceResult.Created(roomReviewDetail).ToActionResult();
    }

    /// <summary>
    /// Upsert a review for a room
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <param name="currentUserAccessor">Current user accessor</param>
    /// <returns>Review details</returns>
    [Authorize]
    [HttpPut("upsert")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UpsertReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UpsertReviewResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpsertReviewResponse>> UpsertAsync([FromBody] RoomReviewCreateRequest request, [FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetUserIdOrThrow();
        var upsertResult = await roomReviewService.UpsertAsync(request, userId, HttpContext.RequestAborted);
        return (upsertResult.Created ? ServiceResult.Created(upsertResult) : ServiceResult.Ok(upsertResult)).ToActionResult();
    }

    /// <summary>
    /// Update a review by id
    /// </summary>
    /// <param name="id">Id review.</param>
    /// <param name="request">User Request.</param>
    /// <returns>Review details.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoomReviewDetail), StatusCodes.Status201Created)]
    [Obsolete("It is deprecated, please use /api/room-reviews/upsert instead.")]
    public Task<RoomReviewDetail> Update([FromRoute] Guid id, [FromBody] RoomReviewUpdateRequest request)
    {
        return roomReviewService.UpdateAsync(id, request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// completion a review.
    /// </summary>
    /// <param name="request">User Request.</param>
    /// <param name="currentUserAccessor">Current user accessor</param>
    /// <returns>HTTP result</returns>
    [Authorize]
    [HttpPost("complete")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoomReviewDetail), StatusCodes.Status201Created)]
    public async Task<RoomCompleteResponse> CompleteAsync([FromBody] RoomReviewCompletionRequest request, [FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetUserIdOrThrow();

        return await roomReviewService.CompleteAsync(request, userId, HttpContext.RequestAborted);
    }
}
