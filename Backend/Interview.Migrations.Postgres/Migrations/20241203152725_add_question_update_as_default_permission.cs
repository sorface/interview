using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class add_question_update_as_default_permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO ""PermissionUser""
(
    ""PermissionsId"",
    ""UserId""
)
SELECT DISTINCT
    uuid('175F724E-7299-4A0B-B827-0D4B0C6AED6B'),
    usr.""Id""
from ""Users"" usr
         LEFT JOIN ""PermissionUser"" pu ON pu.""UserId"" = usr.""Id"" AND pu.""PermissionsId"" = '175F724E-7299-4A0B-B827-0D4B0C6AED6B'
WHERE pu.""UserId"" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
