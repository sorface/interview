using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class question_answer_change_code_editor_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeEditor",
                table: "QuestionAnswers");
            migrationBuilder.AddColumn<bool>(
                name: "CodeEditor",
                table: "QuestionAnswers",
                type: "boolean",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeEditor",
                table: "QuestionAnswers");
            migrationBuilder.AddColumn<bool>(
                name: "CodeEditor",
                table: "QuestionAnswers",
                type: "text",
                nullable: false);
        }
    }
}
