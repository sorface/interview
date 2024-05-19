using System.Transactions;
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

    public Task<IPagedList<RoomPageDetail>> FindPageAsync(
        RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return _roomRepository.GetDetailedPageAsync(filter, pageNumber, pageSize, cancellationToken);
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

    public async Task<Room> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default)
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

        var experts = await FindByIdsOrErrorAsync(_userRepository, request.Experts, "experts", cancellationToken);

        var examinees = await FindByIdsOrErrorAsync(_userRepository, request.Examinees, "examinees", cancellationToken);

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);

        var room = new Room(name, SERoomAcсessType.FromName(request.AccessType)) { Tags = tags, };
        var roomQuestions = questions.Select(question =>
            new RoomQuestion { Room = room, Question = question, State = RoomQuestionState.Open });

        room.Questions.AddRange(roomQuestions);

        var participants = experts
            .Select(e => (e, room, SERoomParticipantType.Expert))
            .Concat(examinees.Select(e => (e, room, SERoomParticipantType.Examinee)))
            .ToList();

        var createdParticipants = await _roomParticipantService.CreateAsync(room.Id, participants, cancellationToken);
        room.Participants.AddRange(createdParticipants);
        await _roomRepository.CreateAsync(room, cancellationToken);

        await GenerateInvitesAsync(room.Id, cancellationToken);

        return room;
    }

    public async Task<RoomItem> UpdateAsync(
        Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default)
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

    public async Task<(Room, RoomParticipant)> AddParticipantAsync(
        Guid roomId, Guid userId, CancellationToken cancellationToken = default)
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

    public async Task SendEventRequestAsync(
        IEventRequest request, CancellationToken cancellationToken = default)
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
    public async Task CloseAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
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

    public async Task<ActualRoomStateResponse> GetActualStateAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
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

    public async Task UpsertRoomStateAsync(
        Guid roomId, string type, string payload, CancellationToken cancellationToken = default)
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

    public async Task<Analytics> GetAnalyticsAsync(
        RoomAnalyticsRequest request,
        CancellationToken cancellationToken = default)
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

    public async Task<List<RoomInviteResponse>> GenerateInvitesAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        List<RoomInviteResponse> invites = new();

        foreach (var participantType in SERoomParticipantType.List)
        {
            invites.Add(await _roomInviteService.GenerateAsync(roomId, participantType, 20, cancellationToken));
        }

        return invites;
    }

    public Task<RoomInviteResponse> GenerateInviteAsync(
        RoomInviteGeneratedRequest roomInviteGenerated,
        CancellationToken cancellationToken = default)
    {
        return _roomInviteService.GenerateAsync(
            roomInviteGenerated.RoomId,
            SERoomParticipantType.FromEnum(roomInviteGenerated.ParticipantType),
            20,
            cancellationToken);
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(
        RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var analytics = await _roomRepository.GetAnalyticsSummaryAsync(request, cancellationToken);

        if (analytics is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        return analytics;
    }

    public async Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(
        TranscriptionRequest request,
        CancellationToken cancellationToken = default)
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

    public async Task<RoomInviteResponse> ApplyInvite(
        Guid roomId,
        Guid? invite,
        CancellationToken cancellationToken = default)
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

        if (room.AcсessType == SERoomAcсessType.Private)
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

    private async Task<RoomParticipant> EnsureParticipantTypeAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
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

    private async Task<List<T>> FindByIdsOrErrorAsync<T>(
        IRepository<T> repository,
        ICollection<Guid> ids,
        string entityName,
        CancellationToken cancellationToken)
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
