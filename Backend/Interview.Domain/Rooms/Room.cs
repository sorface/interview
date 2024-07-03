using System.Runtime.CompilerServices;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;

[assembly: InternalsVisibleTo("Interview.Test")]

namespace Interview.Domain.Rooms;

public class Room : Entity
{
    public Room(string name, SERoomAccessType accessType)
    {
        Name = name;
        Status = SERoomStatus.New;
        AccessType = accessType;
    }

    private Room()
        : this(string.Empty, SERoomAccessType.Public)
    {
    }

    public string Name { get; internal set; }

    public SERoomAccessType AccessType { get; internal set; }

    public SERoomStatus Status { get; internal set; }

    public RoomConfiguration? Configuration { get; set; }

    public RoomTimer? Timer { get; set; }

    public List<RoomQuestion> Questions { get; set; } = new();

    public List<RoomParticipant> Participants { get; set; } = new();

    public List<RoomState> RoomStates { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();

    public List<RoomInvite> Invites { get; set; } = new();

    public QueuedRoomEvent? QueuedRoomEvent { get; set; }
}
