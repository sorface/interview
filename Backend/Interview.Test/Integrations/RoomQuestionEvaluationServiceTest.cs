using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Services;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestions;

namespace Interview.Test.Integrations;

public class RoomQuestionEvaluationServiceTest
{
    public static IEnumerable<object[]> GetUserRoomQuestionEvaluations_Should_Return_Review_Data
    {
        get
        {
            yield return
            [
                Array.Empty<RoomQuestionTest>(),
                Array.Empty<RoomQuestionEvaluationResponse>()
            ];
            yield return
            [
                new RoomQuestionTest[]
                {
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                            State = EVRoomQuestionEvaluationState.Draft,
                        },
                    },
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                            State = EVRoomQuestionEvaluationState.Draft,
                        },
                    },
                },
                new RoomQuestionEvaluationResponse[]
                {
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                        },
                    },
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                        },
                    },
                }
            ];
            yield return
            [
                new RoomQuestionTest[]
                {
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                            State = EVRoomQuestionEvaluationState.Draft,
                        },
                    },
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                            State = EVRoomQuestionEvaluationState.Submitted,
                        },
                    },
                },
                new RoomQuestionEvaluationResponse[]
                {
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                        },
                    },
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                        },
                    }
                }
            ];
            yield return
            [
                new RoomQuestionTest[]
                {
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                            State = EVRoomQuestionEvaluationState.Submitted,
                        },
                    },
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                            State = EVRoomQuestionEvaluationState.Submitted,
                        },
                    },
                },
                new RoomQuestionEvaluationResponse[]
                {
                    new()
                    {
                        Id = new Guid("D2E2260D-8FF6-47C7-B6AA-14D519315DB8"),
                        Value = "r 2",
                        Order = 1,
                        Evaluation = new()
                        {
                            Id = new Guid("6B642B9B-FC99-47BF-820A-946F748871DE"),
                            Mark = 2,
                            Review = "Test 2",
                        },
                    },
                    new()
                    {
                        Id = new Guid("846FB307-BB66-4642-94AC-93C1453F8597"),
                        Value = "r 1",
                        Order = 2,
                        Evaluation = new()
                        {
                            Id = new Guid("6D9BE983-DC87-4D4B-94F6-292DF8AAEB98"),
                            Mark = 10,
                            Review = "Test",
                        },
                    },
                }
            ];
        }
    }

    [Fact]
    public async Task GetUserRoomQuestionEvaluations_Should_Throw_Access_Denied()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, false);

        var request = new UserRoomQuestionEvaluationsRequest { UserId = userId, RoomId = roomId, };

        await Assert.ThrowsAsync<AccessDeniedException>(() => service.GetUserRoomQuestionEvaluationsAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetUserRoomQuestionEvaluations_Should_Return_Null()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, true);

        var request = new UserRoomQuestionEvaluationsRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomQuestionEvaluationsAsync(request, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(GetUserRoomQuestionEvaluations_Should_Return_Review_Data))]
    public async Task GetUserRoomQuestionEvaluations_Should_Return_Review(RoomQuestionTest[] initialData, RoomQuestionEvaluationResponse[] expectedData)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, true);
        var dbData = initialData
            .Select(e =>
            {
                var roomQuestionId = Guid.NewGuid();
                return new
                {
                    Question = new Question(e.Value) { Id = e.Id, },
                    RoomQuestion = new RoomQuestion
                    {
                        Id = roomQuestionId,
                        RoomId = roomId,
                        QuestionId = e.Id,
                        Room = null,
                        Question = null,
                        State = RoomQuestionState.Closed,
                        Order = e.Order
                    },
                    Evaluation = e.Evaluation is null
                        ? (RoomQuestionEvaluation?)null
                        : new RoomQuestionEvaluation
                        {
                            Id = e.Evaluation.Id,
                            RoomQuestionId = roomQuestionId,
                            Mark = e.Evaluation.Mark,
                            Review = e.Evaluation.Review,
                            CreatedById = userId,
                            State = SERoomQuestionEvaluationState.FromValue((int)e.Evaluation.State)
                        },
                };
            })
            .ToList();
        await appDbContext.Questions.AddRangeAsync(dbData.Select(e => e.Question));
        await appDbContext.RoomQuestions.AddRangeAsync(dbData.Select(e => e.RoomQuestion));
        await appDbContext.RoomQuestionEvaluation.AddRangeAsync(dbData.Select(e => e.Evaluation).Where(e => e is not null)!);
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();

        var request = new UserRoomQuestionEvaluationsRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomQuestionEvaluationsAsync(request, CancellationToken.None);
        result.Should().NotBeNull();
        result.Should().HaveSameCount(expectedData).And.BeEquivalentTo(expectedData);
    }

    private static (RoomQuestionEvaluationService Service, Guid UserId, Guid RoomId) CreateService(AppDbContext db, bool addParticipant)
    {
        var user = new User("test user", "ID");
        db.Users.Add(user);
        var room = new Room("MY ROOM", SERoomAccessType.Private);
        db.Rooms.Add(room);

        foreach (var i in Enumerable.Range(0, Random.Shared.Next(1, 5)))
        {
            var testRoom = new Room("TEST ROOM", SERoomAccessType.Private);
            db.Rooms.Add(testRoom);
            var question = new Question("Test " + i);
            db.Questions.Add(question);
            db.SaveChanges();
            var roomQuestionId = Guid.NewGuid();
            db.RoomQuestions.Add(new RoomQuestion
            {
                Id = roomQuestionId,
                RoomId = testRoom.Id,
                QuestionId = question.Id,
                Room = null,
                Question = null,
                State = RoomQuestionState.Closed,
                Order = 0
            });
            db.SaveChanges();
            db.RoomQuestionEvaluation.Add(new RoomQuestionEvaluation
            {
                RoomQuestionId = roomQuestionId,
                State = SERoomQuestionEvaluationState.Submitted,
                CreatedById = user.Id,
            });
            db.SaveChanges();
        }
        db.SaveChanges();

        if (addParticipant)
        {
            var roomParticipant = new RoomParticipant(user, room, SERoomParticipantType.Expert);
            db.RoomParticipants.Add(roomParticipant);
            db.SaveChanges();
        }
        db.ChangeTracker.Clear();

        var userAccessor = new CurrentUserAccessor();
        userAccessor.SetUser(user);

        var service = new RoomQuestionEvaluationService(
            new RoomQuestionRepository(db),
            new RoomParticipantRepository(db),
            db,
            new RoomMembershipChecker(userAccessor, new RoomParticipantRepository(db)));

        return (service, user.Id, room.Id);
    }

    public class QuestionEvaluationDetail
    {
        public required Guid Id { get; init; }

        public required int? Mark { get; set; }

        public required string? Review { get; set; }

        public required EVRoomQuestionEvaluationState State { get; set; }
    }

    public class RoomQuestionTest
    {
        public required Guid Id { get; set; }

        public required string Value { get; set; }

        public required int Order { get; set; }

        public required QuestionEvaluationDetail? Evaluation { get; set; }
    }
}
