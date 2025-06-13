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

namespace Interview.Test.Integrations.Rooms;

public class RoomServiceTest
{
    private const string DefaultRoomName = "Test_Room";

    [Fact(DisplayName = "Patch update room with request name not null")]
    public async Task PatchUpdateRoomWithRequestNameIsNotNull()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var savedRoom = new Domain.Rooms.Room(DefaultRoomName, SERoomAccessType.Public, SERoomType.Standard);

        appDbContext.Rooms.Add(savedRoom);

        var question = new Question("question_value#1");
        appDbContext.Questions.AddRange(question);

        await appDbContext.SaveChangesAsync();

        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "New_Value_Name_Room",
            Questions = [new() { Id = question.Id, Order = 0 }],
        };

        _ = await roomService.UpdateAsync(savedRoom.Id, roomPatchUpdateRequest);

        var foundedRoom = await roomRepository.FindByIdAsync(savedRoom.Id);

        foundedRoom.Should().NotBeNull();
        foundedRoom!.Name.Should().BeEquivalentTo(roomPatchUpdateRequest.Name);
    }

    [Fact]
    public async Task Update_Room_With_Question_And_QuestionTree()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var question = new Question("question_value#1");
        var question2 = new Question("question_value#2");
        appDbContext.Questions.AddRange(question, question2);

        var questionTree = new QuestionTree
        {
            Name = "root",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Type = SEQuestionSubjectTreeType.Question
            }
        };

        var questionTree2 = new QuestionTree
        {
            Name = "Dummy t",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree
            {
                QuestionId = null,
                Question = new Question("question_value#3"),
                Type = SEQuestionSubjectTreeType.Question,
            }
        };
        appDbContext.QuestionTree.AddRange(questionTree, questionTree2);

        var savedRoom = new Domain.Rooms.Room(DefaultRoomName, SERoomAccessType.Public, SERoomType.Standard)
        {
            QuestionTreeId = questionTree.Id,
        };

        appDbContext.Rooms.Add(savedRoom);

        appDbContext.RoomQuestions.Add(new RoomQuestion
        {
            RoomId = savedRoom.Id,
            QuestionId = question.Id,
            Room = null,
            Question = null,
            State = RoomQuestionState.Open,
            Order = 0
        });

        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "New_Value_Name_Room",
            Questions = [new() { Id = question2.Id, Order = 0 }],
            QuestionTreeId = questionTree2.Id,
        };

        _ = await roomService.UpdateAsync(savedRoom.Id, roomPatchUpdateRequest);

        var foundedRoom = await roomRepository.FindByIdAsync(savedRoom.Id);

        foundedRoom.Should().NotBeNull();
        foundedRoom!.Name.Should().BeEquivalentTo(roomPatchUpdateRequest.Name);
        foundedRoom.QuestionTreeId.Should().Be(questionTree2.Id);
        foundedRoom.Questions.Count.Should().Be(1);
        foundedRoom.Questions[0].QuestionId.Should().Be(questionTree2.RootQuestionSubjectTree!.QuestionId!.Value);
    }

    [Fact(DisplayName = "Patch update room with request name not null and add category")]
    public async Task PatchUpdateRoomWithRequestNameIsNotNull_And_Add_Category()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var savedRoom = new Domain.Rooms.Room(DefaultRoomName, SERoomAccessType.Public, SERoomType.Standard);
        appDbContext.Rooms.Add(savedRoom);

        var question = new Question("question_value#1");
        appDbContext.Questions.AddRange(question);

        await appDbContext.SaveChangesAsync();

        var roomRepository = new RoomRepository(appDbContext);
        var roomService = CreateRoomService(appDbContext);

        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "New_Value_Name_Room",
            Questions = [new() { Id = question.Id, Order = 0 }],
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

        var savedRoom = new Domain.Rooms.Room(DefaultRoomName, SERoomAccessType.Public, SERoomType.Standard);

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
            new RoomQuestion
            {
                Room = savedRoom,
                State = RoomQuestionState.Open,
                Question = questions[0],
                QuestionId = default,
                RoomId = default,
                Order = 0
            },
            new RoomQuestion
            {
                Room = savedRoom,
                State = RoomQuestionState.Closed,
                Question = questions[1],
                QuestionId = default,
                RoomId = default,
                Order = 0
            },
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

    [Fact(DisplayName = "GetAnalyticsSummary should return valid analytics by roomId")]
    public async Task GetAnalyticsSummary_Should_Return_Valid_Analytics_By_RoomId()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var room1 = new Domain.Rooms.Room(DefaultRoomName, SERoomAccessType.Public, SERoomType.Standard);

        appDbContext.Rooms.Add(room1);
        appDbContext.Rooms.Add(new Domain.Rooms.Room(DefaultRoomName + "2", SERoomAccessType.Public, SERoomType.Standard));

        var questions = new Question[]
        {
            new("V1") { Id = Guid.Parse("527A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V2") { Id = Guid.Parse("537A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V3") { Id = Guid.Parse("547A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V4") { Id = Guid.Parse("557A0279-4364-4940-BE4E-8DBEC08BA96C") },
            new("V5") { Id = Guid.Parse("567A0279-4364-4940-BE4E-8DBEC08BA96C") }, new("V6") { Id = Guid.Parse("577A0279-4364-4940-BE4E-8DBEC08BA96C") }
        };
        appDbContext.Questions.AddRange(questions);

        var users = new User[]
        {
            new("u1", "v1") { Id = Guid.Parse("587A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { (await appDbContext.Roles.FindAsync(RoleName.User.Id))! } },
            new("u2", "v2") { Id = Guid.Parse("597A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { (await appDbContext.Roles.FindAsync(RoleName.Admin.Id))! } },
            new("u3", "v3") { Id = Guid.Parse("5A7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { (await appDbContext.Roles.FindAsync(RoleName.User.Id))! } },
            new("u4", "v4") { Id = Guid.Parse("5B7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { (await appDbContext.Roles.FindAsync(RoleName.User.Id))! } },
            new("u5", "v5") { Id = Guid.Parse("5C7A0279-4364-4940-BE4E-8DBEC08BA96C"), Roles = { (await appDbContext.Roles.FindAsync(RoleName.User.Id))! } },
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
            new(users[0], room1, SERoomParticipantType.Examinee) { Id = Guid.Parse("C15AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[1], room1, SERoomParticipantType.Expert) { Id = Guid.Parse("C25AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[2], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C35AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
            new(users[3], room1, SERoomParticipantType.Viewer) { Id = Guid.Parse("C45AA6D4-FA7B-49CB-AFA2-EA4F900F2258") },
        };
        appDbContext.RoomParticipants.AddRange(roomParticipants);
        await appDbContext.SaveChangesAsync();

        var like = await appDbContext.Reactions.FindAsync(ReactionType.Like.Id) ?? throw new UserException("Unexpected state");
        var dislike = await appDbContext.Reactions.FindAsync(ReactionType.Dislike.Id) ?? throw new UserException("Unexpected state");

        var questionReactions = new RoomQuestionReaction[]
        {
            new()
            {
                Id = Guid.Parse("D15AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[0], Reaction = like, Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D25AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[0], Reaction = like, Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D35AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[0], Reaction = like, Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D45AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[0], Reaction = dislike, Sender = users[3],
            },
            new()
            {
                Id = Guid.Parse("D55AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[1], Reaction = dislike, Sender = users[1],
            },
            new()
            {
                Id = Guid.Parse("D65AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[1], Reaction = dislike, Sender = users[2],
            },
            new()
            {
                Id = Guid.Parse("D75AA6D4-FA7B-49CB-AFA2-EA4F900F2258"), RoomQuestion = roomQuestion[1], Reaction = dislike, Sender = users[3],
            },
        };
        appDbContext.RoomQuestionReactions.AddRange(questionReactions);
        await appDbContext.SaveChangesAsync();

        var roomService = CreateRoomService(appDbContext);

        var expectAnalytics = new AnalyticsSummary
        {
            Questions =
            [
                new()
                {
                    Id = questions[0].Id,
                    Value = questions[0].Value,
                    Experts =
                    [
                        new()
                        {
                            Nickname = users[1].Nickname,
                            ReactionsSummary =
                                [new() { Id = ReactionType.Like.Id, Type = ReactionType.Like.Name, Count = 2, }]
                        }
                    ],
                    Viewers =
                    [
                        new()
                        {
                            ReactionsSummary =
                            [
                                new() { Id = ReactionType.Like.Id, Type = ReactionType.Like.Name, Count = 1, },
                                new() { Id = ReactionType.Dislike.Id, Type = ReactionType.Dislike.Name, Count = 1, }
                            ],
                        }

                    ]
                },

                new()
                {
                    Id = questions[1].Id,
                    Value = questions[1].Value,
                    Experts =
                    [
                        new()
                        {
                            Nickname = users[1].Nickname,
                            ReactionsSummary =
                                [new() { Id = ReactionType.Dislike.Id, Type = ReactionType.Dislike.Name, Count = 1, }]
                        }
                    ],
                    Viewers =
                    [
                        new()
                        {
                            ReactionsSummary =
                            [
                                new() { Id = ReactionType.Dislike.Id, Type = ReactionType.Dislike.Name, Count = 2, }
                            ]
                        }
                    ],
                }
            ]
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
            .Select(i => new Domain.Rooms.Room(DefaultRoomName + i, SERoomAccessType.Public, SERoomType.Standard)).ToList();
        appDbContext.Rooms.AddRange(generatedRooms);
        var roomInvites = generatedRooms.SelectMany(GenerateInvites).ToList();
        appDbContext.RoomInvites.AddRange(roomInvites);
        await appDbContext.SaveChangesAsync();

        var checkRoom = appDbContext.Rooms.AsEnumerable().OrderBy(_ => Guid.NewGuid()).First();
        var expectInvites = appDbContext.RoomInvites.Where(e => e.RoomId == checkRoom.Id)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteId,
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
    public async Task GetCalendar()
    {
        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user = new User("pavel", "externals");

        var firstScheduled = new DateTime(2024, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc);
        var secondScheduled = new DateTime(2024, 9, 27, 23, 49, 0, 0, DateTimeKind.Utc);

        var room1 = new Domain.Rooms.Room(DefaultRoomName + Random.Shared.Next(10), SERoomAccessType.Public, SERoomType.Standard)
        {
            ScheduleStartTime = firstScheduled,
            Status = SERoomStatus.Active
        };
        var room2 = new Domain.Rooms.Room(DefaultRoomName + Random.Shared.Next(10), SERoomAccessType.Public, SERoomType.Standard)
        {
            ScheduleStartTime = secondScheduled,
            Status = SERoomStatus.Review
        };
        var room3 = new Domain.Rooms.Room(DefaultRoomName + Random.Shared.Next(10), SERoomAccessType.Public, SERoomType.Standard)
        {
            ScheduleStartTime = DateTime.UtcNow,
            Status = SERoomStatus.Close
        };

        memoryDatabase.Rooms.AddRange(room1, room2, room3);

        var roomParticipant = new RoomParticipant(user, room1, SERoomParticipantType.Expert);
        var roomParticipant2 = new RoomParticipant(user, room2, SERoomParticipantType.Expert);
        var roomParticipant3 = new RoomParticipant(user, room3, SERoomParticipantType.Expert);

        memoryDatabase.RoomParticipants.AddRange(roomParticipant, roomParticipant2, roomParticipant3);

        var roomService = CreateRoomService(memoryDatabase, user);

        await memoryDatabase.SaveChangesAsync();

        var roomCalendarResponse = await roomService.GetCalendarAsync(new RoomCalendarRequest
        {
            RoomStatus =
            [
                EVRoomStatus.Active,
                EVRoomStatus.Review
            ],
            TimeZoneOffset = -180
        });

        var roomCalendarItems = roomCalendarResponse;

        roomCalendarItems.Should().NotBeNullOrEmpty();
        roomCalendarItems.Count.Should().Be(2);

        var roomCalendarRoom1 = roomCalendarItems.FirstOrDefault(meeting => meeting.MinScheduledStartTime.Equals(firstScheduled));
        roomCalendarRoom1.Should().NotBeNull();

        roomCalendarRoom1?.Statuses.Count.Should().Be(1);

        var roomCalendarRoom2 = roomCalendarItems.FirstOrDefault(meeting => meeting.MinScheduledStartTime.Equals(secondScheduled));
        roomCalendarRoom2.Should().NotBeNull();

        roomCalendarRoom1?.Statuses.Count.Should().Be(1);
    }

    [Fact(DisplayName = "All participants of the room have left the resulting feedback and the room is ready to close")]
    public async Task TestIsReadyToCloseSuccess()
    {
        var cancellationToken = new CancellationToken();

        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user1 = new User(Guid.NewGuid(), "user1", Guid.NewGuid().ToString());
        var user2 = new User(Guid.NewGuid(), "user2", Guid.NewGuid().ToString());

        var question = new Question("question_test");
        var question1 = new Question("question_test_1");

        var room = new Domain.Rooms.Room("test", SERoomAccessType.Private, SERoomType.Standard);

        memoryDatabase.Users.AddRange(user1, user2);
        memoryDatabase.Rooms.Add(room);
        memoryDatabase.Questions.AddRange(question, question1);

        var roomParticipant1 = new RoomParticipant(user1, room, SERoomParticipantType.Expert);
        var roomParticipant2 = new RoomParticipant(user2, room, SERoomParticipantType.Expert);

        memoryDatabase.RoomParticipants.AddRange(roomParticipant1, roomParticipant2);

        var roomReview1 = new RoomReview(roomParticipant1, SERoomReviewState.Closed);
        var roomReview2 = new RoomReview(roomParticipant2, SERoomReviewState.Closed);

        memoryDatabase.RoomReview.AddRange(roomReview1, roomReview2);

        await memoryDatabase.SaveChangesAsync(cancellationToken);

        var roomRepository = new RoomRepository(memoryDatabase);

        var isReadyToCloseAsync = await roomRepository.IsReadyToCloseAsync(room.Id, cancellationToken);

        isReadyToCloseAsync.Should().BeTrue();
    }

    [Fact(DisplayName = "Any participants of the room have not left the resulting feedback and the room is ready to close")]
    public async Task TestRoomIsNotReadyToCloseSuccess()
    {
        var cancellationToken = new CancellationToken();

        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user1 = new User(Guid.NewGuid(), "user1", Guid.NewGuid().ToString());
        var user2 = new User(Guid.NewGuid(), "user2", Guid.NewGuid().ToString());

        var question = new Question("question_test");
        var question1 = new Question("question_test_1");

        var room = new Domain.Rooms.Room("test", SERoomAccessType.Private, SERoomType.Standard);

        memoryDatabase.Users.AddRange(user1, user2);
        await memoryDatabase.SaveChangesAsync();
        memoryDatabase.Rooms.Add(room);
        memoryDatabase.Questions.AddRange(question, question1);

        var roomQuestions = new[]
        {
            (Question: question, User: user1, State: RoomQuestionState.Active),
            (Question: question1, User: user2, State: RoomQuestionState.Active),
        }
        .Select(e => new RoomQuestion
        {
            RoomId = default,
            QuestionId = default,
            Room = room,
            Question = e.Question,
            State = e.State,
            Order = 0,
            CreatedById = e.User.Id
        })
        .ToList();
        memoryDatabase.RoomQuestions.AddRange(roomQuestions);

        memoryDatabase.RoomQuestionEvaluation.AddRange(roomQuestions.Select(e => new RoomQuestionEvaluation
        {
            RoomQuestionId = e.Id,
            CreatedById = e.CreatedById,
            State = SERoomQuestionEvaluationState.Draft
        }));

        var roomParticipant1 = new RoomParticipant(user1, room, SERoomParticipantType.Expert);
        var roomParticipant2 = new RoomParticipant(user2, room, SERoomParticipantType.Expert);

        memoryDatabase.RoomParticipants.AddRange(roomParticipant1, roomParticipant2);

        var roomReview1 = new RoomReview(roomParticipant1, SERoomReviewState.Closed);
        var roomReview2 = new RoomReview(roomParticipant2, SERoomReviewState.Open);

        memoryDatabase.RoomReview.AddRange(roomReview1, roomReview2);

        await memoryDatabase.SaveChangesAsync(cancellationToken);
        await memoryDatabase.SaveChangesAsync();

        var roomRepository = new RoomRepository(memoryDatabase);

        var isReadyToCloseAsync = await roomRepository.IsReadyToCloseAsync(room.Id, cancellationToken);

        isReadyToCloseAsync.Should().BeFalse();
    }

    [Fact(DisplayName = "One of the users was not in the room at the time of the interview")]
    public async Task TestRoomIsReadyToCloseSuccess()
    {
        var cancellationToken = new CancellationToken();

        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user1 = new User(Guid.NewGuid(), "user1", Guid.NewGuid().ToString());
        var user2 = new User(Guid.NewGuid(), "user2", Guid.NewGuid().ToString());

        var question = new Question("question_test");
        var question1 = new Question("question_test_1");

        var room = new Domain.Rooms.Room("test", SERoomAccessType.Private, SERoomType.Standard);

        memoryDatabase.Users.AddRange(user1, user2);
        await memoryDatabase.SaveChangesAsync();
        memoryDatabase.Rooms.Add(room);
        memoryDatabase.Questions.AddRange(question, question1);

        var roomQuestions = new[]
        {
            (Question: question, User: user1, State: RoomQuestionState.Active),
            (Question: question1, User: user1, State: RoomQuestionState.Active),
        }
        .Select(e => new RoomQuestion
        {
            RoomId = default,
            QuestionId = default,
            Room = room,
            Question = e.Question,
            State = e.State,
            Order = 0,
            CreatedById = e.User.Id
        })
        .ToList();
        memoryDatabase.RoomQuestions.AddRange(roomQuestions);

        memoryDatabase.RoomQuestionEvaluation.AddRange(roomQuestions.Select(e => new RoomQuestionEvaluation
        {
            RoomQuestionId = e.Id,
            CreatedById = e.CreatedById,
            State = SERoomQuestionEvaluationState.Draft
        }));

        var roomParticipant1 = new RoomParticipant(user1, room, SERoomParticipantType.Expert);
        var roomParticipant2 = new RoomParticipant(user2, room, SERoomParticipantType.Expert);

        memoryDatabase.RoomParticipants.AddRange(roomParticipant1, roomParticipant2);

        var roomReview1 = new RoomReview(roomParticipant1, SERoomReviewState.Closed);

        memoryDatabase.RoomReview.AddRange(roomReview1);

        await memoryDatabase.SaveChangesAsync(cancellationToken);
        await memoryDatabase.SaveChangesAsync();

        var roomRepository = new RoomRepository(memoryDatabase);

        var isReadyToCloseAsync = await roomRepository.IsReadyToCloseAsync(room.Id, cancellationToken);

        isReadyToCloseAsync.Should().BeTrue();
    }

    [Fact]
    public async Task GetInvitesAsync()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);

        var generatedRooms = Enumerable.Range(0, 5)
            .Select(i => new Domain.Rooms.Room(DefaultRoomName + i, SERoomAccessType.Public, SERoomType.Standard)).ToList();
        appDbContext.Rooms.AddRange(generatedRooms);
        var roomInvites = generatedRooms.SelectMany(GenerateInvites).ToList();
        appDbContext.RoomInvites.AddRange(roomInvites);
        await appDbContext.SaveChangesAsync();

        var checkRoom = appDbContext.Rooms.AsEnumerable().OrderBy(_ => Guid.NewGuid()).First();
        var expectInvites = appDbContext.RoomInvites.Where(e => e.RoomId == checkRoom.Id)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteId,
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
        var roomPatchUpdateRequest = new RoomUpdateRequest { Name = "new_value_name_room", Questions = [], };
        var roomId = Guid.NewGuid();

        var roomService = CreateRoomService(appDbContext);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            roomService.UpdateAsync(roomId, roomPatchUpdateRequest));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(160)]
    public async Task UpdateTimer_When_Timer_Should_Be_Created(long durationSec)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var room = new Domain.Rooms.Room("test", SERoomAccessType.Public, SERoomType.Standard);
        appDbContext.Rooms.Add(room);

        var question = new Question("question_value#1");
        appDbContext.Questions.AddRange(question);

        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "test",
            Questions = [new() { Id = question.Id, Order = 0 }],
            DurationSec = durationSec
        };

        var roomService = CreateRoomService(appDbContext);

        await roomService.UpdateAsync(room.Id, roomPatchUpdateRequest);

        var actualRoom = await appDbContext.Rooms.Include(e => e.Timer).FirstOrDefaultAsync(e => e.Id == room.Id);

        actualRoom.Should().NotBeNull();
        actualRoom!.Timer.Should().NotBeNull();
        actualRoom!.Timer!.Duration.Should().Be(TimeSpan.FromSeconds(durationSec));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(160)]
    public async Task UpdateTimer_When_Timer_Should_Be_Deleted(long durationSec)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var room = new Domain.Rooms.Room("test", SERoomAccessType.Public, SERoomType.Standard) { Timer = new RoomTimer { Duration = TimeSpan.FromSeconds(durationSec), } };
        appDbContext.Rooms.Add(room);

        var question = new Question("question_value#1");
        appDbContext.Questions.AddRange(question);

        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var initialTimeId = room.Timer!.Id;
        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "test",
            Questions = [new() { Id = question.Id, Order = 0 }],
            DurationSec = null
        };

        var roomService = CreateRoomService(appDbContext);

        await roomService.UpdateAsync(room.Id, roomPatchUpdateRequest);

        var hasTime = await appDbContext.RoomTimers.AnyAsync(e => e.Id == initialTimeId);
        var actualRoom = await appDbContext.Rooms.Include(e => e.Timer).FirstOrDefaultAsync(e => e.Id == room.Id);

        actualRoom.Should().NotBeNull();
        actualRoom!.Timer.Should().BeNull();
        hasTime.Should().BeFalse();
    }

    [Theory]
    [InlineData(60, 0)]
    [InlineData(0, 60)]
    [InlineData(500, 160)]
    public async Task UpdateTimer_When_Timer_Should_Be_Updated(long initialDurationSec, long durationSec)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var room = new Domain.Rooms.Room("test", SERoomAccessType.Public, SERoomType.Standard) { Timer = new RoomTimer { Duration = TimeSpan.FromSeconds(initialDurationSec), } };
        appDbContext.Rooms.Add(room);

        var question = new Question("question_value#1");
        appDbContext.Questions.AddRange(question);

        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var initialTimeId = room.Timer!.Id;
        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = "test",
            Questions = [new() { Id = question.Id, Order = 0 }],
            DurationSec = durationSec
        };

        var roomService = CreateRoomService(appDbContext);

        await roomService.UpdateAsync(room.Id, roomPatchUpdateRequest);

        var actualRoom = await appDbContext.Rooms.Include(e => e.Timer).FirstOrDefaultAsync(e => e.Id == room.Id);

        actualRoom.Should().NotBeNull();
        actualRoom!.Timer.Should().NotBeNull();
        actualRoom!.Timer!.Duration.Should().Be(TimeSpan.FromSeconds(durationSec));
        actualRoom!.Timer!.Id.Should().Be(initialTimeId);
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
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomCreateRequest = new RoomCreateRequest
        {
            Questions = [new() { Id = question.Id, Order = 10 }],
            Experts = [],
            Examinees = [],
            Tags = [],
            Name = "My room",
            AccessType = SERoomAccessType.Public,
            ScheduleStartTime = new DateTime(2024, 1, 1, 0, 0, 0),
        };

        var createdRoom = await roomService.CreateAsync(roomCreateRequest, CancellationToken.None);

        var dbRoom = await appDbContext.Rooms.Include(e => e.Questions).FirstAsync(e => e.Id == createdRoom.Id);

        dbRoom.Name.Should().Be("My room");
        dbRoom.AccessType!.Should().Be(SERoomAccessType.Public);
        dbRoom.Questions.Should().HaveCount(1);
        dbRoom.Questions[0].Order.Should().Be(roomCreateRequest.Questions[0].Order);
        dbRoom.Questions[0].QuestionId.Should().Be(roomCreateRequest.Questions[0].Id);
    }

    [Fact]
    public async Task Create_Room_With_QuestionTree()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var tree = new QuestionTree
        {
            Name = "dummy",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Question = new Question("test"), Type = SEQuestionSubjectTreeType.Question },
        };

        appDbContext.QuestionTree.Add(tree);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomCreateRequest = new RoomCreateRequest
        {
            Questions = [],
            Experts = [],
            Examinees = [],
            Tags = [],
            Name = "My room",
            AccessType = SERoomAccessType.Public,
            ScheduleStartTime = new DateTime(2024, 1, 1, 0, 0, 0),
            QuestionTreeId = tree.Id,
        };

        var createdRoom = await roomService.CreateAsync(roomCreateRequest, CancellationToken.None);

        var dbRoom = await appDbContext.Rooms.Include(e => e.Questions).FirstAsync(e => e.Id == createdRoom.Id);

        dbRoom.Name.Should().Be("My room");
        dbRoom.AccessType!.Should().Be(SERoomAccessType.Public);
        dbRoom.Questions.Should().HaveCount(1);
        dbRoom.Questions[0].QuestionId.Should().Be(tree.RootQuestionSubjectTree!.QuestionId!.Value);
    }

    [Theory]
    [InlineData(EVRoomStatus.New)]
    [InlineData(EVRoomStatus.Active)]
    public async Task Create_Should_Create_Room_When_Have_Exists_Active_Room_For_Another_User_With_QuestionTree(EVRoomStatus status)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user0 = new User("test 1", "test 1");
        appDbContext.Users.Add(user0);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var tree = new QuestionTree
        {
            Name = "dummy",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Question = new Question("test"), Type = SEQuestionSubjectTreeType.Question },
        };

        appDbContext.QuestionTree.Add(tree);
        appDbContext.Rooms.Add(new Domain.Rooms.Room("test room", SERoomAccessType.Private, SERoomType.AI)
        {
            QuestionTreeId = tree.Id,
            Status = SERoomStatus.FromEnum(status),
            CreatedById = user0.Id
        });
        await appDbContext.SaveChangesAsync();
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomCreateRequest = new RoomCreateRequest
        {
            Questions = [],
            Experts = [],
            Examinees = [],
            Tags = [],
            Name = "My room",
            AccessType = SERoomAccessType.Public,
            ScheduleStartTime = new DateTime(2024, 1, 1, 0, 0, 0),
            QuestionTreeId = tree.Id,
        };

        var createdRoom = await roomService.CreateAsync(roomCreateRequest, CancellationToken.None);

        var dbRoom = await appDbContext.Rooms.Include(e => e.Questions).FirstAsync(e => e.Id == createdRoom.Id);

        dbRoom.Name.Should().Be("My room");
        dbRoom.AccessType!.Should().Be(SERoomAccessType.Public);
        dbRoom.Questions.Should().HaveCount(1);
        dbRoom.Questions[0].QuestionId.Should().Be(tree.RootQuestionSubjectTree!.QuestionId!.Value);
    }

    [Theory]
    [InlineData(EVRoomStatus.New)]
    [InlineData(EVRoomStatus.Active)]
    public async Task Create_Should_Throw_Error_When_Exists_Active_Room_With_QuestionTree(EVRoomStatus status)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var tree = new QuestionTree
        {
            Name = "dummy",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Question = new Question("test"), Type = SEQuestionSubjectTreeType.Question },
        };

        appDbContext.QuestionTree.Add(tree);
        await appDbContext.SaveChangesAsync();
        appDbContext.Rooms.Add(new Domain.Rooms.Room("test room", SERoomAccessType.Private, SERoomType.AI)
        {
            QuestionTreeId = tree.Id,
            Status = SERoomStatus.FromEnum(status),
            CreatedById = user.Id
        });
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomCreateRequest = new RoomCreateRequest
        {
            Questions = [],
            Experts = [],
            Examinees = [],
            Tags = [],
            Name = "My room",
            AccessType = SERoomAccessType.Public,
            ScheduleStartTime = new DateTime(2024, 1, 1, 0, 0, 0),
            QuestionTreeId = tree.Id,
        };

        var ex = await Assert.ThrowsAsync<UserException>(() => roomService.CreateAsync(roomCreateRequest, CancellationToken.None));

        ex.Message.Should().Be($"Room with id {tree.Id} already exists");
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
        var initialRoom = new Domain.Rooms.Room("My room", SERoomAccessType.Public, SERoomType.Standard)
        {
            Questions =
            [
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
                }

            ]
        };
        appDbContext.Rooms.Add(initialRoom);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var roomService = CreateRoomService(appDbContext, user);
        var roomUpdateRequest = new RoomUpdateRequest
        {
            Name = "My room 2",
            Tags = [],
            Questions = [new() { Id = question2.Id, Order = 128 }, new() { Id = question3.Id, Order = -1000, }]
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

    private IEnumerable<RoomInvite> GenerateInvites(Domain.Rooms.Room room)
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

        var time = new TestSystemClock { UtcNow = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), };

        var roomParticipantService = new RoomParticipantService(
            new RoomParticipantRepository(appDbContext),
            new RoomRepository(appDbContext),
            new UserRepository(appDbContext),
            userAccessor,
            new PermissionRepository(appDbContext));

        return new RoomService(
            new EmptyRoomEventDispatcher(),
            new EmptyHotEventStorage(),
            new RoomInviteService(appDbContext, roomParticipantService, NullLogger<RoomInviteService>.Instance),
            userAccessor,
            roomParticipantService,
            appDbContext,
            new NullLogger<RoomService>(),
            time,
            new RoomAnalyticService(appDbContext),
            new RoomStatusUpdater(appDbContext, new RoomQuestionRepository(appDbContext)));
    }
}
