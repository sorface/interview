using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Infrastructure.Rooms;

namespace Interview.Test.Units.Rooms;

public class RoomRepositoryTest : AbstractRepositoryTest<Room, RoomRepository>
{
    protected override RoomRepository GetRepository(AppDbContext databaseSet)
    {
        return new RoomRepository(databaseSet);
    }

    protected override Room GetInstance()
    {
        return new Room("TEST_ROOM", SERoomAccessType.Public, SERoomType.Standard);
    }
}
