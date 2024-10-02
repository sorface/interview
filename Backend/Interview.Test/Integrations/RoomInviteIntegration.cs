using FluentAssertions;
using Interview.Domain.Database;
using Interview.Domain.Events.Storage;
using Interview.Domain.Invites;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations;

public class RoomInviteIntegration
{
    [Fact(DisplayName = "using an invitation for a closed room that the user has not been in before")]
    public async Task AccessRoomPrivateByInvite()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user = new User("devpav", Guid.NewGuid().ToString());

        appDbContext.Users.Add(user);
        await appDbContext.SaveChangesAsync();

        var invite = new Invite(5);

        appDbContext.Invites.Add(invite);
        await appDbContext.SaveChangesAsync();

        var room = new Room(name: "something", SERoomAccessType.Private);

        appDbContext.Rooms.Add(room);
        await appDbContext.SaveChangesAsync();

        var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);

        appDbContext.RoomInvites.Add(roomInvite);
        await appDbContext.SaveChangesAsync();

        var userAccessor = new CurrentUserAccessor();
        {
            userAccessor.SetUser(user);
        }
        var roomParticipantService = CreateRoomParticipantService(appDbContext, userAccessor);
        var roomService = CreateRoomService(appDbContext, roomParticipantService, userAccessor);

        await roomService.ApplyInvite(room.Id, invite.Id);

        var foundInvite = await appDbContext.Invites.FirstOrDefaultAsync(item => item.Id == invite.Id);

        Assert.NotNull(foundInvite);

        foundInvite.UsesCurrent.Should().Be(1);

        var roomParticipant = await appDbContext.RoomParticipants.FirstOrDefaultAsync(participant =>
            participant.Room.Id == room.Id
        );

        Assert.NotNull(roomParticipant);

        roomParticipant.User.Id.Should().Be(user.Id);
        roomParticipant.Type.EnumValue.Should().Be(EVRoomParticipantType.Expert);
        roomParticipant.Room.Id.Should().Be(room.Id);
    }

    [Fact(DisplayName = "using an invitation for a closed room that the user has previously been in")]
    public async Task AccessRoomPrivateByInviteWhenUserAlreadyExistsInTheRoom()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        await using var transaction = await appDbContext.Database.BeginTransactionAsync();

        var user = new User("devpav", Guid.NewGuid().ToString());

        appDbContext.Users.Add(user);
        var invite = new Invite(5);

        appDbContext.Invites.Add(invite);
        var room = new Room(name: "something", SERoomAccessType.Private);

        appDbContext.Rooms.Add(room);
        var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);

        appDbContext.RoomInvites.Add(roomInvite);
        var participant = new RoomParticipant(user, room, SERoomParticipantType.Examinee);

        appDbContext.RoomParticipants.Add(participant);
        await appDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        var userAccessor = new CurrentUserAccessor();
        {
            userAccessor.SetUser(user);
        }

        var roomParticipantService = CreateRoomParticipantService(appDbContext, userAccessor);
        var roomService = CreateRoomService(appDbContext, roomParticipantService, userAccessor);

        await roomService.ApplyInvite(room.Id, invite.Id);

        var foundInvite = await appDbContext.Invites.FirstOrDefaultAsync(item => item.Id == invite.Id);

        Assert.NotNull(foundInvite);

        foundInvite.UsesCurrent.Should().Be(0);

        var roomParticipant = await appDbContext.RoomParticipants.FirstOrDefaultAsync(itemParticipant =>
            itemParticipant.Room.Id == room.Id
        );

        Assert.NotNull(roomParticipant);

        roomParticipant.User.Id.Should().Be(participant.User.Id);
        roomParticipant.Type.EnumValue.Should().Be(roomInvite.ParticipantType!.EnumValue);
        roomParticipant.Room.Id.Should().Be(room.Id);
    }

    [Fact(DisplayName = "using an invitation for a closed room that the user has previously been in")]
    public async Task AccessRoomPrivateByInviteWhenInvitationLimited()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user = new User("devpav", Guid.NewGuid().ToString());

        appDbContext.Users.Add(user);
        var invite = new Invite(5) { UsesCurrent = 4 };
        await appDbContext.SaveChangesAsync();

        appDbContext.Invites.Add(invite);
        var room = new Room(name: "something", SERoomAccessType.Private);
        await appDbContext.SaveChangesAsync();

        appDbContext.Rooms.Add(room);
        var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);
        await appDbContext.SaveChangesAsync();

        appDbContext.RoomInvites.Add(roomInvite);
        await appDbContext.SaveChangesAsync();

        var userAccessor = new CurrentUserAccessor();
        {
            userAccessor.SetUser(user);
        }
        var roomParticipantService = CreateRoomParticipantService(appDbContext, userAccessor);
        var roomService = CreateRoomService(appDbContext, roomParticipantService, userAccessor);

        await roomService.ApplyInvite(room.Id, invite.Id);

        var deletedRoom = await appDbContext.Invites.AnyAsync(item => item.Id == invite.Id);

        Assert.False(deletedRoom);

        var foundRoomInvite = await appDbContext.RoomInvites.FirstOrDefaultAsync(item => item.RoomId == room.Id);

        Assert.NotNull(foundRoomInvite);
        foundRoomInvite.ParticipantType.Should().Be(SERoomParticipantType.Expert);

        var generatedInvite = await appDbContext.Invites.FirstOrDefaultAsync(item => foundRoomInvite.InviteId == item.Id);

        Assert.NotNull(generatedInvite);

        generatedInvite.UsesCurrent.Should().Be(0);
    }

    private static RoomParticipantService CreateRoomParticipantService(AppDbContext appDbContext, CurrentUserAccessor userAccessor)
    {
        return new RoomParticipantService(
            new RoomParticipantRepository(appDbContext),
            new RoomRepository(appDbContext),
            new UserRepository(appDbContext),
            userAccessor,
            new PermissionRepository(appDbContext));
    }

    private static RoomService CreateRoomService(
        AppDbContext appDbContext,
        RoomParticipantService roomParticipantService,
        CurrentUserAccessor userAccessor)
    {
        return new RoomService(
            new RoomQuestionRepository(appDbContext),
            new EmptyRoomEventDispatcher(),
            new EmptyHotEventStorage(),
            new RoomInviteService(appDbContext, roomParticipantService, NullLogger<RoomInviteService>.Instance),
            userAccessor,
            roomParticipantService,
            appDbContext,
            NullLogger<RoomService>.Instance,
            new TestSystemClock(),
            new RoomAnalyticService(appDbContext)
        );
    }
}
