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
using Interview.Domain.Reactions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomParticipants;
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Add_Answer(bool codeEditor)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue);
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "t",
                Lang = "ts"
            },
            Answers = new List<QuestionAnswerEditRequest>
            {
                new()
                {
                    Content = "test content",
                    CodeEditor = codeEditor,
                    Title = "test title",
                    Id = null
                }
            },
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.Answers.Should().HaveCount(1);
        dbQuestion.Answers[0].Content.Should().Be("test content");
        dbQuestion.Answers[0].Title.Should().Be("test title");
        dbQuestion.Answers[0].CodeEditor.Should().Be(codeEditor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_Answer(bool codeEditor)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    Title = "test title",
                    Content = "test content",
                    CodeEditor = codeEditor,
                    QuestionId = default,
                    Question = null
                }
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "t",
                Lang = "ts"
            },
            Answers = new List<QuestionAnswerEditRequest>
            {
                new()
                {
                    Id = question.Answers[0].Id,
                    Title = "test title 1",
                    Content = "test content 1",
                    CodeEditor = !question.Answers[0].CodeEditor
                }
            }
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.Answers.Should().HaveCount(1);
        dbQuestion.Answers[0].Content.Should().Be("test content 1");
        dbQuestion.Answers[0].Title.Should().Be("test title 1");
        dbQuestion.Answers[0].CodeEditor.Should().Be(!codeEditor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_And_Add_Answer(bool codeEditor)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    Title = "test title",
                    Content = "test content",
                    CodeEditor = codeEditor,
                    QuestionId = default,
                    Question = null
                }
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "t",
                Lang = "ts"
            },
            Answers = new List<QuestionAnswerEditRequest>
            {
                new()
                {
                    Content = "new test content",
                    CodeEditor = codeEditor,
                    Title = "new test title",
                    Id = null
                },
                new()
                {
                    Id = question.Answers[0].Id,
                    Title = "test title 1",
                    Content = "test content 1",
                    CodeEditor = !question.Answers[0].CodeEditor
                }
            },
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.Answers.Should().HaveCount(2);
        var updatedAnswer = dbQuestion.Answers.First(e => e.Id == question.Answers[0].Id);
        updatedAnswer.Content.Should().Be("test content 1");
        updatedAnswer.Title.Should().Be("test title 1");
        updatedAnswer.CodeEditor.Should().Be(!codeEditor);

        var newAnswer = dbQuestion.Answers.First(e => e.Id != question.Answers[0].Id);
        newAnswer.Content.Should().Be("new test content");
        newAnswer.Title.Should().Be("new test title");
        newAnswer.CodeEditor.Should().Be(codeEditor);
    }

    [Fact]
    public async Task Remove_Answer()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    Title = "test title",
                    Content = "test content",
                    CodeEditor = true,
                    QuestionId = default,
                    Question = null
                }
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = null,
            Answers = null
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().BeNull();
        dbQuestion.Answers.Should().HaveCount(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Remove_And_Add_Answer(bool codeEditor)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    Title = "test title",
                    Content = "test content",
                    CodeEditor = true,
                    QuestionId = default,
                    Question = null
                }
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "t",
                Lang = "ts"
            },
            Answers = new List<QuestionAnswerEditRequest>
            {
                new()
                {
                    Content = "new test content",
                    CodeEditor = codeEditor,
                    Title = "new test title",
                    Id = null
                }
            },
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.Answers.Should().HaveCount(1);
        dbQuestion.Answers[0].Content.Should().Be("new test content");
        dbQuestion.Answers[0].Title.Should().Be("new test title");
        dbQuestion.Answers[0].CodeEditor.Should().Be(codeEditor);

    }

    [Fact]
    public async Task Update_Editor()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            CodeEditor = new QuestionCodeEditor
            {
                Content = "test",
                Lang = "js",
                Source = EVRoomCodeEditorChangeSource.User,
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "new2",
                Lang = "ts"
            },
            Answers = null
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.CodeEditor!.Content.Should().Be("new2");
        dbQuestion.CodeEditor!.Lang.Should().Be("ts");
        dbQuestion.Answers.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_Editor_To_Null()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue)
        {
            CodeEditor = new QuestionCodeEditor
            {
                Content = "test",
                Lang = "js",
                Source = EVRoomCodeEditorChangeSource.User,
            }
        };
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = null,
            Answers = null
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().BeNull();
        dbQuestion.Answers.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_Null_Editor_To_Not_Null()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question(value: DefaultQuestionValue);
        var category = new Category { Name = "Test", };
        appDbContext.Categories.Add(category);
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var questionService = CreateQuestionService(appDbContext);
        var questionEditRequest = new QuestionEditRequest
        {
            Value = DefaultQuestionValue,
            CategoryId = category.Id,
            CodeEditor = new QuestionCodeEditorEditRequest
            {
                Content = "test",
                Lang = "js"
            },
            Answers = null
        };
        var updatedQuestion = await questionService.UpdateAsync(question.Id, questionEditRequest);
        var dbQuestion = await appDbContext.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .FirstAsync(e => e.Id == updatedQuestion.Id);

        Assert.NotNull(updatedQuestion);
        Assert.NotNull(dbQuestion);
        dbQuestion.Value.Should().BeEquivalentTo(question.Value);
        dbQuestion.CodeEditor.Should().NotBeNull();
        dbQuestion.CodeEditor!.Content.Should().Be("test");
        dbQuestion.CodeEditor!.Lang.Should().Be("js");
        dbQuestion.Answers.Should().BeEmpty();
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

        await using var transaction = await appDbContext.Database.BeginTransactionAsync();

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

        var roomQuestion = new RoomQuestion { Room = room, Question = question, State = RoomQuestionState.Active, QuestionId = default, RoomId = default, Order = 0, };
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
            currentUser,
            appDbContext);
    }
}
