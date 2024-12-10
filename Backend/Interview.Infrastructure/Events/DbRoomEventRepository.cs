using Interview.Domain.Database;
using Interview.Domain.Events;

namespace Interview.Infrastructure.Events;

public class DbRoomEventRepository(AppDbContext db) : EfRepository<DbRoomEvent>(db), IDbRoomEventRepository;
