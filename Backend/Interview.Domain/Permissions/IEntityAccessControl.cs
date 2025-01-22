using Interview.Domain.Repository;

namespace Interview.Domain.Permissions;

/// <summary>
/// Entity access control.
/// </summary>
public interface IEntityAccessControl
{
    Task EnsureEditPermissionAsync<T>(Guid entityId, CancellationToken cancellationToken)
        where T : Entity;
}
