using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class table_add_roadmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roadmap_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoadmapMilestone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentRoadmapMilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapMilestone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestone_RoadmapMilestone_ParentRoadmapMilestoneId",
                        column: x => x.ParentRoadmapMilestoneId,
                        principalTable: "RoadmapMilestone",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoadmapMilestone_Roadmap_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestone_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoadmapTag",
                columns: table => new
                {
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapTag", x => new { x.RoadmapId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_RoadmapTag_Roadmap_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoadmapTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapMilestoneItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapMilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionTreeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapMilestoneItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestoneItem_QuestionTree_QuestionTreeId",
                        column: x => x.QuestionTreeId,
                        principalTable: "QuestionTree",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestoneItem_RoadmapMilestone_RoadmapMilestoneId",
                        column: x => x.RoadmapMilestoneId,
                        principalTable: "RoadmapMilestone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestoneItem_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roadmap_CreatedById",
                table: "Roadmap",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestone_CreatedById",
                table: "RoadmapMilestone",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestone_ParentRoadmapMilestoneId",
                table: "RoadmapMilestone",
                column: "ParentRoadmapMilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestone_RoadmapId",
                table: "RoadmapMilestone",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestoneItem_CreatedById",
                table: "RoadmapMilestoneItem",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestoneItem_QuestionTreeId",
                table: "RoadmapMilestoneItem",
                column: "QuestionTreeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestoneItem_RoadmapMilestoneId",
                table: "RoadmapMilestoneItem",
                column: "RoadmapMilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapTag_TagsId",
                table: "RoadmapTag",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadmapMilestoneItem");

            migrationBuilder.DropTable(
                name: "RoadmapTag");

            migrationBuilder.DropTable(
                name: "RoadmapMilestone");

            migrationBuilder.DropTable(
                name: "Roadmap");
        }
    }
}
