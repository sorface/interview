using Interview.Domain.Database;
using Interview.Domain.Questions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Questions;

public class QuestionRepository(AppDbContext db) : EfRepository<Question>(db), IQuestionRepository
{
    public Task DeletePermanentlyAsync(Question entity, CancellationToken cancellationToken = default)
    {
        return Db.RunTransactionAsync(async _ =>
        {
            await Db.RoomQuestionReactions
                .Where(roomQuestionReaction => roomQuestionReaction.RoomQuestion!.Question!.Id == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await Db.RoomQuestions
                .Where(roomQuestion => roomQuestion.Question!.Id == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await Db.Questions
                .Where(question => question.Id == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);
            return DBNull.Value;
        },
        cancellationToken);
    }

    protected override IQueryable<Question> ApplyIncludes(DbSet<Question> set)
    {
        return set
            .Include(e => e.Tags)
            .Include(e => e.Category)
            .Include(e => e.CodeEditor)
            .Include(e => e.Answers)
            .Include(e => e.CreatedBy);
    }
}
