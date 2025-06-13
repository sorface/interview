using System.Linq.Expressions;
using Interview.Domain.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using NSpecifications;

namespace Interview.Domain.Rooms.RoomExpireServices;

public class RoomExpireService(
    AppDbContext dbContext,
    RoomExpireSettings roomExpireSettings,
    ISystemClock clock,
    ILogger<RoomExpireService> logger) : ISelfScopeService
{
    public int PageSize { get; init; } = 500;

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting room expire service.");
        try
        {
            var now = clock.UtcNow;
            var reviewMinDate = now.Subtract(TimeSpan.FromDays(roomExpireSettings.ReviewDayExpiration)).UtcDateTime;
            await MarkExpiredAsync(SERoomStatus.Review, reviewMinDate, cancellationToken);

            var activeMinDate = now.Subtract(TimeSpan.FromDays(roomExpireSettings.ActiveDayExpiration)).UtcDateTime;
            await MarkExpiredAsync(SERoomStatus.Active, activeMinDate, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Stopping room expire service.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Room expire service. Unexpected error occurred.");
            throw;
        }
        finally
        {
            logger.LogInformation("Stoping room expire service.");
        }
    }

    private async Task MarkExpiredAsync(SERoomStatus status, DateTime minDate, CancellationToken cancellationToken)
    {
        _ = logger.BeginScope("Marking room {Status}", status.Name);
        logger.LogInformation("Start");
        try
        {
            List<Room> rooms;
            do
            {
                rooms = await dbContext.Rooms.Where(e => e.Status == status && e.UpdateDate <= minDate)
                    .Take(PageSize)
                    .ToListAsync(cancellationToken);
                logger.LogInformation("Marking room {Count}", rooms.Count);
                foreach (var room in rooms)
                {
                    room.Status = SERoomStatus.Expire;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            while (rooms.Count == PageSize);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Canceling");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred.");
            throw;
        }
        finally
        {
            logger.LogInformation("Stop");
        }
    }
}
