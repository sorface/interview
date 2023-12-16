using Ardalis.SmartEnum;

namespace Interview.Domain.Permissions
{
    public class SEPermission : SmartEnum<SEPermission>
    {
        public static readonly SEPermission Unknown = new(Guid.Parse("129319c5-2bff-46a6-8539-5fc6bf77983e"),
            "Unknown", "Unknown", EVPermission.Unknown);

        public static readonly SEPermission QuestionFindPage = new(Guid.Parse("189309c5-0bff-46a6-8539-5fc6bf77983e"),
            "QuestionFindPage", "Getting the questions page", EVPermission.QuestionFindPage);

        public static readonly SEPermission QuestionFindPageArchive =
            new(Guid.Parse("6f652e58-1229-4a3c-b8cb-328bf817ad54"), "QuestionFindPageArchive",
                "Getting the archived questions page", EVPermission.QuestionFindPageArchive);

        public static readonly SEPermission QuestionCreate = new(Guid.Parse("eac25c4b-28d5-4e22-93b2-5c3caf0f6922"),
            "QuestionCreate", "Create a new question", EVPermission.QuestionCreate);

        public static readonly SEPermission QuestionUpdate = new(Guid.Parse("175f724e-7299-4a0b-b827-0d4b0c6aed6b"),
            "QuestionUpdate", "Question update", EVPermission.QuestionUpdate);

        public static readonly SEPermission QuestionFindById = new(Guid.Parse("94d1bd4d-d3cd-47c0-a223-a4e80287314b"),
            "QuestionFindById", "Search for a question by ID", EVPermission.QuestionFindById);

        public static readonly SEPermission QuestionDeletePermanently =
            new(Guid.Parse("a2117904-59f1-4990-ae7f-d3d9d415f38e"), "QuestionDeletePermanently",
                "Permanently deleting a question", EVPermission.QuestionDeletePermanently);

        public static readonly SEPermission QuestionArchive = new(Guid.Parse("cebe2ad6-d9d5-4cfb-9530-925073e37ad5"),
            "QuestionArchive", "Archiving a question", EVPermission.QuestionArchive);

        public static readonly SEPermission QuestionUnarchive = new(Guid.Parse("32e18595-1c0a-4dd5-bad7-2cbfbccbcb2a"),
            "QuestionUnarchive", "Unarchiving the question", EVPermission.QuestionUnarchive);

        public static readonly SEPermission ReactionFindPage = new(Guid.Parse("004cca49-9857-4973-9bda-79b57f60279b"),
            "ReactionFindPage", "Getting the reactions page", EVPermission.ReactionFindPage);

        public static readonly SEPermission RoomParticipantFindByRoomIdAndUserId = new(
            Guid.Parse("4c3386da-cbb2-4493-86e8-036e8802782d"), "RoomParticipantFindByRoomIdAndUserId",
            "Getting a room member", EVPermission.RoomParticipantFindByRoomIdAndUserId);

        public static readonly SEPermission RoomParticipantChangeStatus =
            new(Guid.Parse("9ce5949f-a7b9-489c-8b04-bd6724aff687"), "RoomParticipantChangeStatus",
                "Changing a participant's status in a room", EVPermission.RoomParticipantChangeStatus);

        public static readonly SEPermission RoomParticipantCreate =
            new(Guid.Parse("d1916ab5-462e-41d7-ae46-f1ce27d514d4"), "RoomParticipantCreate",
                "Adding a new participant to a room", EVPermission.RoomParticipantCreate);

        public static readonly SEPermission RoomQuestionReactionCreate =
            new(Guid.Parse("1bb49aa7-1305-427c-9523-e9687392d385"), "RoomQuestionReactionCreate",
                "Adding a reaction to an active question in a room", EVPermission.RoomQuestionReactionCreate);

        public static readonly SEPermission RoomQuestionChangeActiveQuestion =
            new(Guid.Parse("4f7a0200-9fe1-4d04-9bcc-6ed668d07828"), "RoomQuestionChangeActiveQuestion",
                "Setting an active question in room", EVPermission.RoomQuestionChangeActiveQuestion);

