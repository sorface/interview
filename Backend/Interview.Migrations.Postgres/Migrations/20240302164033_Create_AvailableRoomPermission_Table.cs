using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Create_AvailableRoomPermission_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailableRoomPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableRoomPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableRoomPermission_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailableRoomPermission_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("10d0fccb-e1a6-487c-a90a-f151d28c7520"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("0827aeef-bcc1-4412-b584-0de4694422ce"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7397) },
                    { new Guid("29783be4-03a2-463c-a2ea-d60586f582c7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5f088b45-704f-4f61-b4c5-05bd08b80303"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7399) },
                    { new Guid("2b492711-38cd-47ba-82bf-55d31a149c16"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7c4d9ac2-72e7-466a-bcff-68f3ee0bc65e"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7394) },
                    { new Guid("2b4dd437-a2d6-4e0b-9a35-fc7f8a0c7503"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9f020c9e-e0b4-4e6d-9fb3-38ba44cfa3f9"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7396) },
                    { new Guid("320dfad1-a3b5-454a-b00e-5c652c3da40d"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4c3386da-cbb2-4493-86e8-036e8802782d"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7397) },
                    { new Guid("472a31d0-ccf3-4f83-80e8-bb49262547ec"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4f7a0200-9fe1-4d04-9bcc-6ed668d07828"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7398) },
                    { new Guid("539a3c93-3d47-441b-883e-b1adffba5263"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5c4eb71-50c8-4c13-a144-0496ce56e095"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7394) },
                    { new Guid("68fd7f70-a8f6-4e48-9f6f-239ea5403c0b"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("220380d1-fd72-4004-aed4-22187e88b386"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7391) },
                    { new Guid("6c3b3738-27e3-4672-ab65-51009b086774"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d1916ab5-462e-41d7-ae46-f1ce27d514d4"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7397) },
                    { new Guid("6d960671-f5d3-4990-9c80-949bc06a21b8"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7ad620a-0614-494a-89ca-623e47b7415a"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7396) },
                    { new Guid("75a4dc48-00ae-4d6c-b75e-d5b5fc499106"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("97b2411a-b9d4-49cb-9525-0e31b7d35496"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7395) },
                    { new Guid("76ceabda-48e1-473f-89ce-c47d3f975cbb"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5ac11db0-b079-40ab-b32b-a02243a451b3"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7395) },
                    { new Guid("7d9d21f2-cc49-44e6-85e0-cca68268cdb5"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a115f072-638a-4472-8cc3-4cf04da67cfc"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7398) },
                    { new Guid("8c27b00d-f883-4c90-868f-6d1cb3784b3c"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("150f05e3-8d73-45e9-8ecd-6187f7b96461"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7398) },
                    { new Guid("a1487bb9-a09d-4a34-aa99-2c3b508e3712"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1bb49aa7-1305-427c-9523-e9687392d385"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7398) },
                    { new Guid("a764ecf3-c604-41f4-acf0-d078011a0104"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9ce5949f-a7b9-489c-8b04-bd6724aff687"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7397) },
                    { new Guid("b7dd1cd0-151f-43f0-8066-8cc6c356cf10"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7df4ea9b-ded5-4a1d-a8ea-e92e6bd85269"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7395) },
                    { new Guid("cc5b111f-3dad-49f9-94bd-e792b73fa02c"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("882ffc55-3439-4d0b-8add-ba79e2a7df45"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7395) },
                    { new Guid("deab2b06-2bd8-43b6-8e8a-3d9292b79ee7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("6938365f-752d-453e-b0be-93facac0c5b8"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7394) },
                    { new Guid("e99d462d-4212-40da-8f73-1c09740c2264"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a63b2ca5-304b-40a0-8e82-665a3327e407"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7396) },
                    { new Guid("edf149fb-5170-410c-9441-e47754fb90ff"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1f6c85db-c2a0-4096-8ead-a292397ab4e5"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7396) },
                    { new Guid("f6415e91-8fec-4db1-b10d-cb0aa701e433"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("eac25c4b-28d5-4e22-93b2-5c3caf0f6922"), new DateTime(2024, 3, 2, 16, 40, 33, 582, DateTimeKind.Utc).AddTicks(7394) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableRoomPermission_CreatedById",
                table: "AvailableRoomPermission",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableRoomPermission_PermissionId",
                table: "AvailableRoomPermission",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableRoomPermission");
        }
    }
}
