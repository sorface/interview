using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Permission_Add_RoomInviteGet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("b530321a-a51a-4a36-8afd-6e8a8dbae248"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "RoomInviteGet", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b530321a-a51a-4a36-8afd-6e8a8dbae248"));
        }
    }
}
