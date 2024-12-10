using Interview.Domain.Repository;
using Interview.Domain.Users;
using Microsoft.Extensions.Internal;

namespace Interview.Domain.Database.Processors;

public class DateEntityPreProcessor(ICurrentUserAccessor currentUserAccessor, ISystemClock clock) : AbstractPreProcessor
{
    public bool TestEnv { get; init; }

    protected override void AddEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateCreateDate(clock.UtcNow.UtcDateTime);
        if (TestEnv)
        {
            entity.CreatedById ??= currentUserAccessor.UserId;
        }
        else
        {
            entity.CreatedById = currentUserAccessor.UserId;
        }
    }

    protected override void ModifyOriginalEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateUpdateDate(clock.UtcNow.UtcDateTime);
    }
}
