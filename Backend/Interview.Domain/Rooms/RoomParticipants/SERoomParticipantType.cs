using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms.RoomParticipants;

public sealed class SERoomParticipantType : SmartEnum<SERoomParticipantType>
{
    public static readonly SERoomParticipantType Viewer = new("Viewer", EVRoomParticipantType.Viewer, new HashSet<SEAvailableRoomPermission>
    {
        SEAvailableRoomPermission.RoomReviewUpdate,
        SEAvailableRoomPermission.QuestionCreate,
        SEAvailableRoomPermission.RoomFindById,
        SEAvailableRoomPermission.RoomSendEventRequest,
        SEAvailableRoomPermission.RoomGetState,
        SEAvailableRoomPermission.TranscriptionGet,
        SEAvailableRoomPermission.RoomGetAnalyticsSummary,
        SEAvailableRoomPermission.RoomGetAnalytics,
        SEAvailableRoomPermission.RoomParticipantFindByRoomIdAndUserId,
        SEAvailableRoomPermission.RoomQuestionReactionCreate,
        SEAvailableRoomPermission.RoomQuestionFindGuids,
        SEAvailableRoomPermission.RoomReviewCreate,
        SEAvailableRoomPermission.GetRoomQuestionAnswerDetails,
    });

    public static readonly SERoomParticipantType Expert = new("Expert", EVRoomParticipantType.Expert, new HashSet<SEAvailableRoomPermission>
    {
        SEAvailableRoomPermission.RoomReviewUpdate,
        SEAvailableRoomPermission.QuestionCreate,
        SEAvailableRoomPermission.RoomFindById,
        SEAvailableRoomPermission.RoomUpdate,
        SEAvailableRoomPermission.RoomAddParticipant,
        SEAvailableRoomPermission.RoomSendEventRequest,
        SEAvailableRoomPermission.RoomClose,
        SEAvailableRoomPermission.RoomStartReview,
        SEAvailableRoomPermission.RoomGetState,
        SEAvailableRoomPermission.TranscriptionGet,
        SEAvailableRoomPermission.RoomGetAnalyticsSummary,
        SEAvailableRoomPermission.RoomGetAnalytics,
        SEAvailableRoomPermission.DeleteRoomState,
        SEAvailableRoomPermission.UpsertRoomState,
        SEAvailableRoomPermission.RoomParticipantCreate,
        SEAvailableRoomPermission.RoomParticipantChangeStatus,
        SEAvailableRoomPermission.RoomParticipantFindByRoomIdAndUserId,
        SEAvailableRoomPermission.RoomQuestionReactionCreate,
        SEAvailableRoomPermission.RoomQuestionFindGuids,
        SEAvailableRoomPermission.RoomQuestionCreate,
        SEAvailableRoomPermission.RoomQuestionUpdate,
        SEAvailableRoomPermission.RoomQuestionChangeActiveQuestion,
        SEAvailableRoomPermission.RoomReviewCreate,
        SEAvailableRoomPermission.RoomInviteGenerate,
        SEAvailableRoomPermission.RoomQuestionEvaluationMerge,
        SEAvailableRoomPermission.RoomQuestionEvaluationFind,
        SEAvailableRoomPermission.RoomReviewCompletion,
        SEAvailableRoomPermission.RoomReviewUpsert,
        SEAvailableRoomPermission.GetRoomQuestionAnswerDetails,
    });

    public static readonly SERoomParticipantType Examinee = new("Examinee", EVRoomParticipantType.Examinee, new HashSet<SEAvailableRoomPermission>
    {
        SEAvailableRoomPermission.RoomReviewUpdate,
        SEAvailableRoomPermission.QuestionCreate,
        SEAvailableRoomPermission.RoomFindById,
        SEAvailableRoomPermission.RoomSendEventRequest,
        SEAvailableRoomPermission.RoomGetState,
        SEAvailableRoomPermission.TranscriptionGet,
        SEAvailableRoomPermission.RoomGetAnalyticsSummary,
        SEAvailableRoomPermission.RoomGetAnalytics,
        SEAvailableRoomPermission.RoomParticipantFindByRoomIdAndUserId,
        SEAvailableRoomPermission.RoomQuestionReactionCreate,
        SEAvailableRoomPermission.RoomQuestionFindGuids,
        SEAvailableRoomPermission.RoomReviewCreate,
        SEAvailableRoomPermission.GetRoomQuestionAnswerDetails,
    });

    private SERoomParticipantType(string name, EVRoomParticipantType value, IReadOnlySet<SEAvailableRoomPermission> defaultRoomPermission)
        : base(name, (int)value)
    {
        DefaultRoomPermission = defaultRoomPermission;
    }

    public EVRoomParticipantType EnumValue => (EVRoomParticipantType)Value;

    public static SERoomParticipantType FromEnum(EVRoomParticipantType participantType) => List.First(it => it.Value.Equals((int)participantType));

    public IReadOnlySet<SEAvailableRoomPermission> DefaultRoomPermission { get; }
}
