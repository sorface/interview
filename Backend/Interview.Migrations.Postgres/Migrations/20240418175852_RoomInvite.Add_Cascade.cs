using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RoomInviteAdd_Cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites",
                column: "InviteById",
                principalTable: "Invites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomInvites_Invites_InviteById",
                table: "RoomInvites",
                column: "InviteById",
                principalTable: "Invites",
                principalColumn: "Id");
        }
    }
}
