using Interview.Backend.Common;
using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Users;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    public Task<IPagedList<UserDetail>> FindPage([FromQuery] PageRequest request)
    {
        return userService.FindPageAsync(request.PageNumber, request.PageSize, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("nickname/{nickname}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedListResponse<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<UserDetail> FindByNickname([FromRoute] string nickname)
    {
        return userService.FindByNicknameAsync(nickname, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("role/{role}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<IPagedList<UserDetail>> FindByRole([FromQuery] PageRequest pageRequest, [FromRoute] RoleNameType role)
    {
        return userService.FindByRoleAsync(pageRequest.PageNumber, pageRequest.PageSize, role, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("admins")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<UserDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<IPagedList<UserDetail>> FindAdmins([FromQuery] PageRequest pageRequest)
    {
        return userService.FindByRoleAsync(pageRequest.PageNumber, pageRequest.PageSize, RoleNameType.Admin, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpGet("self")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    public Task<UserDetail> GetMyself()
    {
        return userService.GetSelfAsync();
    }

    [Authorize]
    [HttpGet("{id:guid}/permissions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Dictionary<string, List<PermissionItem>>), StatusCodes.Status200OK)]
    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissions(Guid id)
    {
        return await userService.GetPermissionsAsync(id, HttpContext.RequestAborted);
    }

    [Authorize]
    [HttpPut("{id:guid}/permissions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PermissionItem), StatusCodes.Status200OK)]
    public async Task<PermissionItem> GetPermissions(Guid id, [FromBody] PermissionModifyRequest request)
    {
        return await userService.ChangePermissionAsync(id, request, HttpContext.RequestAborted);
    }
}
