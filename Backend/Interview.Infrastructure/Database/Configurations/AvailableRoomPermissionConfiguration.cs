using Interview.Domain.Permissions;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class AvailableRoomPermissionConfiguration : EntityTypeConfigurationBase<AvailableRoomPermission>
{
    protected override void ConfigureCore(EntityTypeBuilder<AvailableRoomPermission> builder)
    {
        builder.Property(e => e.PermissionId);
        builder
            .HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId);

        var availablePermissions = new[]
        {
            (Id: new Guid("95D476A0-EB0E-470D-9C57-A0EC8A2E4CD6"), Permission: SEPermission.RoomReviewUpdate),
            (Id: new Guid("38CD9540-27F5-4482-A261-2A08F6D8CF30"), Permission: SEPermission.QuestionCreate),
            (Id: new Guid("C68385EE-093A-457F-A03A-B1A53371C248"), Permission: SEPermission.RoomFindById),
            (Id: new Guid("AA3F81EC-9A87-493F-A7D5-FA4CA6E75BF7"), Permission: SEPermission.RoomUpdate),
            (Id: new Guid("D40A2C28-3A84-47F3-9981-88BDF50BB4CA"), Permission: SEPermission.RoomAddParticipant),
            (Id: new Guid("48EB3B31-6632-4B4D-B36D-F61C68865C9D"), Permission: SEPermission.RoomSendEventRequest),
            (Id: new Guid("AD9B444A-67B7-4B85-B592-8578E569B12A"), Permission: SEPermission.RoomClose),
            (Id: new Guid("9ACECC78-79CA-41B1-960E-A4EB9CF03A2C"), Permission: SEPermission.RoomStartReview),
            (Id: new Guid("3B1A04F3-8D35-4608-87FB-1D83D76CD99D"), Permission: SEPermission.RoomGetState),
            (Id: new Guid("8D3C4087-B34D-48F7-BA2A-B1A85F69FE95"), Permission: SEPermission.TranscriptionGet),
            (Id: new Guid("6B3985BF-05DD-47E7-B894-781E28428596"), Permission: SEPermission.RoomGetAnalyticsSummary),
            (Id: new Guid("6CF93811-C44A-4B86-86A1-18D72DF7E1A0"), Permission: SEPermission.RoomGetAnalytics),
            (Id: new Guid("4DC0B8E6-4C1D-46E9-B181-5D2A31E7BDB5"), Permission: SEPermission.DeleteRoomState),
            (Id: new Guid("209A47F7-F1C5-439C-8DE5-7792C08B7CE2"), Permission: SEPermission.UpsertRoomState),
            (Id: new Guid("556D9330-9FF3-46A9-913B-28543FD213E4"), Permission: SEPermission.RoomParticipantCreate),
            (Id: new Guid("B9AD0F66-08C6-4F95-900C-94750F1ADA6B"), Permission: SEPermission.RoomParticipantChangeStatus),
            (Id: new Guid("A1ACBADE-3835-4A9E-9729-56067AF66D53"), Permission: SEPermission.RoomParticipantFindByRoomIdAndUserId),
            (Id: new Guid("5EFEACE0-78CA-4616-AEE0-9F08574132CE"), Permission: SEPermission.RoomQuestionReactionCreate),
            (Id: new Guid("369F0B92-915C-4334-BDAC-6E82FB3C0C74"), Permission: SEPermission.RoomQuestionFindGuids),
            (Id: new Guid("4157604C-FDE9-45CF-B79E-09B7FDE71833"), Permission: SEPermission.RoomQuestionCreate),
            (Id: new Guid("241F76F2-3746-4EE4-9191-A64BA3B3A86E"), Permission: SEPermission.RoomQuestionChangeActiveQuestion),
            (Id: new Guid("BD3496E3-6E57-447E-A7DF-744EFFF03DE5"), Permission: SEPermission.RoomReviewCreate),
        }.Select(e => new AvailableRoomPermission
        {
            Id = e.Id,
            PermissionId = e.Permission.Id,
        })
        .ToArray();
        foreach (var availableRoomPermission in availablePermissions)
        {
            availableRoomPermission.UpdateUpdateDate(new DateTime(2024, 03, 02, 15, 0, 0, DateTimeKind.Utc));
        }

        builder.HasData(availablePermissions);
    }
}
