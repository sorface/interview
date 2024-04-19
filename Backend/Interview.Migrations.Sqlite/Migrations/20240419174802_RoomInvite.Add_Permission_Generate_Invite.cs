using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomInviteAdd_Permission_Generate_Invite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("c1f43ca8-21f1-41e6-9794-e7d44156bf73"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomInviteGenerate", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[] { new Guid("95f1f088-6931-4914-92c1-c1f1d7d75a18"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c1f43ca8-21f1-41e6-9794-e7d44156bf73"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("95f1f088-6931-4914-92c1-c1f1d7d75a18"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1f43ca8-21f1-41e6-9794-e7d44156bf73"));
        }
    }
}
