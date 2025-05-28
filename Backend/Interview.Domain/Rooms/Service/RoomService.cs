using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.QuestionTreeById;
using Interview.Domain.Questions.QuestionTreePage;
using Interview.Domain.Reactions;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.RoomQuestionReactions.Mappers;
using Interview.Domain.Rooms.RoomQuestionReactions.Specifications;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using NSpecifications;
using X.PagedList;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Rooms.Service;

public sealed class RoomService(
    IRoomEventDispatcher roomEventDispatcher,
    IHotEventStorage hotEventStorage,
    IRoomInviteService roomInviteService,
    ICurrentUserAccessor currentUserAccessor,
    IRoomParticipantService roomParticipantService,
    AppDbContext db,
    ILogger<RoomService> logger,
    ISystemClock clock,
    RoomAnalyticService roomAnalyticService,
    RoomStatusUpdater roomStatusUpdater)
    : IRoomServiceWithoutPermissionCheck
{
    public async Task<IPagedList<RoomPageDetail>> FindPageAsync(
        RoomPageDetailRequestFilter filter,
        int pageNumber,
        int pageSize,
        EVSortOrder dateSort,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Room> queryable = db.Rooms
            .Include(e => e.Participants)
            .ThenInclude(e => e.User)
            .Include(e => e.Configuration)
            .Include(e => e.Tags)
            .Include(e => e.Timer)
            .OrderBy(e => e.Status == SERoomStatus.Active ? 1 :
                e.Status == SERoomStatus.Review ? 2 :
                e.Status == SERoomStatus.New ? 3 :
                4)
            .ThenBy(e => e.ScheduleStartTime, dateSort);
        var filterName = filter.Name?.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(filterName))
        {
            queryable = queryable.Where(e => e.Name.ToLower().Contains(filterName));
        }

        if (filter.StartValue is not null)
        {
            queryable = queryable.Where(e => filter.StartValue <= e.ScheduleStartTime);
        }

        if (filter.EndValue is not null)
        {
            queryable = queryable.Where(e => filter.EndValue >= e.ScheduleStartTime);
        }

        if (filter.Statuses is not null && filter.Statuses.Count > 0)
        {
            var mapStatuses = filter.Statuses.Join(
                SERoomStatus.List,
                status => status,
                status => status.EnumValue,
                (_, roomStatus) => roomStatus).ToList();
            queryable = queryable.Where(e => mapStatuses.Contains(e.Status));
        }

        if (!currentUserAccessor.IsAdmin())
        {
            var currentUserId = currentUserAccessor.GetUserIdOrThrow();
            queryable = queryable.Where(e =>
                e.AccessType == SERoomAccessType.Public || (e.AccessType == SERoomAccessType.Private &&
                                                            e.Participants.Any(p => currentUserId == p.User.Id)));
        }

        if (filter.Participants is not null && filter.Participants.Count > 0)
        {
            queryable = queryable.Where(e => e.Participants.Any(p => filter.Participants.Contains(p.User.Id)));
        }

        var tmpRes = await queryable
            .AsSplitQuery()
            .Select(e => new
            {
                Id = e.Id,
                Name = e.Name,
                Participants = e.Participants.Select(participant =>
                        new RoomUserDetail
                        {
                            Id = participant.User.Id,
                            Nickname = participant.User.Nickname,
                            Avatar = participant.User.Avatar,
                            Type = participant.Type.Name,
                        })
                    .ToList(),
                RoomStatus = e.Status.EnumValue,
                Tags = e.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
                Timer = e.Timer == null ? null : new { Duration = e.Timer.Duration, ActualStartTime = e.Timer.ActualStartTime, },
                ScheduledStartTime = e.ScheduleStartTime,
                Type = e.Type,
            })
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        return tmpRes.ConvertAll(e => new RoomPageDetail
        {
            Id = e.Id,
            Name = e.Name,
            Participants = e.Participants,
            Status = e.RoomStatus,
            Tags = e.Tags,
            Timer = e.Timer == null ? null : new RoomTimerDetail { DurationSec = (long)e.Timer.Duration.TotalSeconds, StartTime = e.Timer.ActualStartTime, },
            ScheduledStartTime = e.ScheduledStartTime,
            Type = e.Type.EnumValue,
        });
    }

    public async Task<List<RoomCalendarItem>> GetCalendarAsync(RoomCalendarRequest filter, CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUserAccessor.GetUserIdOrThrow();
        var spec = BuildSpecification(filter);
        var rooms = await db.RoomParticipants
            .AsNoTracking()
            .Where(e => e.UserId == currentUserId)
            .Select(e => e.Room)
            .Where(spec)
            .OrderBy(room => room.ScheduleStartTime)
            .Select(room => new { Time = room.ScheduleStartTime, room.Status })
            .ToListAsync(cancellationToken);

        var offset = TimeSpan.FromMinutes(filter.TimeZoneOffset);
        return rooms
            .Select(roomInfo => new
            {
                TimeOffset = roomInfo.Time.Add(offset),
                UtcTime = roomInfo.Time,
                roomInfo.Status,
            })
            .GroupBy(info => info.TimeOffset.Date)
            .Select(values => new RoomCalendarItem
            {
                MinScheduledStartTime = values.MinBy(it => it.UtcTime)!.UtcTime,
                Statuses = values.Select(it => it.Status.EnumValue).ToList(),
            })
            .ToList();

        static ASpec<Room> BuildSpecification(RoomCalendarRequest filter)
        {
            var res = Spec<Room>.Any;
            if (filter.StartDateTime is not null)
            {
                res &= new Spec<Room>(e => filter.StartDateTime <= e.ScheduleStartTime);
            }

            if (filter.EndDateTime is not null)
            {
                res &= new Spec<Room>(e => filter.EndDateTime >= e.ScheduleStartTime);
            }

            if (filter.RoomStatus is not null && filter.RoomStatus.Count > 0)
            {
                var mapStatuses = filter.RoomStatus.Select(SERoomStatus.FromEnum).ToList();
                res &= new Spec<Room>(e => mapStatuses.Contains(e.Status));
            }

            return res;
        }
    }

    public async Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var res = await db.Rooms
            .AsSplitQuery()
            .Include(e => e.Participants)
            .Include(e => e.Configuration)
            .Include(e => e.Timer)
            .Include(e => e.QuestionTree)
            .Include(e => e.Questions).ThenInclude(e => e.Question).ThenInclude(e => e!.Answers)
            .Include(e => e.Questions).ThenInclude(e => e.Question).ThenInclude(e => e!.CodeEditor)
            .Select(e => new
            {
                Id = e.Id,
                Name = e.Name,
                Owner = new RoomUserDetail
                {
                    Id = e.CreatedBy!.Id,
                    Nickname = e.CreatedBy!.Nickname,
                    Avatar = e.CreatedBy!.Avatar,
                    Type = null,
                },
                Participants = e.Participants.Select(participant =>
                        new RoomUserDetail
                        {
                            Id = participant.User.Id,
                            Nickname = participant.User.Nickname,
                            Avatar = participant.User.Avatar,
                            Type = participant.Type.Name,
                        })
                    .ToList(),
                Status = e.Status.EnumValue,
                Invites = e.Invites.Select(roomInvite => new RoomInviteResponse()
                {
                    InviteId = roomInvite.InviteId,
                    ParticipantType = roomInvite.ParticipantType!.EnumValue,
                    Max = roomInvite.Invite!.UsesMax,
                    Used = roomInvite.Invite.UsesCurrent,
                })
                    .ToList(),
                Type = e.AccessType.EnumValue,
                Timer = e.Timer == null ? null : new { Duration = e.Timer.Duration, ActualStartTime = e.Timer.ActualStartTime, },
                ScheduledStartTime = e.ScheduleStartTime,
                Questions = e.Questions.Select(q => new RoomQuestionDetail
                {
                    Id = q.QuestionId,
                    Order = q.Order,
                    Value = q.Question!.Value,
                    Answers = q.Question!.Answers.Select(a => new QuestionAnswerResponse
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Content = a.Content,
                        CodeEditor = a.CodeEditor,
                    })
                        .ToList(),
                    CodeEditor = q.Question.CodeEditor == null
                        ? null
                        : new QuestionCodeEditorResponse
                        {
                            Content = q.Question.CodeEditor.Content,
                            Lang = q.Question.CodeEditor.Lang,
                        },
                }).ToList(),
                QuestionTree = e.QuestionTree != null
                    ? new QuestionTreeByIdResponse
                    {
                        Id = e.QuestionTree.Id,
                        Name = e.QuestionTree.Name,
                        RootQuestionSubjectTreeId = e.QuestionTree.RootQuestionSubjectTreeId,
                        Tree = new List<QuestionTreeByIdResponseTree>(),
                    }
                    : null,
            })
            .FirstOrDefaultAsync(room => room.Id == id, cancellationToken: cancellationToken) ?? throw NotFoundException.Create<Room>(id);

        if (res.QuestionTree is not null)
        {
            await res.QuestionTree.FillTreeAsync(db, cancellationToken);
        }

        return new RoomDetail
        {
            Id = res.Id,
            Name = res.Name,
            Owner = res.Owner,
            Participants = res.Participants,
            Status = res.Status,
            Invites = res.Invites,
            Type = res.Type,
            Timer = res.Timer == null
                ? null
                : new RoomTimerDetail
                {
                    DurationSec = (long)res.Timer.Duration.TotalSeconds,
                    StartTime = res.Timer.ActualStartTime,
                },
            ScheduledStartTime = res.ScheduledStartTime,
            Questions = res.Questions,
            QuestionTree = res.QuestionTree,
        };
    }

    public async Task<RoomPageDetail> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new UserException(nameof(request));
        }

        var name = request.Name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new UserException("Room name should not be empty");
        }

        var currentUserId = currentUserAccessor.GetUserIdOrThrow();
        ICollection<Guid> requestExperts = request.Experts;
        if (!request.Experts.Contains(currentUserId) && !request.Examinees.Contains(currentUserId))
        {
            // If the current user is not listed as a member of the room, add him/her with the role 'Expert'
            requestExperts = requestExperts.Concat([currentUserId]).ToList();
        }

        EnsureValidScheduleStartTime(request.ScheduleStartTime, null);

        var experts = await FindByIdsOrErrorAsync(db.Users, requestExperts, "experts", cancellationToken);
        var examinees = await FindByIdsOrErrorAsync(db.Users, request.Examinees, "examinees", cancellationToken);
        var tags = await Tag.EnsureValidTagsAsync(db.Tag, request.Tags, cancellationToken);
        var type = request.QuestionTreeId is not null ? SERoomType.AI : SERoomType.Standard;
        var room = new Room(name, request.AccessType, type)
        {
            Tags = tags,
            ScheduleStartTime = request.ScheduleStartTime,
            Timer = CreateRoomTimer(request.DurationSec),
            QuestionTreeId = request.QuestionTreeId,
        };

        if (request.QuestionTreeId is not null)
        {
            var questionTree = await db.QuestionTree
                .Select(e => new { Id = e.Id, Name = e.Name, RootQuestionSubjectTreeId = e.RootQuestionSubjectTreeId, })
                .FirstAsync(e => e.Id == request.QuestionTreeId, cancellationToken);
            var allSubjectTreeIds = await db.QuestionSubjectTree.GetAllChildrenAsync(questionTree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
            var questionsFromTree = await db.QuestionSubjectTree.AsNoTracking()
                .Where(e => allSubjectTreeIds.Contains(e.Id) && e.QuestionId != null)
                .Select(e => new
                {
                    Id = e.QuestionId!.Value,
                    e.Order,
                })
                .ToListAsync(cancellationToken);

            foreach (var question in questionsFromTree)
            {
                request.Questions.Add(new RoomQuestionRequest
                {
                    Id = question.Id,
                    Order = question.Order,
                });
            }
        }

        var requiredQuestions = request.Questions.Select(e => e.Id).ToHashSet();
        if (requiredQuestions.Count == 0)
        {
            throw new UserException("The room must contain at least one question");
        }

        var questions =
            await FindByIdsOrErrorAsync(db.Questions, requiredQuestions, "questions", cancellationToken);
        var roomQuestions = questions
            .Join(request.Questions,
                e => e.Id,
                e => e.Id,
                (dbQ, requestQ) => new RoomQuestion
                {
                    Room = room,
                    Question = dbQ,
                    State = RoomQuestionState.Open,
                    RoomId = default,
                    QuestionId = default,
                    Order = requestQ.Order,
                })
            .OrderBy(e => e.Order);

        room.Questions.AddRange(roomQuestions);

        var participants = experts
            .Select(e => (e, room, SERoomParticipantType.Expert))
            .Concat(examinees.Select(e => (e, room, SERoomParticipantType.Examinee)))
            .ToList();

        return await db.RunTransactionAsync(async _ =>
            {
                await db.Rooms.AddAsync(room, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                await roomParticipantService.CreateAsync(room.Id, participants, cancellationToken);

                await GenerateInvitesAsync(room.Id, cancellationToken);

                return new RoomPageDetail
                {
                    Id = room.Id,
                    Name = room.Name,
                    Participants = room.Participants.Select(participant =>
                            new RoomUserDetail
                            {
                                Id = participant.User.Id,
                                Nickname = participant.User.Nickname,
                                Avatar = participant.User.Avatar,
                                Type = participant.Type.Name,
                            })
                        .ToList(),
                    Status = room.Status.EnumValue,
                    Tags = room.Tags.Select(t => new TagItem
                    {
                        Id = t.Id,
                        Value = t.Value,
                        HexValue = t.HexColor,
                    })
                        .ToList(),
                    Timer = room.Timer == null
                        ? null
                        : new RoomTimerDetail
                        {
                            DurationSec = (long)room.Timer.Duration.TotalSeconds,
                            StartTime = room.Timer.ActualStartTime,
                        },
                    ScheduledStartTime = room.ScheduleStartTime,
                    Type = room.Type.EnumValue,
                };
            },
            cancellationToken);
    }

    public async Task<RoomItem> UpdateAsync(Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new UserException($"Room update request should not be null [{nameof(request)}]");
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new UserException("Room name should not be empty");
        }

        var foundRoom = await db.Rooms
            .Include(e => e.Questions)
            .Include(e => e.Tags)
            .Include(e => e.Timer)
            .FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);
        if (foundRoom is null)
        {
            throw NotFoundException.Create<User>(roomId);
        }

        if (foundRoom.QuestionTreeId is not null && request.QuestionTreeId is null)
        {
            throw new UserException("Question tree id should not be null");
        }

        if (foundRoom.QuestionTreeId is null && request.QuestionTreeId is not null)
        {
            throw new UserException("Question tree id should be null");
        }

        EnsureAvailableRoomEdit(foundRoom);
        EnsureValidScheduleStartTime(request.ScheduleStartTime, foundRoom.ScheduleStartTime);

        var tags = await Tag.EnsureValidTagsAsync(db.Tag, request.Tags, cancellationToken);

        if (request.QuestionTreeId != null && request.QuestionTreeId != (foundRoom.QuestionTreeId ?? Guid.Empty))
        {
            // When a user sets up a category, you must remove all questions from the room.
            request.Questions.Clear();

            var questionTree = await db.QuestionTree
                .Select(e => new { Id = e.Id, Name = e.Name, RootQuestionSubjectTreeId = e.RootQuestionSubjectTreeId, })
                .FirstAsync(e => e.Id == request.QuestionTreeId, cancellationToken);
            var allSubjectTreeIds = await db.QuestionSubjectTree.GetAllChildrenAsync(questionTree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
            var questions = await db.QuestionSubjectTree.AsNoTracking()
                .Where(e => allSubjectTreeIds.Contains(e.Id) && e.QuestionId != null)
                .Select(e => new
                {
                    Id = e.QuestionId!.Value,
                    e.Order,
                })
                .ToListAsync(cancellationToken);

            foreach (var question in questions)
            {
                request.Questions.Add(new RoomQuestionRequest
                {
                    Id = question.Id,
                    Order = question.Order,
                });
            }
        }

        var requiredQuestions = request.Questions.Select(e => e.Id).ToHashSet();

        if (requiredQuestions.Count == 0)
        {
            throw new UserException("The room must contain at least one question");
        }

        foundRoom.Questions.RemoveAll(e => !requiredQuestions.Contains(e.QuestionId));
        foreach (var (dbQuestions, order) in foundRoom.Questions
                     .Join(request.Questions,
                         e => e.QuestionId,
                         e => e.Id,
                         (question, questionRequest) => (DbQuesstions: question, questionRequest.Order)))
        {
            dbQuestions.Order = order;
        }

        requiredQuestions.ExceptWith(foundRoom.Questions.Select(e => e.QuestionId));
        foreach (var roomQuestionRequest in requiredQuestions.Join(
                     request.Questions,
                     id => id,
                     e => e.Id,
                     (_, questionRequest) => questionRequest))
        {
            foundRoom.Questions.Add(new RoomQuestion
            {
                RoomId = foundRoom.Id,
                QuestionId = roomQuestionRequest.Id,
                Room = null,
                Question = null,
                State = RoomQuestionState.Open,
                Order = roomQuestionRequest.Order,
            });
        }

        if (request.DurationSec is null)
        {
            if (foundRoom.Timer is not null)
            {
                db.RoomTimers.Remove(foundRoom.Timer);
                foundRoom.Timer = null;
            }
        }
        else
        {
            var timer = CreateRoomTimer(request.DurationSec);
            if (foundRoom.Timer is null)
            {
                foundRoom.Timer = timer;
            }
            else
            {
                foundRoom.Timer.Duration = timer!.Duration;
            }
        }

        foundRoom.ScheduleStartTime = request.ScheduleStartTime;
        foundRoom.Name = name;
        foundRoom.QuestionTreeId = request.QuestionTreeId;
        foundRoom.Tags.Clear();
        foundRoom.Tags.AddRange(tags);
        db.Update(foundRoom);
        await db.SaveChangesAsync(cancellationToken);

        return new RoomItem
        {
            Id = foundRoom.Id,
            Name = foundRoom.Name,
            Tags = tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
        };
    }

    public async Task<(Room, RoomParticipant)> AddParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        var currentRoom = await db.Rooms.FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        EnsureAvailableRoomEdit(currentRoom);
        var user = await db.Users.FirstOrDefaultAsync(e => e.Id == userId, cancellationToken);
        if (user is null)
        {
            throw NotFoundException.Create<User>(userId);
        }

        var participant = await db.RoomParticipants.FirstOrDefaultAsync(
            roomParticipant => roomParticipant.Room.Id == roomId && roomParticipant.User.Id == userId,
            cancellationToken);
        if (participant is not null)
        {
            return (currentRoom, participant);
        }

        return await db.RunTransactionAsync(async _ =>
            {
                var participants = await roomParticipantService.CreateAsync(
                    roomId,
                    [(user, currentRoom, SERoomParticipantType.Viewer)],
                    cancellationToken);
                participant = participants.First();
                return (currentRoom, participant);
            },
            cancellationToken);
    }

    public async Task SendEventRequestAsync(IEventRequest request, CancellationToken cancellationToken = default)
    {
        var dbEvent = await db.AppEvent
            .Include(appEvent => appEvent.Roles)
            .FirstOrDefaultAsync(e => e.Type == request.Type, cancellationToken);
        if (dbEvent is null)
        {
            throw new NotFoundException($"Event not found by type {request.Type}");
        }

        var currentRoom = await db.Rooms.FirstOrDefaultAsync(e => e.Id == request.RoomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var user = await db.Users.AsNoTracking()
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            throw NotFoundException.Create<User>(request.UserId);
        }

        var userRoles = user.Roles.Select(e => e.Id).ToHashSet();
        if (dbEvent.Roles is not null && dbEvent.Roles.Count > 0 && dbEvent.Roles.All(e => !userRoles.Contains(e.Id)))
        {
            throw new AccessDeniedException("The user does not have the required role");
        }

        if (dbEvent.ParticipantTypes is not null && dbEvent.ParticipantTypes.Count > 0)
        {
            var participantType = await EnsureParticipantTypeAsync(request.RoomId, request.UserId, cancellationToken);

            if (dbEvent.ParticipantTypes.All(e => e != participantType.Type))
            {
                throw new AccessDeniedException("The user does not have the required participant type");
            }
        }

        await roomEventDispatcher.WriteAsync(request.ToRoomEvent(dbEvent.Stateful), cancellationToken);
    }

    /// <summary>
    /// Close non closed room.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="cancellationToken">Token.</param>
    /// <returns>Result.</returns>
    public Task CloseAsync(Guid roomId, CancellationToken cancellationToken = default) => roomStatusUpdater.CloseWithoutReviewAsync(roomId, cancellationToken: cancellationToken);

    public Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken) => roomStatusUpdater.StartReviewAsync(roomId, cancellationToken: cancellationToken);

    public async Task<ActualRoomStateResponse> GetActualStateAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var roomState = await db.Rooms
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.RoomStates)
            .Include(e => e.QuestionTree)
            .Where(e => e.Id == roomId)
            .Select(room => new ActualRoomStateResponse
            {
                Id = room.Id,
                Name = room.Name,
                ActiveQuestion = room.Questions.Select(q => new RoomStateQuestion
                {
                    Id = q.Id,
                    Value = q.Question!.Value,
                    State = q.State!,
                }).FirstOrDefault(q => q.State == RoomQuestionState.Active),
                CodeEditor = new CodeEditorStateResponse
                {
                    Content = room.Configuration == null ? null : room.Configuration.CodeEditorContent,
                    Enabled = room.Configuration != null && room.Configuration.CodeEditorEnabled,
                },
                States = room.RoomStates.Select(e => new RoomStateResponse
                {
                    Payload = e.Payload,
                    Type = e.Type,
                }).ToList(),
                QuestionTree = room.QuestionTree != null
                    ? new QuestionTreeByIdResponse
                    {
                        Id = room.QuestionTree.Id,
                        Name = room.QuestionTree.Name,
                        RootQuestionSubjectTreeId = room.QuestionTree.RootQuestionSubjectTreeId,
                        Tree = new List<QuestionTreeByIdResponseTree>(),
                    }
                    : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (roomState == null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        if (roomState.QuestionTree is not null)
        {
            await roomState.QuestionTree.FillTreeAsync(db, cancellationToken);
        }

        var spec = new RoomReactionsSpecification(roomId);
        var reactions = await db.RoomQuestionReactions
            .Include(e => e.Reaction)
            .Where(spec)
            .Select(ReactionTypeOnlyMapper.Instance.Expression)
            .ToListAsync(cancellationToken);

        roomState.DislikeCount = reactions.Count(e => e == ReactionType.Dislike);
        roomState.LikeCount = reactions.Count(e => e == ReactionType.Like);

        return roomState;
    }

    public async Task UpsertRoomStateAsync(Guid roomId, string type, string payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new UserException("The type cannot be empty.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);
        if (room is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        EnsureAvailableRoomEdit(room);
        var state = await db.RoomStates.FirstOrDefaultAsync(e => e.RoomId == roomId && e.Type == type, cancellationToken);
        if (state is not null)
        {
            state.Payload = payload;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        state = new RoomState
        {
            Payload = payload,
            RoomId = roomId,
            Type = type,
            Room = null,
        };
        await db.RoomStates.AddAsync(state, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRoomStateAsync(Guid roomId, string type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new UserException("The type cannot be empty.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);
        if (room is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        EnsureAvailableRoomEdit(room);

        var state = await db.RoomStates.FirstOrDefaultAsync(e => e.RoomId == roomId && e.Type == type, cancellationToken);
        if (state is null)
        {
            throw new UserException($"No room state with type '{type}' was found");
        }

        db.RoomStates.Remove(state);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<Analytics> GetAnalyticsAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
        => roomAnalyticService.GetAsync(request, cancellationToken);

    public Task<List<RoomInviteResponse>> GetInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return db.RoomInvites.AsNoTracking()
            .Include(roomInvite => roomInvite.Invite)
            .Where(e => e.RoomId == roomId && e.ParticipantType != null)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteId,
                ParticipantType = e.ParticipantType!.EnumValue,
                Max = e.Invite!.UsesMax,
                Used = e.Invite.UsesCurrent,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GenerateInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        List<RoomInviteResponse> invites = [];

        foreach (var participantType in SERoomParticipantType.List)
        {
            invites.Add(await roomInviteService.GenerateAsync(roomId, participantType, 20, cancellationToken));
        }

        return invites;
    }

    public Task<RoomInviteResponse> GenerateInviteAsync(RoomInviteGeneratedRequest roomInviteGenerated, CancellationToken cancellationToken = default)
    {
        return roomInviteService.GenerateAsync(
            roomInviteGenerated.RoomId,
            SERoomParticipantType.FromEnum(roomInviteGenerated.ParticipantType),
            20,
            cancellationToken);
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var questions = await db.RoomQuestions.AsNoTracking()
            .Include(e => e.Question)
            .Include(e => e.Room)
            .Where(e => e.Room!.Id == request.RoomId)
            .Select(e => new { e.Question!.Id, e.Question!.Value })
            .ToListAsync(cancellationToken);

        var reactions = await db.RoomQuestionReactions.AsNoTracking()
            .Include(e => e.RoomQuestion).ThenInclude(e => e!.Question)
            .Include(e => e.RoomQuestion).ThenInclude(e => e!.Room)
            .Include(e => e.Reaction)
            .Include(e => e.Sender)
            .Where(e => e.RoomQuestion!.Room!.Id == request.RoomId)
            .ToListAsync(cancellationToken);

        var participants = await db.RoomParticipants.AsNoTracking()
            .Include(e => e.Room)
            .Include(e => e.User)
            .Where(e => e.Room.Id == request.RoomId)
            .ToDictionaryAsync(e => e.User.Id, e => e.Type, cancellationToken);

        var summary = new AnalyticsSummary { Questions = new List<AnalyticsSummaryQuestion>(questions.Count), };
        foreach (var question in questions)
        {
            var reactionQuestions = reactions
                .Where(e => e.RoomQuestion!.Question!.Id == question.Id)
                .ToList();

            var viewers = reactionQuestions
                .Where(e => participants[e.Sender!.Id] == SERoomParticipantType.Viewer)
                .GroupBy(e => participants[e.Sender!.Id])
                .Select(e => new AnalyticsSummaryViewer
                {
                    ReactionsSummary = e.GroupBy(t => (t.Reaction!.Id, t.Reaction.Type))
                        .Select(t => new Analytics.AnalyticsReactionSummary { Id = t.Key.Id, Type = t.Key.Type.Name, Count = t.Count(), })
                        .ToList(),
                })
                .ToList();

            var experts = reactionQuestions
                .Where(e => participants[e.Sender!.Id] == SERoomParticipantType.Expert)
                .GroupBy(e => (e.Sender!.Id, e.Sender!.Nickname))
                .Select(e => new AnalyticsSummaryExpert
                {
                    Nickname = e.Key.Nickname,
                    ReactionsSummary = e.GroupBy(t => (t.Reaction!.Id, t.Reaction.Type))
                        .Select(t => new Analytics.AnalyticsReactionSummary { Id = t.Key.Id, Type = t.Key.Type.Name, Count = t.Count(), })
                        .ToList(),
                })
                .ToList();

            var noReactions = viewers.Count == 0 && experts.Count == 0;
            if (noReactions)
            {
                continue;
            }

            summary.Questions.Add(new AnalyticsSummaryQuestion
            {
                Id = question.Id,
                Value = question.Value,
                Experts = experts,
                Viewers = viewers,
            });
        }

        return summary;
    }

    public async Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureParticipantTypeAsync(request.RoomId, request.UserId, cancellationToken);
        var response = new Dictionary<string, List<IStorageEvent>>();
        foreach (var (type, option) in request.TranscriptionTypeMap)
        {
            var spec = new Spec<IStorageEvent>(e => e.Type == type && e.RoomId == request.RoomId);
            var result = await hotEventStorage.GetLatestBySpecAsync(spec, option.Last, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
            if (response.TryGetValue(option.ResponseName, out var responses))
            {
                if (result is null)
                {
                    continue;
                }

                foreach (var @event in result.Take(option.Last))
                {
                    responses.Add(@event);
                }
            }
            else
            {
                response[option.ResponseName] = result?.Take(option.Last).ToList() ?? [];
            }
        }

        return response;
    }

    public async Task<RoomInviteResponse> ApplyInvite(Guid roomId, Guid? invite, CancellationToken cancellationToken = default)
    {
        var userId = currentUserAccessor.UserId!.Value;
        using var loggerLocalScope = logger.BeginScope("apply invite for room [id -> {roomId}] with invite [value -> {inviteId}]", roomId, invite);

        logger.LogInformation("search room for invite");

        var room = await db.Rooms.FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        EnsureAvailableRoomEdit(room);
        logger.LogInformation("room found");

        if (invite is not null)
        {
            logger.LogInformation("apply invite");
            return await roomInviteService
                .ApplyInvite(invite.Value, currentUserAccessor.UserId!.Value, cancellationToken);
        }

        if (room.AccessType == SERoomAccessType.Private)
        {
            throw AccessDeniedException.CreateForAction("private room");
        }

        logger.LogInformation("room has open type");

        var participant = await db.RoomParticipants.FirstOrDefaultAsync(e => e.RoomId == roomId && e.UserId == userId, cancellationToken);

        if (participant is not null)
        {
            logger.LogInformation("participant is not null and just return room invite");
            return new RoomInviteResponse
            {
                ParticipantType = participant.Type!.EnumValue,
                InviteId = invite!.Value,
                Max = 0,
                Used = 0,
            };
        }

        logger.LogInformation("participant is null. will be created new");

        var user = await db.Users.FirstOrDefaultAsync(e => e.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Current user not found");
        }

        logger.LogInformation("Create participant for user [id -> {userId}]", user.Id);

        return await db.RunTransactionAsync(async _ =>
            {
                var participants = await roomParticipantService.CreateAsync(
                    roomId,
                    [(user, room, SERoomParticipantType.Viewer)],
                    cancellationToken);
                participant = participants.First();

                logger.LogInformation("room participant [id -> {participantId}, type -> {participantType}] created", participant.Id, participant.Type.Name);

                return new RoomInviteResponse
                {
                    ParticipantType = participant.Type.EnumValue,
                    InviteId = invite!.Value,
                    Max = 0,
                    Used = 0,
                };
            },
            cancellationToken);
    }

    private static RoomTimer? CreateRoomTimer(long? durationSec)
    {
        if (durationSec is null)
        {
            return null;
        }

        return new RoomTimer { Duration = TimeSpan.FromSeconds(durationSec.Value), };
    }

    private void EnsureAvailableRoomEdit(Room room)
    {
        if (room.Status == SERoomStatus.Close || room.Status == SERoomStatus.Review)
        {
            throw new UserException("The room is in a status where it cannot be changed.");
        }
    }

    private void EnsureValidScheduleStartTime(DateTime scheduleStartTime, DateTime? dbScheduleStartTime)
    {
        // Nothing has changed.
        if (dbScheduleStartTime == scheduleStartTime)
        {
            return;
        }

        var minDateTime = clock.UtcNow.Subtract(TimeSpan.FromMinutes(15)).UtcDateTime;
        if (minDateTime > scheduleStartTime)
        {
            throw new UserException("The scheduled start date must be greater than current time - 15 minutes");
        }
    }

    private async Task<RoomParticipant> EnsureParticipantTypeAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var participantType = await db.RoomParticipants
            .Include(e => e.Room)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Room.Id == roomId && e.User.Id == userId, cancellationToken);
        if (participantType is null)
        {
            throw new NotFoundException($"Not found participant type by room id {roomId} and user id {userId}");
        }

        return participantType;
    }

    private string FormatNotFoundEntityIds<T>(IEnumerable<Guid> guids, IEnumerable<T> collection)
        where T : Entity
    {
        var notFoundEntityIds = guids.Except(collection.Select(entity => entity.Id));
        return string.Join(", ", notFoundEntityIds);
    }

    private async Task<List<T>> FindByIdsOrErrorAsync<T>(IQueryable<T> repository, ICollection<Guid> ids, string entityName, CancellationToken cancellationToken)
        where T : Entity
    {
        var entities = await repository.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
        var notFoundEntities = FormatNotFoundEntityIds(ids, entities);
        if (!string.IsNullOrEmpty(notFoundEntities))
        {
            throw new NotFoundException($"Not found {entityName} with id [{notFoundEntities}]");
        }

        return entities;
    }
}
