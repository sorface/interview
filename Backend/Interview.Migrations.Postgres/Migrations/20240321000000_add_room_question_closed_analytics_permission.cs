using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations;

/// <inheritdoc />
public partial class add_room_question_closed_analytics_permission : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Permissions",
            columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
            values: new object[] { new Guid("7D8F5E3A-B2C1-4E9D-9F6A-1234567890AB"), new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "GetRoomQuestionClosedAnalytics", new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) });

        migrationBuilder.InsertData(
            table: "AvailableRoomPermission",
            columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
            values: new object[] { new Guid("8A9F6B4C-D3E2-4F1A-B5C7-1234567890AB"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7D8F5E3A-B2C1-4E9D-9F6A-1234567890AB"), null, new DateTime(2024, 3, 21, 15, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.Sql(@"
            INSERT INTO ""RoomParticipantPermission"" (""FK_PERMISSION_ID"", ""FK_PARTICIPANT_ID"")
            SELECT permissions.""Id"", RP.""Id""
            FROM ""RoomParticipants"" RP,
            (SELECT permission.""Id"", permission.""Type""
            FROM ""Permissions"" permission
                WHERE ""Type"" = 'GetRoomQuestionClosedAnalytics') permissions
                WHERE RP.""Type"" = 'Expert'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AvailableRoomPermission",
            keyColumn: "Id",
            keyValue: new Guid("8A9F6B4C-D3E2-4F1A-B5C7-1234567890AB"));

        migrationBuilder.DeleteData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("7D8F5E3A-B2C1-4E9D-9F6A-1234567890AB"));
    }
} 