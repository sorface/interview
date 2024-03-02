using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Sqlite.Migrations
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    { new Guid("03fdab95-94f9-4199-b3b7-1b2a57883739"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5c4eb71-50c8-4c13-a144-0496ce56e095"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9018) },
                    { new Guid("0d6ac765-c336-4afd-a28d-aee3e84905ce"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5f088b45-704f-4f61-b4c5-05bd08b80303"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9023) },
                    { new Guid("1c3280a2-6c32-467f-81f7-30fb004b4064"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("6938365f-752d-453e-b0be-93facac0c5b8"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9018) },
                    { new Guid("333ce961-9c64-4be4-9c27-1469f79f6946"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9f020c9e-e0b4-4e6d-9fb3-38ba44cfa3f9"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9020) },
                    { new Guid("42c9ba6c-f1b9-4000-a196-a220587b8463"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("97b2411a-b9d4-49cb-9525-0e31b7d35496"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9019) },
                    { new Guid("4c317cad-e1c0-49fb-aba3-b819d21baada"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7df4ea9b-ded5-4a1d-a8ea-e92e6bd85269"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9019) },
                    { new Guid("4d29ae35-7dc3-4b01-aad0-b46a69d8caed"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4f7a0200-9fe1-4d04-9bcc-6ed668d07828"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9022) },
                    { new Guid("55e9a416-c592-452d-bdd7-c64437263b96"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a63b2ca5-304b-40a0-8e82-665a3327e407"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9020) },
                    { new Guid("588e76b9-960f-4719-9f92-0ab367fca432"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a115f072-638a-4472-8cc3-4cf04da67cfc"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9022) },
                    { new Guid("5f593115-4e43-40cd-aa3f-24d31d756cef"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5ac11db0-b079-40ab-b32b-a02243a451b3"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9019) },
                    { new Guid("652af334-a031-4791-a56b-6b58ebcc3945"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4c3386da-cbb2-4493-86e8-036e8802782d"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9021) },
                    { new Guid("72e17454-7866-48af-97fd-f77f38989ae0"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("eac25c4b-28d5-4e22-93b2-5c3caf0f6922"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9018) },
                    { new Guid("769f615c-4245-448a-b1c0-72389233b9b7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1bb49aa7-1305-427c-9523-e9687392d385"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9022) },
                    { new Guid("8335db33-648e-46a2-adee-e016d0e3c187"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d1916ab5-462e-41d7-ae46-f1ce27d514d4"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9021) },
                    { new Guid("8ab87264-77ea-42c4-abae-acd4f462ab42"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("0827aeef-bcc1-4412-b584-0de4694422ce"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9021) },
                    { new Guid("8e2157a2-1727-45c9-abb0-b56164008cfb"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9ce5949f-a7b9-489c-8b04-bd6724aff687"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9021) },
                    { new Guid("9adbf882-db94-44ed-b3ee-37682159b616"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("150f05e3-8d73-45e9-8ecd-6187f7b96461"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9022) },
                    { new Guid("c81d5a58-74a4-4645-a8db-41bd3ded060b"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("882ffc55-3439-4d0b-8add-ba79e2a7df45"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9019) },
                    { new Guid("caf3ff38-20f1-4adf-a36f-7289d8f643d1"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1f6c85db-c2a0-4096-8ead-a292397ab4e5"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9020) },
                    { new Guid("dc9ed7ed-55b3-498f-ae1f-4c13c091b19a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7c4d9ac2-72e7-466a-bcff-68f3ee0bc65e"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9018) },
                    { new Guid("f65c1843-8221-41f1-920b-0611e26c5bd5"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7ad620a-0614-494a-89ca-623e47b7415a"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9020) },
                    { new Guid("f7588d82-7257-4271-93ac-eda128da2350"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("220380d1-fd72-4004-aed4-22187e88b386"), new DateTime(2024, 3, 2, 16, 40, 53, 564, DateTimeKind.Utc).AddTicks(9015) }
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
