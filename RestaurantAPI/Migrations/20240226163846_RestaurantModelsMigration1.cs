using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAPI.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantModelsMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_IdentityUserId1",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId1",
                table: "Reservations",
                newName: "IdentityUserModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_IdentityUserId1",
                table: "Reservations",
                newName: "IX_Reservations_IdentityUserModelId");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId1",
                table: "Orders",
                newName: "IdentityUserModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_IdentityUserId1",
                table: "Orders",
                newName: "IX_Orders_IdentityUserModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserModelId",
                table: "Orders",
                column: "IdentityUserModelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_IdentityUserModelId",
                table: "Reservations",
                column: "IdentityUserModelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserModelId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_IdentityUserModelId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "IdentityUserModelId",
                table: "Reservations",
                newName: "IdentityUserId1");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_IdentityUserModelId",
                table: "Reservations",
                newName: "IX_Reservations_IdentityUserId1");

            migrationBuilder.RenameColumn(
                name: "IdentityUserModelId",
                table: "Orders",
                newName: "IdentityUserId1");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_IdentityUserModelId",
                table: "Orders",
                newName: "IX_Orders_IdentityUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId1",
                table: "Orders",
                column: "IdentityUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_IdentityUserId1",
                table: "Reservations",
                column: "IdentityUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
