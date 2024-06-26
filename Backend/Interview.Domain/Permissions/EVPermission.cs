using System.ComponentModel;

namespace Interview.Domain.Permissions;

public enum EVPermission
{
#pragma warning disable SA1602
    Unknown,

    [Description("Getting the questions page")]
    QuestionFindPage,

    [Description("Getting the archived questions page")]
    QuestionFindPageArchive,

    [Description("Create a new question")]
    QuestionCreate,

    [Description("Question update")]
    QuestionUpdate,

    [Description("Search for a question by ID")]

    QuestionFindById,

    [Description("Permanently deleting a question")]
    QuestionDeletePermanently,

    [Description("Archiving a question")]
    QuestionArchive,

    [Description("Unarchiving the question")]
    QuestionUnarchive,

    [Description("Getting the reactions page")]
    ReactionFindPage,

    [Description("Getting a room member")]
    RoomParticipantFindByRoomIdAndUserId,

    [Description("Changing a participant's status in a room")]
    RoomParticipantChangeStatus,

    [Description("Adding a new participant to a room")]
    RoomParticipantCreate,

    [Description("Adding a reaction to an active question in a room")]
    RoomQuestionReactionCreate,

    [Description("Setting an active question in a room")]
    RoomQuestionChangeActiveQuestion,

    [Description("Adding an existing question to a room")]
    RoomQuestionCreate,

    [Description("Getting a list of question IDs in a room by room ID and question status")]
    RoomQuestionFindGuids,

    [Description("Getting a list of reviews")]
    RoomReviewFindPage,

    [Description("Create a new review")]
    RoomReviewCreate,

    [Description("Review update")]
    RoomReviewUpdate,

    [Description("Creating a new room")]
    RoomCreate,

    [Description("Getting the rooms page")]
    RoomFindPage,

    [Description("Getting a room by ID")]
    RoomFindById,

    [Description("Room update")]
    RoomUpdate,

    [Description("Adding a new participant to a room")]
    RoomAddParticipant,

    [Description("Sending an event to a room")]
    RoomSendEventRequest,

    [Description("Closing the room")]
    RoomClose,

    [Description("Transferring a room to stage - reviews")]
    RoomStartReview,

    [Description("Getting room status")]
    RoomGetState,

    [Description("Obtaining complete analytics based on the interview results")]
    RoomGetAnalytics,

    [Description("Obtaining brief analytics based on the interview results")]
    RoomGetAnalyticsSummary,

    [Description("Getting the users page")]
    UserFindPage,

    [Description("Getting a user by login")]
    UserFindByNickname,

    [Description("Getting a user by ID")]
    UserFindById,

    [Description("Updating user data by twitch ID")]
    UserUpsertByTwitchIdentity,

    [Description("Getting a page of users by role")]
    UserFindByRole,

    [Description("Getting a list of user permissions by ID")]
    UserGetPermissions,

    [Description("Granting permission to a user")]
    UserChangePermission,

    [Description("Getting a page of tags")]
    TagFindPage,

    [Description("Creating a tag")]
    TagCreate,

    [Description("Tag update")]
    TagUpdate,

    [Description("Getting the Events Page")]
    AppEventPage,

    [Description("Retrieving an event by ID")]
    AppEventById,

    [Description("Retrieving an event by type")]
    AppEventByType,

    [Description("Create an event")]
    AppEventCreate,

    [Description("Event Update")]
    AppEventUpdate,

    [Description("Update/Insert Room State")]
    UpsertRoomState,

    [Description("Deleting a room state")]
    DeleteRoomState,

    [Description("Getting a Room Transcription")]
    TranscriptionGet,

    [Description("Getting a Room Invites")]
    RoomInviteGet,

    [Description("Generate a Room Invites")]
    RoomInviteGenerate,

    [Description("Creating a new public room")]
    PublicRoomCreate,

    [Description("Create/Update category")]
    EditCategory,

    [Description("Find category page")]
    FindCategoryPage,

    [Description("Find archived category page")]
    FindCategoryPageArchive,

    [Description("Archiving a category")]
    CategoryArchive,

    [Description("Unarchiving a category")]
    CategoryUnarchive,

    [Description("Get category by id")]
    GetCategoryById,

    [Description("Merge evaluation")]
    RoomQuestionEvaluationMerge,

    [Description("Find evaluation")]
    RoomQuestionEvaluationFind,
#pragma warning restore SA1602
}
