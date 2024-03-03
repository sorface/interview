using Interview.Domain.Database;
using Interview.Domain.Questions;

namespace Interview.Infrastructure.Questions;

public class QuestionNonArchiveRepository : EfNonArchiveRepository<Question>, IQuestionNonArchiveRepository
{
    public QuestionNonArchiveRepository(AppDbContext db)
        : base(db)
    {
    }
}
