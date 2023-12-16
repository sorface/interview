using Interview.Domain.Repository;

namespace Interview.Domain.Questions;

public interface IQuestionRepository : IRepository<Question>
{
    /// <summary>
    /// Deleting a question from the database and related records from other tables with this question.
    /// </summary>
    /// <param name="entity">Question entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Empty task.</returns>
    public Task DeletePermanentlyAsync(Question entity, CancellationToken cancellationToken = default);
}
