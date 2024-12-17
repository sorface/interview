using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class DbRoomEvent_Add_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomEvents_RoomId",
                table: "RoomEvents");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEvents_RoomId_Type",
                table: "RoomEvents",
                columns: new[] { "RoomId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomEvents_RoomId_Type",
                table: "RoomEvents");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEvents_RoomId",
                table: "RoomEvents",
                column: "RoomId");
        }
    }
}
