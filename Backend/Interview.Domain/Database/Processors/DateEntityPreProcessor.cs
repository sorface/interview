using Interview.Domain.Repository;
using Interview.Domain.Users;
using Microsoft.Extensions.Internal;

namespace Interview.Domain.Database.Processors;

public class DateEntityPreProcessor : AbstractPreProcessor
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ISystemClock _clock;

    public bool TestEnv { get; init; }

    public DateEntityPreProcessor(ICurrentUserAccessor currentUserAccessor, ISystemClock clock)
    {
        _currentUserAccessor = currentUserAccessor;
        _clock = clock;
    }

    protected override void AddEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateCreateDate(_clock.UtcNow.UtcDateTime);
        if (TestEnv && entity.CreatedById is null)
        {
            entity.CreatedById = _currentUserAccessor.UserId;
        }
    }

    protected override void ModifyOriginalEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateUpdateDate(_clock.UtcNow.UtcDateTime);
    }
}
