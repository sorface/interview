using FluentAssertions;
using Interview.Domain.Events;
using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Interview.Infrastructure.Events;
using Interview.Infrastructure.Rooms;
using Microsoft.Extensions.Logging.Abstractions;
using NSpecifications;
using X.PagedList;

namespace Interview.Test.Integrations;

public class EventStorage2DatabaseServiceTest
{
    [Fact]
    public async Task Process()
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);

        var user = new User("TEST", "ID");
        appDbContext.Users.Add(user);
        var room1 = new Room("Test 1", SERoomAccessType.Public, SERoomType.Standard) { Status = SERoomStatus.Close, };
        appDbContext.Rooms.Add(room1);
        appDbContext.Rooms.Add(new Room("Test 2", SERoomAccessType.Public, SERoomType.Standard) { Status = SERoomStatus.Close, });
        appDbContext.Rooms.Add(new Room("Test 3", SERoomAccessType.Public, SERoomType.Standard) { Status = SERoomStatus.Close, });
        appDbContext.Rooms.Add(new Room("Test 4", SERoomAccessType.Public, SERoomType.Standard) { Status = SERoomStatus.Close, });
        appDbContext.SaveChanges();
        var queuedRoomEvents = appDbContext.Rooms.Where(e => e.Id != room1.Id).Select(e => new QueuedRoomEvent { RoomId = e.Id, });
        appDbContext.QueuedRoomEvents.AddRange(queuedRoomEvents);
        appDbContext.SaveChanges();

        var eventStorage = new TestInMemoryHotEventStorage();
        var storageEvents = new StorageEvent[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                Payload = "1",
                Stateful = false,
                Type = "Test 1",
                CreatedAt = new DateTime(2000, 1, 15),
                RoomId = room1.Id,
                CreatedById = user.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Payload = "2",
                Stateful = true,
                Type = "Test 2",
                CreatedAt = new DateTime(2015, 12, 1),
                RoomId = room1.Id,
                CreatedById = user.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Payload = "3",
                Stateful = false,
                Type = "Test 3",
                CreatedAt = new DateTime(2023, 05, 04),
                RoomId = room1.Id,
                CreatedById = user.Id
            },
        }
            .OrderBy(e => e.Payload)
            .ToArray();
        foreach (var storageEvent in storageEvents)
        {
            await eventStorage.AddAsync(storageEvent, CancellationToken.None);
        }

        var initialEvents = await appDbContext.RoomEvents.ToListAsync();
        var service = new EventStorage2DatabaseService(
            new QueuedRoomEventRepository(appDbContext),
            new DbRoomEventRepository(appDbContext),
            eventStorage,
            NullLogger<EventStorage2DatabaseService>.Instance);

        await service.ProcessAsync(CancellationToken.None);

        var actualEvents = await appDbContext.RoomEvents.OrderBy(e => e.Payload).ToListAsync();
        var actualStorageEvents = await eventStorage
            .GetBySpecAsync(Spec<IStorageEvent>.Any, 100, CancellationToken.None)
            .ToListAsync();

        initialEvents.Should().BeEmpty();
        var checks = storageEvents
            .Select(storageEvent => new Action<DbRoomEvent>(ev =>
            {
                ev.RoomId.Should().Be(storageEvent.RoomId);
                ev.Stateful.Should().Be(storageEvent.Stateful);
                ev.Payload.Should().Be(storageEvent.Payload);
                ev.Type.Should().Be(storageEvent.Type);
                ev.Id.Should().Be(storageEvent.Id);
            }))
            .ToList();
        actualEvents.Should().HaveSameCount(storageEvents).And.SatisfyRespectively(checks);
        actualStorageEvents.SelectMany(e => e).Count().Should().Be(0);
    }
}
