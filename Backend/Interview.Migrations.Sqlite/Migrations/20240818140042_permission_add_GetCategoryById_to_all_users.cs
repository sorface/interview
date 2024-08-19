using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_GetCategoryById_to_all_users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO PermissionUser
(
 UserId,
 PermissionsId
)
SELECT
    ru.UserId,
    'BC98D0B8-B4A3-4B66-B8C1-DB1FCA0647E0'
FROM RoleUser ru
WHERE ru.RolesId = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'
  AND NOT EXISTS(SELECT 1 FROM PermissionUser pu WHERE pu.UserId = ru.UserId AND pu.PermissionsId = 'BC98D0B8-B4A3-4B66-B8C1-DB1FCA0647E0')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
