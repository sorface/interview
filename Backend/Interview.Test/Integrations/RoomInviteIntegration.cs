using FluentAssertions;
using Interview.Domain.Events.Storage;
using Interview.Domain.Invites;
using Interview.Domain.RoomInvites;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.Events;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomInvites;
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
            await using var databaseContext = new TestAppDbContextFactory().Create(new TestSystemClock());

            var user = new User("devpav", Guid.NewGuid().ToString());

            databaseContext.Users.Add(user);
            await databaseContext.SaveChangesAsync();

            var invite = new Invite(5);

            databaseContext.Invites.Add(invite);
            await databaseContext.SaveChangesAsync();

            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);

            databaseContext.Rooms.Add(room);
            await databaseContext.SaveChangesAsync();

            var roomInvite = new RoomInvite(invite, room, RoomParticipantType.Expert);

            databaseContext.RoomInvites.Add(roomInvite);
            await databaseContext.SaveChangesAsync();

            var roomRepository = new RoomRepository(databaseContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }

            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(databaseContext),
                new QuestionRepository(databaseContext),
                new UserRepository(databaseContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(databaseContext),
                new TagRepository(databaseContext),
                new RoomParticipantRepository(databaseContext),
                new AppEventRepository(databaseContext),
                new RoomStateRepository(databaseContext),
                new EmptyEventStorage(),
                new RoomInviteRepository(databaseContext),
                userAccessor
            );

            await roomService.ApplyInvite(room.Id, invite.Id);

            var foundInvite = await databaseContext.Invites.FirstOrDefaultAsync(item => item.Id == invite.Id);

            Assert.NotNull(foundInvite);

            foundInvite.UsesCurrent.Should().Be(1);

            var roomParticipant = await databaseContext.RoomParticipants.FirstOrDefaultAsync(participant =>
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
            await using var databaseContext = new TestAppDbContextFactory().Create(new TestSystemClock());

            var transaction = await databaseContext.Database.BeginTransactionAsync();

            var user = new User("devpav", Guid.NewGuid().ToString());

            databaseContext.Users.Add(user);
            var invite = new Invite(5);

            databaseContext.Invites.Add(invite);
            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);

            databaseContext.Rooms.Add(room);
            var roomInvite = new RoomInvite(invite, room, RoomParticipantType.Expert);

            databaseContext.RoomInvites.Add(roomInvite);
            var participant = new RoomParticipant(user, room, RoomParticipantType.Examinee);

            databaseContext.RoomParticipants.Add(participant);
            await databaseContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var roomRepository = new RoomRepository(databaseContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }

            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(databaseContext),
                new QuestionRepository(databaseContext),
                new UserRepository(databaseContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(databaseContext),
                new TagRepository(databaseContext),
                new RoomParticipantRepository(databaseContext),
                new AppEventRepository(databaseContext),
                new RoomStateRepository(databaseContext),
                new EmptyEventStorage(),
                new RoomInviteRepository(databaseContext),
                userAccessor
            );

            await roomService.ApplyInvite(room.Id, invite.Id);

            var foundInvite = await databaseContext.Invites.FirstOrDefaultAsync(item => item.Id == invite.Id);

            Assert.NotNull(foundInvite);

            foundInvite.UsesCurrent.Should().Be(0);

            var roomParticipant = await databaseContext.RoomParticipants.FirstOrDefaultAsync(itemParticipant =>
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
            await using var databaseContext = new TestAppDbContextFactory().Create(new TestSystemClock());

            var user = new User("devpav", Guid.NewGuid().ToString());

            databaseContext.Users.Add(user);
            var invite = new Invite(5) { UsesCurrent = 4 };
            await databaseContext.SaveChangesAsync();

            databaseContext.Invites.Add(invite);
            var room = new Room(name: "something", twitchChannel: "twitch channel", SERoomAcсessType.Private);
            await databaseContext.SaveChangesAsync();

            databaseContext.Rooms.Add(room);
            var roomInvite = new RoomInvite(invite, room, RoomParticipantType.Expert);
            await databaseContext.SaveChangesAsync();

            databaseContext.RoomInvites.Add(roomInvite);
            await databaseContext.SaveChangesAsync();

            var roomRepository = new RoomRepository(databaseContext);
            var userAccessor = new CurrentUserAccessor();
            {
                userAccessor.SetUser(user);
            }

            var roomService = new RoomService(
                roomRepository,
                new RoomQuestionRepository(databaseContext),
                new QuestionRepository(databaseContext),
                new UserRepository(databaseContext),
                new EmptyRoomEventDispatcher(),
                new RoomQuestionReactionRepository(databaseContext),
                new TagRepository(databaseContext),
                new RoomParticipantRepository(databaseContext),
                new AppEventRepository(databaseContext),
                new RoomStateRepository(databaseContext),
                new EmptyEventStorage(),
                new RoomInviteRepository(databaseContext),
                userAccessor
            );

            await roomService.ApplyInvite(room.Id, invite.Id);

            var deletedRoom = await databaseContext.Invites.AnyAsync(item => item.Id == invite.Id);

            Assert.False(deletedRoom);

            var foundRoomInvite = await databaseContext.RoomInvites.FirstOrDefaultAsync(item => item.RoomById == room.Id);

            Assert.NotNull(foundRoomInvite);
            foundRoomInvite.ParticipantType.Should().Be(RoomParticipantType.Expert);

            var generatedInvite = await databaseContext.Invites.FirstOrDefaultAsync(item => foundRoomInvite.InviteById == item.Id);

            Assert.NotNull(generatedInvite);

            generatedInvite.UsesCurrent.Should().Be(0);
        }
    }
}
