using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_get_category_by_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("bc98d0b8-b4a3-4b66-b8c1-db1fca0647e0"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "GetCategoryById", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.Sql(@"INSERT INTO ""PermissionUser""
(
 ""UserId"",
 ""PermissionsId""
)
SELECT
    ru.""UserId"",
    'bc98d0b8-b4a3-4b66-b8c1-db1fca0647e0'
FROM ""RoleUser"" ru
WHERE ru.""RolesId"" = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'
  AND NOT EXISTS(SELECT 1 FROM ""PermissionUser"" pu WHERE pu.""UserId"" = ru.""UserId"" AND pu.""PermissionsId"" = 'bc98d0b8-b4a3-4b66-b8c1-db1fca0647e0')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bc98d0b8-b4a3-4b66-b8c1-db1fca0647e0"));
        }
    }
}
