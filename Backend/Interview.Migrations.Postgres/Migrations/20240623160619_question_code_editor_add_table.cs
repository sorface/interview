using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class question_code_editor_add_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionCodeEditors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Lang = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "IX_QuestionCodeEditors_CreatedById",
                table: "QuestionCodeEditors",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionCodeEditors_Id",
                table: "Questions",
                column: "Id",
                principalTable: "QuestionCodeEditors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionCodeEditors_Id",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionCodeEditors");
        }
    }
}
