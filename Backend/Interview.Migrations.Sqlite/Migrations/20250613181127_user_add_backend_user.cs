using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class user_add_backend_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreateDate", "CreatedById", "ExternalId", "Nickname", "UpdateDate" },
                values: new object[] { new Guid("c560b516-238c-45dd-b394-352757fc6380"), null, DateTime.UtcNow, null, "", "Backend", DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c560b516-238c-45dd-b394-352757fc6380"));
        }
    }
}
