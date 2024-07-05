using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;

namespace Interview.Domain.Events.Sender;

public interface IEventSender
{
    Task SendAsync(ReadOnlyMemory<byte> @event, CancellationToken cancellationToken);
}

public interface IConnectionDetail
{
    Room Room { get; }

    User User { get; }

    EVRoomParticipantType ParticipantType { get; }
}

public interface IEventSenderFactory
{
    IEventSender Create(IConnectionDetail connectionDetail);
}
