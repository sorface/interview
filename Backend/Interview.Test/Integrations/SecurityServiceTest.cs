using Bogus;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Permissions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations;

public class SecurityServiceTest
{
    public static IEnumerable<object[]> EnsureRoomPermission_AvailableData
    {
        get
        {
            foreach (var participantType in SERoomParticipantType.List)
            {
                foreach (var availablePermission in participantType.DefaultRoomPermission)
                {
                    yield return [participantType, availablePermission];
                }
            }
        }
    }

    public static IEnumerable<object[]> EnsureRoomPermission_NotAvailableData
    {
        get
        {
            foreach (var participantType in SERoomParticipantType.List)
            {
                foreach (var notAvailablePermission in SEPermission.List.Where(it => !it.Equals(SEPermission.Unknown))
                             .Except(participantType.DefaultRoomPermission))
                {
                    yield return [participantType, notAvailablePermission];
                }
            }
        }
    }

    [Theory(DisplayName = "Checks if the default permission is available to a member of the room")]
    [MemberData(nameof(EnsureRoomPermission_AvailableData))]
    public async Task EnsureRoomPermission_Available(SERoomParticipantType type, SEPermission checkPermission)
    {
        await using var db = CreateDbContext(type, out var room, out var user);
        var service = CreateService(db, user);

        await service.EnsureRoomPermissionAsync(room.Id, checkPermission, CancellationToken.None);
        Assert.True(true);
    }

    [Theory]
    [MemberData(nameof(EnsureRoomPermission_NotAvailableData))]
    public async Task EnsureRoomPermission_NotAvailable(SERoomParticipantType type, SEPermission checkPermission)
    {
        await using var db = CreateDbContext(type, out var room, out var user);
        var service = CreateService(db, user);

        await Assert.ThrowsAsync<AccessDeniedException>(() => service.EnsureRoomPermissionAsync(room.Id, checkPermission, CancellationToken.None));
    }

    [Theory(DisplayName = "A user who has not participated in the room does not have access.")]
    [MemberData(nameof(EnsureRoomPermission_AvailableData))]
    public async Task EnsureRoomPermission_UserNotRoomParticipant(SERoomParticipantType type, SEPermission checkPermission)
    {
        await using var db = CreateDbContext(type, out var room, out _);
        var userWithoutRoomParticipant = new User("WITHOUT ROOM PARTICIPANT", string.Empty);
        db.Users.Add(userWithoutRoomParticipant);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = CreateService(db, userWithoutRoomParticipant);

        await Assert.ThrowsAsync<AccessDeniedException>(() => service.EnsureRoomPermissionAsync(room.Id, checkPermission, CancellationToken.None));
    }

    private static AppDbContext CreateDbContext(SERoomParticipantType type, out Room room, out User user)
    {
        var faker = new Faker();
        var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        room = new Room(faker.Random.Word(), SERoomAccessType.Public, SERoomType.Standard);
        appDbContext.Rooms.Add(room);
        user = new User(faker.Random.Word(), string.Empty);
        appDbContext.Users.Add(user);
        appDbContext.SaveChanges();

        var request = new RoomParticipantCreateRequest { Type = type.Name, RoomId = room.Id, UserId = user.Id, };
        var roomParticipantService = CreateRoomParticipantService(appDbContext);
        roomParticipantService.CreateAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();
        return appDbContext;
    }

    private static RoomParticipantService CreateRoomParticipantService(AppDbContext db)
    {
        return new RoomParticipantService(
            new RoomParticipantRepository(db),
            new RoomRepository(db),
            new UserRepository(db),
            new CurrentUserAccessor(),
            new PermissionRepository(db)
        );
    }

    private static ISecurityService CreateService(AppDbContext dbContext, User user)
    {
        return new SecurityService(
#pragma warning disable CA2000
            new CurrentPermissionAccessor(dbContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<CurrentPermissionAccessor>.Instance),
#pragma warning restore CA2000
            new CurrentUserAccessor(user),
            new RoomParticipantRepository(dbContext));
    }
}
