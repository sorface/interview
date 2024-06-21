using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RoomTimerChange_Duration_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Duration",
                table: "RoomTimer",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Duration",
                table: "RoomTimer",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
