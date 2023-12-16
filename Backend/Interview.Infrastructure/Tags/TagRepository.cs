using Interview.Domain.Tags;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Tags;

public class TagRepository : EfRepository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext db)
        : base(db)
    {
    }
}
