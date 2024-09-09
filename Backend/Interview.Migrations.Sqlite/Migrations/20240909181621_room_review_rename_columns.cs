using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class room_review_rename_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Rooms_RoomById",
                table: "RoomInvites");

            migrationBuilder.DropIndex(
                name: "IX_RoomInvites_InviteById",
                table: "RoomInvites");

            migrationBuilder.DropIndex(
                name: "IX_RoomInvites_RoomById",
                table: "RoomInvites");

            migrationBuilder.RenameColumn(
                name: "InviteById",
                table: "RoomInvites",
                newName: "InviteId");

            migrationBuilder.RenameColumn(
                name: "RoomById",
                table: "RoomInvites",
                newName: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_InviteId",
                table: "RoomInvites",
                column: "InviteId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_RoomId",
                table: "RoomInvites",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Invites_InviteId",
                table: "RoomInvites",
                column: "InviteId",
                principalTable: "Invites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Rooms_RoomId",
                table: "RoomInvites",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Invites_InviteId",
                table: "RoomInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Rooms_RoomId",
                table: "RoomInvites");

            migrationBuilder.DropIndex(
                name: "IX_RoomInvites_InviteId",
                table: "RoomInvites");

            migrationBuilder.DropIndex(
                name: "IX_RoomInvites_RoomId",
                table: "RoomInvites");

            migrationBuilder.RenameColumn(
                name: "InviteId",
                table: "RoomInvites",
                newName: "InviteById");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "RoomInvites",
                newName: "RoomById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_InviteById",
                table: "RoomInvites",
                column: "InviteById");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvites_RoomById",
                table: "RoomInvites",
                column: "RoomById");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites",
                column: "InviteById",
                principalTable: "Invites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Rooms_RoomById",
                table: "RoomInvites",
                column: "RoomById",
                principalTable: "Rooms",
                principalColumn: "Id");
        }
    }
}
