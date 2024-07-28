using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class add_room_create_permission_to_all_users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO ""PermissionUser""
(
 ""UserId"",
 ""PermissionsId""
)
SELECT
    ru.""UserId"",
    'C4C21128-F672-47D0-B0F5-2B3CA53FC420'
FROM ""RoleUser"" ru
WHERE ru.""RolesId"" = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'
  AND NOT EXISTS(SELECT 1 FROM ""PermissionUser"" pu WHERE pu.""UserId"" = ru.""UserId"" AND pu.""PermissionsId"" = 'C4C21128-F672-47D0-B0F5-2B3CA53FC420')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