        public static readonly SEPermission RoomQuestionCreate = new(Guid.Parse("a115f072-638a-4472-8cc3-4cf04da67cfc"),
            "RoomQuestionCreate", "Adding an existing question to a room", EVPermission.RoomQuestionCreate);

        public static readonly SEPermission RoomQuestionFindGuids =
            new(Guid.Parse("150f05e3-8d73-45e9-8ecd-6187f7b96461"), "RoomQuestionFindGuids",
                "Getting a list of question IDs in a room by room ID and question status",
                EVPermission.RoomQuestionFindGuids);

        public static readonly SEPermission RoomReviewFindPage = new(Guid.Parse("64f1a5ed-e22a-4574-8732-c1aa6525f010"),
            "RoomReviewFindPage", "Getting a list of reviews", EVPermission.RoomReviewFindPage);

        public static readonly SEPermission RoomReviewCreate = new(Guid.Parse("5f088b45-704f-4f61-b4c5-05bd08b80303"),
            "RoomReviewCreate", "Create a new review", EVPermission.RoomReviewCreate);

        public static readonly SEPermission RoomReviewUpdate = new(Guid.Parse("220380d1-fd72-4004-aed4-22187e88b386"),
            "RoomReviewUpdate", "Review update", EVPermission.RoomReviewUpdate);

        public static readonly SEPermission RoomCreate = new(Guid.Parse("c4c21128-f672-47d0-b0f5-2b3ca53fc420"),
            "RoomCreate", "Creating a new room", EVPermission.RoomCreate);

        public static readonly SEPermission RoomFindPage = new(Guid.Parse("aad7a083-b4dc-437e-a5db-c28512dedb5f"),
            "RoomFindPage", "Getting the rooms page", EVPermission.RoomFindPage);

        public static readonly SEPermission RoomFindById = new(Guid.Parse("6938365f-752d-453e-b0be-93facac0c5b8"),
            "RoomFindById", "Getting a room by ID", EVPermission.RoomFindById);

        public static readonly SEPermission RoomUpdate = new(Guid.Parse("b5c4eb71-50c8-4c13-a144-0496ce56e095"),
            "RoomUpdate", "Room update", EVPermission.RoomUpdate);

        public static readonly SEPermission RoomAddParticipant = new(Guid.Parse("7c4d9ac2-72e7-466a-bcff-68f3ee0bc65e"),
            "RoomAddParticipant", "Adding a new participant to a room", EVPermission.RoomAddParticipant);

        public static readonly SEPermission RoomSendEventRequest =
            new(Guid.Parse("882ffc55-3439-4d0b-8add-ba79e2a7df45"), "RoomSendEventRequest",
                "Sending an event to a room", EVPermission.RoomSendEventRequest);

        public static readonly SEPermission RoomClose = new(Guid.Parse("5ac11db0-b079-40ab-b32b-a02243a451b3"),
            "RoomClose", "Closing the room", EVPermission.RoomClose);

        public static readonly SEPermission RoomStartReview = new(Guid.Parse("7df4ea9b-ded5-4a1d-a8ea-e92e6bd85269"),
            "RoomStartReview", "Transferring a room to stage - reviews", EVPermission.RoomStartReview);

        public static readonly SEPermission RoomGetState = new(Guid.Parse("97b2411a-b9d4-49cb-9525-0e31b7d35496"),
            "RoomGetState", "Getting room status", EVPermission.RoomGetState);

        public static readonly SEPermission RoomGetAnalytics = new(Guid.Parse("a63b2ca5-304b-40a0-8e82-665a3327e407"),
            "RoomGetAnalytics", "Obtaining complete analytics based on the interview results",
            EVPermission.RoomGetAnalytics);

        public static readonly SEPermission RoomGetAnalyticsSummary =
            new(Guid.Parse("b7ad620a-0614-494a-89ca-623e47b7415a"), "RoomGetAnalyticsSummary",
                "Obtaining brief analytics based on the interview results", EVPermission.RoomGetAnalyticsSummary);

        public static readonly SEPermission UserFindPage = new(Guid.Parse("c65b3cd1-0532-4b3a-8b25-5128b4124aa0"),
            "UserFindPage", "Getting the users page", EVPermission.UserFindPage);

