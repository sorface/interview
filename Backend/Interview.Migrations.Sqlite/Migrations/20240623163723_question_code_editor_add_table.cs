using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class question_code_editor_add_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CodeEditorId",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionCodeEditors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Lang = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionCodeEditors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionCodeEditors_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CodeEditorId",
                table: "Questions",
                column: "CodeEditorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCodeEditors_CreatedById",
                table: "QuestionCodeEditors",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionCodeEditors_CodeEditorId",
                table: "Questions",
                column: "CodeEditorId",
                principalTable: "QuestionCodeEditors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionCodeEditors_CodeEditorId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionCodeEditors");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CodeEditorId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CodeEditorId",
                table: "Questions");
        }
    }
}
