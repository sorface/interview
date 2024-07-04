using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Reactions;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestionReactions.Mappers;
using Interview.Domain.Rooms.RoomQuestionReactions.Specifications;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Rooms.Service;

public sealed class RoomService : IRoomServiceWithoutPermissionCheck
{
    private readonly IAppEventRepository _eventRepository;
    private readonly IRoomStateRepository _roomStateRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomQuestionRepository _roomQuestionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomEventDispatcher _roomEventDispatcher;
    private readonly IRoomQuestionReactionRepository _roomQuestionReactionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IRoomInviteService _roomInviteService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRoomParticipantService _roomParticipantService;
    private readonly AppDbContext _db;

    public RoomService(
        IRoomRepository roomRepository,
        IRoomQuestionRepository roomQuestionRepository,
        IQuestionRepository questionRepository,
        IUserRepository userRepository,
        IRoomEventDispatcher roomEventDispatcher,
        IRoomQuestionReactionRepository roomQuestionReactionRepository,
        ITagRepository tagRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IAppEventRepository eventRepository,
        IRoomStateRepository roomStateRepository,
        IEventStorage eventStorage,
        IRoomInviteService roomInviteService,
        ICurrentUserAccessor currentUserAccessor,
        IRoomParticipantService roomParticipantService,
        AppDbContext db)
    {
        _roomRepository = roomRepository;
        _questionRepository = questionRepository;
        _userRepository = userRepository;
        _roomEventDispatcher = roomEventDispatcher;
        _roomQuestionReactionRepository = roomQuestionReactionRepository;
        _tagRepository = tagRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _eventRepository = eventRepository;
        _roomStateRepository = roomStateRepository;
        _eventStorage = eventStorage;
        _roomQuestionRepository = roomQuestionRepository;
        _roomInviteService = roomInviteService;
        _currentUserAccessor = currentUserAccessor;
        _roomParticipantService = roomParticipantService;
        _db = db;
    }

