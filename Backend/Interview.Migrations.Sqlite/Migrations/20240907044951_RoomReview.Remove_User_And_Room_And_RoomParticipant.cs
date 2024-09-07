using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomReviewRemove_User_And_Room_And_RoomParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomReview_Rooms_RoomId",
                table: "RoomReview");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomReview_Users_UserId",
                table: "RoomReview");

            migrationBuilder.DropIndex(
                name: "IX_RoomReview_RoomId",
                table: "RoomReview");

            migrationBuilder.DropIndex(
                name: "IX_RoomReview_UserId",
                table: "RoomReview");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "RoomReview");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RoomReview");

            migrationBuilder.RenameColumn(
                name: "SeRoomReviewState",
                table: "RoomReview",
                newName: "State");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomReview_RoomParticipants_Id",
                table: "RoomReview",
                column: "Id",
                principalTable: "RoomParticipants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomReview_RoomParticipants_Id",
                table: "RoomReview");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "RoomReview",
                newName: "SeRoomReviewState");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "RoomReview",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RoomReview",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RoomReview_RoomId",
                table: "RoomReview",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomReview_UserId",
                table: "RoomReview",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomReview_Rooms_RoomId",
                table: "RoomReview",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomReview_Users_UserId",
                table: "RoomReview",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
