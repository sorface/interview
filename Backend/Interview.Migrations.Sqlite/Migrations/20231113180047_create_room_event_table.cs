using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class create_room_event_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_queued_room_event_Rooms_RoomId",
                table: "queued_room_event");

            migrationBuilder.DropForeignKey(
                name: "FK_queued_room_event_Users_CreatedById",
                table: "queued_room_event");

            migrationBuilder.DropPrimaryKey(
                name: "PK_queued_room_event",
                table: "queued_room_event");

            migrationBuilder.RenameTable(
                name: "queued_room_event",
                newName: "QueuedRoomEvents");

            migrationBuilder.RenameIndex(
                name: "IX_queued_room_event_RoomId",
                table: "QueuedRoomEvents",
                newName: "IX_QueuedRoomEvents_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_queued_room_event_CreatedById",
                table: "QueuedRoomEvents",
                newName: "IX_QueuedRoomEvents_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueuedRoomEvents",
                table: "QueuedRoomEvents",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "RoomEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoomId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Stateful = table.Column<bool>(type: "INTEGER", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomEvents_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomEvents_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomEvents_CreatedById",
                table: "RoomEvents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEvents_RoomId",
                table: "RoomEvents",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedRoomEvents_Rooms_RoomId",
                table: "QueuedRoomEvents",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedRoomEvents_Users_CreatedById",
                table: "QueuedRoomEvents",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueuedRoomEvents_Rooms_RoomId",
                table: "QueuedRoomEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedRoomEvents_Users_CreatedById",
                table: "QueuedRoomEvents");

            migrationBuilder.DropTable(
                name: "RoomEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QueuedRoomEvents",
                table: "QueuedRoomEvents");

            migrationBuilder.RenameTable(
                name: "QueuedRoomEvents",
                newName: "queued_room_event");

            migrationBuilder.RenameIndex(
                name: "IX_QueuedRoomEvents_RoomId",
                table: "queued_room_event",
                newName: "IX_queued_room_event_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_QueuedRoomEvents_CreatedById",
                table: "queued_room_event",
                newName: "IX_queued_room_event_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_queued_room_event",
                table: "queued_room_event",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_queued_room_event_Rooms_RoomId",
                table: "queued_room_event",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_queued_room_event_Users_CreatedById",
                table: "queued_room_event",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
