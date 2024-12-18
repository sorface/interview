using System.Runtime.CompilerServices;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.EventProvider;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.Records.Response.Detail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;

public class AnswerDetailService(ISystemClock clock, RoomEventProviderFactory roomEventProviderFactory, AppDbContext db, IEventDeserializer eventDeserializer, ILoggerFactory loggerFactory)
    : ISelfScopeService
{
    public async Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
    {
        var roomQuestionTmp = await db.RoomQuestions
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
                    : new
                    {
                        Id = e.Question!.CodeEditor.Id,
                        Content = e.Question!.CodeEditor!.Content,
                        Lang = e.Question!.CodeEditor!.Lang,
                    },
            })
            .FirstOrDefaultAsync(cancellationToken);
        var roomQuestion = roomQuestionTmp is null
            ? null
            : new RoomQuestionAnswerDetailResponse
            {
                CodeEditor = roomQuestionTmp.CodeEditor is null
                    ? null
                    : new QuestionCodeEditorResponse
                    {
                        Content = roomQuestionTmp.CodeEditor.Content,
                        Lang = roomQuestionTmp.CodeEditor.Lang,
                    },
                Details = new List<RoomQuestionAnswerDetailResponse.Detail>(),
            };

        if (roomQuestion is null)
        {
            throw NotFoundException.Create<RoomQuestion>((request.RoomId, "RoomId"), (request.QuestionId, "QuestionId"));
        }

        var questionDetails =
            new CodeEditorContentProvider.QuestionDetail(request.RoomId, request.QuestionId, roomQuestion.CodeEditor?.Content, roomQuestionTmp?.CodeEditor?.Id);

        var eventProvider = await roomEventProviderFactory.CreateProviderAsync(request.RoomId, cancellationToken);
        var facade = new RoomEventActiveQuestionProvider(eventProvider, eventDeserializer, clock);
        var codeEditorContentProvider = new CodeEditorContentProvider(eventProvider, eventDeserializer, db, loggerFactory.CreateLogger<CodeEditorContentProvider>());
        await foreach (var (startActiveDate, endActiveDate) in facade.GetActiveQuestionDateAsync(request.QuestionId, cancellationToken))
        {
            var res = await eventProvider
                .GetEventsAsync(new EPStorageEventRequest { Type = EventType.VoiceRecognition, From = startActiveDate, To = endActiveDate }, cancellationToken);

            var lastCodeEditorContent = await codeEditorContentProvider.GetCodeEditorContentAsync((startActiveDate, endActiveDate), questionDetails, false, cancellationToken);
            roomQuestion.Details.Add(new RoomQuestionAnswerDetailResponse.Detail
            {
                Transcription = GetTranscriptions(res),
                AnswerCodeEditorContent = lastCodeEditorContent,
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
}
