using System.Runtime.CompilerServices;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.EventProvider;
using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms.Records.Response.Detail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using NSpecifications;

namespace Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;

public class AnswerDetailService : ISelfScopeService
{
    private readonly ISystemClock _clock;
    private readonly RoomEventProviderFactory _roomEventProviderFactory;
    private readonly AppDbContext _db;

    public AnswerDetailService(ISystemClock clock, RoomEventProviderFactory roomEventProviderFactory, AppDbContext db)
    {
        _clock = clock;
        _roomEventProviderFactory = roomEventProviderFactory;
        _db = db;
    }

    public async Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
    {
        var roomQuestion = await _db.RoomQuestions
            .Include(e => e.Room)
                .ThenInclude(e => e!.QueuedRoomEvent)
            .Include(e => e.Question)
                .ThenInclude(e => e!.CodeEditor)
            .AsNoTracking()
            .Where(e => e.RoomId == request.RoomId && e.QuestionId == request.QuestionId)
            .Select(e => new RoomQuestionAnswerDetailResponse
            {
                CodeEditor = e.Question!.CodeEditor == null
                    ? null
                    : new QuestionCodeEditorResponse { Content = e.Question!.CodeEditor!.Content, Lang = e.Question!.CodeEditor!.Lang, },
                Details = new List<RoomQuestionAnswerDetailResponse.Detail>(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (roomQuestion is null)
        {
            throw NotFoundException.Create<RoomQuestion>((request.RoomId, "RoomId"), (request.QuestionId, "QuestionId"));
        }

        var eventProvider = await _roomEventProviderFactory.CreateProviderAsync(request.RoomId, cancellationToken);
        await foreach (var (startActiveDate, endActiveDate) in GetActiveQuestionDateAsync(request, eventProvider, cancellationToken))
        {
            var res = await eventProvider
                .GetEventsAsync(new EPStorageEventRequest { Type = EventType.VoiceRecognition, From = startActiveDate, To = endActiveDate }, cancellationToken);

            var lastCodeEditorState = await eventProvider.GetLatestEventAsync(new EPStorageEventRequest { Type = EventType.CodeEditorChange, From = startActiveDate, To = endActiveDate }, cancellationToken);
            roomQuestion.Details.Add(new RoomQuestionAnswerDetailResponse.Detail
            {
                Transcription = GetTranscriptions(res),
                AnswerCodeEditorContent = lastCodeEditorState?.Payload,
                StartActiveDate = startActiveDate,
                EndActiveDate = endActiveDate,
            });
        }

        return roomQuestion;
    }

    private static List<QuestionDetailTranscriptionResponse> GetTranscriptions(IEnumerable<EPStorageEvent> res)
    {
        return res
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
            .ToList();
    }

    private async IAsyncEnumerable<(DateTime StartActiveDate, DateTime EndActiveDate)> GetActiveQuestionDateAsync(
        RoomQuestionAnswerDetailRequest request, IRoomEventProvider roomEventStorage, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var changeRoomQuestionState = EventType.ChangeRoomQuestionState;
        var changedRooms = await roomEventStorage
            .GetEventsAsync(new EPStorageEventRequest { Type = changeRoomQuestionState, From = null, To = null, }, cancellationToken);

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
                .Where(evDetail => evDetail.Payload is not null && evDetail.Payload.QuestionId == request.QuestionId && evDetail.Payload.OldState == RoomQuestionStateType.Active &&
                            evDetail.CreateAt > minDate)
                .Select(evDetail => (DateTime?)evDetail.CreateAt)
                .FirstOrDefault();

            yield return (minDate, endActiveDate ?? _clock.UtcNow.UtcDateTime);
        }
    }
}
