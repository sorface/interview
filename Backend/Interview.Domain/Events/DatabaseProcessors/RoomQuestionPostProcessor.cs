using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor : EntityPostProcessor<RoomQuestion>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly AppDbContext _db;
    private readonly RoomCodeEditorChangeEventHandler _roomCodeEditorChangeEventHandler;

    public RoomQuestionPostProcessor(IRoomEventDispatcher eventDispatcher, AppDbContext db, RoomCodeEditorChangeEventHandler roomCodeEditorChangeEventHandler)
    {
        _eventDispatcher = eventDispatcher;
        _db = db;
        _roomCodeEditorChangeEventHandler = roomCodeEditorChangeEventHandler;
    }

    public override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent(entity.RoomId, new RoomQuestionAddEventPayload(entity.QuestionId, entity.State));

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
            new RoomQuestionChangeEventPayload(current.QuestionId, original.State, current.State));

        await _eventDispatcher.WriteAsync(@event, cancellationToken);

        if (current.State == RoomQuestionState.Active)
        {
            var codeEditorPayload = await GetCodeEditorEventPayload(current, cancellationToken);
            await _roomCodeEditorChangeEventHandler.HandleAsync(current.RoomId, codeEditorPayload.Enabled, cancellationToken);
        }
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
