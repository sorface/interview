using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class permissions_add_GetRoomBusinessAnalytic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("aff113ac-c65b-4566-993b-aa7f6577f397"), DateTime.UtcNow, null, "GetRoomBusinessAnalytic", DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aff113ac-c65b-4566-993b-aa7f6577f397"));
        }
    }
}
