using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPreProcessor : EntityPreProcessor<RoomQuestion>
{
    private readonly AppDbContext _db;
    private readonly IRoomConfigurationRepository _roomConfigurationRepository;

    public RoomQuestionPreProcessor(IRoomConfigurationRepository roomConfigurationRepository, AppDbContext db)
    {
        _roomConfigurationRepository = roomConfigurationRepository;
        _db = db;
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

        if (original.State == RoomQuestionState.Active || current.State != RoomQuestionState.Active)
        {
            return;
        }

        var codeEditorContent = await GetCodeEditorContentAsync(current, cancellationToken);
        var upsertCodeStateRequest = new UpsertCodeStateRequest
        {
            RoomId = current.RoomId,
            CodeEditorContent = codeEditorContent ?? string.Empty,
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
}
