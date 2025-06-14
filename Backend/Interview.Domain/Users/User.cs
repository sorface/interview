using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Users;

public class User(Guid id, string nickname, string externalId) : Entity(id)
{
    public static readonly User Backend = new(Guid.Parse("C560B516-238C-45DD-B394-352757FC6380"), "Backend", string.Empty);

    public User(string nickname, string externalId)
        : this(Guid.Empty, nickname, externalId)
    {
    }

    private User()
        : this(string.Empty, string.Empty)
    {
    }

    public string Nickname { get; internal set; } = nickname;

    public string? Avatar { get; set; }

    public string ExternalId { get; private set; } = externalId;

    public List<Role> Roles { get; private set; } = [];

    public List<Permission> Permissions { get; private set; } = [];

    public List<RoomQuestionEvaluation> RoomQuestionEvaluations { get; private set; } = [];

    public List<RoomParticipant> RoomParticipants { get; private set; } = [];
}
