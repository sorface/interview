using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Bogus;
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
            foreach (var item in GenerateAllCases(
                         new DateTime(2024, 1, 1, 10, 00, 00),
                         new DateTime(2024, 1, 1, 14, 13, 9)))
            {
                yield return new object[]
                {
                    new List<GenerateData>
                    {
                        item,
                    }
                };
            }

            var i1 = GenerateAllCases(new DateTime(2024, 1, 1, 10, 00, 00), new DateTime(2024, 1, 1, 14, 13, 9)).ToList();
            var i2 = GenerateAllCases(new DateTime(2024, 2, 13, 2, 3, 55), new DateTime(2024, 2, 23, 23, 8, 55)).ToList();
            foreach (var generateDatase in i1.SelectMany(_ => i2, (data, generateData) => new List<GenerateData> { data, generateData }))
            {
                yield return new object[]
                {
                    generateDatase
                };
            }

            var i3 = GenerateAllCases(new DateTime(2024, 2, 23, 23, 8, 55), new DateTime(2024, 2, 24, 2, 0, 2)).ToList();
            foreach (var generateDatase in i1
                         .SelectMany(_ => i2, (data, generateData) => (Item1: data, Item2: generateData))
                         .SelectMany(_ => i3, (list, data) => new List<GenerateData>
                         {
                             list.Item1,
                             list.Item2,
                             data
                         }))
            {
                yield return new object[]
                {
                    generateDatase
                };
            }

            static IEnumerable<GenerateData> GenerateAllCases(DateTime start, DateTime end)
            {
                foreach (var (generateTranscription, generateCodeEditor) in new[]
                         {
                             (false, false),
                             (true, false),
                             (false, true),
                             (true, true),
                         })
                {
                    yield return new GenerateData(start, end, generateTranscription, generateCodeEditor);
                }
            }
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
    public async Task GetAnswerDetails_With_Active_Question(List<GenerateData> dates)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var storage = new InMemoryEventStorage();
        var (service, roomId, questionId) = CreateService(appDbContext, storage, testSystemClock);

        var op = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
        var expectedDetails = new List<RoomQuestionAnswerDetailResponse.Detail>();
        foreach (var (start, end, generateTranscription, generateCodeEditor) in dates)
        {
            await storage.AddAsync(new StorageEvent
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Type = EventType.ChangeRoomQuestionState,
                Payload = JsonSerializer.Serialize(new RoomQuestionChangeEventPayload(questionId, RoomQuestionStateType.Open, RoomQuestionStateType.Active), op),
                Stateful = false,
                CreatedAt = start,
                CreatedById = Guid.NewGuid(),
            }, CancellationToken.None);
            await storage.AddAsync(new StorageEvent
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Type = EventType.ChangeRoomQuestionState,
                Payload = JsonSerializer.Serialize(new RoomQuestionChangeEventPayload(questionId, RoomQuestionStateType.Active, RoomQuestionStateType.Closed), op),
                Stateful = false,
                CreatedAt = end,
                CreatedById = Guid.NewGuid(),
            }, CancellationToken.None);

            string? answerCodeEditorContent = null;
            if (generateCodeEditor)
            {
                var faker = new Faker<StorageEvent>()
                    .RuleFor(e => e.Id, f => f.Random.Guid())
                    .RuleFor(e => e.RoomId, roomId)
                    .RuleFor(e => e.Type, EventType.CodeEditorChange)
                    .RuleFor(e => e.Payload, f => f.Random.Words().OrDefault(f, 0.2f, string.Empty))
                    .RuleFor(e => e.CreatedAt, f => f.Date.Between(start, end))
                    .RuleFor(e => e.CreatedById, f => f.Random.Guid());

                var codeEditorEvents = faker.GenerateForever().Take(Random.Shared.Next(1, 11)).ToList();
                foreach (var codeEditorEvent in codeEditorEvents)
                {
                    await storage.AddAsync(codeEditorEvent, CancellationToken.None);
                }
                answerCodeEditorContent = codeEditorEvents.MaxBy(e => e.CreatedAt)?.Payload;
            }

            var transcriptions = new List<QuestionDetailTranscriptionResponse>();
            if (generateTranscription)
            {
                var faker = new Faker<TranscriptionFakeData>()
                    .RuleFor(e => e.Id, f => f.Random.Guid())
                    .RuleFor(e => e.Message, f => f.Random.Words().OrDefault(f, 0.2f, string.Empty))
                    .RuleFor(e => e.Nickname, f => f.Person.FullName)
                    .RuleFor(e => e.CreatedAt, f => f.Date.Between(start, end))
                    .RuleFor(e => e.CreatedById, f => f.Random.Guid());

                var voiceRecognitionEvents = faker.GenerateForever().Take(Random.Shared.Next(1, 11)).ToList();

                foreach (var voiceRecognitionEvent in voiceRecognitionEvents)
                {
                    var @event = new StorageEvent
                    {
                        Id = voiceRecognitionEvent.Id,
                        RoomId = roomId,
                        Type = EventType.VoiceRecognition,
                        Payload = JsonSerializer.Serialize(new { Message = voiceRecognitionEvent.Message, Nickname = voiceRecognitionEvent.Nickname }, op),
                        Stateful = false,
                        CreatedAt = voiceRecognitionEvent.CreatedAt,
                        CreatedById = voiceRecognitionEvent.CreatedById,
                    };
                    await storage.AddAsync(@event, CancellationToken.None);
                }
                foreach (var voiceRecognitionEvent in voiceRecognitionEvents)
                {
                    transcriptions.Add(new QuestionDetailTranscriptionResponse
                    {
                        Id = voiceRecognitionEvent.Id,
                        Payload = voiceRecognitionEvent.Message,
                        CreatedAt = voiceRecognitionEvent.CreatedAt,
                        User = new QuestionDetailTranscriptionUserResponse
                        {
                            Id = voiceRecognitionEvent.CreatedById,
                            Nickname = voiceRecognitionEvent.Nickname,
                        }
                    });
                }
            }

            expectedDetails.Add(new RoomQuestionAnswerDetailResponse.Detail
            {
                AnswerCodeEditorContent = answerCodeEditorContent,
                Transcription = transcriptions,
                StartActiveDate = start,
                EndActiveDate = end,
            });
        }

        var request = new RoomQuestionAnswerDetailRequest
        {
            QuestionId = questionId,
            RoomId = roomId,
        };
        var answerDetails = await service.GetAnswerDetailsAsync(request, CancellationToken.None);
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

    private class TranscriptionFakeData
    {
        public required Guid Id { get; set; }
        public required Guid CreatedById { get; set; }
        public required string Message { get; set; }
        public required string Nickname { get; set; }
        public required DateTime CreatedAt { get; set; }
    }

    public record GenerateData(DateTime Start, DateTime End, bool GenerateTranscription, bool GenerateCodeEditor);
}
