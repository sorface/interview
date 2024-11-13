using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class add_room_code_editor_change_source : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeEditorChangeSource",
                table: "RoomConfiguration",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "User",
                comment: "Available values: [User, System]");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "QuestionCodeEditors",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "User",
                comment: "Available values: [User, System]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeEditorChangeSource",
                table: "RoomConfiguration");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "QuestionCodeEditors");
        }
    }
}
