using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomConfigurations;

public interface IRoomConfigurationRepository : IRepository<RoomConfiguration>
{
    Task UpsertCodeStateAsync(UpsertCodeStateRequest request, CancellationToken cancellationToken);
}
