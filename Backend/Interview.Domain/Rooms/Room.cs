using System.Runtime.CompilerServices;
using Interview.Domain.Repository;
using Interview.Domain.RoomConfigurations;
using Interview.Domain.RoomParticipants;
using Interview.Domain.RoomQuestions;
using Interview.Domain.Tags;

[assembly: InternalsVisibleTo("Interview.Test")]

namespace Interview.Domain.Rooms;

public class Room : Entity
{
    public Room(string name, string twitchChannel, SeRoomAcсessType acсessType)
    {
        Name = name;
        TwitchChannel = twitchChannel;
        Status = SERoomStatus.New;
        AcсessType = acсessType;
    }

    private Room()
        : this(string.Empty, string.Empty, SeRoomAcсessType.Public)
    {
    }

    public string Name { get; internal set; }

    public string TwitchChannel { get; internal set; }

    public SeRoomAcсessType AcсessType { get; internal set; }

    public SERoomStatus Status { get; internal set; }

    public RoomConfiguration? Configuration { get; set; }

    public List<RoomQuestion> Questions { get; set; } = new();

    public List<RoomParticipant> Participants { get; set; } = new();

    public List<RoomState> RoomStates { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();

    public QueuedRoomEvent? QueuedRoomEvent { get; set; }
}
