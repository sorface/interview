using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.EventProvider;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor(
    IRoomEventDispatcher eventDispatcher,
    AppDbContext db,
    RoomCodeEditorChangeEventHandler roomCodeEditorChangeEventHandler,
    ICurrentUserAccessor currentUserAccessor,
    IRoomConfigurationRepository roomConfigurationRepository,
    ISystemClock clock,
    RoomEventProviderFactory roomEventProviderFactory,
    IEventDeserializer eventDeserializer,
    ILogger<RoomQuestionPostProcessor> logger)
    : EntityPostProcessor<RoomQuestion>
{
    public override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent
        {
            RoomId = entity.RoomId,
            Value = new RoomQuestionAddEventPayload(entity.QuestionId, entity.State.EnumValue),
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    public override async ValueTask ProcessModifiedAsync(
        RoomQuestion original,
        RoomQuestion current,
        CancellationToken cancellationToken)
    {
        if (original.State == current.State)
        {
            return;
        }

        (DateTime StartActiveDate, DateTime EndActiveDate)? lastActiveQuestionTime = null;

        var eventProvider = await roomEventProviderFactory.CreateProviderAsync(current.RoomId, cancellationToken);
        if (current.State == RoomQuestionState.Active)
        {
            var facade = new RoomEventActiveQuestionProvider(eventProvider, eventDeserializer, clock);
            lastActiveQuestionTime = await facade
                .GetActiveQuestionDateAsync(current.QuestionId, cancellationToken)
                .Where(e => e.EndActiveDate != e.StartActiveDate)
                .OrderByDescending(e => e.StartActiveDate)
                .Select(e => ((DateTime StartActiveDate, DateTime EndActiveDate)?)e)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var @event = new RoomQuestionChangeEvent
        {
            RoomId = current.RoomId,
            Value = new RoomQuestionChangeEventPayload(current.QuestionId, original.State.EnumValue, current.State.EnumValue),
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);

        if (current.State == RoomQuestionState.Active)
        {
            await ChangeCodeEditorEnabledStateAsync(current, cancellationToken);
            await ChangeCodeEditorContentAsync(lastActiveQuestionTime, eventProvider, current, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string?> GetLastCodeEditorStateAsync(
        (DateTime StartActiveDate, DateTime EndActiveDate) lastActiveQuestionTime,
        IRoomEventProvider eventProvider,
        CancellationToken cancellationToken)
    {
        var codeEditorContentRequest = new EPStorageEventRequest
        {
            Type = EventType.CodeEditorChange,
            From = lastActiveQuestionTime.StartActiveDate,
            To = lastActiveQuestionTime.EndActiveDate,
        };
        var lastCodeEditorState = await eventProvider.GetLatestEventAsync(codeEditorContentRequest, cancellationToken);

        var changeCodeEditorEditorRequest = new EPStorageEventRequest
        {
            Type = EventType.ChangeCodeEditor,
            From = lastActiveQuestionTime.StartActiveDate,
            To = lastActiveQuestionTime.EndActiveDate,
        };
        var changeCodeEditorEditorResponse = await eventProvider.GetLatestEventAsync(changeCodeEditorEditorRequest, cancellationToken);
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

    private async Task ChangeCodeEditorEnabledStateAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var codeEditorPayload = await GetCodeEditorEventPayload(current, cancellationToken);
        var request = new RoomCodeEditorChangeEventHandler.Request(current.RoomId, codeEditorPayload.Enabled, EVRoomCodeEditorChangeSource.System)
        {
            SaveChanges = false,
        };
        await roomCodeEditorChangeEventHandler.HandleAsync(request, cancellationToken);
    }

    private async Task ChangeCodeEditorContentAsync((DateTime StartActiveDate, DateTime EndActiveDate)? lastActiveQuestionTime,
                                                    IRoomEventProvider eventProvider,
                                                    RoomQuestion current,
                                                    CancellationToken cancellationToken)
    {
        var codeEditorContent = await GetCodeEditorContentAsync(lastActiveQuestionTime, eventProvider, current, cancellationToken);
        var upsertCodeStateRequest = new UpsertCodeStateRequest
        {
            RoomId = current.RoomId,
            CodeEditorContent = codeEditorContent ?? string.Empty,
            ChangeCodeEditorContentSource = EVRoomCodeEditorChangeSource.System,
            SaveChanges = false,
        };
        await roomConfigurationRepository.UpsertCodeStateAsync(upsertCodeStateRequest, cancellationToken);
    }

    private async ValueTask<string?> GetCodeEditorContentAsync((DateTime StartActiveDate, DateTime EndActiveDate)? lastActiveQuestionTime,
                                                               IRoomEventProvider eventProvider,
                                                               RoomQuestion current,
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
            var lastCodeEditorState = await GetLastCodeEditorStateAsync(lastActiveQuestionTime.Value, eventProvider, cancellationToken);
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

        if (current.Question is not null && current.Question.CodeEditorId is null)
        {
            logger.LogDebug("Return empty content for question without code editor");
            return null;
        }

        var content = current.Question?.CodeEditor?.Content;
        if (content is not null)
        {
            logger.LogDebug("Return loaded question content {Content}", content);
            return content;
        }

        var resultContent = await db.Questions
            .AsNoTracking()
            .Include(e => e.CodeEditor)
            .Where(e => e.Id == current.QuestionId)
            .Select(e => e.CodeEditor == null ? null : e.CodeEditor.Content)
            .FirstOrDefaultAsync(cancellationToken);
        logger.LogDebug("Return db question content {Content}", resultContent);
        return resultContent;
    }

    private async Task<RoomCodeEditorEnabledEvent.Payload> GetCodeEditorEventPayload(RoomQuestion roomQuestion, CancellationToken cancellationToken)
    {
        if (roomQuestion.Question is not null)
        {
            return new RoomCodeEditorEnabledEvent.Payload { Enabled = roomQuestion.Question.CodeEditorId is not null, };
        }

        var hasCodeEditor = await db.Questions
            .Where(e => e.Id == roomQuestion.QuestionId)
            .AnyAsync(e => e.CodeEditorId != null, cancellationToken);
        return new RoomCodeEditorEnabledEvent.Payload { Enabled = hasCodeEditor, };
    }
}