        public static readonly SEPermission UserFindByNickname = new(Guid.Parse("3f05ddc0-ef78-4916-b8b2-17fa11e95bb5"),
            "UserFindByNickname", "Getting a user by login", EVPermission.UserFindByNickname);

        public static readonly SEPermission UserFindById = new(Guid.Parse("6439dbeb-1b8e-49b3-99e4-2a95712a3958"),
            "UserFindById", "Getting a user by ID", EVPermission.UserFindById);

        public static readonly SEPermission UserUpsertByTwitchIdentity =
            new(Guid.Parse("1c876e71-24d2-4868-9385-23078c0b1c18"), "UserUpsertByTwitchIdentity",
                "Updating user data by twitch ID", EVPermission.UserUpsertByTwitchIdentity);

        public static readonly SEPermission UserFindByRole = new(Guid.Parse("0cb3a389-14b6-41a5-914b-3fd5cc876b28"),
            "UserFindByRole", "Getting a page of users by role", EVPermission.UserFindByRole);

        public static readonly SEPermission UserGetPermissions = new(Guid.Parse("946dff13-dfa5-424c-9891-6fcaa1e45ad1"),
            "UserGetPermissions", "Getting a list of user permissions by ID",
            EVPermission.UserGetPermissions);

        public static readonly SEPermission UserChangePermission =
            new(Guid.Parse("53af37b0-2c68-4775-9ddb-ab143ce92fec"), "UserChangePermission",
                "Granting permission to a user", EVPermission.UserChangePermission);

        public static readonly SEPermission TagFindPage =
            new(Guid.Parse("5c12dbf7-3cf9-40b2-9cab-203621129342"), "TagFindPage",
                "Getting a page of tags", EVPermission.TagFindPage);

        public static readonly SEPermission TagCreate =
            new(Guid.Parse("6eac768e-7345-42e0-80d3-b4d269d80e2e"), "TagCreate",
                "Creating a tag", EVPermission.TagCreate);

        public static readonly SEPermission TagUpdate =
            new(Guid.Parse("6f814f95-59f5-4591-acd5-545dfb981a31"), "TagUpdate",
                "Tag update", EVPermission.TagUpdate);

        public static readonly SEPermission AppEventPage =
            new(Guid.Parse("e71da275-6ee3-4def-a964-127f1aea9be6"), "AppEventPage",
                "Getting the Events Page", EVPermission.AppEventPage);

        public static readonly SEPermission AppEventById =
            new(Guid.Parse("08a4237e-4b59-4896-a7f1-d41c6810d1aa"), "AppEventById",
                "Retrieving an event by ID", EVPermission.AppEventById);

        public static readonly SEPermission AppEventByType =
            new(Guid.Parse("ecced60f-2e64-45a6-8a0d-9d2c67a18792"), "AppEventByType",
                "Retrieving an event by type", EVPermission.AppEventByType);

        public static readonly SEPermission AppEventCreate =
            new(Guid.Parse("7dd0b001-12f2-4d1b-8d2f-db800fbe54b2"), "AppEventCreate",
                "Create an event", EVPermission.AppEventCreate);

        public static readonly SEPermission AppEventUpdate =
            new(Guid.Parse("7cb3381b-4135-4b2f-ad8a-85992b3be582"), "AppEventUpdate",
                "Event Update", EVPermission.AppEventUpdate);

        public static readonly SEPermission UpsertRoomState =
            new(Guid.Parse("0827aeef-bcc1-4412-b584-0de4694422ce"), "UpsertRoomState",
                    "Update/Insert Room State", EVPermission.UpsertRoomState);

        public static readonly SEPermission DeleteRoomState =
                new(Guid.Parse("1f6c85db-c2a0-4096-8ead-a292397ab4e5"), "DeleteRoomState",
                    "Deleting a room state", EVPermission.DeleteRoomState);

        public static readonly SEPermission TranscriptionGet =
            new(Guid.Parse("9f020c9e-e0b4-4e6d-9fb3-38ba44cfa3f9"), "TranscriptionGet",
                "Getting a Room Transcription", EVPermission.TranscriptionGet);

        public Guid Id { get; }

        public string Description { get; }

        public SEPermission(Guid id, string name, string description, EVPermission value)
            : base(name, (int)value)
        {
            Id = id;
            Description = description;
        }
    }
}
