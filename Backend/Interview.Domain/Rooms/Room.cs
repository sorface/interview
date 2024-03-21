using System.Runtime.CompilerServices;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Tags;

[assembly: InternalsVisibleTo("Interview.Test")]

namespace Interview.Domain.Rooms;

public class Room : Entity
{
    public Room(string name, string twitchChannel, SERoomAcсessType acсessType)
    {
        Name = name;
        TwitchChannel = twitchChannel;
        Status = SERoomStatus.New;
        AcсessType = acсessType;
    }

    private Room()
        : this(string.Empty, string.Empty, SERoomAcсessType.Public)
    {
    }

    public string Name { get; internal set; }

    public string TwitchChannel { get; internal set; }

    public SERoomAcсessType AcсessType { get; internal set; }

    public SERoomStatus Status { get; internal set; }

    public RoomConfiguration? Configuration { get; set; }

    public List<RoomQuestion> Questions { get; set; } = new();

    public List<RoomParticipant> Participants { get; set; } = new();

    public List<RoomState> RoomStates { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();

    public QueuedRoomEvent? QueuedRoomEvent { get; set; }
}
