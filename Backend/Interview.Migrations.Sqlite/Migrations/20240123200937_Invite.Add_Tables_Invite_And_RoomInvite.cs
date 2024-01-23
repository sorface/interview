using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InviteAdd_Tables_Invite_And_RoomInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcсessType",
                table: "Rooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsesCurrent = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UsesMax = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoomInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InviteById = table.Column<Guid>(type: "TEXT", nullable: true),
                    RoomById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParticipantType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "Viewer"),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomInvites_Invites_InviteById",
                        column: x => x.InviteById,
                        principalTable: "Invites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomInvites_Rooms_RoomById",
                        column: x => x.RoomById,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invites_CreatedById",
                table: "Invites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_CreatedById",
                table: "RoomInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_InviteById",
                table: "RoomInvites",
                column: "InviteById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_RoomById",
                table: "RoomInvites",
                column: "RoomById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomInvites");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropColumn(
                name: "AcсessType",
                table: "Rooms");
        }
    }
}
