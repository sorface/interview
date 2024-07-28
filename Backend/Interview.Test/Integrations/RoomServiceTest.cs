using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Events.Storage;
using Interview.Domain.Invites;
using Interview.Domain.Questions;
using Interview.Domain.Reactions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Interview.Infrastructure.Events;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestionReactions;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Tags;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations;

public class RoomServiceTest
{
    private const string DefaultRoomName = "Test_Room";

    [Fact(DisplayName = "Patch update room with request name not null")]
    public async Task PatchUpdateRoomWithRequestNameIsNotNull()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var savedRoom = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(savedRoom);

        await appDbContext.SaveChangesAsync();

        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "New_Value_Name_Room",
            Questions = new List<RoomQuestionRequest>(),
        };

        _ = await roomService.UpdateAsync(savedRoom.Id, roomPatchUpdateRequest);

        var foundedRoom = await roomRepository.FindByIdAsync(savedRoom.Id);

        foundedRoom.Should().NotBeNull();
        foundedRoom!.Name.Should().BeEquivalentTo(roomPatchUpdateRequest.Name);
    }

    [Fact(DisplayName = "Patch update room with request name not null and add category")]
    public async Task PatchUpdateRoomWithRequestNameIsNotNull_And_Add_Category()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var savedRoom = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(savedRoom);

        await appDbContext.SaveChangesAsync();

        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "New_Value_Name_Room",
            Questions = new List<RoomQuestionRequest>(),
        };

        _ = await roomService.UpdateAsync(savedRoom.Id, roomPatchUpdateRequest);

        var foundedRoom = await roomRepository.FindByIdAsync(savedRoom.Id);

        foundedRoom.Should().NotBeNull();
        foundedRoom!.Name.Should().BeEquivalentTo(roomPatchUpdateRequest.Name);
    }

    [Fact(DisplayName = "Close room should correctly close active room")]
    public async Task CloseActiveRoom()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var savedRoom = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(savedRoom);
        var questions = new[] { new Question("V1"), new Question("V2"), new Question("V3") };
        appDbContext.Questions.AddRange(questions);
        var activeRoomQuestion = new RoomQuestion
        {
            Room = savedRoom,
            State = RoomQuestionState.Active,
            Question = questions[2],
            QuestionId = default,
            RoomId = default,
            Order = 0,
        };
        appDbContext.RoomQuestions.AddRange(
            new RoomQuestion { Room = savedRoom, State = RoomQuestionState.Open, Question = questions[0], QuestionId = default, RoomId = default, Order = 0 },
            new RoomQuestion { Room = savedRoom, State = RoomQuestionState.Closed, Question = questions[1], QuestionId = default, RoomId = default, Order = 0 },
            activeRoomQuestion);

        await appDbContext.SaveChangesAsync();
        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        await roomService.CloseAsync(savedRoom.Id);

        var foundedRoom = await roomRepository.FindByIdAsync(savedRoom.Id);
        foundedRoom!.Status.Should().BeEquivalentTo(SERoomStatus.Close);

        var activeQuestions = appDbContext.RoomQuestions.Count(e =>
            e.Room!.Id == savedRoom.Id &&
            e.State == RoomQuestionState.Active);
        activeQuestions.Should().Be(0);
    }

    [Fact(DisplayName = "GetAnalytics should return valid analytics by roomId")]
    public async Task GetAnalytics_Should_Return_Valid_Analytics_By_RoomId()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var room1 = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(room1);

        var dummyUser = new User("dummy", "dummy");
        appDbContext.Users.Add(dummyUser);
        appDbContext.SaveChanges();
        var dummyRoom = new Room("test room", SERoomAccessType.Public)
        {
            Questions = new List<RoomQuestion>
            {
                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q"),
                    State = RoomQuestionState.Open,
                    Order = 0,
                    Evaluations = new List<RoomQuestionEvaluation>
                    {
                        new()
                        {
                            RoomQuestionId = default,
                            CreatedBy = dummyUser,
                            Review = "dummy 2",
                            Mark = 2
                        },
                    }
                },
                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q 2"),
                    State = RoomQuestionState.Active,
                    Order = 0,
                    Evaluations = new List<RoomQuestionEvaluation>
                    {
                        new()
                        {
                            RoomQuestionId = default,
                            CreatedBy = dummyUser,
                            Review = "dummy 1",
                            Mark = 1
                        },
                    }
                },
            },
        };
        dummyRoom.Participants.Add(new RoomParticipant(dummyUser, dummyRoom, SERoomParticipantType.Expert));

        appDbContext.Rooms.Add(dummyRoom);
        appDbContext.Rooms.Add(new Room(DefaultRoomName + "2", SERoomAccessType.Public));

        var questions = new Question[]
        {
            new("V1") { Id = Guid.Parse("527A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V2") { Id = Guid.Parse("537A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V3") { Id = Guid.Parse("547A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V4") { Id = Guid.Parse("557A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V5") { Id = Guid.Parse("567A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V6") { Id = Guid.Parse("577A0279-4364-4940-BE4E-8DBEC08BA96C") }
        };
        appDbContext.Questions.AddRange(questions);

        var users = new User[]
        {
            new("u1", "v1")
            {
                Id = Guid.Parse("587A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u2", "v2")
            {
                Id = Guid.Parse("597A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.Admin.Id)! }
            },
            new("u3", "v3")
            {
                Id = Guid.Parse("5A7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u4", "v4")
            {
                Id = Guid.Parse("5B7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u5", "v5")
            {
                Id = Guid.Parse("5C7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
        };
        appDbContext.Users.AddRange(users);
        await appDbContext.SaveChangesAsync();

        var roomQuestion = new RoomQuestion[]
        {
            new()
            {
                Id = Guid.Parse("B15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[0],
                Room = room1,
                State = RoomQuestionState.Open,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B25AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[1],
                Room = room1,
                State = RoomQuestionState.Closed,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B35AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[2],
                Room = room1,
                State = RoomQuestionState.Closed,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B45AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[3],
                Room = room1,
                State = RoomQuestionState.Active,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
        };
        appDbContext.RoomQuestions.AddRange(roomQuestion);

        var roomParticipants = new RoomParticipant[]
        {
            new(users[0], room1, SERoomParticipantType.Examinee)
            {
                Id = Guid.Parse("C15AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[1], room1, SERoomParticipantType.Expert)
            {
                Id = Guid.Parse("C25AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[2], room1, SERoomParticipantType.Viewer)
            {
                Id = Guid.Parse("C35AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[3], room1, SERoomParticipantType.Viewer)
            {
                Id = Guid.Parse("C45AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
        };
        appDbContext.RoomParticipants.AddRange(roomParticipants);

        var roomQuestionEvaluation = new RoomQuestionEvaluation[]
        {
            new()
            {
                RoomQuestionId = roomQuestion[1].Id,
                CreatedById = users[1].Id,
                Mark = 5,
                Review = "test",
                State = SERoomQuestionEvaluationState.Submitted,
            },
            new()
            {
                RoomQuestionId = roomQuestion[0].Id,
                CreatedById = users[3].Id,
                Mark = 10,
                Review = "test test",
                State = SERoomQuestionEvaluationState.Draft,
            },
        };
        appDbContext.RoomQuestionEvaluation.AddRange(roomQuestionEvaluation);
        await appDbContext.SaveChangesAsync();

        var like = appDbContext.Reactions.Find(ReactionType.Like.Id) ?? throw new Exception("Unexpected state");
        var dislike = appDbContext.Reactions.Find(ReactionType.Dislike.Id) ?? throw new Exception("Unexpected state");

        var questionReactions = new RoomQuestionReaction[]
        {
            new()
            {
                Id = Guid.Parse("D15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D25AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D35AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D45AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = dislike,
                Sender = users[3],
            },
            new()
            {
                Id = Guid.Parse("D55AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = dislike,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D65AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = like,
                Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D75AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = dislike,
                Sender = users[3],
            },
        };
        appDbContext.RoomQuestionReactions.AddRange(questionReactions);
        await appDbContext.SaveChangesAsync();

        var roomService = CreateRoomService(appDbContext);

        var expectAnalytics = new Analytics
        {
            Questions = new List<Analytics.AnalyticsQuestion>
            {
                new()
                {
                    Id = questions[0].Id,
                    Value = questions[0].Value,
                    Status = RoomQuestionState.Open.Name,
                    Users = new List<Analytics.AnalyticsUser>
                    {
                        new()
                        {
                            Id = users[0].Id,
                            Nickname = users[0].Nickname,
                            Avatar = users[0].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Examinee.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[1].Id,
                            Nickname = users[1].Nickname,
                            Avatar = users[1].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Expert.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[2].Id,
                            Nickname = users[2].Nickname,
                            Avatar = users[2].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[3].Id,
                            Nickname = users[3].Nickname,
                            Avatar = users[3].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = new()
                            {
                                Mark = 10,
                                Review = "test test",
                            },
                        },
                    }
                },
                new()
                {
                    Id = questions[1].Id,
                    Value = questions[1].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users = new List<Analytics.AnalyticsUser>
                    {
                        new()
                        {
                            Id = users[0].Id,
                            Nickname = users[0].Nickname,
                            Avatar = users[0].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Examinee.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[1].Id,
                            Nickname = users[1].Nickname,
                            Avatar = users[1].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Expert.Name,
                            Evaluation = new()
                            {
                                Mark = 5,
                                Review = "test",
                            },
                        },
                        new()
                        {
                            Id = users[2].Id,
                            Nickname = users[2].Nickname,
                            Avatar = users[2].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[3].Id,
                            Nickname = users[3].Nickname,
                            Avatar = users[3].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                    }
                },
                new()
                {
                    Id = questions[2].Id,
                    Value = questions[2].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users = new List<Analytics.AnalyticsUser>
                    {
                        new()
                        {
                            Id = users[0].Id,
                            Nickname = users[0].Nickname,
                            Avatar = users[0].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Examinee.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[1].Id,
                            Nickname = users[1].Nickname,
                            Avatar = users[1].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Expert.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[2].Id,
                            Nickname = users[2].Nickname,
                            Avatar = users[2].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[3].Id,
                            Nickname = users[3].Nickname,
                            Avatar = users[3].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                    },
                },
                new()
                {
                    Id = questions[3].Id,
                    Value = questions[3].Value,
                    Status = RoomQuestionState.Active.Name,
                    Users = new List<Analytics.AnalyticsUser>
                    {
                        new()
                        {
                            Id = users[0].Id,
                            Nickname = users[0].Nickname,
                            Avatar = users[0].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Examinee.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[1].Id,
                            Nickname = users[1].Nickname,
                            Avatar = users[1].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Expert.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[2].Id,
                            Nickname = users[2].Nickname,
                            Avatar = users[2].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                        new()
                        {
                            Id = users[3].Id,
                            Nickname = users[3].Nickname,
                            Avatar = users[3].Avatar ?? string.Empty,
                            ParticipantType = SERoomParticipantType.Viewer.Name,
                            Evaluation = null,
                        },
                    },
                }
            }
        };

        var analyticsResult = await roomService.GetAnalyticsAsync(new RoomAnalyticsRequest(room1.Id));

        var serviceResult = analyticsResult;
        serviceResult.Should().NotBeNull();
        serviceResult.Should().BeEquivalentTo(expectAnalytics);
    }

    [Fact(DisplayName = "GetAnalyticsSummary should return valid analytics by roomId")]
    public async Task GetAnalyticsSummary_Should_Return_Valid_Analytics_By_RoomId()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var room1 = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(room1);
        appDbContext.Rooms.Add(new Room(DefaultRoomName + "2", SERoomAccessType.Public));

        var questions = new Question[]
        {
            new("V1") { Id = Guid.Parse("527A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V2") { Id = Guid.Parse("537A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V3") { Id = Guid.Parse("547A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V4") { Id = Guid.Parse("557A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V5") { Id = Guid.Parse("567A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V6") { Id = Guid.Parse("577A0279-4364-4940-BE4E-8DBEC08BA96C") }
        };
        appDbContext.Questions.AddRange(questions);

        var users = new User[]
        {
            new("u1", "v1")
            {
                Id = Guid.Parse("587A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u2", "v2")
            {
                Id = Guid.Parse("597A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.Admin.Id)! }
            },
            new("u3", "v3")
            {
                Id = Guid.Parse("5A7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u4", "v4")
            {
                Id = Guid.Parse("5B7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
            new("u5", "v5")
            {
                Id = Guid.Parse("5C7A0279-4364-4940-BE4E-8DBEC08BA96C"),
                Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }
            },
        };
        appDbContext.Users.AddRange(users);
        await appDbContext.SaveChangesAsync();

        var roomQuestion = new RoomQuestion[]
        {
            new()
            {
                Id = Guid.Parse("B15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[0],
                Room = room1,
                State = RoomQuestionState.Open,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B25AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[1],
                Room = room1,
                State = RoomQuestionState.Closed,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B35AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[2],
                Room = room1,
                State = RoomQuestionState.Closed,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
            new()
            {
                Id = Guid.Parse("B45AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                Question = questions[3],
                Room = room1,
                State = RoomQuestionState.Active,
                QuestionId = default,
                RoomId = default,
                Order = 0,
            },
        };
        appDbContext.RoomQuestions.AddRange(roomQuestion);

        var roomParticipants = new RoomParticipant[]
        {
            new(users[0], room1, SERoomParticipantType.Examinee)
            {
                Id = Guid.Parse("C15AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[1], room1, SERoomParticipantType.Expert)
            {
                Id = Guid.Parse("C25AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[2], room1, SERoomParticipantType.Viewer)
            {
                Id = Guid.Parse("C35AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
            new(users[3], room1, SERoomParticipantType.Viewer)
            {
                Id = Guid.Parse("C45AA6D4-FA7B-49CB-AFA2-EA4F900F2258")
            },
        };
        appDbContext.RoomParticipants.AddRange(roomParticipants);
        await appDbContext.SaveChangesAsync();

        var like = appDbContext.Reactions.Find(ReactionType.Like.Id) ?? throw new Exception("Unexpected state");
        var dislike = appDbContext.Reactions.Find(ReactionType.Dislike.Id) ?? throw new Exception("Unexpected state");

        var questionReactions = new RoomQuestionReaction[]
        {
            new()
            {
                Id = Guid.Parse("D15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D25AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D35AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = like,
                Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D45AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[0],
                Reaction = dislike,
                Sender = users[3],
            },
            new()
            {
                Id = Guid.Parse("D55AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = dislike,
                Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D65AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = dislike,
                Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D75AA6D4-FA7B-49CB-AFA2-EA4F900F2258"),
                RoomQuestion = roomQuestion[1],
                Reaction = dislike,
                Sender = users[3],
            },
        };
        appDbContext.RoomQuestionReactions.AddRange(questionReactions);
        await appDbContext.SaveChangesAsync();

        var roomService = CreateRoomService(appDbContext);

        var expectAnalytics = new AnalyticsSummary
        {
            Questions = new List<AnalyticsSummaryQuestion>
            {
                new()
                {
                    Id = questions[0].Id,
                    Value = questions[0].Value,
                    Experts = new List<AnalyticsSummaryExpert>
                    {
                        new()
                        {
                            Nickname = users[1].Nickname,
                            ReactionsSummary = new List<Analytics.AnalyticsReactionSummary>
                            {
                                new()
                                {
                                    Id = ReactionType.Like.Id,
                                    Type = ReactionType.Like.Name,
                                    Count = 2,
                                }
                            }
                        }
                    },
                    Viewers = new List<AnalyticsSummaryViewer>
                    {
                        new()
                        {
                            ReactionsSummary = new List<Analytics.AnalyticsReactionSummary>
                            {
                                new()
                                {
                                    Id = ReactionType.Like.Id,
                                    Type = ReactionType.Like.Name,
                                    Count = 1,
                                },
                                new()
                                {
                                    Id = ReactionType.Dislike.Id,
                                    Type = ReactionType.Dislike.Name,
                                    Count = 1,
                                }
                            },
                        },
                    }
                },
                new()
                {
                    Id = questions[1].Id,
                    Value = questions[1].Value,
                    Experts = new List<AnalyticsSummaryExpert>
                    {
                        new()
                        {
                            Nickname = users[1].Nickname,
                            ReactionsSummary = new List<Analytics.AnalyticsReactionSummary>
                            {
                                new()
                                {
                                    Id = ReactionType.Dislike.Id,
                                    Type = ReactionType.Dislike.Name,
                                    Count = 1,
                                }
                            }
                        }
                    },
                    Viewers = new List<AnalyticsSummaryViewer>
                    {
                        new()
                        {
                            ReactionsSummary = new List<Analytics.AnalyticsReactionSummary>
                            {
                                new()
                                {
                                    Id = ReactionType.Dislike.Id,
                                    Type = ReactionType.Dislike.Name,
                                    Count = 2,
                                },
                            }
                        }
                    },
                }
            }
        };

        var analyticsResult = await roomService.GetAnalyticsSummaryAsync(new RoomAnalyticsRequest(room1.Id));

        analyticsResult.Should().NotBeNull();
        analyticsResult.Should().BeEquivalentTo(expectAnalytics);
    }

    [Fact]
    public async Task GetInvites()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var generatedRooms = Enumerable.Range(0, 5)
            .Select(i => new Room(DefaultRoomName + i, SERoomAccessType.Public)).ToList();
        appDbContext.Rooms.AddRange(generatedRooms);
        var roomInvites = generatedRooms.SelectMany(GenerateInvites).ToList();
        appDbContext.RoomInvites.AddRange(roomInvites);
        await appDbContext.SaveChangesAsync();

        var checkRoom = appDbContext.Rooms.AsEnumerable().OrderBy(_ => Guid.NewGuid()).First();
        var expectInvites = appDbContext.RoomInvites.Where(e => e.RoomById == checkRoom.Id)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteById!.Value,
                ParticipantType = e.ParticipantType!.EnumValue,
                Max = e.Invite!.UsesMax,
                Used = e.Invite!.UsesCurrent,
            })
            .OrderBy(e => e.InviteId)
            .ToList();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext);
        var actualInvites = await roomService.GetInvitesAsync(checkRoom.Id);
        actualInvites.Sort((i1, i2) => i1.InviteId.CompareTo(i2.InviteId));
        actualInvites.Should().HaveCount(expectInvites.Count).And.BeEquivalentTo(expectInvites);
    }

    [Fact]
    public async Task GetInvitesAsync()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var generatedRooms = Enumerable.Range(0, 5)
            .Select(i => new Room(DefaultRoomName + i, SERoomAccessType.Public)).ToList();
        appDbContext.Rooms.AddRange(generatedRooms);
        var roomInvites = generatedRooms.SelectMany(GenerateInvites).ToList();
        appDbContext.RoomInvites.AddRange(roomInvites);
        await appDbContext.SaveChangesAsync();

        var checkRoom = appDbContext.Rooms.AsEnumerable().OrderBy(_ => Guid.NewGuid()).First();
        var expectInvites = appDbContext.RoomInvites.Where(e => e.RoomById == checkRoom.Id)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteById!.Value,
                ParticipantType = e.ParticipantType!.EnumValue,
                Max = e.Invite!.UsesMax,
                Used = e.Invite!.UsesCurrent,
            })
            .OrderBy(e => e.InviteId)
            .ToList();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext);
        var actualInvites = await roomService.GetInvitesAsync(checkRoom.Id);
        actualInvites.Sort((i1, i2) => i1.InviteId.CompareTo(i2.InviteId));
        actualInvites.Should().HaveCount(expectInvites.Count).And.BeEquivalentTo(expectInvites);
    }

    [Fact(DisplayName = "Patch update of room when room not found")]
    public async Task PatchUpdateRoomWhenRoomNotFound()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roomPatchUpdateRequest = new RoomUpdateRequest { Name = "new_value_name_room", Questions = new List<RoomQuestionRequest>(), };
        var roomId = Guid.NewGuid();

        var roomService = CreateRoomService(appDbContext);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            roomService.UpdateAsync(roomId, roomPatchUpdateRequest));
    }

    [Fact]
    public async Task Create_Room_With_Questions()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question = new Question("test");
        appDbContext.Questions.Add(question);
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomCreateRequest = new RoomCreateRequest
        {
            Questions = new List<RoomQuestionRequest>
            {
                new() { Id = question.Id, Order = 10 }
            },
            Experts = new HashSet<Guid>(),
            Examinees = new HashSet<Guid>(),
            Tags = new HashSet<Guid>(),
            Name = "My room",
            AccessType = SERoomAccessType.Public,
        };

        var createdRoom = await roomService.CreateAsync(roomCreateRequest, CancellationToken.None);

        var dbRoom = await appDbContext.Rooms.Include(e => e.Questions).FirstAsync(e => e.Id == createdRoom.Id);

        dbRoom.Name.Should().Be("My room");
        dbRoom.AccessType!.Should().Be(SERoomAccessType.Public);
        dbRoom.Questions.Should().HaveCount(1);
        dbRoom.Questions[0].Order.Should().Be(10);
    }

    [Fact]
    public async Task Update_Room_With_Questions()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question1 = new Question("test");
        var question2 = new Question("test 2");
        var question3 = new Question("test 3");
        appDbContext.Questions.AddRange(question1, question2, question3);
        var initialRoom = new Room("My room", SERoomAccessType.Public)
        {
            Questions = new List<RoomQuestion>
            {
                new()
                {
                    RoomId = default,
                    QuestionId = question1.Id,
                    Room = null,
                    Question = null,
                    State = RoomQuestionState.Open,
                    Order = 5
                },
                new()
                {
                    RoomId = default,
                    QuestionId = question2.Id,
                    Room = null,
                    Question = null,
                    State = RoomQuestionState.Open,
                    Order = -4
                },
            }
        };
        appDbContext.Rooms.Add(initialRoom);
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomUpdateRequest = new RoomUpdateRequest
        {
            Name = "My room 2",
            Tags = new HashSet<Guid>(),
            Questions = new List<RoomQuestionRequest>
            {
                new() { Id = question2.Id, Order = 128 },
                new() { Id = question3.Id, Order = -1000, }
            }
        };

        var createdRoom = await roomService.UpdateAsync(initialRoom.Id, roomUpdateRequest, CancellationToken.None);

        var dbRoom = await appDbContext.Rooms.Include(e => e.Questions).FirstAsync(e => e.Id == createdRoom.Id);

        dbRoom.Name.Should().Be("My room 2");
        dbRoom.AccessType!.Should().Be(SERoomAccessType.Public);
        dbRoom.Questions.Should().HaveCount(2);

        var dbLinkedQuestion2 = dbRoom.Questions.FirstOrDefault(e => e.QuestionId == question2.Id);
        dbLinkedQuestion2.Should().NotBeNull();
        dbLinkedQuestion2!.Order.Should().Be(128);

        var dbLinkedQuestion3 = dbRoom.Questions.FirstOrDefault(e => e.QuestionId == question3.Id);
        dbLinkedQuestion3.Should().NotBeNull();
        dbLinkedQuestion3!.Order.Should().Be(-1000);
    }

    private IEnumerable<RoomInvite> GenerateInvites(Room room)
    {
        foreach (var participantType in SERoomParticipantType.List)
        {
            var invite = new Invite(Random.Shared.Next(1, 20));
            yield return new RoomInvite(invite, room, participantType);
        }
    }

    private static RoomService CreateRoomService(AppDbContext appDbContext, User? user = null)
    {
        var userAccessor = new CurrentUserAccessor();
        if (user is not null)
        {
            userAccessor.SetUser(user);
        }

        var roomRepository = new RoomRepository(appDbContext); var roomParticipantService = new RoomParticipantService(new RoomParticipantRepository(appDbContext), new RoomRepository(appDbContext), new UserRepository(appDbContext), new AvailableRoomPermissionRepository(appDbContext), userAccessor);
        var roomService = new RoomService(
            roomRepository,
            new RoomQuestionRepository(appDbContext),
            new QuestionRepository(appDbContext),
            new UserRepository(appDbContext),
            new EmptyRoomEventDispatcher(),
            new RoomQuestionReactionRepository(appDbContext),
            new TagRepository(appDbContext),
            new RoomParticipantRepository(appDbContext),
            new AppEventRepository(appDbContext),
            new RoomStateRepository(appDbContext),
            new EmptyEventStorage(),
            new RoomInviteService(appDbContext, roomParticipantService, NullLogger<RoomInviteService>.Instance),
            userAccessor,
            roomParticipantService,
            appDbContext,
            new NullLogger<RoomService>());
        return roomService;
    }
}
