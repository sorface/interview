using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor : EntityPostProcessor<RoomQuestion>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly AppDbContext _db;
    private readonly RoomCodeEditorChangeEventHandler _roomCodeEditorChangeEventHandler;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRoomConfigurationRepository _roomConfigurationRepository;

    public RoomQuestionPostProcessor(
        IRoomEventDispatcher eventDispatcher,
        AppDbContext db,
        RoomCodeEditorChangeEventHandler roomCodeEditorChangeEventHandler,
        ICurrentUserAccessor currentUserAccessor,
        IRoomConfigurationRepository roomConfigurationRepository)
    {
        _eventDispatcher = eventDispatcher;
        _db = db;
        _roomCodeEditorChangeEventHandler = roomCodeEditorChangeEventHandler;
        _currentUserAccessor = currentUserAccessor;
        _roomConfigurationRepository = roomConfigurationRepository;
    }

    public override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent(entity.RoomId, new RoomQuestionAddEventPayload(entity.QuestionId, entity.State.EnumValue), _currentUserAccessor.GetUserIdOrThrow());

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

        var @event = new RoomQuestionChangeEvent(
            current.RoomId,
            new RoomQuestionChangeEventPayload(current.QuestionId, original.State.EnumValue, current.State.EnumValue),
            _currentUserAccessor.GetUserIdOrThrow());

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

    private ValueTask<string?> GetCodeEditorContentAsync(RoomQuestion current, CancellationToken cancellationToken)
    {
        if (current.Question is not null && current.Question.CodeEditorId is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        var content = current.Question?.CodeEditor?.Content;
        if (content is not null)
        {
            return ValueTask.FromResult<string?>(content);
        }

        var task = _db.Questions
            .AsNoTracking()
            .Include(e => e.CodeEditor)
            .Where(e => e.Id == current.QuestionId)
            .Select(e => e.CodeEditor == null ? null : e.CodeEditor.Content)
            .FirstOrDefaultAsync(cancellationToken);
        return new ValueTask<string?>(task);
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
