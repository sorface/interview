using Interview.Domain.Database;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.EventProvider;

public class CodeEditorContentProvider(
    IRoomEventProvider roomEventStorage,
    IEventDeserializer eventDeserializer,
    AppDbContext db,
    ILogger<CodeEditorContentProvider> logger)
{
    public async ValueTask<string?> GetCodeEditorContentAsync((DateTime StartActiveDate, DateTime EndActiveDate)? lastActiveQuestionTime,
                                                              QuestionDetail current,
                                                              bool includeDbQuestionCodeEditor,
                                                              CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Ge code editor content for {RoomId} {QuestionId}", current.RoomId, current.QuestionId);
        if (lastActiveQuestionTime is null)
        {
            logger.LogDebug("Not found last active question time");
        }
        else
        {
            logger.LogDebug("Found last active question time {StartTime} {EndTime}", lastActiveQuestionTime?.StartActiveDate, lastActiveQuestionTime?.EndActiveDate);
        }

        if (lastActiveQuestionTime is not null)
        {
            var lastCodeEditorState = await GetLastCodeEditorStateAsync(lastActiveQuestionTime.Value, cancellationToken);
            if (lastCodeEditorState is null)
            {
                logger.LogDebug("Not found code editor content for {StartTime} {EndTime}", lastActiveQuestionTime.Value.StartActiveDate, lastActiveQuestionTime.Value.EndActiveDate);
            }
            else
            {
                logger.LogDebug("Found code editor content for {Content} {StartTime} {EndTime}", lastCodeEditorState, lastActiveQuestionTime.Value.StartActiveDate, lastActiveQuestionTime.Value.EndActiveDate);
                return lastCodeEditorState;
            }
        }

        if (current.CodeEditorId is null)
        {
            logger.LogDebug("Return empty content for question without code editor");
            return null;
        }

        var content = current.CodeEditorContent;
        if (content is not null)
        {
            logger.LogDebug("Return loaded question content {Content}", content);
            return content;
        }

        if (includeDbQuestionCodeEditor)
        {
            var resultContent = await db.Questions
                .AsNoTracking()
                .Include(e => e.CodeEditor)
                .Where(e => e.Id == current.QuestionId)
                .Select(e => e.CodeEditor == null ? null : e.CodeEditor.Content)
                .FirstOrDefaultAsync(cancellationToken);
            logger.LogDebug("Return db question content {Content}", resultContent);
            return resultContent;
        }

        return null;
    }

    /// <summary>
    /// Returns the last state of the code editor.
    /// The problem is that we have 2 types of code change events
    /// 1. On the frontend side
    /// 2. Backend side.
    ///
    /// We get the latest events in the required range and return the most recent one (if any)
    /// </summary>
    /// <param name="lastActiveQuestionTime">Last active question time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Code editor state</returns>
    private async Task<string?> GetLastCodeEditorStateAsync(
        (DateTime StartActiveDate, DateTime EndActiveDate) lastActiveQuestionTime,
        CancellationToken cancellationToken)
    {
        var codeEditorContentRequest = new EPStorageEventRequest
        {
            Type = EventType.CodeEditorChange,
            From = lastActiveQuestionTime.StartActiveDate,
            To = lastActiveQuestionTime.EndActiveDate,
        };
        var lastCodeEditorState = await roomEventStorage.GetLatestEventAsync(codeEditorContentRequest, cancellationToken);

        var changeCodeEditorEditorRequest = new EPStorageEventRequest
        {
            Type = EventType.ChangeCodeEditor,
            From = lastActiveQuestionTime.StartActiveDate,
            To = lastActiveQuestionTime.EndActiveDate,
        };
        var changeCodeEditorEditorResponse = await roomEventStorage.GetLatestEventAsync(changeCodeEditorEditorRequest, cancellationToken);
        if (changeCodeEditorEditorResponse is not null)
        {
            try
            {
                changeCodeEditorEditorResponse.Payload = changeCodeEditorEditorResponse.Payload is not null
                    ? eventDeserializer.Deserialize<RoomCodeEditorChangeEvent.Payload>(changeCodeEditorEditorResponse.Payload)?.Content
                    : null;
            }
            catch (Exception e)
            {
                logger.LogError(e, "During deserialization RoomCodeEditorChangeEvent.Payload {Payload}", changeCodeEditorEditorResponse.Payload);
            }
        }

        List<EPStorageEvent?> result = [lastCodeEditorState, changeCodeEditorEditorResponse];
        result.RemoveAll(e => e is null);
        if (result.Count == 0)
        {
            return null;
        }

        return result.MaxBy(e => e!.CreatedAt)?.Payload;
    }

    public record QuestionDetail(Guid RoomId, Guid QuestionId, string? CodeEditorContent, Guid? CodeEditorId);
}
