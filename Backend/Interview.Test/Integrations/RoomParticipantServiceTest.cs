using Bogus;
using FluentAssertions;
using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations;

public class RoomParticipantServiceTest
{
    public static IEnumerable<object[]> CreateData
    {
        get
        {
            yield return new object[] { SERoomParticipantType.Viewer };
            yield return new object[] { SERoomParticipantType.Examinee };
            yield return new object[] { SERoomParticipantType.Expert };
        }
    }

    public static IEnumerable<object[]> ChangeStatusData
    {
        get
        {
            foreach (var initialType in SERoomParticipantType.List)
            {
                foreach (var newType in SERoomParticipantType.List.Where(e => e != initialType))
                {
                    yield return new object[] { initialType, newType };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(CreateData))]
    public async Task Create(SERoomParticipantType type)
    {
        await using var appDbContext = CreateDbContext(out var room, out var user);

        var service = CreateService(appDbContext);

        var request = new RoomParticipantCreateRequest { Type = type.Name, RoomId = room.Id, UserId = user.Id, };
        var participantResponse = await service.CreateAsync(request);
        Assert(appDbContext, participantResponse.Id, type, room, user);
    }

    [Theory]
    [MemberData(nameof(ChangeStatusData))]
    public async Task ChangeStatus(SERoomParticipantType initialType, SERoomParticipantType newType)
    {
        await using var appDbContext = CreateDbContext(out var room, out var user);

        var service = CreateService(appDbContext);

        var request = new RoomParticipantCreateRequest { Type = initialType.Name, RoomId = room.Id, UserId = user.Id, };
        var participantResponse = await service.CreateAsync(request);
        appDbContext.ChangeTracker.Clear();

        var changeStatusRequest = new RoomParticipantChangeStatusRequest
        {
            RoomId = room.Id,
            UserId = user.Id,
            UserType = newType.EnumValue
        };
        _ = await service.ChangeStatusAsync(changeStatusRequest);

        Assert(appDbContext, participantResponse.Id, newType, room, user);
    }

    private static AppDbContext CreateDbContext(out Room room, out User user)
    {
        var faker = new Faker();
        var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        room = new Room(faker.Random.Word(), SERoomAccessType.Public);
        appDbContext.Rooms.Add(room);
        user = new User(faker.Random.Word(), string.Empty);
        appDbContext.Users.Add(user);
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();
        return appDbContext;
    }

    private static void Assert(AppDbContext appDbContext,
        Guid participantId,
        SERoomParticipantType type,
        Room room,
        User user)
    {
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var participant = appDbContext.RoomParticipants
            .Include(e => e.Room)
            .Include(e => e.User)
            .Include(e => e.Permissions)
            .First(e => e.Id == participantId);

        participant.Room.Id.Should().Be(room.Id);
        participant.User.Id.Should().Be(user.Id);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        participant.Type.Should().Be(type);
        var defaultRoomPermission = type.DefaultRoomPermission.Select(e => e.Id).ToHashSet();
        participant.Permissions.Should()
            .HaveCount(defaultRoomPermission.Count)
            .And
            .Match(e => e.All(p => defaultRoomPermission.Contains(p.Id)));
    }

    private RoomParticipantService CreateService(AppDbContext db)
    {
        return new RoomParticipantService(
            new RoomParticipantRepository(db),
            new RoomRepository(db),
            new UserRepository(db),
            new CurrentUserAccessor(),
            new PermissionRepository(db)
        );
    }
}
