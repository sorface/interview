using Ardalis.SmartEnum;
using Interview.Domain.Permissions;

namespace Interview.Domain.Rooms.RoomParticipants;

public sealed class SERoomParticipantType : SmartEnum<SERoomParticipantType>
{
    public static readonly SERoomParticipantType Viewer = new("Viewer", EVRoomParticipantType.Viewer, new HashSet<SEPermission>
    {
        SEPermission.RoomReviewUpdate,
        SEPermission.QuestionCreate,
        SEPermission.RoomFindById,
        SEPermission.RoomSendEventRequest,
        SEPermission.RoomGetState,
        SEPermission.TranscriptionGet,
        SEPermission.RoomParticipantFindByRoomIdAndUserId,
        SEPermission.RoomQuestionReactionCreate,
        SEPermission.RoomQuestionFindGuids,
        SEPermission.RoomReviewCreate,
        SEPermission.GetRoomQuestionAnswerDetails,
    });

    public static readonly SERoomParticipantType Expert = new("Expert", EVRoomParticipantType.Expert, new HashSet<SEPermission>
    {
        SEPermission.RoomReviewUpdate,
        SEPermission.QuestionCreate,
        SEPermission.RoomFindById,
        SEPermission.RoomUpdate,
        SEPermission.RoomAddParticipant,
        SEPermission.RoomSendEventRequest,
        SEPermission.RoomClose,
        SEPermission.RoomStartReview,
        SEPermission.RoomGetState,
        SEPermission.TranscriptionGet,
        SEPermission.RoomGetAnalyticsSummary,
        SEPermission.RoomGetAnalytics,
        SEPermission.DeleteRoomState,
        SEPermission.UpsertRoomState,
        SEPermission.RoomParticipantCreate,
        SEPermission.RoomParticipantChangeStatus,
        SEPermission.RoomParticipantFindByRoomIdAndUserId,
        SEPermission.RoomQuestionReactionCreate,
        SEPermission.RoomQuestionFindGuids,
        SEPermission.RoomQuestionCreate,
        SEPermission.RoomQuestionUpdate,
        SEPermission.RoomQuestionChangeActiveQuestion,
        SEPermission.RoomReviewCreate,
        SEPermission.RoomInviteGenerate,
        SEPermission.RoomQuestionEvaluationMerge,
        SEPermission.RoomQuestionEvaluationFind,
        SEPermission.RoomReviewCompletion,
        SEPermission.RoomReviewUpsert,
        SEPermission.GetRoomQuestionAnswerDetails,
        SEPermission.GetRoomQuestionClosedAnalytics,
    });

    public static readonly SERoomParticipantType Examinee = new("Examinee", EVRoomParticipantType.Examinee, new HashSet<SEPermission>
    {
        SEPermission.RoomReviewUpdate,
        SEPermission.QuestionCreate,
        SEPermission.RoomFindById,
        SEPermission.RoomSendEventRequest,
        SEPermission.RoomGetState,
        SEPermission.TranscriptionGet,
        SEPermission.RoomParticipantFindByRoomIdAndUserId,
        SEPermission.RoomQuestionReactionCreate,
        SEPermission.RoomQuestionFindGuids,
        SEPermission.RoomReviewCreate,
        SEPermission.GetRoomQuestionAnswerDetails,
    });

    private SERoomParticipantType(string name, EVRoomParticipantType value, IReadOnlySet<SEPermission> defaultRoomPermission)
        : base(name, (int)value)
    {
        DefaultRoomPermission = defaultRoomPermission;
    }

    public EVRoomParticipantType EnumValue => (EVRoomParticipantType)Value;

    public static SERoomParticipantType FromEnum(EVRoomParticipantType participantType) => FromValue((int)participantType);

    public IReadOnlySet<SEPermission> DefaultRoomPermission { get; }
}
