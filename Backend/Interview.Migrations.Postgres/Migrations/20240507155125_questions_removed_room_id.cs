using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class questions_removed_room_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO RoomQuestions
(
 Id,
 CreateDate,
 CreatedById,
 QuestionId,
 RoomId,
 State,
 UpdateDate
)
SELECT
    q.Id,
    q.CreateDate,
    q.CreatedById,
    q.Id,
    q.RoomId,
    'Open',
    q.UpdateDate
FROM Questions AS q
WHERE q.RoomId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM RoomQuestions AS rq WHERE rq.RoomId = q.RoomId AND rq.QuestionId = rq.Id)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
