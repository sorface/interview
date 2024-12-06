using Interview.Domain.Database;
using Interview.Domain.Tags;

namespace Interview.Infrastructure.Tags;

public class TagRepository(AppDbContext db) : EfRepository<Tag>(db), ITagRepository;
