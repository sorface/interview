using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomPermissionSql_Change_Permission_For_Active_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO ""RoomParticipantPermission"" (""FK_PERMISSION_ID"", ""FK_PARTICIPANT_ID"")
            SELECT permissions.""Id"", RP.""Id""
            FROM ""RoomParticipants"" RP,
            (SELECT permission.""Id"", permission.""Type""
            FROM ""Permissions"" permission
                WHERE ""Type"" in (
                'RoomReviewUpdate',
                'QuestionCreate',
                'RoomFindById',
                'RoomUpdate',
                'RoomAddParticipant',
                'RoomSendEventRequest',
                'RoomClose',
                'RoomStartReview',
                'RoomGetState',
                'TranscriptionGet',
                'RoomGetAnalyticsSummary',
                'RoomGetAnalytics',
                'DeleteRoomState',
                'UpsertRoomState',
                'RoomParticipantCreate',
                'RoomParticipantChangeStatus',
                'RoomParticipantFindByRoomIdAndUserId',
                'RoomQuestionReactionCreate',
                'RoomQuestionFindGuids',
                'RoomQuestionCreate',
                'RoomQuestionUpdate',
                'RoomQuestionChangeActiveQuestion',
                'RoomReviewCreate',
                'RoomInviteGenerate',
                'RoomQuestionEvaluationMerge',
                'RoomQuestionEvaluationFind',
                'RoomReviewCompletion',
                'RoomReviewUpsert',
                'GetRoomQuestionAnswerDetails'
            )) permissions WHERE RP.""Type"" = 'Expert'");

            migrationBuilder.Sql(@"
            INSERT INTO ""RoomParticipantPermission"" (""FK_PERMISSION_ID"", ""FK_PARTICIPANT_ID"")
            SELECT permissions.""Id"", RP.""Id""
            FROM ""RoomParticipants"" RP,
            (SELECT permission.""Id"", permission.""Type""
            FROM ""Permissions"" permission
                WHERE ""Type"" in (
                'RoomReviewUpdate',
                'QuestionCreate',
                'RoomFindById',
                'RoomSendEventRequest',
                'RoomGetState',
                'TranscriptionGet',
                'RoomGetAnalyticsSummary',
                'RoomGetAnalytics',
                'RoomParticipantFindByRoomIdAndUserId',
                'RoomQuestionReactionCreate',
                'RoomQuestionFindGuids',
                'RoomReviewCreate',
                'GetRoomQuestionAnswerDetails'
            )) permissions
                WHERE RP.""Type"" = 'Viewer'");

            migrationBuilder.Sql(@"
            INSERT INTO ""RoomParticipantPermission"" (""FK_PERMISSION_ID"", ""FK_PARTICIPANT_ID"")
            SELECT permissions.""Id"", RP.""Id""
            FROM ""RoomParticipants"" RP,
            (SELECT permission.""Id"", permission.""Type""
            FROM ""Permissions"" permission
                WHERE ""Type"" in (
                'RoomReviewUpdate',
                'QuestionCreate',
                'RoomFindById',
                'RoomSendEventRequest',
                'RoomGetState',
                'TranscriptionGet',
                'RoomGetAnalyticsSummary',
                'RoomGetAnalytics',
                'RoomParticipantFindByRoomIdAndUserId',
                'RoomQuestionReactionCreate',
                'RoomQuestionFindGuids',
                'RoomReviewCreate',
                'GetRoomQuestionAnswerDetails'
            )) permissions
                WHERE RP.""Type"" = 'Examinee'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
