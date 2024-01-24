using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Questions;
using Interview.Domain.Questions.Services;
using Interview.Domain.Reactions;
using Interview.Domain.RoomQuestionReactions;
using Interview.Domain.RoomQuestions;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.Tags;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations;

public class QuestionServiceTest
{
    private const string DefaultQuestionValue = "TEST_QUESTION";

    [Fact(DisplayName = "Searching question by id")]
    public async Task FindByIdSuccessful()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue);

        appDbContext.Questions.Add(question);

        await appDbContext.SaveChangesAsync();

        var questionRepository = new QuestionRepository(appDbContext);
        var questionArchiveRepository = new QuestionNonArchiveRepository(appDbContext);
        var archiveService = new ArchiveService<Question>(questionRepository);
        var tagRepository = new TagRepository(appDbContext);
        var questionService = new QuestionService(questionRepository, questionArchiveRepository, archiveService, tagRepository);

        var foundQuestion = await questionService.FindByIdAsync(question.Id);

        Assert.NotNull(foundQuestion);

        foundQuestion.Value.Should().BeEquivalentTo(question.Value);
    }

    [Fact(DisplayName = "Searching question by id when question not found")]
    public async Task FindByIdWhenQuestionNotExists()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var questionRepository = new QuestionRepository(appDbContext);
        var questionArchiveRepository = new QuestionNonArchiveRepository(appDbContext);
        var archiveService = new ArchiveService<Question>(questionRepository);
        var tagRepository = new TagRepository(appDbContext);
        var questionService = new QuestionService(questionRepository, questionArchiveRepository, archiveService, tagRepository);

        var notFoundException =
            await Assert.ThrowsAsync<NotFoundException>(() => questionService.FindByIdAsync(Guid.NewGuid()));

        Assert.NotNull(notFoundException);
    }

    [Fact(DisplayName = "Permanent deleting the question")]
    public async Task DeletePermanentQuestion()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        var transaction = await appDbContext.Database.BeginTransactionAsync();

        var user = new User("nickname", "twitchChannel");
        appDbContext.Users.Add(user);
        appDbContext.SaveChanges();

        var room = new Room("room#1", "twitch", SeRoomAcсessType.Public);
        appDbContext.Rooms.Add(room);
        appDbContext.SaveChanges();

        var question = new Question("question#1");
        appDbContext.Questions.Add(question);
        appDbContext.SaveChanges();

        var reaction = new Reaction { Type = ReactionType.Like };
        appDbContext.Reactions.Add(reaction);
        appDbContext.SaveChanges();

        var roomQuestion = new RoomQuestion() { Room = room, Question = question, State = RoomQuestionState.Active };
        appDbContext.RoomQuestions.Add(roomQuestion);
        appDbContext.SaveChanges();

        var roomQuestionReaction = new RoomQuestionReaction
        {
            Reaction = reaction,
            RoomQuestion = roomQuestion,
            Sender = user
        };
        appDbContext.RoomQuestionReactions.Add(roomQuestionReaction);
        appDbContext.SaveChanges();

        await transaction.CommitAsync();

        var questionRepository = new QuestionRepository(appDbContext);
        var questionArchiveRepository = new QuestionNonArchiveRepository(appDbContext);
        var archiveService = new ArchiveService<Question>(questionRepository);
        var tagRepository = new TagRepository(appDbContext);
        var questionService = new QuestionService(questionRepository, questionArchiveRepository, archiveService, tagRepository);

        var result = await questionService.DeletePermanentlyAsync(question.Id);

        Assert.NotNull(result);

        var foundQuestion = await appDbContext.Questions
            .Where(it => it.Id == question.Id)
            .FirstOrDefaultAsync();

        Assert.Null(foundQuestion);

        var fountRoomQuestion = await appDbContext.RoomQuestions
            .Where(it => it.Id == roomQuestion.Id)
            .FirstOrDefaultAsync();

        Assert.Null(fountRoomQuestion);

        var fountRoomQuestionReaction = await appDbContext.RoomQuestionReactions
            .Where(it => it.Id == roomQuestion.Id)
            .FirstOrDefaultAsync();

        Assert.Null(fountRoomQuestionReaction);
    }
}
