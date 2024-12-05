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

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor : EntityPostProcessor<RoomQuestion>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly AppDbContext _db;
    private readonly RoomCodeEditorChangeEventHandler _roomCodeEditorChangeEventHandler;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRoomConfigurationRepository _roomConfigurationRepository;

    private readonly ISystemClock _clock;
    private readonly RoomEventProviderFactory _roomEventProviderFactory;
    private readonly IRoomEventDeserializer _eventDeserializer;

    public RoomQuestionPostProcessor(
        IRoomEventDispatcher eventDispatcher,
        AppDbContext db,
        RoomCodeEditorChangeEventHandler roomCodeEditorChangeEventHandler,
        ICurrentUserAccessor currentUserAccessor,
        IRoomConfigurationRepository roomConfigurationRepository,
        ISystemClock clock,
        RoomEventProviderFactory roomEventProviderFactory,
        IRoomEventDeserializer eventDeserializer)
    {
        _eventDispatcher = eventDispatcher;
        _db = db;
        _roomCodeEditorChangeEventHandler = roomCodeEditorChangeEventHandler;
        _currentUserAccessor = currentUserAccessor;
        _roomConfigurationRepository = roomConfigurationRepository;
        _clock = clock;
        _roomEventProviderFactory = roomEventProviderFactory;
        _eventDeserializer = eventDeserializer;
    }

    public override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent
        {
            RoomId = entity.RoomId,
            Value = new RoomQuestionAddEventPayload(entity.QuestionId, entity.State.EnumValue),
            CreatedById = _currentUserAccessor.GetUserIdOrThrow(),
        };

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
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

        var @event = new RoomQuestionChangeEvent
        {
            RoomId = current.RoomId,
            Value = new RoomQuestionChangeEventPayload(current.QuestionId, original.State.EnumValue, current.State.EnumValue),
            CreatedById = _currentUserAccessor.GetUserIdOrThrow(),
        };

        await _eventDispatcher.WriteAsync(@event, cancellationToken);

        if (current.State == RoomQuestionState.Active)
        {
            await ChangeCodeEditorEnabledStateAsync(current, cancellationToken);

            // Changed active question
            if (original.State != RoomQuestionState.Active)
            {
                await ChangeCodeEditorContentAsync(current, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ChangeCodeEditorEnabledStateAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var codeEditorPayload = await GetCodeEditorEventPayload(current, cancellationToken);
        var request = new RoomCodeEditorChangeEventHandler.Request(current.RoomId, codeEditorPayload.Enabled, EVRoomCodeEditorChangeSource.System)
        {
            SaveChanges = false,
        };
        await _roomCodeEditorChangeEventHandler.HandleAsync(request, cancellationToken);
    }

    private async Task ChangeCodeEditorContentAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var codeEditorContent = await GetCodeEditorContentAsync(current, cancellationToken);
        var upsertCodeStateRequest = new UpsertCodeStateRequest
        {
            RoomId = current.RoomId,
            CodeEditorContent = codeEditorContent ?? string.Empty,
            ChangeCodeEditorContentSource = EVRoomCodeEditorChangeSource.System,
            SaveChanges = false,
        };
        await _roomConfigurationRepository.UpsertCodeStateAsync(upsertCodeStateRequest, cancellationToken);
    }

    private async ValueTask<string?> GetCodeEditorContentAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        var eventProvider = await _roomEventProviderFactory.CreateProviderAsync(current.RoomId, cancellationToken);
        var facade = new RoomEventActiveQuestionProvider(eventProvider, _eventDeserializer, _clock);
        var lastActiveQuestionTime = await facade
            .GetActiveQuestionDateAsync(current.QuestionId, cancellationToken)
            .OrderByDescending(e => e.StartActiveDate)
            .Select(e => ((DateTime StartActiveDate, DateTime EndActiveDate)?)e)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastActiveQuestionTime is not null)
        {
            var codeEditorContentRequest = new EPStorageEventRequest
            {
                Type = EventType.CodeEditorChange,
                From = lastActiveQuestionTime.Value.StartActiveDate,
                To = lastActiveQuestionTime.Value.EndActiveDate,
            };
            var lastCodeEditorState = await eventProvider.GetLatestEventAsync(codeEditorContentRequest, cancellationToken);
            if (lastCodeEditorState is not null)
            {
                return lastCodeEditorState.Payload;
            }
        }

        if (current.Question is not null && current.Question.CodeEditorId is null)
        {
            return null;
        }

        var content = current.Question?.CodeEditor?.Content;
        if (content is not null)
        {
            return content;
        }

        var resultContent = await _db.Questions
            .AsNoTracking()
            .Include(e => e.CodeEditor)
            .Where(e => e.Id == current.QuestionId)
            .Select(e => e.CodeEditor == null ? null : e.CodeEditor.Content)
            .FirstOrDefaultAsync(cancellationToken);
        return resultContent;
    }

    private async Task<RoomCodeEditorEnabledEvent.Payload> GetCodeEditorEventPayload(RoomQuestion roomQuestion, CancellationToken cancellationToken)
    {
        if (roomQuestion.Question is not null)
        {
            return new RoomCodeEditorEnabledEvent.Payload { Enabled = roomQuestion.Question.CodeEditorId is not null, };
        }

        var hasCodeEditor = await _db.Questions
            .Where(e => e.Id == roomQuestion.QuestionId)
            .AnyAsync(e => e.CodeEditorId != null, cancellationToken);
        return new RoomCodeEditorEnabledEvent.Payload { Enabled = hasCodeEditor, };
    }
}
