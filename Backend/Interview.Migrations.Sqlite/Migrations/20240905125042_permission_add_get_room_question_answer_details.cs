using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_get_room_question_answer_details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("edab0e5d-7ac2-4761-b47f-a5f41a9ae48c"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "GetRoomQuestionAnswerDetails", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[] { new Guid("ba198396-d07a-4054-95d0-4fe0ba393ecd"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("edab0e5d-7ac2-4761-b47f-a5f41a9ae48c"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("ba198396-d07a-4054-95d0-4fe0ba393ecd"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("edab0e5d-7ac2-4761-b47f-a5f41a9ae48c"));
        }
    }
}
