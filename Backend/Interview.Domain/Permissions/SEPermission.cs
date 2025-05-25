using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;
using Ardalis.SmartEnum;

namespace Interview.Domain.Permissions;

public class SEPermission(Guid id, EVPermission value) : SmartEnum<SEPermission>(value.ToString(), (int)value)
{
    private static readonly FrozenDictionary<EVPermission, string> _descriptionMap = BuildDescriptionMap();

    public static readonly SEPermission Unknown = new(
        Guid.Parse("129319c5-2bff-46a6-8539-5fc6bf77983e"),
        EVPermission.Unknown);

    public static readonly SEPermission QuestionFindPage = new(
        Guid.Parse("189309c5-0bff-46a6-8539-5fc6bf77983e"),
        EVPermission.QuestionFindPage);

    public static readonly SEPermission QuestionFindPageArchive =
        new(
            Guid.Parse("6f652e58-1229-4a3c-b8cb-328bf817ad54"),
            EVPermission.QuestionFindPageArchive);

    public static readonly SEPermission QuestionCreate = new(
        Guid.Parse("eac25c4b-28d5-4e22-93b2-5c3caf0f6922"),
        EVPermission.QuestionCreate);

    public static readonly SEPermission QuestionUpdate = new(
        Guid.Parse("175f724e-7299-4a0b-b827-0d4b0c6aed6b"),
        EVPermission.QuestionUpdate);

    public static readonly SEPermission QuestionFindById = new(
        Guid.Parse("94d1bd4d-d3cd-47c0-a223-a4e80287314b"),
        EVPermission.QuestionFindById);

    public static readonly SEPermission QuestionDeletePermanently =
        new(
            Guid.Parse("a2117904-59f1-4990-ae7f-d3d9d415f38e"),
            EVPermission.QuestionDeletePermanently);

    public static readonly SEPermission QuestionArchive = new(
        Guid.Parse("cebe2ad6-d9d5-4cfb-9530-925073e37ad5"),
        EVPermission.QuestionArchive);

    public static readonly SEPermission QuestionUnarchive = new(
        Guid.Parse("32e18595-1c0a-4dd5-bad7-2cbfbccbcb2a"),
        EVPermission.QuestionUnarchive);

    public static readonly SEPermission ReactionFindPage = new(
        Guid.Parse("004cca49-9857-4973-9bda-79b57f60279b"),
        EVPermission.ReactionFindPage);

    public static readonly SEPermission RoomParticipantFindByRoomIdAndUserId = new(
        Guid.Parse("4c3386da-cbb2-4493-86e8-036e8802782d"),
        EVPermission.RoomParticipantFindByRoomIdAndUserId);

    public static readonly SEPermission RoomParticipantChangeStatus =
        new(
            Guid.Parse("9ce5949f-a7b9-489c-8b04-bd6724aff687"),
            EVPermission.RoomParticipantChangeStatus);

    public static readonly SEPermission RoomParticipantCreate =
        new(
            Guid.Parse("d1916ab5-462e-41d7-ae46-f1ce27d514d4"),
            EVPermission.RoomParticipantCreate);

    public static readonly SEPermission RoomQuestionReactionCreate =
        new(
            Guid.Parse("1bb49aa7-1305-427c-9523-e9687392d385"),
            EVPermission.RoomQuestionReactionCreate);

    public static readonly SEPermission RoomQuestionChangeActiveQuestion =
        new(
            Guid.Parse("4f7a0200-9fe1-4d04-9bcc-6ed668d07828"),
            EVPermission.RoomQuestionChangeActiveQuestion);

    public static readonly SEPermission RoomQuestionCreate = new(
        Guid.Parse("a115f072-638a-4472-8cc3-4cf04da67cfc"),
        EVPermission.RoomQuestionCreate);

    public static readonly SEPermission RoomQuestionFindGuids =
        new(
            Guid.Parse("150f05e3-8d73-45e9-8ecd-6187f7b96461"),
            EVPermission.RoomQuestionFindGuids);

    public static readonly SEPermission RoomReviewFindPage = new(
        Guid.Parse("64f1a5ed-e22a-4574-8732-c1aa6525f010"),
        EVPermission.RoomReviewFindPage);

