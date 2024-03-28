using FluentAssertions;
using Interview.Domain.Events.Storage;
using Interview.Domain.Invites;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.Events;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestionReactions;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Tags;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations
{
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

            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);

            appDbContext.Rooms.Add(room);
            await appDbContext.SaveChangesAsync();

            var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);

            appDbContext.RoomInvites.Add(roomInvite);
            await appDbContext.SaveChangesAsync();

            var roomRepository = new RoomRepository(appDbContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }
            var roomParticipantService = new RoomParticipantService(new RoomParticipantRepository(appDbContext), new RoomRepository(appDbContext), new UserRepository(appDbContext), new AvailableRoomPermissionRepository(appDbContext), userAccessor);
            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(appDbContext),
                new QuestionRepository(appDbContext),
                new UserRepository(appDbContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(appDbContext),
                new TagRepository(appDbContext),
                new RoomParticipantRepository(appDbContext),
                new AppEventRepository(appDbContext),
                new RoomStateRepository(appDbContext),
                new EmptyEventStorage(),
                new RoomInviteService(appDbContext, roomParticipantService),
                userAccessor,
                roomParticipantService
            );

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

            var transaction = await appDbContext.Database.BeginTransactionAsync();

            var user = new User("devpav", Guid.NewGuid().ToString());

            appDbContext.Users.Add(user);
            var invite = new Invite(5);

            appDbContext.Invites.Add(invite);
            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);

            appDbContext.Rooms.Add(room);
            var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);

            appDbContext.RoomInvites.Add(roomInvite);
            var participant = new RoomParticipant(user, room, SERoomParticipantType.Examinee);

            appDbContext.RoomParticipants.Add(participant);
            await appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var roomRepository = new RoomRepository(appDbContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }
            var roomParticipantService = new RoomParticipantService(new RoomParticipantRepository(appDbContext), new RoomRepository(appDbContext), new UserRepository(appDbContext), new AvailableRoomPermissionRepository(appDbContext), userAccessor);
            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(appDbContext),
                new QuestionRepository(appDbContext),
                new UserRepository(appDbContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(appDbContext),
                new TagRepository(appDbContext),
                new RoomParticipantRepository(appDbContext),
                new AppEventRepository(appDbContext),
                new RoomStateRepository(appDbContext),
                new EmptyEventStorage(),
                new RoomInviteService(appDbContext, roomParticipantService),
                userAccessor,
                roomParticipantService
            );

            await roomService.ApplyInvite(room.Id, invite.Id);

            var foundInvite = await appDbContext.Invites.FirstOrDefaultAsync(item => item.Id == invite.Id);

            Assert.NotNull(foundInvite);

            foundInvite.UsesCurrent.Should().Be(0);

            var roomParticipant = await appDbContext.RoomParticipants.FirstOrDefaultAsync(itemParticipant =>
                itemParticipant.Room.Id == room.Id
            );

            Assert.NotNull(roomParticipant);

            roomParticipant.User.Id.Should().Be(participant.User.Id);
            roomParticipant.Type.EnumValue.Should().Be(EVRoomParticipantType.Examinee);
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
            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);
            await appDbContext.SaveChangesAsync();

            appDbContext.Rooms.Add(room);
            var roomInvite = new RoomInvite(invite, room, SERoomParticipantType.Expert);
            await appDbContext.SaveChangesAsync();

            appDbContext.RoomInvites.Add(roomInvite);
            await appDbContext.SaveChangesAsync();

            var roomRepository = new RoomRepository(appDbContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }
            var roomParticipantService = new RoomParticipantService(new RoomParticipantRepository(appDbContext), new RoomRepository(appDbContext), new UserRepository(appDbContext), new AvailableRoomPermissionRepository(appDbContext), userAccessor);
            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(appDbContext),
                new QuestionRepository(appDbContext),
                new UserRepository(appDbContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(appDbContext),
                new TagRepository(appDbContext),
                new RoomParticipantRepository(appDbContext),
                new AppEventRepository(appDbContext),
                new RoomStateRepository(appDbContext),
                new EmptyEventStorage(),
                new RoomInviteService(appDbContext, roomParticipantService),
                userAccessor,
                roomParticipantService
            );

            await roomService.ApplyInvite(room.Id, invite.Id);

            var deletedRoom = await appDbContext.Invites.AnyAsync(item => item.Id == invite.Id);

            Assert.False(deletedRoom);

            var foundRoomInvite = await appDbContext.RoomInvites.FirstOrDefaultAsync(item => item.RoomById == room.Id);

            Assert.NotNull(foundRoomInvite);
            foundRoomInvite.ParticipantType.Should().Be(SERoomParticipantType.Expert);

            var generatedInvite = await appDbContext.Invites.FirstOrDefaultAsync(item => foundRoomInvite.InviteById == item.Id);

            Assert.NotNull(generatedInvite);

            generatedInvite.UsesCurrent.Should().Be(0);
        }
    }
}
