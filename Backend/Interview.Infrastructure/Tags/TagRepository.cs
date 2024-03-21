using Interview.Domain.Database;
using Interview.Domain.Tags;

namespace Interview.Infrastructure.Tags;

public class TagRepository : EfRepository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext db)
        : base(db)
    {
    }
}
