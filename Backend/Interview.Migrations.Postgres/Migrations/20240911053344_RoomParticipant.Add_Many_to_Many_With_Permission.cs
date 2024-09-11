using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RoomParticipantAdd_Many_to_Many_With_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailableRoomPermission_RoomParticipants_RoomParticipantId",
                table: "AvailableRoomPermission");

            migrationBuilder.DropIndex(
                name: "IX_AvailableRoomPermission_RoomParticipantId",
                table: "AvailableRoomPermission");

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("07dea11a-65a4-4826-ab4f-9d2cdfaa72f3"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("0c24de0f-6fe3-4d95-81fb-e9e7542852f7"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("209a47f7-f1c5-439c-8de5-7792c08b7ce2"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("241f76f2-3746-4ee4-9191-a64ba3b3a86e"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("25c2bc73-39ea-4288-9756-cef28ddc4534"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("369f0b92-915c-4334-bdac-6e82fb3c0c74"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("38cd9540-27f5-4482-a261-2a08f6d8cf30"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("3b1a04f3-8d35-4608-87fb-1d83d76cd99d"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("4157604c-fde9-45cf-b79e-09b7fde71833"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("487c74cb-3502-4f1a-957a-cbcea5773702"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("48eb3b31-6632-4b4d-b36d-f61c68865c9d"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("49cb7cd3-7329-4098-9ac1-c3972ba09138"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("4dc0b8e6-4c1d-46e9-b181-5d2a31e7bdb5"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("556d9330-9ff3-46a9-913b-28543fd213e4"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("5efeace0-78ca-4616-aee0-9f08574132ce"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("6b3985bf-05dd-47e7-b894-781e28428596"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("6cf93811-c44a-4b86-86a1-18d72df7e1a0"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("8d3c4087-b34d-48f7-ba2a-b1a85f69fe95"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("95d476a0-eb0e-470d-9c57-a0ec8a2e4cd6"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("95f1f088-6931-4914-92c1-c1f1d7d75a18"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("9acecc78-79ca-41b1-960e-a4eb9cf03a2c"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("a1acbade-3835-4a9e-9729-56067af66d53"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("aa3f81ec-9a87-493f-a7d5-fa4ca6e75bf7"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("ad9b444a-67b7-4b85-b592-8578e569b12a"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("b9ad0f66-08c6-4f95-900c-94750f1ada6b"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("ba198396-d07a-4054-95d0-4fe0ba393ecd"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("bd3496e3-6e57-447e-a7df-744efff03de5"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("be526dee-9c74-44bb-af6b-1b2298fa1197"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("c68385ee-093a-457f-a03a-b1a53371c248"));

            migrationBuilder.DeleteData(
                table: "AvailableRoomPermission",
                keyColumn: "Id",
                keyValue: new Guid("d40a2c28-3a84-47f3-9981-88bdf50bb4ca"));

            migrationBuilder.DropColumn(
                name: "RoomParticipantId",
                table: "AvailableRoomPermission");

            migrationBuilder.CreateTable(
                name: "RoomParticipantPermission",
                columns: table => new
                {
                    FK_PERMISSION_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    FK_PARTICIPANT_ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomParticipantPermission", x => new { x.FK_PERMISSION_ID, x.FK_PARTICIPANT_ID });
                    table.ForeignKey(
                        name: "FK_RoomParticipantPermission_Permissions_FK_PERMISSION_ID",
                        column: x => x.FK_PERMISSION_ID,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomParticipantPermission_RoomParticipants_FK_PARTICIPANT_ID",
                        column: x => x.FK_PARTICIPANT_ID,
                        principalTable: "RoomParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipantPermission_FK_PARTICIPANT_ID",
                table: "RoomParticipantPermission",
                column: "FK_PARTICIPANT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomParticipantPermission");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomParticipantId",
                table: "AvailableRoomPermission",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AvailableRoomPermission",
                columns: new[] { "Id", "CreateDate", "CreatedById", "PermissionId", "RoomParticipantId", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("07dea11a-65a4-4826-ab4f-9d2cdfaa72f3"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b530321a-a51a-4a36-8afd-6e8a8dbae248"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("0c24de0f-6fe3-4d95-81fb-e9e7542852f7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d74df965-84d3-4bcc-af1b-13f5c6299fa7"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("209a47f7-f1c5-439c-8de5-7792c08b7ce2"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("0827aeef-bcc1-4412-b584-0de4694422ce"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("241f76f2-3746-4ee4-9191-a64ba3b3a86e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4f7a0200-9fe1-4d04-9bcc-6ed668d07828"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("25c2bc73-39ea-4288-9756-cef28ddc4534"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("2a6f981e-f79e-4497-83d0-35018cbd24d3"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("369f0b92-915c-4334-bdac-6e82fb3c0c74"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("150f05e3-8d73-45e9-8ecd-6187f7b96461"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("38cd9540-27f5-4482-a261-2a08f6d8cf30"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("eac25c4b-28d5-4e22-93b2-5c3caf0f6922"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("3b1a04f3-8d35-4608-87fb-1d83d76cd99d"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("97b2411a-b9d4-49cb-9525-0e31b7d35496"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4157604c-fde9-45cf-b79e-09b7fde71833"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a115f072-638a-4472-8cc3-4cf04da67cfc"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("487c74cb-3502-4f1a-957a-cbcea5773702"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("695914fe-a627-4959-b8b9-e0413ba63755"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("48eb3b31-6632-4b4d-b36d-f61c68865c9d"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("882ffc55-3439-4d0b-8add-ba79e2a7df45"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("49cb7cd3-7329-4098-9ac1-c3972ba09138"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7b231e25-446a-418c-9281-4eb453dd4893"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4dc0b8e6-4c1d-46e9-b181-5d2a31e7bdb5"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1f6c85db-c2a0-4096-8ead-a292397ab4e5"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("556d9330-9ff3-46a9-913b-28543fd213e4"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d1916ab5-462e-41d7-ae46-f1ce27d514d4"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("5efeace0-78ca-4616-aee0-9f08574132ce"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("1bb49aa7-1305-427c-9523-e9687392d385"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6b3985bf-05dd-47e7-b894-781e28428596"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7ad620a-0614-494a-89ca-623e47b7415a"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6cf93811-c44a-4b86-86a1-18d72df7e1a0"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a63b2ca5-304b-40a0-8e82-665a3327e407"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8d3c4087-b34d-48f7-ba2a-b1a85f69fe95"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9f020c9e-e0b4-4e6d-9fb3-38ba44cfa3f9"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("95d476a0-eb0e-470d-9c57-a0ec8a2e4cd6"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("220380d1-fd72-4004-aed4-22187e88b386"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("95f1f088-6931-4914-92c1-c1f1d7d75a18"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c1f43ca8-21f1-41e6-9794-e7d44156bf73"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("9acecc78-79ca-41b1-960e-a4eb9cf03a2c"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7df4ea9b-ded5-4a1d-a8ea-e92e6bd85269"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a1acbade-3835-4a9e-9729-56067af66d53"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4c3386da-cbb2-4493-86e8-036e8802782d"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aa3f81ec-9a87-493f-a7d5-fa4ca6e75bf7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5c4eb71-50c8-4c13-a144-0496ce56e095"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ad9b444a-67b7-4b85-b592-8578e569b12a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5ac11db0-b079-40ab-b32b-a02243a451b3"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b9ad0f66-08c6-4f95-900c-94750f1ada6b"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("9ce5949f-a7b9-489c-8b04-bd6724aff687"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ba198396-d07a-4054-95d0-4fe0ba393ecd"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("edab0e5d-7ac2-4761-b47f-a5f41a9ae48c"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bd3496e3-6e57-447e-a7df-744efff03de5"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("5f088b45-704f-4f61-b4c5-05bd08b80303"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("be526dee-9c74-44bb-af6b-1b2298fa1197"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("4f39059a-e69f-4494-9b48-54e3a6aea2f3"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c68385ee-093a-457f-a03a-b1a53371c248"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("6938365f-752d-453e-b0be-93facac0c5b8"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d40a2c28-3a84-47f3-9981-88bdf50bb4ca"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("7c4d9ac2-72e7-466a-bcff-68f3ee0bc65e"), null, new DateTime(2024, 3, 2, 15, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableRoomPermission_RoomParticipantId",
                table: "AvailableRoomPermission",
                column: "RoomParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvailableRoomPermission_RoomParticipants_RoomParticipantId",
                table: "AvailableRoomPermission",
                column: "RoomParticipantId",
                principalTable: "RoomParticipants",
                principalColumn: "Id");
        }
    }
}
