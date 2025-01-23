using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class category_add_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"WITH OrderedCategories AS (
    SELECT
        Id,
        ROW_NUMBER() OVER (PARTITION BY ParentId ORDER BY Name) AS new_order
    FROM Categories
)
UPDATE Categories
SET ""Order"" = (
    SELECT new_order
    FROM OrderedCategories
    WHERE OrderedCategories.Id = Categories.Id
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Categories");
        }
    }
}
