using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_ability_to_archive_category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("84dc5bce-fa74-47cb-949a-042da1126c0c"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "CategoryUnarchive", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b4dca27c-5733-4b37-bb63-7eca6f8e831b"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "FindCategoryPageArchive", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("c0afec8d-04d0-4a7a-9f20-c3d4c891f04e"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "CategoryArchive", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84dc5bce-fa74-47cb-949a-042da1126c0c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b4dca27c-5733-4b37-bb63-7eca6f8e831b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c0afec8d-04d0-4a7a-9f20-c3d4c891f04e"));
        }
    }
}