    public static readonly SEPermission RoomReviewCreate = new(
        Guid.Parse("5f088b45-704f-4f61-b4c5-05bd08b80303"),
        EVPermission.RoomReviewCreate);

    public static readonly SEPermission RoomReviewUpdate = new(
        Guid.Parse("220380d1-fd72-4004-aed4-22187e88b386"),
        EVPermission.RoomReviewUpdate);

    public static readonly SEPermission RoomReviewUpsert = new(
        Guid.Parse("695914fe-a627-4959-b8b9-e0413ba63755"),
        EVPermission.RoomReviewUpsert);

    public static readonly SEPermission RoomReviewCompletion = new(
        Guid.Parse("2a6f981e-f79e-4497-83d0-35018cbd24d3"),
        EVPermission.RoomReviewCompletion);

    public static readonly SEPermission RoomCreate = new(
        Guid.Parse("c4c21128-f672-47d0-b0f5-2b3ca53fc420"),
        EVPermission.RoomCreate);

    public static readonly SEPermission RoomFindPage = new(
        Guid.Parse("aad7a083-b4dc-437e-a5db-c28512dedb5f"),
        EVPermission.RoomFindPage);

    public static readonly SEPermission RoomFindById = new(
        Guid.Parse("6938365f-752d-453e-b0be-93facac0c5b8"),
        EVPermission.RoomFindById);

    public static readonly SEPermission RoomUpdate = new(
        Guid.Parse("b5c4eb71-50c8-4c13-a144-0496ce56e095"),
        EVPermission.RoomUpdate);

    public static readonly SEPermission RoomAddParticipant = new(
        Guid.Parse("7c4d9ac2-72e7-466a-bcff-68f3ee0bc65e"),
        EVPermission.RoomAddParticipant);

    public static readonly SEPermission RoomSendEventRequest =
        new(
            Guid.Parse("882ffc55-3439-4d0b-8add-ba79e2a7df45"),
            EVPermission.RoomSendEventRequest);

    public static readonly SEPermission RoomClose = new(
        Guid.Parse("5ac11db0-b079-40ab-b32b-a02243a451b3"),
        EVPermission.RoomClose);

    public static readonly SEPermission RoomStartReview = new(
        Guid.Parse("7df4ea9b-ded5-4a1d-a8ea-e92e6bd85269"),
        EVPermission.RoomStartReview);

    public static readonly SEPermission RoomGetState = new(
        Guid.Parse("97b2411a-b9d4-49cb-9525-0e31b7d35496"),
        EVPermission.RoomGetState);

    public static readonly SEPermission RoomGetAnalytics = new(
        Guid.Parse("a63b2ca5-304b-40a0-8e82-665a3327e407"),
        EVPermission.RoomGetAnalytics);

    public static readonly SEPermission RoomGetAnalyticsSummary =
        new(
            Guid.Parse("b7ad620a-0614-494a-89ca-623e47b7415a"),
            EVPermission.RoomGetAnalyticsSummary);

    public static readonly SEPermission UserFindPage = new(
        Guid.Parse("c65b3cd1-0532-4b3a-8b25-5128b4124aa0"),
        EVPermission.UserFindPage);

    public static readonly SEPermission UserFindByNickname = new(
        Guid.Parse("3f05ddc0-ef78-4916-b8b2-17fa11e95bb5"),
        EVPermission.UserFindByNickname);

    public static readonly SEPermission UserFindById = new(
        Guid.Parse("6439dbeb-1b8e-49b3-99e4-2a95712a3958"),
        EVPermission.UserFindById);

    public static readonly SEPermission UserUpsertByTwitchIdentity =
        new(
            Guid.Parse("1c876e71-24d2-4868-9385-23078c0b1c18"),
            EVPermission.UserUpsertByTwitchIdentity);

    public static readonly SEPermission UserFindByRole = new(
        Guid.Parse("0cb3a389-14b6-41a5-914b-3fd5cc876b28"),
        EVPermission.UserFindByRole);

    public static readonly SEPermission UserGetPermissions = new(
        Guid.Parse("946dff13-dfa5-424c-9891-6fcaa1e45ad1"),
        EVPermission.UserGetPermissions);

    public static readonly SEPermission UserChangePermission =
        new(
            Guid.Parse("53af37b0-2c68-4775-9ddb-ab143ce92fec"),
            EVPermission.UserChangePermission);

