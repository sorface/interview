using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.EventProvider;
using Interview.Domain.Events.Events.Serializers;
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
    ILoggerFactory loggerFactory)
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
        var codeEditorContentProvider = new CodeEditorContentProvider(eventProvider, eventDeserializer, db, loggerFactory.CreateLogger<CodeEditorContentProvider>());
        var questionDetail = new CodeEditorContentProvider.QuestionDetail(current.RoomId, current.QuestionId, current.Question?.CodeEditor?.Content, current.Question?.CodeEditorId);
        var codeEditorContent = await codeEditorContentProvider.GetCodeEditorContentAsync(lastActiveQuestionTime, questionDetail, true, cancellationToken);
        var upsertCodeStateRequest = new UpsertCodeStateRequest
        {
            RoomId = current.RoomId,
            CodeEditorContent = codeEditorContent ?? string.Empty,
            ChangeCodeEditorContentSource = EVRoomCodeEditorChangeSource.System,
            SaveChanges = false,
        };
        await roomConfigurationRepository.UpsertCodeStateAsync(upsertCodeStateRequest, cancellationToken);
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
