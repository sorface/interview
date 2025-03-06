using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class question_subject_tree_make_question_id_non_required : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSubjectTree_Questions_QuestionId",
                table: "QuestionSubjectTree");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                table: "QuestionSubjectTree",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSubjectTree_Questions_QuestionId",
                table: "QuestionSubjectTree",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSubjectTree_Questions_QuestionId",
                table: "QuestionSubjectTree");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                table: "QuestionSubjectTree",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSubjectTree_Questions_QuestionId",
                table: "QuestionSubjectTree",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
