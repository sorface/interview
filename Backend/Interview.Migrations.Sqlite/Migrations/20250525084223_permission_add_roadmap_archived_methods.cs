using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_roadmap_archived_methods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("c410e408-6051-461f-b75c-7545d499cb73"), DateTime.UtcNow, null, "UnarchiveRoadmap", DateTime.UtcNow },
                    { new Guid("cf8fcdbc-b140-440f-becc-406259e7ab77"), DateTime.UtcNow, null, "ArchiveRoadmap", DateTime.UtcNow },
                    { new Guid("d281118b-2806-4563-9381-ed7ea47d6578"), DateTime.UtcNow, null, "RoadmapFindArchivedPage", DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c410e408-6051-461f-b75c-7545d499cb73"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cf8fcdbc-b140-440f-becc-406259e7ab77"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d281118b-2806-4563-9381-ed7ea47d6578"));
        }
    }
}
