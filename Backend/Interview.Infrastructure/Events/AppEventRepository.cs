using Interview.Domain.Database;
using Interview.Domain.Events;

namespace Interview.Infrastructure.Events;

public class AppEventRepository : EfRepository<AppEvent>, IAppEventRepository
{
    public AppEventRepository(AppDbContext db)
        : base(db)
    {
    }
}
