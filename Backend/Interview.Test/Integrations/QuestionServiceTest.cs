using Bogus;
using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using Interview.Domain.Reactions;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Tags;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Interview.Test.Integrations;

public class QuestionServiceTest
{
    private const string DefaultQuestionValue = "TEST_QUESTION";

    public static IEnumerable<object[]> FindPageAsyncShouldNotReturnRoomQuestionsData
    {
        get
        {
            var factory = new FakerFactory();
            var roomFaker = factory.Room();
            var questionFaker = factory.Question();
            var faker = new Faker();
            foreach (var i in Enumerable.Range(0, 30))
            {
                var rooms = roomFaker.GenerateForever().Take(faker.Random.Number(1, 10)).ToList();
                var questions = questionFaker.GenerateForever()
                    .Take(faker.Random.Number(1, faker.Random.Number(1, 300))).ToList();

                foreach (var question in questions)
                {
                    question.Type = faker.Random.Bool() ? SEQuestionType.Private : SEQuestionType.Public;
                }

                yield return new object[] { rooms, questions, };
            }
        }
    }

    [Fact(DisplayName = "Searching question by id")]
    public async Task FindByIdSuccessful()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();

        var questionService = CreateQuestionService(appDbContext);
        var foundQuestion = await questionService.FindByIdAsync(question.Id);

        Assert.NotNull(foundQuestion);
        foundQuestion.Value.Should().BeEquivalentTo(question.Value);
    }

    [Fact(DisplayName = "Searching question by id when question not found")]
    public async Task FindByIdWhenQuestionNotExists()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var questionService = CreateQuestionService(appDbContext);

        var notFoundException =
            await Assert.ThrowsAsync<NotFoundException>(() => questionService.FindByIdAsync(Guid.NewGuid()));
        Assert.NotNull(notFoundException);
    }

    [MemberData(nameof(FindPageAsyncShouldNotReturnRoomQuestionsData))]
    [Theory(DisplayName = "Searching for questions should not return questions tied to the room")]
    public async Task FindPageAsyncShouldNotReturnRoomQuestions(List<Room> rooms, List<Question> questions)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        appDbContext.Rooms.AddRange(rooms);
        appDbContext.Questions.AddRange(questions);
        await appDbContext.SaveChangesAsync();
        var questionsWithoutRoomId = appDbContext.Questions
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.Type == SEQuestionType.Public)
            .Select(e => e.Id)
            .ToHashSet();
        var totalQuestionCount = await appDbContext.Questions
            .AsNoTracking()
            .IgnoreQueryFilters()
            .CountAsync();
        appDbContext.ChangeTracker.Clear();

        var roomMemberChecker = new Mock<IRoomMembershipChecker>();
        roomMemberChecker
            .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var questionService = CreateQuestionService(appDbContext, roomMemberChecker.Object);

        var findPageRequest = new FindPageRequest
        {
            Page = new PageRequest
            {
                PageNumber = 1,
                PageSize = totalQuestionCount,
            },
            Tags = null,
            Value = null,
        };
        var page = await questionService.FindPageAsync(findPageRequest, CancellationToken.None);

        page.TotalItemCount.Should().Be(questionsWithoutRoomId.Count);
        page.PageCount.Should().Be(questionsWithoutRoomId.Count > 0 ? 1 : 0);
        page.Should().Match(e => e.All(t => questionsWithoutRoomId.Contains(t.Id)), "The response should only consist of questions not tied to the room");
    }

    [Fact(DisplayName = "Permanent deleting the question")]
    public async Task DeletePermanentQuestion()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        var transaction = await appDbContext.Database.BeginTransactionAsync();

        var user = new User("nickname", "twitchChannel");
        appDbContext.Users.Add(user);
        appDbContext.SaveChanges();

        var room = new Room("room#1", SERoomAccessType.Public);
        appDbContext.Rooms.Add(room);
        appDbContext.SaveChanges();

        var question = new Question("question#1");
        appDbContext.Questions.Add(question);
        appDbContext.SaveChanges();

        var reaction = new Reaction { Type = ReactionType.Like };
        appDbContext.Reactions.Add(reaction);
        appDbContext.SaveChanges();

        var roomQuestion = new RoomQuestion { Room = room, Question = question, State = RoomQuestionState.Active, QuestionId = default, RoomId = default, };
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

        var questionService = CreateQuestionService(appDbContext);

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

    private static QuestionService CreateQuestionService(AppDbContext appDbContext, IRoomMembershipChecker? roomMembershipChecker = null)
    {
        var questionRepository = new QuestionRepository(appDbContext);
        var questionArchiveRepository = new QuestionNonArchiveRepository(appDbContext);
        var archiveService = new ArchiveService<Question>(appDbContext);
        var tagRepository = new TagRepository(appDbContext);
        var currentUser = new CurrentUserAccessor();
        currentUser.SetUser(appDbContext.Users.First());
        var aRoomMembershipChecker = roomMembershipChecker ?? new RoomMembershipChecker(currentUser, new RoomParticipantRepository(appDbContext));
        return new QuestionService(
            questionRepository,
            questionArchiveRepository,
            archiveService,
            tagRepository,
            aRoomMembershipChecker,
            currentUser);
    }
}
