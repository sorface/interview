using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.RoomConfigurations;

public class RoomConfigurationRepository : EfRepository<RoomConfiguration>, IRoomConfigurationRepository
{
    public RoomConfigurationRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task UpsertCodeStateAsync(UpsertCodeStateRequest request, CancellationToken cancellationToken)
    {
        var room = await Db.Rooms.Include(e => e.Configuration)
            .FirstOrDefaultAsync(e => e.Id == request.RoomId, cancellationToken);
        if (room is null)
        {
            throw new ApplicationException($"Unknown room '{request.RoomId}'");
        }

        if (room.Configuration is null)
        {
            room.Configuration = new RoomConfiguration
            {
                CodeEditorContent = request.CodeEditorContent,
            };
        }
        else
        {
            room.Configuration.CodeEditorContent = request.CodeEditorContent;
        }

        await Db.SaveChangesAsync(cancellationToken);
    }
}
