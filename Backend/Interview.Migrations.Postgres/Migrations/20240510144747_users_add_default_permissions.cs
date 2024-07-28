using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class users_add_default_permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@";INSERT INTO ""PermissionUser""
(
 ""PermissionsId"",
 ""UserId""
)
SELECT
    '5C12DBF7-3CF9-40B2-9CAB-203621129342',
    usr.""Id""
FROM ""RoleUser"" ru
    INNER JOIN ""Users"" usr ON usr.""Id"" = ru.""UserId""
                          AND NOT EXISTS(SELECT 1 FROM ""PermissionUser"" pu WHERE pu.""PermissionsId"" = '5C12DBF7-3CF9-40B2-9CAB-203621129342' AND pu.""UserId"" = usr.""Id"")
WHERE ru.""RolesId"" = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'

;INSERT INTO ""PermissionUser""
(
    ""PermissionsId"",
    ""UserId""
)
SELECT
    'EAC25C4B-28D5-4E22-93B2-5C3CAF0F6922',
    usr.""Id""
FROM ""RoleUser"" ru
         INNER JOIN ""Users"" usr ON usr.""Id"" = ru.""UserId""
    AND NOT EXISTS(SELECT 1 FROM ""PermissionUser"" pu WHERE pu.""PermissionsId"" = 'EAC25C4B-28D5-4E22-93B2-5C3CAF0F6922' AND pu.""UserId"" = usr.""Id"")
WHERE ru.""RolesId"" = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
