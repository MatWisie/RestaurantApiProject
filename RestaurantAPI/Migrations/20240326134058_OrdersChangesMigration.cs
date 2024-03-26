using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAPI.Migrations
{
    /// <inheritdoc />
    public partial class OrdersChangesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Dishes_DishModelId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DishModelId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DishModelId",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "DishModelOrderModel",
                columns: table => new
                {
                    DishModelsId = table.Column<int>(type: "int", nullable: false),
                    OrdersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishModelOrderModel", x => new { x.DishModelsId, x.OrdersId });
                    table.ForeignKey(
                        name: "FK_DishModelOrderModel_Dishes_DishModelsId",
                        column: x => x.DishModelsId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishModelOrderModel_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishModelOrderModel_OrdersId",
                table: "DishModelOrderModel",
                column: "OrdersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishModelOrderModel");

            migrationBuilder.AddColumn<int>(
                name: "DishModelId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DishModelId",
                table: "Orders",
                column: "DishModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Dishes_DishModelId",
                table: "Orders",
                column: "DishModelId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
