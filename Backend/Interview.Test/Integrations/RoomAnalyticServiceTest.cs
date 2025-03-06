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
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations;

public class RoomAnalyticServiceTest
{
    private const string DefaultRoomName = "Test_Room";

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
            Questions =
            [
                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q"),
                    State = RoomQuestionState.Open,
                    Order = 0,
                    Evaluations = [new() { RoomQuestionId = default, CreatedBy = dummyUser, Review = "dummy 2", Mark = 2 }]
                },

                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q 2"),
                    State = RoomQuestionState.Active,
                    Order = 0,
                    Evaluations = [new() { RoomQuestionId = default, CreatedBy = dummyUser, Review = "dummy 1", Mark = 1 }]
                }

            ],
        };
        dummyRoom.Participants.Add(new RoomParticipant(dummyUser, dummyRoom, SERoomParticipantType.Expert));

        appDbContext.Rooms.Add(dummyRoom);
        appDbContext.Rooms.Add(new Room(DefaultRoomName + "2", SERoomAccessType.Public));

        var questions = new Question[]
        {
            new("V1") { Id = Guid.Parse("527A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V2") { Id = Guid.Parse("537A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V3") { Id = Guid.Parse("547A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V4") { Id = Guid.Parse("557A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V5") { Id = Guid.Parse("567A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V6") { Id = Guid.Parse("577A0279-4364-4940-BE4E-8DBEC08BA96C") }
        };
        appDbContext.Questions.AddRange(questions);

        var users = new User[]
        {
            new("u1", "v1")
            {
                Id = Guid.Parse("587A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 1"
            },
            new("u2", "v2") { Id = Guid.Parse("597A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.Admin.Id)! } },
            new("u3", "v3")
            {
                Id = Guid.Parse("5A7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 3"
            },
            new("u4", "v4") { Id = Guid.Parse("5B7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! } },
            new("u5", "v5")
            {
                Id = Guid.Parse("5C7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 5"
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
            new(users[0], room1, SERoomParticipantType.Examinee) { Id = Guid.Parse("C15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), },
            new(users[1], room1, SERoomParticipantType.Expert) { Id = Guid.Parse("C25AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[2], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C35AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[3], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C45AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
        };
        appDbContext.RoomParticipants.AddRange(roomParticipants);

        var roomQuestionEvaluation = new RoomQuestionEvaluation[]
        {
            new()
            {
                RoomQuestionId = roomQuestion[1].Id,
                CreatedById = users[0].Id,
                Mark = 2,
                Review = "test 2",
                State = SERoomQuestionEvaluationState.Submitted,
            },
            new()
            {
                RoomQuestionId = roomQuestion[3].Id,
                CreatedById = users[0].Id,
                Mark = 10,
                Review = "test 4444",
                State = SERoomQuestionEvaluationState.Submitted,
            },
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
                State = SERoomQuestionEvaluationState.Submitted,
            },
        };
        appDbContext.RoomQuestionEvaluation.AddRange(roomQuestionEvaluation);
        await appDbContext.SaveChangesAsync();

        appDbContext.RoomReview.AddRange(
            new RoomReview(roomParticipants[0], SERoomReviewState.Closed) { Review = "test review", },
            new RoomReview(roomParticipants[3], SERoomReviewState.Closed) { Review = "test review 22", });
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = new RoomAnalyticService(appDbContext);

        var expectAnalytics = new Analytics
        {
            Questions =
            [
                new()
                {
                    Id = questions[0].Id,
                    Value = questions[0].Value,
                    Status = RoomQuestionState.Open.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = null, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = new() { Mark = 10, Review = "test test", }, }
                    ],
                    AverageMark = 10
                },

                new()
                {
                    Id = questions[1].Id,
                    Value = questions[1].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = new() { Mark = 2, Review = "test 2", }, },
                        new() { Id = users[1].Id, Evaluation = new() { Mark = 5, Review = "test", }, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = 3.5
                },

                new()
                {
                    Id = questions[2].Id,
                    Value = questions[2].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = null, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = 0,
                },

                new()
                {
                    Id = questions[3].Id,
                    Value = questions[3].Value,
                    Status = RoomQuestionState.Active.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = new() { Review = "test 4444", Mark = 10 }, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = 10,
                }
            ],
            AverageMark = 8,
            UserReview =
            [
                new()
                {
                    UserId = users[0].Id,
                    AverageMark = 6,
                    Comment = "test review",
                    Nickname = users[0].Nickname,
                    Avatar = users[0].Avatar,
                    ParticipantType = roomParticipants[0].Type.EnumValue
                },

                new()
                {
                    UserId = users[1].Id,
                    AverageMark = null,
                    Comment = null,
                    Nickname = users[1].Nickname,
                    Avatar = users[1].Avatar,
                    ParticipantType = roomParticipants[1].Type.EnumValue
                },

                new()
                {
                    UserId = users[2].Id,
                    AverageMark = null,
                    Comment = null,
                    Nickname = users[2].Nickname,
                    Avatar = users[2].Avatar,
                    ParticipantType = roomParticipants[2].Type.EnumValue
                },

                new()
                {
                    UserId = users[3].Id,
                    AverageMark = 10,
                    Comment = "test review 22",
                    Nickname = users[3].Nickname,
                    Avatar = users[3].Avatar,
                    ParticipantType = roomParticipants[3].Type.EnumValue
                }

            ],
            Completed = true
        };

        var analyticsResult = await roomService.GetAsync(new RoomAnalyticsRequest(room1.Id));

        var serviceResult = analyticsResult;
        serviceResult.Should().NotBeNull();
        serviceResult.Should().BeEquivalentTo(expectAnalytics);
    }

    [Theory(DisplayName = "GetAnalytics should return valid analytics by roomId with incomplete")]
    [InlineData(EVRoomQuestionEvaluationState.Draft, EVRoomReviewState.Open)]
    [InlineData(EVRoomQuestionEvaluationState.Draft, EVRoomReviewState.Rejected)]
    public async Task GetAnalytics_Should_Return_Valid_Analytics_By_RoomId_With_Incomplete(EVRoomQuestionEvaluationState roomQuestionEvaluationState, EVRoomReviewState roomReviewState)
    {
        var closeRoomQuestionEvaluation = SERoomQuestionEvaluationState.FromValue((int)roomQuestionEvaluationState);
        var closeReviewState = SERoomReviewState.FromValue((int)roomReviewState);
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var room1 = new Room(DefaultRoomName, SERoomAccessType.Public);

        appDbContext.Rooms.Add(room1);

        var dummyUser = new User("dummy", "dummy");
        appDbContext.Users.Add(dummyUser);
        appDbContext.SaveChanges();
        var dummyRoom = new Room("test room", SERoomAccessType.Public)
        {
            Questions =
            [
                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q"),
                    State = RoomQuestionState.Open,
                    Order = 0,
                    Evaluations = [new() { RoomQuestionId = default, CreatedBy = dummyUser, Review = "dummy 2", Mark = 2 }]
                },

                new()
                {
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Question = new Question("test q 2"),
                    State = RoomQuestionState.Active,
                    Order = 0,
                    Evaluations = [new() { RoomQuestionId = default, CreatedBy = dummyUser, Review = "dummy 1", Mark = 1 }]
                }

            ],
        };
        dummyRoom.Participants.Add(new RoomParticipant(dummyUser, dummyRoom, SERoomParticipantType.Expert));

        appDbContext.Rooms.Add(dummyRoom);
        appDbContext.Rooms.Add(new Room(DefaultRoomName + "2", SERoomAccessType.Public));

        var questions = new Question[]
        {
            new("V1") { Id = Guid.Parse("527A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V2") { Id = Guid.Parse("537A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V3") { Id = Guid.Parse("547A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V4") { Id = Guid.Parse("557A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V5") { Id = Guid.Parse("567A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V6") { Id = Guid.Parse("577A0279-4364-4940-BE4E-8DBEC08BA96C") }
        };
        appDbContext.Questions.AddRange(questions);

        var users = new User[]
        {
            new("u1", "v1")
            {
                Id = Guid.Parse("587A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 1"
            },
            new("u2", "v2") { Id = Guid.Parse("597A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.Admin.Id)! } },
            new("u3", "v3")
            {
                Id = Guid.Parse("5A7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 3"
            },
            new("u4", "v4") { Id = Guid.Parse("5B7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! } },
            new("u5", "v5")
            {
                Id = Guid.Parse("5C7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { appDbContext.Roles.Find(RoleName.User.Id)! }, Avatar = "dummy avatar 5"
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
            new(users[0], room1, SERoomParticipantType.Examinee) { Id = Guid.Parse("C15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), },
            new(users[1], room1, SERoomParticipantType.Expert) { Id = Guid.Parse("C25AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[2], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C35AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[3], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C45AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
        };
        appDbContext.RoomParticipants.AddRange(roomParticipants);

        var roomQuestionEvaluation = new RoomQuestionEvaluation[]
        {
            new()
            {
                RoomQuestionId = roomQuestion[1].Id,
                CreatedById = users[0].Id,
                Mark = 2,
                Review = "test 2",
                State = closeRoomQuestionEvaluation,
            },
            new()
            {
                RoomQuestionId = roomQuestion[3].Id,
                CreatedById = users[0].Id,
                Mark = 10,
                Review = "test 4444",
                State = SERoomQuestionEvaluationState.Submitted,
            },
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
                State = SERoomQuestionEvaluationState.Submitted,
            },
        };
        appDbContext.RoomQuestionEvaluation.AddRange(roomQuestionEvaluation);
        await appDbContext.SaveChangesAsync();

        appDbContext.RoomReview.AddRange(
            new RoomReview(roomParticipants[0], SERoomReviewState.Closed) { Review = "test review", },
            new RoomReview(roomParticipants[3], closeReviewState) { Review = "test review 22", });
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = new RoomAnalyticService(appDbContext);

        var expectAnalytics = new Analytics
        {
            Questions =
            [
                new()
                {
                    Id = questions[0].Id,
                    Value = questions[0].Value,
                    Status = RoomQuestionState.Open.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = null, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = new() { Mark = 10, Review = "test test", }, }
                    ],
                    AverageMark = null
                },

                new()
                {
                    Id = questions[1].Id,
                    Value = questions[1].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = null, },
                        new() { Id = users[1].Id, Evaluation = new() { Mark = 5, Review = "test", }, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = null
                },

                new()
                {
                    Id = questions[2].Id,
                    Value = questions[2].Value,
                    Status = RoomQuestionState.Closed.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = null, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = null,
                },

                new()
                {
                    Id = questions[3].Id,
                    Value = questions[3].Value,
                    Status = RoomQuestionState.Active.Name,
                    Users =
                    [
                        new() { Id = users[0].Id, Evaluation = new() { Review = "test 4444", Mark = 10 }, },
                        new() { Id = users[1].Id, Evaluation = null, },
                        new() { Id = users[2].Id, Evaluation = null, },
                        new() { Id = users[3].Id, Evaluation = null, }
                    ],
                    AverageMark = null,
                }
            ],
            AverageMark = null,
            UserReview =
            [
                new()
                {
                    UserId = users[0].Id,
                    AverageMark = 6.0,
                    Comment = "test review",
                    Nickname = users[0].Nickname,
                    Avatar = users[0].Avatar,
                    ParticipantType = roomParticipants[0].Type.EnumValue
                },

                new()
                {
                    UserId = users[1].Id,
                    AverageMark = null,
                    Comment = null,
                    Nickname = users[1].Nickname,
                    Avatar = users[1].Avatar,
                    ParticipantType = roomParticipants[1].Type.EnumValue
                },

                new()
                {
                    UserId = users[2].Id,
                    AverageMark = null,
                    Comment = null,
                    Nickname = users[2].Nickname,
                    Avatar = users[2].Avatar,
                    ParticipantType = roomParticipants[2].Type.EnumValue
                },

                new()
                {
                    UserId = users[3].Id,
                    AverageMark = null,
                    Comment = null,
                    Nickname = users[3].Nickname,
                    Avatar = users[3].Avatar,
                    ParticipantType = roomParticipants[3].Type.EnumValue
                }

            ],
            Completed = false
        };

        var analyticsResult = await roomService.GetAsync(new RoomAnalyticsRequest(room1.Id));

        var serviceResult = analyticsResult;
        serviceResult.Should().NotBeNull();
        serviceResult.Should().BeEquivalentTo(expectAnalytics);
    }
}
