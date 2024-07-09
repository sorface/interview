using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomQuestionEvaluationAdd_Table_And_Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomQuestionEvaluation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoomQuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Review = table.Column<string>(type: "TEXT", nullable: true),
                    Mark = table.Column<int>(type: "INTEGER", nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomQuestionEvaluation", x => x.Id);
                    table.UniqueConstraint("AK_RoomQuestionEvaluation_RoomQuestionId_CreatedById", x => new { x.RoomQuestionId, x.CreatedById });
                    table.ForeignKey(
                        name: "FK_RoomQuestionEvaluation_RoomQuestions_RoomQuestionId",
                        column: x => x.RoomQuestionId,
                        principalTable: "RoomQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomQuestionEvaluation_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("7b231e25-446a-418c-9281-4eb453dd4893"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomQuestionEvaluationFind", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("d74df965-84d3-4bcc-af1b-13f5c6299fa7"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomQuestionEvaluationMerge", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("0c24de0f-6fe3-4d95-81fb-e9e7542852f7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d74df965-84d3-4bcc-af1b-13f5c6299fa7"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("49cb7cd3-7329-4098-9ac1-c3972ba09138"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7b231e25-446a-418c-9281-4eb453dd4893"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomQuestionEvaluation_CreatedById",
                table: "RoomQuestionEvaluation",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomQuestionEvaluation");

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("0c24de0f-6fe3-4d95-81fb-e9e7542852f7"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("49cb7cd3-7329-4098-9ac1-c3972ba09138"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b231e25-446a-418c-9281-4eb453dd4893"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d74df965-84d3-4bcc-af1b-13f5c6299fa7"));
        }
    }
}
