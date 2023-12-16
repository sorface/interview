using Interview.Domain.Questions;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Questions;

public class QuestionNonArchiveRepository : EfNonArchiveRepository<Question>, IQuestionNonArchiveRepository
{
    public QuestionNonArchiveRepository(AppDbContext db)
        : base(db)
    {
    }
}
