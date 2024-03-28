using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AvailableRoomPermission_Add_RoomInviteGet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[] { new Guid("07dea11a-65a4-4826-ab4f-9d2cdfaa72f3"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b530321a-a51a-4a36-8afd-6e8a8dbae248"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("07dea11a-65a4-4826-ab4f-9d2cdfaa72f3"));
        }
    }
}
