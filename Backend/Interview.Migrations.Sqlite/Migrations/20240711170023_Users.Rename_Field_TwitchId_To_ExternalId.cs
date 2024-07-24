using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class UsersRename_Field_TwitchId_To_ExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Users",
                type: "TEXT",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE Users SET ExternalId = TwitchIdentity");
            
            migrationBuilder.DropColumn(
                name: "TwitchIdentity",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwitchIdentity",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
            
            migrationBuilder.Sql(@"UPDATE Users SET TwitchIdentity = ExternalId");
            
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Users");
        }
    }
}
