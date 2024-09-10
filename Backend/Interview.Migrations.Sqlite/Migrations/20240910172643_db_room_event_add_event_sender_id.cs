using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class db_room_event_add_event_sender_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventSenderId",
                table: "RoomEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomEvents_EventSenderId",
                table: "RoomEvents",
                column: "EventSenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomEvents_Users_EventSenderId",
                table: "RoomEvents",
                column: "EventSenderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomEvents_Users_EventSenderId",
                table: "RoomEvents");

            migrationBuilder.DropIndex(
                name: "IX_RoomEvents_EventSenderId",
                table: "RoomEvents");

            migrationBuilder.DropColumn(
                name: "EventSenderId",
                table: "RoomEvents");
        }
    }
}
