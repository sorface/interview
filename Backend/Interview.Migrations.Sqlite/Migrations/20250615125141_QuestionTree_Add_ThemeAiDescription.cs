using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class QuestionTree_Add_ThemeAiDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeAiDescription",
                table: "QuestionTree",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThemeAiDescription",
                table: "QuestionTree");
        }
    }
}
