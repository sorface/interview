using Interview.Domain.Events.ChangeEntityProcessors;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Microsoft.Extensions.Internal;

namespace Interview.Infrastructure.Database.Processors;

public class DateEntityPreProcessor : AbstractPreProcessor
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ISystemClock _clock;

    public DateEntityPreProcessor(ICurrentUserAccessor currentUserAccessor, ISystemClock clock)
    {
        _currentUserAccessor = currentUserAccessor;
        _clock = clock;
    }

    protected override void AddEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateCreateDate(_clock.UtcNow.UtcDateTime);
        entity.CreatedById = _currentUserAccessor.UserId;
    }

    protected override void ModifyOriginalEntityHandler(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdateUpdateDate(_clock.UtcNow.UtcDateTime);
    }
}
