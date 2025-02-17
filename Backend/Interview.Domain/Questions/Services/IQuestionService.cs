using Interview.Domain.Questions.QuestionTreePage;
using Interview.Domain.Questions.Records.FindPage;
using X.PagedList;

namespace Interview.Domain.Questions.Services;

public interface IQuestionService : IService
{
    public Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken);

    Task<IPagedList<QuestionTreePageResponse>> FindQuestionTreePageAsync(QuestionTreePageRequest request, CancellationToken cancellationToken);

    Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default);

    Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default);

    public Task<QuestionItem> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuestionItem> DeletePermanentlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuestionItem> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuestionItem> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
