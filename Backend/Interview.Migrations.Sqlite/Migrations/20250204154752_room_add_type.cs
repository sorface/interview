using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class room_add_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<char>(
                name: "Type",
                table: "Rooms",
                type: "TEXT",
                unicode: false,
                maxLength: 1,
                nullable: false,
                defaultValue: 'S',
                comment: "Available values: A: AI, S: Standard");
            migrationBuilder.Sql(@"UPDATE Rooms SET
    Type = 'A'
WHERE CategoryId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Rooms");
        }
    }
}