    public static readonly SEPermission TagFindPage =
        new(
            Guid.Parse("5c12dbf7-3cf9-40b2-9cab-203621129342"),
            EVPermission.TagFindPage);

    public static readonly SEPermission TagCreate =
        new(
            Guid.Parse("6eac768e-7345-42e0-80d3-b4d269d80e2e"),
            EVPermission.TagCreate);

    public static readonly SEPermission TagUpdate =
        new(
            Guid.Parse("6f814f95-59f5-4591-acd5-545dfb981a31"),
            EVPermission.TagUpdate);

    public static readonly SEPermission AppEventPage =
        new(
            Guid.Parse("e71da275-6ee3-4def-a964-127f1aea9be6"),
            EVPermission.AppEventPage);

    public static readonly SEPermission AppEventById =
        new(
            Guid.Parse("08a4237e-4b59-4896-a7f1-d41c6810d1aa"),
            EVPermission.AppEventById);

    public static readonly SEPermission AppEventByType =
        new(
            Guid.Parse("ecced60f-2e64-45a6-8a0d-9d2c67a18792"),
            EVPermission.AppEventByType);

    public static readonly SEPermission AppEventCreate =
        new(
            Guid.Parse("7dd0b001-12f2-4d1b-8d2f-db800fbe54b2"),
            EVPermission.AppEventCreate);

    public static readonly SEPermission AppEventUpdate =
        new(
            Guid.Parse("7cb3381b-4135-4b2f-ad8a-85992b3be582"),
            EVPermission.AppEventUpdate);

    public static readonly SEPermission UpsertRoomState =
        new(
            Guid.Parse("0827aeef-bcc1-4412-b584-0de4694422ce"),
            EVPermission.UpsertRoomState);

    public static readonly SEPermission DeleteRoomState =
        new(
            Guid.Parse("1f6c85db-c2a0-4096-8ead-a292397ab4e5"),
            EVPermission.DeleteRoomState);

    public static readonly SEPermission TranscriptionGet =
        new(
            Guid.Parse("9f020c9e-e0b4-4e6d-9fb3-38ba44cfa3f9"),
            EVPermission.TranscriptionGet);

    public static readonly SEPermission RoomInviteGet =
        new(
            Guid.Parse("B530321A-A51A-4A36-8AFD-6E8A8DBAE248"),
            EVPermission.RoomInviteGet);

    public static readonly SEPermission RoomInviteGenerate =
        new(
            Guid.Parse("C1F43CA8-21F1-41E6-9794-E7D44156BF73"),
            EVPermission.RoomInviteGenerate);

    public static readonly SEPermission PublicRoomCreate = new(
        Guid.Parse("FCC9BBCA-15C6-4221-8D2D-E052B8CD4385"),
        EVPermission.PublicRoomCreate);

    public static readonly SEPermission EditCategory = new(
        Guid.Parse("1B2DD31B-B35E-48E2-8F33-D0366B9D60BA"),
        EVPermission.EditCategory);

    public static readonly SEPermission FindCategoryPage = new(
        Guid.Parse("9001520D-B1D2-4ADE-8F70-570D2B7EFEA1"),
        EVPermission.FindCategoryPage);

    public static readonly SEPermission FindCategoryPageArchive = new(
        Guid.Parse("B4DCA27C-5733-4B37-BB63-7ECA6F8E831B"),
        EVPermission.FindCategoryPageArchive);

    public static readonly SEPermission CategoryArchive = new(
        Guid.Parse("C0AFEC8D-04D0-4A7A-9F20-C3D4C891F04E"),
        EVPermission.CategoryArchive);

    public static readonly SEPermission CategoryUnarchive = new(
        Guid.Parse("84DC5BCE-FA74-47CB-949A-042DA1126C0C"),
        EVPermission.CategoryUnarchive);

    public static readonly SEPermission GetCategoryById = new(
        Guid.Parse("BC98D0B8-B4A3-4B66-B8C1-DB1FCA0647E0"),
        EVPermission.GetCategoryById);

    public static readonly SEPermission RoomQuestionEvaluationMerge = new(
        Guid.Parse("d74df965-84d3-4bcc-af1b-13f5c6299fa7"),
        EVPermission.RoomQuestionEvaluationMerge);

