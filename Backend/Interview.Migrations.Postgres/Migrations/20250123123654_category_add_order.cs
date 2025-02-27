using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
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
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"WITH OrderedCategories AS (
    SELECT
        ""Id"",
        ROW_NUMBER() OVER (PARTITION BY ""ParentId"" ORDER BY ""Name"") AS new_order
    FROM ""Categories""
)
UPDATE ""Categories""
SET ""Order"" = OrderedCategories.new_order
FROM OrderedCategories
WHERE ""Categories"".""Id"" = OrderedCategories.""Id"";");
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