    public Task<IPagedList<RoomPageDetail>> FindPageAsync(RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Room> queryable = _db.Rooms
            .Include(e => e.Participants)
            .ThenInclude(e => e.User)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.Tags)
            .Include(e => e.Timer)
            .OrderBy(e => e.Status == SERoomStatus.Active ? 1 :
                e.Status == SERoomStatus.Review ? 2 :
                e.Status == SERoomStatus.New ? 3 :
                4)
            .ThenByDescending(e => e.CreateDate);
        var filterName = filter.Name?.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(filterName))
        {
            queryable = queryable.Where(e => e.Name.ToLower().Contains(filterName));
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

        if (!_currentUserAccessor.IsAdmin())
        {
            var currentUserId = _currentUserAccessor.GetUserIdOrThrow();
            queryable = queryable.Where(e =>
                e.AccessType == SERoomAccessType.Public || (e.AccessType == SERoomAccessType.Private &&
                                                            e.Participants.Any(p => currentUserId == p.User.Id)));
        }

        if (filter.Participants is not null && filter.Participants.Count > 0)
        {
            queryable = queryable.Where(e => e.Participants.Any(p => filter.Participants.Contains(p.User.Id)));
        }

        return queryable
            .Select(e => new RoomPageDetail
            {
                Id = e.Id,
                Name = e.Name,
                Questions = e.Questions.Select(question => question.Question)
                    .Select(question => new RoomQuestionDetail { Id = question!.Id, Value = question.Value, })
                    .ToList(),
                Participants = e.Participants.Select(participant =>
                        new RoomUserDetail { Id = participant.User.Id, Nickname = participant.User.Nickname, Avatar = participant.User.Avatar, })
                    .ToList(),
                RoomStatus = e.Status.EnumValue,
                Tags = e.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
                Timer = e.Timer == null ? null : new RoomTimerDetail { DurationSec = (long)e.Timer.Duration.TotalSeconds, StartTime = e.Timer.ActualStartTime, },
                ScheduledStartTime = e.ScheduleStartTime,
            })
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(id, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(id);
        }

        return room;
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

        var questions =
            await FindByIdsOrErrorAsync(_questionRepository, request.Questions, "questions", cancellationToken);

        var currentUserId = _currentUserAccessor.GetUserIdOrThrow();
        ICollection<Guid> requestExperts = request.Experts;
        if (!request.Experts.Contains(currentUserId) && !request.Examinees.Contains(currentUserId))
        {
            // If the current user is not listed as a member of the room, add him/her with the role 'Expert'
            requestExperts = requestExperts.Concat(new[] { currentUserId }).ToList();
        }

        if (request.ScheduleStartTime is not null && DateTime.UtcNow > request.ScheduleStartTime)
        {
            throw new UserException("The scheduled start date must be greater than the current time");
        }

        var experts = await FindByIdsOrErrorAsync(_userRepository, requestExperts, "experts", cancellationToken);
        var examinees = await FindByIdsOrErrorAsync(_userRepository, request.Examinees, "examinees", cancellationToken);
        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);
        var room = new Room(name, request.AccessType) { Tags = tags, ScheduleStartTime = request.ScheduleStartTime, };
        var roomQuestions = questions.Select(question =>
            new RoomQuestion
            {
                Room = room,
                Question = question,
                State = RoomQuestionState.Open,
                RoomId = default,
                QuestionId = default,
            });

        room.Questions.AddRange(roomQuestions);

        var participants = experts
            .Select(e => (e, room, SERoomParticipantType.Expert))
            .Concat(examinees.Select(e => (e, room, SERoomParticipantType.Examinee)))
            .ToList();

        var createdParticipants = await _roomParticipantService.CreateAsync(room.Id, participants, cancellationToken);
        room.Participants.AddRange(createdParticipants);

        if (request.DurationSec is not null)
        {
            room.Timer = new RoomTimer { Duration = TimeSpan.FromSeconds(request.DurationSec.Value), };
        }

        await _roomRepository.CreateAsync(room, cancellationToken);
        await GenerateInvitesAsync(room.Id, cancellationToken);

        return new RoomPageDetail
        {
            Id = room.Id,
            Name = room.Name,
            Questions = room.Questions.Select(question => question.Question)
                .Select(question => new RoomQuestionDetail { Id = question!.Id, Value = question.Value, })
                .ToList(),
            Participants = room.Participants.Select(participant =>
                    new RoomUserDetail { Id = participant.User.Id, Nickname = participant.User.Nickname, Avatar = participant.User.Avatar, })
                .ToList(),
            RoomStatus = room.Status.EnumValue,
            Tags = room.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
            Timer = room.Timer == null ? null : new RoomTimerDetail { DurationSec = (long)room.Timer.Duration.TotalSeconds, StartTime = room.Timer.ActualStartTime, },
            ScheduledStartTime = room.ScheduleStartTime,
        };
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

        var foundRoom = await _roomRepository.FindByIdAsync(roomId, cancellationToken);
        if (foundRoom is null)
        {
            throw NotFoundException.Create<User>(roomId);
        }

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);

        foundRoom.Name = name;
        foundRoom.Tags.Clear();
        foundRoom.Tags.AddRange(tags);
        await _roomRepository.UpdateAsync(foundRoom, cancellationToken);

        return new RoomItem
        {
            Id = foundRoom.Id,
            Name = foundRoom.Name,
            Tags = tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
        };
    }

    public async Task<(Room, RoomParticipant)> AddParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        var currentRoom = await _roomRepository.FindByIdAsync(roomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw NotFoundException.Create<User>(userId);
        }

        var participant = await _roomRepository.FindParticipantOrDefaultAsync(roomId, user.Id, cancellationToken);
        if (participant is not null)
        {
            return (currentRoom, participant);
        }

        var participants = await _roomParticipantService.CreateAsync(
            roomId,
            new[] { (user, currentRoom, SERoomParticipantType.Viewer) },
            cancellationToken);
        participant = participants.First();
        currentRoom.Participants.Add(participant);
        await _roomRepository.UpdateAsync(currentRoom, cancellationToken);
        return (currentRoom, participant);
    }

    public async Task SendEventRequestAsync(IEventRequest request, CancellationToken cancellationToken = default)
    {
        var eventSpecification = new Spec<AppEvent>(e => e.Type == request.Type);
        var dbEvent = await _eventRepository.FindFirstOrDefaultAsync(eventSpecification, cancellationToken);
        if (dbEvent is null)
        {
            throw new NotFoundException($"Event not found by type {request.Type}");
        }

        var currentRoom = await _roomRepository.FindByIdAsync(request.RoomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var user = await _userRepository.FindByIdDetailedAsync(request.UserId, cancellationToken);
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

        await _roomEventDispatcher.WriteAsync(request.ToRoomEvent(dbEvent.Stateful), cancellationToken);
    }

    /// <summary>
    /// Close non closed room.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="cancellationToken">Token.</param>
    /// <returns>Result.</returns>
    public async Task CloseAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var currentRoom = await _roomRepository.FindByIdAsync(roomId, cancellationToken);
        if (currentRoom == null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        if (currentRoom.Status == SERoomStatus.Close)
        {
            throw new UserException("Room already closed");
        }

        await _roomQuestionRepository.CloseActiveQuestionAsync(roomId, cancellationToken);

        currentRoom.Status = SERoomStatus.Close;

        await _roomRepository.UpdateAsync(currentRoom, cancellationToken);
    }

    public async Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var currentRoom = await _roomRepository.FindByIdAsync(roomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        if (currentRoom.Status == SERoomStatus.Review)
        {
            throw new UserException("Room already reviewed");
        }

        currentRoom.Status = SERoomStatus.Review;

        await _roomRepository.UpdateAsync(currentRoom, cancellationToken);
    }

    public async Task<ActualRoomStateResponse> GetActualStateAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var roomState =
            await _roomRepository.FindByIdDetailedAsync(roomId, ActualRoomStateResponse.Mapper, cancellationToken);

        if (roomState == null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var spec = new RoomReactionsSpecification(roomId);

        var reactions = await _roomQuestionReactionRepository.FindDetailed(
            spec,
            ReactionTypeOnlyMapper.Instance,
            cancellationToken);

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

        var hasRoom = await _roomRepository.HasAsync(new Spec<Room>(e => e.Id == roomId), cancellationToken);
        if (!hasRoom)
        {
            throw new UserException("No room was found by id.");
        }

        var spec = new Spec<RoomState>(e => e.RoomId == roomId && e.Type == type);
        var state = await _roomStateRepository.FindFirstOrDefaultAsync(spec, cancellationToken);
        if (state is not null)
        {
            state.Payload = payload;
            await _roomStateRepository.UpdateAsync(state, cancellationToken);
            return;
        }

        state = new RoomState
        {
            Payload = payload,
            RoomId = roomId,
            Type = type,
            Room = null,
        };
        await _roomStateRepository.CreateAsync(state, cancellationToken);
    }

    public async Task DeleteRoomStateAsync(Guid roomId, string type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new UserException("The type cannot be empty.");
        }

        var hasRoom = await _roomRepository.HasAsync(new Spec<Room>(e => e.Id == roomId), cancellationToken);
        if (!hasRoom)
        {
            throw new UserException("No room was found by id.");
        }

        var spec = new Spec<RoomState>(e => e.RoomId == roomId && e.Type == type);
        var state = await _roomStateRepository.FindFirstOrDefaultAsync(spec, cancellationToken);
        if (state is null)
        {
            throw new UserException($"No room state with type '{type}' was found");
        }

        await _roomStateRepository.DeleteAsync(state, cancellationToken);
    }

    public async Task<Analytics> GetAnalyticsAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var analytics = await _roomRepository.GetAnalyticsAsync(request, cancellationToken);

        if (analytics is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        return analytics;
    }

    public Task<List<RoomInviteResponse>> GetInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return _db.RoomInvites.AsNoTracking()
            .Include(roomInvite => roomInvite.Invite)
            .Where(e => e.RoomById == roomId && e.InviteById != null && e.ParticipantType != null)
            .Select(e => new RoomInviteResponse
            {
                InviteId = e.InviteById!.Value,
                ParticipantType = e.ParticipantType!.EnumValue,
                Max = e.Invite!.UsesMax,
                Used = e.Invite.UsesCurrent,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GenerateInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        List<RoomInviteResponse> invites = new();

        foreach (var participantType in SERoomParticipantType.List)
        {
            invites.Add(await _roomInviteService.GenerateAsync(roomId, participantType, 20, cancellationToken));
        }

        return invites;
    }

    public Task<RoomInviteResponse> GenerateInviteAsync(RoomInviteGeneratedRequest roomInviteGenerated, CancellationToken cancellationToken = default)
    {
        return _roomInviteService.GenerateAsync(
            roomInviteGenerated.RoomId,
            SERoomParticipantType.FromEnum(roomInviteGenerated.ParticipantType),
            20,
            cancellationToken);
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var analytics = await _roomRepository.GetAnalyticsSummaryAsync(request, cancellationToken);

        if (analytics is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        return analytics;
    }

    public async Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureParticipantTypeAsync(request.RoomId, request.UserId, cancellationToken);
        var response = new Dictionary<string, List<IStorageEvent>>();
        foreach (var (type, option) in request.TranscriptionTypeMap)
        {
            var spec = new Spec<IStorageEvent>(e => e.Type == type && e.RoomId == request.RoomId);
            var result = await _eventStorage.GetLatestBySpecAsync(spec, option.Last, cancellationToken)
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
                response[option.ResponseName] = result?.Take(option.Last).ToList() ?? new List<IStorageEvent>();
            }
        }

        return response;
    }

    public async Task<RoomInviteResponse> ApplyInvite(Guid roomId, Guid? invite, CancellationToken cancellationToken = default)
    {
        var roomSpec = new Spec<Room>(room => room.Id == roomId);

        var room = await _roomRepository.FindFirstOrDefaultAsync(roomSpec, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        if (invite is not null)
        {
            return await _roomInviteService
                .ApplyInvite(invite.Value, _currentUserAccessor.UserId!.Value, cancellationToken);
        }

        if (room.AccessType == SERoomAccessType.Private)
        {
            throw AccessDeniedException.CreateForAction("private room");
        }

        var participant = await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(
            roomId, _currentUserAccessor.UserId!.Value, cancellationToken);

        if (participant is not null)
        {
            return new RoomInviteResponse
            {
                ParticipantType = participant.Type.EnumValue,
                InviteId = invite!.Value,
                Max = 0,
                Used = 0,
            };
        }

        var user = await _userRepository.FindByIdAsync(_currentUserAccessor.UserId!.Value, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Current user not found");
        }

        var participants = await _roomParticipantService.CreateAsync(
            roomId,
            new[] { (user, room, SERoomParticipantType.Viewer) },
            cancellationToken);
        participant = participants.First();
        await _roomParticipantRepository.CreateAsync(participant, cancellationToken);

        return new RoomInviteResponse
        {
            ParticipantType = participant.Type.EnumValue,
            InviteId = invite!.Value,
            Max = 0,
            Used = 0,
        };
    }

    private async Task<RoomParticipant> EnsureParticipantTypeAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var participantType =
            await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId, cancellationToken);
        if (participantType is null)
        {
            throw new NotFoundException($"Not found participant type by room id {roomId} and user id {userId}");
        }

        return participantType;
    }

    private string FindNotFoundEntityIds<T>(IEnumerable<Guid> guids, IEnumerable<T> collection)
        where T : Entity
    {
        var notFoundEntityIds = guids.Except(collection.Select(entity => entity.Id));

        return string.Join(", ", notFoundEntityIds);
    }

    private async Task<List<T>> FindByIdsOrErrorAsync<T>(IRepository<T> repository, ICollection<Guid> ids, string entityName, CancellationToken cancellationToken)
        where T : Entity
    {
        var entities = await repository.FindByIdsAsync(ids, cancellationToken);

        var notFoundEntities = FindNotFoundEntityIds(ids, entities);

        if (!string.IsNullOrEmpty(notFoundEntities))
        {
            throw new NotFoundException($"Not found {entityName} with id [{notFoundEntities}]");
        }

        return entities;
    }
}
