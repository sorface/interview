using System.Collections.Immutable;
using Ardalis.SmartEnum;
using Interview.Domain.Permissions;

namespace Interview.Domain.Users.Roles;

public enum RoleNameType
{
    /// <summary>
    /// User.
    /// </summary>
    User = 1,

    /// <summary>
    /// Admin.
    /// </summary>
    Admin,
}

public sealed class RoleName : SmartEnum<RoleName>
{
    public static readonly RoleName Unknown = new(
        Guid.Empty,
        "Unknown",
        0,
        ImmutableHashSet<SEPermission>.Empty);

    public static readonly RoleName User = new(
        Guid.Parse("ab45a82b-aa1c-11ed-abe8-f2b335a02ee9"),
        RoleNameConstants.User,
        RoleNameType.User,
        new HashSet<SEPermission>
        {
            SEPermission.QuestionFindPage,
            SEPermission.QuestionFindPageArchive,
            SEPermission.RoomQuestionReactionCreate,
            SEPermission.RoomFindPage,
            SEPermission.RoomFindById,
            SEPermission.RoomSendEventRequest,
            SEPermission.RoomGetState,
            SEPermission.RoomGetAnalyticsSummary,
            SEPermission.ReactionFindPage,
            SEPermission.RoomParticipantFindByRoomIdAndUserId,
            SEPermission.RoomQuestionFindGuids,
            SEPermission.RoomAddParticipant,
            SEPermission.QuestionFindById,
            SEPermission.AppEventPage,
            SEPermission.TranscriptionGet,
            SEPermission.RoomCreate,
            SEPermission.TagFindPage,
            SEPermission.QuestionCreate,
            SEPermission.FindCategoryPage,
            SEPermission.GetCategoryById,
        });

    public static readonly RoleName Admin = new(
        Guid.Parse("ab45cf57-aa1c-11ed-970f-98dc442de35a"),
        RoleNameConstants.Admin,
        RoleNameType.Admin,
        ImmutableHashSet<SEPermission>.Empty);

    private RoleName(Guid id, string name, RoleNameType value, IReadOnlySet<SEPermission> defaultPermissions)
        : base(name, (int)value)
    {
        Id = id;
        DefaultPermissions = defaultPermissions;
    }

    public Guid Id { get; }

    public IReadOnlySet<SEPermission> DefaultPermissions { get; }

    public RoleNameType EnumValue => (RoleNameType)Value;
}
