using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Question_Add_RoomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_RoomId",
                table: "Questions",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Rooms_RoomId",
                table: "Questions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Rooms_RoomId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_RoomId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Questions");
        }
    }
}
