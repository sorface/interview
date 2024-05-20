using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Reactions;
using Interview.Infrastructure.Questions;

namespace Interview.Test.Units.Questions;

public class QuestionsRepositoryTest : AbstractRepositoryTest<Question, QuestionRepository>
{

    protected override QuestionRepository GetRepository(AppDbContext databaseSet)
    {
        return new QuestionRepository(databaseSet);
    }

    protected override Question GetInstance()
    {
        return new Question("TEST_QUESTION");
    }

}
