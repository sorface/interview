using Interview.Domain.Database;
using Interview.Domain.Questions;

namespace Interview.Infrastructure.Questions;

public class QuestionNonArchiveRepository(AppDbContext db) : EfNonArchiveRepository<Question>(db), IQuestionNonArchiveRepository;
