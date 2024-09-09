using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Logging;
using NSpecifications;

namespace Interview.Domain.Events;

public class EventStorage2DatabaseService
{
    private readonly IQueuedRoomEventRepository _queuedRoomEventRepository;
    private readonly IDbRoomEventRepository _roomEventRepository;
    private readonly IEventStorage _eventStorage;
    private readonly ILogger<EventStorage2DatabaseService> _logger;

    private int ChunkSize { get; set; } = 100;

    public EventStorage2DatabaseService(
        IQueuedRoomEventRepository queuedRoomEventRepository,
        IDbRoomEventRepository roomEventRepository,
        IEventStorage eventStorage,
        ILogger<EventStorage2DatabaseService> logger)
    {
        _queuedRoomEventRepository = queuedRoomEventRepository;
        _roomEventRepository = roomEventRepository;
        _eventStorage = eventStorage;
        _logger = logger;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start EventStorage2DatabaseService");
        try
        {
            const int pageNumber = 1;
            const int pageSize = 200;
            var rooms = await _queuedRoomEventRepository.GetNotProcessedRoomsAsync(pageNumber, pageSize, cancellationToken);
            while (rooms.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var roomId in rooms)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        _logger.LogInformation("Start process room {RoomId}", roomId);
                        await ProcessRoomAsync(roomId, cancellationToken);
                        _logger.LogInformation("End process room {RoomId}", roomId);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "During process room {RoomId}", roomId);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                rooms = await _queuedRoomEventRepository.GetNotProcessedRoomsAsync(pageNumber, pageSize, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "During process");
            throw;
        }
        finally
        {
            _logger.LogInformation("End EventStorage2DatabaseService");
        }
    }

    public async Task ProcessRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var specification = new Spec<IStorageEvent>(e => e.RoomId == roomId);
        var pageNumber = 0;
        await foreach (var collection in _eventStorage.GetBySpecAsync(specification, ChunkSize, cancellationToken))
        {
            pageNumber += 1;
            var dbEvents = collection.Select(e => new DbRoomEvent
            {
                Id = e.Id,
                RoomId = e.RoomId,
                Type = e.Type,
                Stateful = e.Stateful,
                Payload = e.Payload,
                CreateDate = e.CreatedAt,
                UpdateDate = e.CreatedAt,
            });
            await _roomEventRepository.CreateRangeAsync(dbEvents, cancellationToken);

            try
            {
                _logger.LogInformation("Start delete {RoomId} events {Page}", roomId, pageNumber);
                await _eventStorage.DeleteAsync(collection, cancellationToken);
                _logger.LogInformation("End delete {RoomId} events {Page}", roomId, pageNumber);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "During delete events {RoomId} {Page}", roomId, pageNumber);
            }
        }

        await _queuedRoomEventRepository.CreateAsync(new QueuedRoomEvent { RoomId = roomId }, cancellationToken);
    }
}
