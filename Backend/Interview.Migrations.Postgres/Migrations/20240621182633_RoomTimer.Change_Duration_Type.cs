using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
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
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Duration",
                table: "RoomTimer",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