    public static readonly SEPermission RoomQuestionEvaluationFind = new(
        Guid.Parse("7b231e25-446a-418c-9281-4eb453dd4893"),
        EVPermission.RoomQuestionEvaluationFind);

    public static readonly SEPermission RoomQuestionUpdate = new(
        Guid.Parse("4F39059A-E69F-4494-9B48-54E3A6AEA2F3"),
        EVPermission.RoomQuestionUpdate);

    public static readonly SEPermission GetRoomQuestionAnswerDetails = new(
        Guid.Parse("EDAB0E5D-7AC2-4761-B47F-A5F41A9AE48C"),
        EVPermission.GetRoomQuestionAnswerDetails);

    public static readonly SEPermission GetRoomCalendar = new(
        Guid.Parse("06D64B89-090F-4FD0-B81D-20268EE91CEA"),
        EVPermission.GetRoomCalendar);

    public static readonly SEPermission QuestionTreeFindPage = new(
        Guid.Parse("2E1670EC-F9D2-4CB8-BB57-F6239B16F24F"),
        EVPermission.QuestionTreeFindPage);

    public static readonly SEPermission QuestionTreeFindArchivedPage = new(
        Guid.Parse("BADFC74C-4F2F-4B53-9B46-763B28676009"),
        EVPermission.QuestionTreeFindArchivedPage);

    public static readonly SEPermission GetQuestionTreeById = new(
        Guid.Parse("8F12692D-8E94-4409-AAEF-84F2CEAACD5D"),
        EVPermission.GetQuestionTreeById);

    public static readonly SEPermission GetArchiveQuestionTreeById = new(
        Guid.Parse("1119039F-8127-43DC-AC11-247FC4D221D3"),
        EVPermission.GetArchiveQuestionTreeById);

    public static readonly SEPermission UpsertQuestionTree = new(
        Guid.Parse("03B8BDCA-DDA6-4063-915B-31BF0A2DBA74"),
        EVPermission.UpsertQuestionTree);

    public static readonly SEPermission QuestionTreeArchive = new(
        Guid.Parse("E6B59BAE-2F2C-40F3-8587-49A3E703AC44"),
        EVPermission.QuestionTreeArchive);

    public static readonly SEPermission QuestionTreeUnarchive = new(
        Guid.Parse("1EE1E860-A1DC-4DEF-A191-1CBAC8956A90"),
        EVPermission.QuestionTreeUnarchive);

    public static readonly SEPermission RoadmapUpsert = new(
        Guid.Parse("91EEED61-9E52-4486-9881-DE48991732E5"),
        EVPermission.RoadmapUpsert);

    public static readonly SEPermission RoadmapGetById = new(
        Guid.Parse("53F985B6-05CC-47EA-A889-52BC263603CB"),
        EVPermission.RoadmapGetById);

    public static readonly SEPermission RoadmapFindPage = new(
        Guid.Parse("47084566-7F68-4621-94B7-D36256EB6ACA"),
        EVPermission.RoadmapFindPage);

    public static readonly SEPermission ArchiveRoadmap = new(
        Guid.Parse("CF8FCDBC-B140-440F-BECC-406259E7AB77"),
        EVPermission.ArchiveRoadmap);

    public static readonly SEPermission UnarchiveRoadmap = new(
        Guid.Parse("C410E408-6051-461F-B75C-7545D499CB73"),
        EVPermission.UnarchiveRoadmap);

    public static readonly SEPermission RoadmapFindArchivedPage = new(
        Guid.Parse("D281118B-2806-4563-9381-ED7EA47D6578"),
        EVPermission.RoadmapFindArchivedPage);

    public Guid Id { get; } = id;

    public string Description { get; } = _descriptionMap[value];

    private static FrozenDictionary<EVPermission, string> BuildDescriptionMap()
    {
        var fields = typeof(EVPermission).GetFields();
        var res = new Dictionary<EVPermission, string>(fields.Length);
        foreach (var fieldInfo in fields)
        {
            if (!fieldInfo.IsStatic)
            {
                continue;
            }

            var value = (EVPermission)fieldInfo.GetValue(null)!;
            var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>(false) ??
                            throw new InvalidOperationException($"Not found description for '{value}' permission");
            res.Add(value, attribute.Description);
        }

        return res.ToFrozenDictionary();
    }
}
