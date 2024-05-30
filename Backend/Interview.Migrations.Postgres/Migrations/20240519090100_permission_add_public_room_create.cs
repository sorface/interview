using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_public_room_create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AcсessType",
                table: "Rooms",
                newName: "AccessType");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("fcc9bbca-15c6-4221-8d2d-e052b8cd4385"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "PublicRoomCreate", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fcc9bbca-15c6-4221-8d2d-e052b8cd4385"));

            migrationBuilder.RenameColumn(
                name: "AccessType",
                table: "Rooms",
                newName: "AcсessType");
        }
    }
}
