using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomParticipantAdd_Expert_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("2a6f981e-f79e-4497-83d0-35018cbd24d3"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomReviewCompletion", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("695914fe-a627-4959-b8b9-e0413ba63755"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomReviewUpsert", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("25c2bc73-39ea-4288-9756-cef28ddc4534"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("2a6f981e-f79e-4497-83d0-35018cbd24d3"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("487c74cb-3502-4f1a-957a-cbcea5773702"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("695914fe-a627-4959-b8b9-e0413ba63755"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("25c2bc73-39ea-4288-9756-cef28ddc4534"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("487c74cb-3502-4f1a-957a-cbcea5773702"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2a6f981e-f79e-4497-83d0-35018cbd24d3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("695914fe-a627-4959-b8b9-e0413ba63755"));
        }
    }
}
