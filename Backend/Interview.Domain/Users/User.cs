using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Users;

public class User : Entity
{
    public User(Guid id, string nickname, string externalId)
        : base(id)
    {
        Nickname = nickname;
        ExternalId = externalId;
    }

    public User(string nickname, string externalId)
        : this(Guid.Empty, nickname, externalId)
    {
    }

    private User()
        : this(string.Empty, string.Empty)
    {
    }

    public string Nickname { get; internal set; }

    public string? Avatar { get; set; }

    public string ExternalId { get; private set; }

    public List<Role> Roles { get; private set; } = new List<Role>();

    public List<Permission> Permissions { get; private set; } = new List<Permission>();

    public List<RoomQuestionEvaluation> RoomQuestionEvaluations { get; private set; } = new List<RoomQuestionEvaluation>();
}
