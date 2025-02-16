using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class create_question_tree_and_subject_tree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionSubjectTree",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<char>(type: "character(1)", nullable: false, comment: "Available values: [Empty: E, Question: Q]"),
                    ParentQuestionSubjectTreeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSubjectTree", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSubjectTree_QuestionSubjectTree_ParentQuestionSubje~",
                        column: x => x.ParentQuestionSubjectTreeId,
                        principalTable: "QuestionSubjectTree",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionSubjectTree_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSubjectTree_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionTree",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RootQuestionSubjectTreeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentQuestionTreeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTree", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTree_QuestionSubjectTree_RootQuestionSubjectTreeId",
                        column: x => x.RootQuestionSubjectTreeId,
                        principalTable: "QuestionSubjectTree",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionTree_QuestionTree_ParentQuestionTreeId",
                        column: x => x.ParentQuestionTreeId,
                        principalTable: "QuestionTree",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionTree_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubjectTree_CreatedById",
                table: "QuestionSubjectTree",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubjectTree_ParentQuestionSubjectTreeId",
                table: "QuestionSubjectTree",
                column: "ParentQuestionSubjectTreeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubjectTree_QuestionId",
                table: "QuestionSubjectTree",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTree_CreatedById",
                table: "QuestionTree",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTree_ParentQuestionTreeId",
                table: "QuestionTree",
                column: "ParentQuestionTreeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTree_RootQuestionSubjectTreeId",
                table: "QuestionTree",
                column: "RootQuestionSubjectTreeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionTree");

            migrationBuilder.DropTable(
                name: "QuestionSubjectTree");
        }
    }
}
