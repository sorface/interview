using Interview.Domain.Repository;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Service.Records.Response.Detail;
using Interview.Domain.Rooms.Service.Records.Response.Page;
using X.PagedList;

namespace Interview.Domain.Rooms;

public interface IRoomStateRepository : IRepository<RoomState>
{
}
