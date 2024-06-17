using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class permission_add_category_permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("1b2dd31b-b35e-48e2-8f33-d0366b9d60ba"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "EditCategory", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("9001520d-b1d2-4ade-8f70-570d2b7efea1"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "FindCategoryPage", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.Sql(@"INSERT INTO PermissionUser
(
 UserId,
 PermissionsId
)
SELECT
    ru.UserId,
    '9001520d-b1d2-4ade-8f70-570d2b7efea1'
FROM RoleUser ru
WHERE ru.RolesId = 'AB45A82B-AA1C-11ED-ABE8-F2B335A02EE9'
  AND NOT EXISTS(SELECT 1 FROM PermissionUser pu WHERE pu.UserId = ru.UserId AND pu.PermissionsId = '9001520d-b1d2-4ade-8f70-570d2b7efea1')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1b2dd31b-b35e-48e2-8f33-d0366b9d60ba"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9001520d-b1d2-4ade-8f70-570d2b7efea1"));
        }
    }
}
