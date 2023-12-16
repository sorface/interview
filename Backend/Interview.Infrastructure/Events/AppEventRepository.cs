using Interview.Domain.Events;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Events;

public class AppEventRepository : EfRepository<AppEvent>, IAppEventRepository
{
    public AppEventRepository(AppDbContext db)
        : base(db)
    {
    }
}
