using Interview.Backend.Auth;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Users.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Users;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<UserDetail>> FindPage([FromQuery] PageRequest request)
    {
        return _userService.FindPageAsync(request.PageNumber, request.PageSize, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("nickname/{nickname}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<UserDetail> FindByNickname([FromRoute] string nickname)
    {
        return _userService.FindByNicknameAsync(nickname, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("role/{role}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<UserDetail>> FindByRole([FromQuery] PageRequest pageRequest, [FromRoute] RoleNameType role)
    {
        return _userService.FindByRoleAsync(pageRequest.PageNumber, pageRequest.PageSize, role, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("admins")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IPagedList<UserDetail>> FindAdmins([FromQuery] PageRequest pageRequest)
    {
        return _userService.FindByRoleAsync(pageRequest.PageNumber, pageRequest.PageSize, RoleNameType.Admin, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("self")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<UserDetail> GetMyself()
    {
        return _userService.GetSelfAsync();
    }

    [Authorize]
    [HttpGet("{id:guid}/permissions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Dictionary<string, List<PermissionItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissions(Guid id)
    {
        return await _userService.GetPermissionsAsync(id, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpPut("{id:guid}/permissions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PermissionItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<PermissionItem> GetPermissions(Guid id, [FromBody] PermissionModifyRequest request)
    {
        return await _userService.ChangePermissionAsync(id, request, HttpContext.RequestAborted);
    }
}
