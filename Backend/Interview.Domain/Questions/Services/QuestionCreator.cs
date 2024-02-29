using Interview.Domain.RoomParticipants;
using Interview.Domain.Tags;

namespace Interview.Domain.Questions.Services;

public interface IQuestionCreator : IService
{
    Task<Question> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default);
}

public class QuestionCreator : IQuestionCreator
{
    private readonly ITagRepository _tagRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRoomMembershipChecker _roomMembershipChecker;

    public QuestionCreator(ITagRepository tagRepository, IQuestionRepository questionRepository, IRoomMembershipChecker roomMembershipChecker)
    {
        _tagRepository = tagRepository;
        _questionRepository = questionRepository;
        _roomMembershipChecker = roomMembershipChecker;
    }

    public async Task<Question> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default)
    {
        if (roomId is not null)
        {
            await _roomMembershipChecker.EnsureCurrentUserMemberOfRoomAsync(roomId.Value, cancellationToken);
        }

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);
        var result = new Question(request.Value)
        {
            Tags = tags,
            RoomId = roomId,
        };

        await _questionRepository.CreateAsync(result, cancellationToken);
        return result;
    }
}
