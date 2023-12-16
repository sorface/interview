using Interview.Domain.Reactions;
using Interview.Domain.RoomQuestionReactions.Records;
using Interview.Domain.RoomQuestionReactions.Records.Response;
using Interview.Domain.RoomQuestions;
using Interview.Domain.Users;

namespace Interview.Domain.RoomQuestionReactions.Services;

public class RoomQuestionReactionService : IRoomQuestionReactionService
{
    private readonly IRoomQuestionReactionRepository _roomQuestionReactionRepository;
    private readonly IRoomQuestionRepository _questionRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly IUserRepository _userRepository;

    public RoomQuestionReactionService(
        IRoomQuestionReactionRepository roomQuestionReactionRepository,
        IRoomQuestionRepository questionRepository,
        IReactionRepository reactionRepository,
        IUserRepository userRepository)
    {
        _roomQuestionReactionRepository = roomQuestionReactionRepository;
        _questionRepository = questionRepository;
        _reactionRepository = reactionRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Creates a reaction from the user to an active question in the room.
    /// </summary>
    /// <param name="request">Reaction data.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Token.</param>
    /// <returns>Result of operation.</returns>
    public async Task<RoomQuestionReactionDetail> CreateAsync(
        RoomQuestionReactionCreateRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(userId);
        }

        var roomQuestion = await _questionRepository.FindFirstByRoomAndStateAsync(
            request.RoomId,
            RoomQuestionState.Active,
            cancellationToken);

        if (roomQuestion is null)
        {
            throw new UserException($"Active question not found in room id {request.RoomId}");
        }

        var reaction = await _reactionRepository.FindByIdAsync(request.ReactionId, cancellationToken);

        if (reaction is null)
        {
            throw NotFoundException.Create<Reaction>(request.ReactionId);
        }

        var questionReaction = new RoomQuestionReaction
        {
            RoomQuestion = roomQuestion,
            Reaction = reaction,
            Sender = user,
            Payload = request.Payload,
        };

        await _roomQuestionReactionRepository.CreateAsync(questionReaction, cancellationToken);

        return new RoomQuestionReactionDetail
        {
            RoomId = request.RoomId,
            Question = roomQuestion.Question!.Id,
            Reaction = reaction.Id,
        };
    }
}
