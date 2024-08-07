using Bogus;
using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Categories;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.Tags;
using Moq;

namespace Interview.Test.Integrations;

public class QuestionCreatorTest
{
    public static IEnumerable<object?[]> CreateData
    {
        get
        {
            var faker = new Faker();
            var roomFaker = new FakerFactory().Room();
            foreach (var _ in Enumerable.Range(1, 15))
            {
                var room = faker.Random.Bool() ? null : roomFaker.Generate();
                var word = faker.Random.Word();
                if (word.Length < 3)
                {
                    word += new string('*', 3 - word.Length);
                }
                yield return new object?[] { word, room, };
            }
        }
    }

    [Theory]
    [MemberData(nameof(CreateData))]
    public async Task Create(string value, Room? room)
    {
        var category = new Category { Name = "Test" };
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        if (room is not null)
        {
            appDbContext.Rooms.Add(room);
            await appDbContext.SaveChangesAsync();
            appDbContext.ChangeTracker.Clear();
        }

        var roomMemberChecker = new Mock<IRoomMembershipChecker>();
        roomMemberChecker
            .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(room == null ? It.IsAny<Guid>() : room.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
        var questionCreateRequest = new QuestionCreateRequest
        {
            Tags = new HashSet<Guid>(),
            Value = value,
            Type = EVQuestionType.Private,
            CategoryId = category.Id,
            Answers = new List<QuestionAnswerCreateRequest>
            {
                new()
                {
                    Title = "#1",
                    Content = "test",
                    CodeEditor = false
                },
                new()
                {
                    Title = "#2",
                    Content = "test 2",
                    CodeEditor = false
                },
            },
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "my content",
                Lang = "C#"
            },
        };
        var question = await creator.CreateAsync(questionCreateRequest, room?.Id, CancellationToken.None);
        question.Should().NotBeNull().And.Match<QuestionItem>(e => e.Value == value);
        roomMemberChecker.Verify(e => e.EnsureCurrentUserMemberOfRoomAsync(room == null ? It.IsAny<Guid>() : room.Id, It.IsAny<CancellationToken>()), room is not null ? Times.Once() : Times.Never());
        question.Category.Should().NotBeNull();
        question.Category!.Id.Should().Be(category.Id);
        question.Answers.Should().HaveCount(2);
        question.Answers[0].Title.Should().Be("#1");
        question.Answers[0].Content.Should().Be("test");
        question.Answers[0].CodeEditor.Should().BeFalse();

        question.Answers[1].Title.Should().Be("#2");
        question.Answers[1].Content.Should().Be("test 2");
        question.Answers[1].CodeEditor.Should().BeFalse();

        question.CodeEditor.Should().NotBeNull();
        question.CodeEditor!.Content.Should().Be("my content");
        question.CodeEditor!.Lang.Should().Be("C#");
    }

    [Fact(DisplayName = "Creation should not succeed if the room is not available")]
    public async Task CreateShouldFailIfRoomIsNotAvailable()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var room = new Room("Test Room", SERoomAccessType.Public);
        appDbContext.Rooms.Add(room);
        var category = new Category { Name = "Test" };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomMemberChecker = new Mock<IRoomMembershipChecker>();
        roomMemberChecker
            .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(room.Id, It.IsAny<CancellationToken>()))
            .Throws<UnavailableException>();

        var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
        var questionCreateRequest = new QuestionCreateRequest
        {
            Tags = new HashSet<Guid>(),
            Value = "Test",
            Type = EVQuestionType.Private,
            CategoryId = category.Id,
            Answers = new List<QuestionAnswerCreateRequest>(),
            CodeEditor = null,
        };
        await Assert.ThrowsAsync<UnavailableException>(() => creator.CreateAsync(questionCreateRequest, room.Id, CancellationToken.None));
        roomMemberChecker.Verify(e => e.EnsureCurrentUserMemberOfRoomAsync(room.Id, It.IsAny<CancellationToken>()), Times.Once());
    }

    private static IQuestionService CreateQuestionCreate(AppDbContext appDbContext, IRoomMembershipChecker roomMembershipChecker)
    {
        var questionRepository = new QuestionRepository(appDbContext);
        var tagRepository = new TagRepository(appDbContext);
        var currentUser = new CurrentUserAccessor();
        currentUser.SetUser(appDbContext.Users.First());
        return new QuestionService(
            questionRepository,
            new QuestionNonArchiveRepository(appDbContext),
            new ArchiveService<Question>(appDbContext),
            tagRepository,
            roomMembershipChecker,
            currentUser,
            appDbContext);
    }

    private class UnavailableException : Exception
    {
    }
}
