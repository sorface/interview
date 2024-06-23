using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomTimerAdd_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomTimer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Duration = table.Column<long>(type: "INTEGER", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTimer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomTimer_Rooms_Id",
                        column: x => x.Id,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomTimer_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomTimer_CreatedById",
                table: "RoomTimer",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomTimer");
        }
    }
}
