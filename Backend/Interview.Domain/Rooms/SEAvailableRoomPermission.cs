using Ardalis.SmartEnum;
using Interview.Domain.Permissions;

namespace Interview.Domain.Rooms;

/// <summary>
/// AvailableRoomPermission Smart enum.
/// </summary>
public class SEAvailableRoomPermission : SmartEnum<SEAvailableRoomPermission, Guid>
{
    public static readonly SEAvailableRoomPermission RoomReviewUpdate = new(SEPermission.RoomReviewUpdate, new Guid("95D476A0-EB0E-470D-9C57-A0EC8A2E4CD6"));

    public static readonly SEAvailableRoomPermission QuestionCreate = new(SEPermission.QuestionCreate, new Guid("38CD9540-27F5-4482-A261-2A08F6D8CF30"));

    public static readonly SEAvailableRoomPermission RoomFindById = new(SEPermission.RoomFindById, new Guid("C68385EE-093A-457F-A03A-B1A53371C248"));

    public static readonly SEAvailableRoomPermission RoomUpdate = new(SEPermission.RoomUpdate, new Guid("AA3F81EC-9A87-493F-A7D5-FA4CA6E75BF7"));

    public static readonly SEAvailableRoomPermission RoomAddParticipant = new(SEPermission.RoomAddParticipant, new Guid("D40A2C28-3A84-47F3-9981-88BDF50BB4CA"));

    public static readonly SEAvailableRoomPermission RoomSendEventRequest = new(SEPermission.RoomSendEventRequest, new Guid("48EB3B31-6632-4B4D-B36D-F61C68865C9D"));

    public static readonly SEAvailableRoomPermission RoomClose = new(SEPermission.RoomClose, new Guid("AD9B444A-67B7-4B85-B592-8578E569B12A"));

    public static readonly SEAvailableRoomPermission RoomStartReview = new(SEPermission.RoomStartReview, new Guid("9ACECC78-79CA-41B1-960E-A4EB9CF03A2C"));

    public static readonly SEAvailableRoomPermission RoomGetState = new(SEPermission.RoomGetState, new Guid("3B1A04F3-8D35-4608-87FB-1D83D76CD99D"));

    public static readonly SEAvailableRoomPermission TranscriptionGet = new(SEPermission.TranscriptionGet, new Guid("8D3C4087-B34D-48F7-BA2A-B1A85F69FE95"));

    public static readonly SEAvailableRoomPermission RoomGetAnalyticsSummary = new(SEPermission.RoomGetAnalyticsSummary, new Guid("6B3985BF-05DD-47E7-B894-781E28428596"));

    public static readonly SEAvailableRoomPermission RoomGetAnalytics = new(SEPermission.RoomGetAnalytics, new Guid("6CF93811-C44A-4B86-86A1-18D72DF7E1A0"));

    public static readonly SEAvailableRoomPermission DeleteRoomState = new(SEPermission.DeleteRoomState, new Guid("4DC0B8E6-4C1D-46E9-B181-5D2A31E7BDB5"));

    public static readonly SEAvailableRoomPermission UpsertRoomState = new(SEPermission.UpsertRoomState, new Guid("209A47F7-F1C5-439C-8DE5-7792C08B7CE2"));

    public static readonly SEAvailableRoomPermission RoomParticipantCreate = new(SEPermission.RoomParticipantCreate, new Guid("556D9330-9FF3-46A9-913B-28543FD213E4"));

    public static readonly SEAvailableRoomPermission RoomParticipantChangeStatus = new(SEPermission.RoomParticipantChangeStatus, new Guid("B9AD0F66-08C6-4F95-900C-94750F1ADA6B"));

    public static readonly SEAvailableRoomPermission RoomParticipantFindByRoomIdAndUserId = new(SEPermission.RoomParticipantFindByRoomIdAndUserId, new Guid("A1ACBADE-3835-4A9E-9729-56067AF66D53"));

    public static readonly SEAvailableRoomPermission RoomQuestionReactionCreate = new(SEPermission.RoomQuestionReactionCreate, new Guid("5EFEACE0-78CA-4616-AEE0-9F08574132CE"));

    public static readonly SEAvailableRoomPermission RoomQuestionFindGuids = new(SEPermission.RoomQuestionFindGuids, new Guid("369F0B92-915C-4334-BDAC-6E82FB3C0C74"));

    public static readonly SEAvailableRoomPermission RoomQuestionCreate = new(SEPermission.RoomQuestionCreate, new Guid("4157604C-FDE9-45CF-B79E-09B7FDE71833"));

    public static readonly SEAvailableRoomPermission RoomQuestionChangeActiveQuestion = new(SEPermission.RoomQuestionChangeActiveQuestion, new Guid("241F76F2-3746-4EE4-9191-A64BA3B3A86E"));

    public static readonly SEAvailableRoomPermission RoomReviewCreate = new(SEPermission.RoomReviewCreate, new Guid("BD3496E3-6E57-447E-A7DF-744EFFF03DE5"));

    public static readonly SEAvailableRoomPermission RoomInviteGet = new(SEPermission.RoomInviteGet, new Guid("07DEA11A-65A4-4826-AB4F-9D2CDFAA72F3"));

    public static readonly SEAvailableRoomPermission RoomInviteGenerate = new(SEPermission.RoomInviteGenerate, new Guid("95F1F088-6931-4914-92C1-C1F1D7D75A18"));
    public static readonly SEAvailableRoomPermission RoomQuestionUpdate = new(SEPermission.RoomQuestionUpdate, new Guid("BE526DEE-9C74-44BB-AF6B-1B2298FA1197"));

    public static readonly SEAvailableRoomPermission RoomQuestionEvaluationMerge = new(SEPermission.RoomQuestionEvaluationMerge, new Guid("0c24de0f-6fe3-4d95-81fb-e9e7542852f7"));

    public static readonly SEAvailableRoomPermission RoomQuestionEvaluationFind = new(SEPermission.RoomQuestionEvaluationFind, new Guid("49cb7cd3-7329-4098-9ac1-c3972ba09138"));

    public SEAvailableRoomPermission(SEPermission name, Guid value)
        : base(name.Name, value)
    {
        Permission = name;
    }

    public SEPermission Permission { get; }
}
