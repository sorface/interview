using Interview.Domain.Database;
using Interview.Domain.Events;

namespace Interview.Infrastructure.Events;

public class AppEventRepository(AppDbContext db) : EfRepository<AppEvent>(db), IAppEventRepository;
