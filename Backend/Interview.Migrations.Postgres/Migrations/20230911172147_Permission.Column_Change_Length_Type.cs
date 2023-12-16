using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PermissionColumn_Change_Length_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Tag",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreateDate", "CreatedById", "Type", "UpdateDate" },
                values: new object[] { new Guid("189309c5-0bff-46a6-8539-5fc6bf77983e"), new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "QuestionFindPage", new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CreatedById",
                table: "Tag",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Users_CreatedById",
                table: "Tag",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Users_CreatedById",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_CreatedById",
                table: "Tag");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("189309c5-0bff-46a6-8539-5fc6bf77983e"));

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Tag");
        }
    }
}
