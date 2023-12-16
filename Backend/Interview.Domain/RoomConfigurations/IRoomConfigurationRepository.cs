using Interview.Domain.Repository;

namespace Interview.Domain.RoomConfigurations;

public interface IRoomConfigurationRepository : IRepository<RoomConfiguration>
{
    Task UpsertCodeStateAsync(UpsertCodeStateRequest request, CancellationToken cancellationToken);
}
