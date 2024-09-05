using System.Text.Json;
using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.Services;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomQuestions.AnswerDetail;
using Interview.Domain.Rooms.RoomQuestions.Services;
using Interview.Domain.Users;
using Microsoft.Extensions.Internal;
using Moq;

namespace Interview.Test.Integrations;

public class RoomQuestionServiceTest
{
    public static IEnumerable<object[]> GetAnswerDetails_With_Active_Question_Data
    {
        get
        {
            yield return new object[]
            {
                new List<(DateTime Start, DateTime End)>
                {
                    (new DateTime(2024, 1, 1, 10, 00, 00), new DateTime(2024, 1, 1, 14, 13, 9)),
                }
            };

            yield return new object[]
            {
                new List<(DateTime Start, DateTime End)>
                {
                    (new DateTime(2024, 1, 1, 10, 00, 00), new DateTime(2024, 1, 1, 14, 13, 9)),
                    (new DateTime(2024, 2, 13, 2, 3, 55), new DateTime(2024, 2, 23, 23, 8, 55)),
                }
            };

            yield return new object[]
            {
                new List<(DateTime Start, DateTime End)>
                {
                    (new DateTime(2024, 1, 1, 10, 00, 00), new DateTime(2024, 1, 1, 14, 13, 9)),
                    (new DateTime(2024, 2, 13, 2, 3, 55), new DateTime(2024, 2, 23, 23, 8, 55)),
                    (new DateTime(2024, 2, 23, 23, 8, 55), new DateTime(2024, 2, 24, 2, 0, 2)),
                }
            };
        }
    }

    [Fact]
    public async Task GetAnswerDetails_Unknown_Room()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, _, questionId) = CreateService(appDbContext, storage, testSystemClock);
        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = questionId,
            RoomId = Guid.NewGuid(),
        };
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAnswerDetailsAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAnswerDetails_Unknown_Question()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, roomId, _) = CreateService(appDbContext, storage, testSystemClock);
        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = Guid.NewGuid(),
            RoomId = roomId,
        };
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAnswerDetailsAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAnswerDetails_Without_Details_And_CodeEditor()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, roomId, questionId) = CreateService(appDbContext, storage, testSystemClock);
        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = questionId,
            RoomId = roomId,
        };
        var answerDetails = await service.GetAnswerDetailsAsync(request, CancellationToken.None);
        answerDetails.Details.Should().BeEmpty();
        answerDetails.CodeEditor.Should().BeNull();
    }

    [Fact]
    public async Task GetAnswerDetails_Without_Details()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, roomId, questionId) = CreateService(appDbContext, storage, testSystemClock);
        var q = appDbContext.Questions.First(e => e.Id == questionId);
        q.CodeEditor = new QuestionCodeEditor { Content = "test cont", Lang = "C#" };
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();

        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = questionId,
            RoomId = roomId,
        };
        var answerDetails = await service.GetAnswerDetailsAsync(request, CancellationToken.None);
        answerDetails.Details.Should().BeEmpty();
        answerDetails.CodeEditor.Should().NotBeNull()
            .And.BeEquivalentTo(new QuestionCodeEditorResponse
            {
                Content = "test cont",
                Lang = "C#",
            });
    }

    [Theory]
    [MemberData(nameof(GetAnswerDetails_With_Active_Question_Data))]
    public async Task GetAnswerDetails_With_Active_Question(List<(DateTime Start, DateTime End)> dates)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, roomId, questionId) = CreateService(appDbContext, storage, testSystemClock);

        foreach (var (start, end) in dates)
        {
            await storage.AddAsync(new StorageEvent
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Type = EventType.ChangeRoomQuestionState,
                Payload = JsonSerializer.Serialize(new RoomQuestionChangeEventPayload(questionId, RoomQuestionStateType.Open, RoomQuestionStateType.Active)),
                Stateful = false,
                CreatedAt = start,
                CreatedById = Guid.NewGuid(),
            }, CancellationToken.None);
            await storage.AddAsync(new StorageEvent
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Type = EventType.ChangeRoomQuestionState,
                Payload = JsonSerializer.Serialize(new RoomQuestionChangeEventPayload(questionId, RoomQuestionStateType.Active, RoomQuestionStateType.Closed)),
                Stateful = false,
                CreatedAt = end,
                CreatedById = Guid.NewGuid(),
            }, CancellationToken.None);
        }

        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = questionId,
            RoomId = roomId,
        };
        var answerDetails = await service.GetAnswerDetailsAsync(request, CancellationToken.None);
        var expectedDetails = dates.Select(e => new RoomQuestionAnswerDetailResponse.Detail
        {
            AnswerCodeEditorContent = null,
            Transcription = new List<QuestionDetailTranscriptionResponse>(),
            StartActiveDate = e.Start,
            EndActiveDate = e.End,
        }).ToList();
        answerDetails.Details.Should().HaveSameCount(expectedDetails).And.BeEquivalentTo(expectedDetails);
        answerDetails.CodeEditor.Should().BeNull();
    }

    private static (RoomQuestionService Service, Guid RoomId, Guid QuestionId) CreateService(AppDbContext db, IEventStorage storage, ISystemClock clock)
    {
        var user = new User("test user", "ID");
        db.Users.Add(user);
        var room = new Room("MY ROOM", SERoomAccessType.Private);
        db.Rooms.Add(room);

        var question = new Question("Test ");
        db.Questions.Add(question);
        db.SaveChanges();
        var roomQuestionId = Guid.NewGuid();
        db.RoomQuestions.Add(new RoomQuestion
        {
            Id = roomQuestionId,
            RoomId = room.Id,
            QuestionId = question.Id,
            Room = null,
            Question = null,
            State = RoomQuestionState.Open,
            Order = 0
        });
        db.SaveChanges();

        var roomParticipant = new RoomParticipant(user, room, SERoomParticipantType.Expert);
        db.RoomParticipants.Add(roomParticipant);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var userAccessor = new CurrentUserAccessor();
        userAccessor.SetUser(user);

        var service = new RoomQuestionService(
            new Mock<IRoomQuestionRepository>().Object,
            new Mock<IRoomRepository>().Object,
            new Mock<IQuestionRepository>().Object,
            new Mock<IQuestionService>().Object,
            db,
            storage,
            clock
        );

        return (service, room.Id, question.Id);
    }
}
