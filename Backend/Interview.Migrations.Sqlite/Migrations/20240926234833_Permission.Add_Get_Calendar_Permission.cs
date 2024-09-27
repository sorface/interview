using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class PermissionAdd_Get_Calendar_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("06d64b89-090f-4fd0-b81d-20268ee91cea"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "GetRoomCalendar", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.Sql(@"INSERT INTO PermissionUser(PermissionsId, UserId) 
SELECT tab2.Id, tab1.Id
FROM (SELECT distinct Id FROM Users) tab1,
     (SELECT PERMISSION.Id FROM Permissions PERMISSION WHERE PERMISSION.Type = 'GetRoomCalendar') tab2;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("06d64b89-090f-4fd0-b81d-20268ee91cea"));
        }
    }
}
