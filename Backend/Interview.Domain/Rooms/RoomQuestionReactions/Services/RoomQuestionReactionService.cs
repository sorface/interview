using Interview.Domain.Reactions;
using Interview.Domain.Rooms.RoomQuestionReactions.Records;
using Interview.Domain.Rooms.RoomQuestionReactions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Services;

public class RoomQuestionReactionService(
    IRoomQuestionReactionRepository roomQuestionReactionRepository,
    IRoomQuestionRepository questionRepository,
    IReactionRepository reactionRepository,
    IUserRepository userRepository)
    : IRoomQuestionReactionService
{
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
        var user = await userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(userId);
        }

        var roomQuestion = await questionRepository.FindFirstByRoomAndStateAsync(
            request.RoomId,
            RoomQuestionState.Active,
            cancellationToken);

        if (roomQuestion is null)
        {
            throw new UserException($"Active question not found in room id {request.RoomId}");
        }

        var reaction = await reactionRepository.FindByIdAsync(request.ReactionId, cancellationToken);

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

        await roomQuestionReactionRepository.CreateAsync(questionReaction, cancellationToken);

        return new RoomQuestionReactionDetail
        {
            RoomId = request.RoomId,
            Question = roomQuestion.Question!.Id,
            Reaction = reaction.Id,
        };
    }
}
