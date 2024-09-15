using System.Runtime.CompilerServices;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms.Records.Response.Detail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using NSpecifications;

namespace Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;

public class AnswerDetailService : ISelfScopeService
{
    private readonly AppDbContext _db;
    private readonly IEventStorage _eventStorage;
    private readonly ISystemClock _clock;

    public AnswerDetailService(AppDbContext db, IEventStorage eventStorage, ISystemClock clock)
    {
        _db = db;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
    {
        var roomQuestionRes = await _db.RoomQuestions
            .Include(e => e.Room)
                .ThenInclude(e => e!.QueuedRoomEvent)
            .Include(e => e.Question)
                .ThenInclude(e => e!.CodeEditor)
            .AsNoTracking()
            .Where(e => e.RoomId == request.RoomId && e.QuestionId == request.QuestionId)
            .Select(e => new
            {
                CodeEditor = e.Question!.CodeEditor == null
                    ? null
                    : new QuestionCodeEditorResponse { Content = e.Question!.CodeEditor!.Content, Lang = e.Question!.CodeEditor!.Lang, },
                Details = new List<RoomQuestionAnswerDetailResponse.Detail>(),
                HasDbEvents = e.Room != null && e.Room.QueuedRoomEvent != null,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (roomQuestionRes is null)
        {
            throw NotFoundException.Create<RoomQuestion>((request.RoomId, "RoomId"), (request.QuestionId, "QuestionId"));
        }

        var roomQuestion = new RoomQuestionAnswerDetailResponse { Details = roomQuestionRes.Details, CodeEditor = roomQuestionRes.CodeEditor };
        IEventProvider eventProvider = roomQuestionRes.HasDbEvents
            ? new DbEventProvider(_db)
            : new RedisEventProvider(_eventStorage);
        await foreach (var (startActiveDate, endActiveDate) in GetActiveQuestionDateAsync(request, eventProvider, _clock, cancellationToken))
        {
            var voiceRecognition = EventType.VoiceRecognition;
            var codeEditorChangeEventType = EventType.CodeEditorChange;
            var res = await eventProvider
                .GetEventsAsync(new StorageEventRequest { Type = voiceRecognition, RoomId = request.RoomId, From = startActiveDate, To = endActiveDate }, cancellationToken);

            var lastCodeEditorState = await eventProvider.GetLatestEventAsync(new StorageEventRequest { Type = codeEditorChangeEventType, RoomId = request.RoomId, From = startActiveDate, To = endActiveDate }, cancellationToken);
            roomQuestion.Details.Add(new RoomQuestionAnswerDetailResponse.Detail
            {
                Transcription = res
                    .Select(e =>
                    {
                        using var payloadAsJson = JsonDocument.Parse(e.Payload ?? "{}");
                        return new QuestionDetailTranscriptionResponse
                        {
                            Id = e.Id,
                            Payload = payloadAsJson.RootElement.GetProperty("Message").GetString(),
                            CreatedAt = e.CreatedAt,
                            User = new QuestionDetailTranscriptionUserResponse
                            {
                                Id = e.CreatedById ?? Guid.Empty,
                                Nickname = payloadAsJson.RootElement.GetProperty("Nickname").GetString() ?? "Anonymous",
                            },
                        };
                    })
                    .ToList(),
                AnswerCodeEditorContent = lastCodeEditorState?.Payload,
                StartActiveDate = startActiveDate,
                EndActiveDate = endActiveDate,
            });
        }

        return roomQuestion;

        static async IAsyncEnumerable<(DateTime StartActiveDate, DateTime EndActiveDate)> GetActiveQuestionDateAsync(
            RoomQuestionAnswerDetailRequest request, IEventProvider eventStorage, ISystemClock clock, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var changeRoomQuestionState = EventType.ChangeRoomQuestionState;
            var changedRooms = await eventStorage
                .GetEventsAsync(new StorageEventRequest { RoomId = request.RoomId, Type = changeRoomQuestionState, From = null, To = null, }, cancellationToken);

            var list = changedRooms
                .Where(e => e.Payload is not null)
                .Select(e => new { Payload = JsonSerializer.Deserialize<RoomQuestionChangeEventPayload>(e.Payload!), CreateAt = e.CreatedAt, })
                .OrderBy(e => e.CreateAt)
                .ToList();
            foreach (var e in list)
            {
                if (e.Payload is null || e.Payload.QuestionId != request.QuestionId || e.Payload.NewState != RoomQuestionStateType.Active)
                {
                    continue;
                }

                var minDate = e.CreateAt;
                var endActiveDate = list
                    .Where(e => e.Payload is not null && e.Payload.QuestionId == request.QuestionId && e.Payload.OldState == RoomQuestionStateType.Active &&
                                e.CreateAt > minDate)
                    .Select(e => (DateTime?)e.CreateAt)
                    .FirstOrDefault();

                yield return (minDate, endActiveDate ?? clock.UtcNow.UtcDateTime);
            }
        }
    }

    private sealed class StorageEvent
    {
        public required Guid Id { get; set; }

        public required string? Payload { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required Guid? CreatedById { get; set; }
    }

    private sealed class StorageEventRequest
    {
        public required string Type { get; set; }

        public required Guid RoomId { get; set; }

        public required DateTime? From { get; set; }

        public required DateTime? To { get; set; }
    }

    private interface IEventProvider
    {
        Task<IEnumerable<StorageEvent>> GetEventsAsync(StorageEventRequest request, CancellationToken cancellationToken);

        Task<StorageEvent?> GetLatestEventAsync(StorageEventRequest request, CancellationToken cancellationToken);
    }

    private sealed class DbEventProvider : IEventProvider
    {
        private readonly AppDbContext _db;

        public DbEventProvider(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<StorageEvent>> GetEventsAsync(StorageEventRequest request, CancellationToken cancellationToken)
        {
            var spec = BuildSpecification(request);
            var res = await _db.RoomEvents.AsNoTracking()
                .Where(spec)
                .OrderBy(e => e.CreateDate)
                .Select(e => new StorageEvent
                {
                    Id = e.Id,
                    Payload = e.Payload,
                    CreatedAt = e.CreateDate,
                    CreatedById = e.EventSenderId,
                })
                .ToListAsync(cancellationToken);
            return res;
        }

        public Task<StorageEvent?> GetLatestEventAsync(StorageEventRequest request, CancellationToken cancellationToken)
        {
            var spec = BuildSpecification(request);
            return _db.RoomEvents.AsNoTracking()
                .Where(spec)
                .OrderByDescending(e => e.CreateDate)
                .Select(e => new StorageEvent
                {
                    Id = e.Id,
                    Payload = e.Payload,
                    CreatedAt = e.CreateDate,
                    CreatedById = e.EventSenderId,
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private ASpec<DbRoomEvent> BuildSpecification(StorageEventRequest request)
        {
            ASpec<DbRoomEvent> spec = new Spec<DbRoomEvent>(e => e.RoomId == request.RoomId && e.Type == request.Type);
            if (request.From is not null)
            {
                var dateFrom = request.From.Value;
                spec &= new Spec<DbRoomEvent>(e => e.CreateDate >= dateFrom);
            }

            if (request.To is not null)
            {
                var dateTo = request.To.Value;
                spec &= new Spec<DbRoomEvent>(e => e.CreateDate <= dateTo);
            }

            return spec;
        }
    }

    private sealed class RedisEventProvider : IEventProvider
    {
        private readonly IEventStorage _eventStorage;

        public RedisEventProvider(IEventStorage eventStorage)
        {
            _eventStorage = eventStorage;
        }

        public async Task<IEnumerable<StorageEvent>> GetEventsAsync(StorageEventRequest request, CancellationToken cancellationToken)
        {
            const int DefaultChunkSize = 500;
            var spec = BuildSpecification(request);
            var changedRooms = await _eventStorage
                .GetBySpecAsync(spec, DefaultChunkSize, cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken);

            return changedRooms
                .SelectMany(e => e)
                .Select(ToStorageEvent);
        }

        public async Task<StorageEvent?> GetLatestEventAsync(StorageEventRequest request, CancellationToken cancellationToken)
        {
            var spec = BuildSpecification(request);
            var lastCodeEditorState = await _eventStorage
                .GetLatestBySpecAsync(spec, 1, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
            return lastCodeEditorState?.Select(ToStorageEvent).FirstOrDefault();
        }

        private static StorageEvent ToStorageEvent(IStorageEvent e)
        {
            return new StorageEvent
            {
                Id = e.Id,
                Payload = e.Payload,
                CreatedAt = e.CreatedAt,
                CreatedById = e.CreatedById,
            };
        }

        private ASpec<IStorageEvent> BuildSpecification(StorageEventRequest request)
        {
            ASpec<IStorageEvent> spec = new Spec<IStorageEvent>(e => e.RoomId == request.RoomId && e.Type == request.Type);
            if (request.From is not null)
            {
                var dateFrom = request.From.Value;
                spec &= new Spec<IStorageEvent>(e => e.CreatedAt >= dateFrom);
            }

            if (request.To is not null)
            {
                var dateTo = request.To.Value;
                spec &= new Spec<IStorageEvent>(e => e.CreatedAt <= dateTo);
            }

            return spec;
        }
    }
}
