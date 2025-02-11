using System.Runtime.CompilerServices;
using Interview.Domain.Categories;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;

[assembly: InternalsVisibleTo("Interview.Test")]

namespace Interview.Domain.Rooms;

public class Room(string name, SERoomAccessType accessType, SERoomType type) : Entity
{
    private Room()
        : this(string.Empty, SERoomAccessType.Public, SERoomType.Standard)
    {
    }

    public string Name { get; internal set; } = name;

    public DateTime ScheduleStartTime { get; internal set; }

    public SERoomAccessType AccessType { get; internal set; } = accessType;

    public SERoomStatus Status { get; internal set; } = SERoomStatus.New;

    public RoomConfiguration? Configuration { get; set; }

    public RoomTimer? Timer { get; set; }

    public List<RoomQuestion> Questions { get; set; } = [];

    public List<RoomParticipant> Participants { get; set; } = [];

    public List<RoomState> RoomStates { get; set; } = [];

    public List<Tag> Tags { get; set; } = [];

    public List<RoomInvite> Invites { get; set; } = [];

    public QueuedRoomEvent? QueuedRoomEvent { get; set; }

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }

    public SERoomType Type { get; set; } = type;
}
