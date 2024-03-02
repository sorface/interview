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

        /*
        var availablePermissions = new[]
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
            SEPermission.RoomQuestionChangeActiveQuestion,
            SEPermission.RoomReviewCreate,
        };
        */
    }
}
