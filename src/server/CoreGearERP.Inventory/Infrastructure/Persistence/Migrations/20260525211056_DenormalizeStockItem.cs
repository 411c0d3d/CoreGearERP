using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DenormalizeStockItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCode",
                schema: "inventory",
                table: "stock_items");

            migrationBuilder.DropColumn(
                name: "ProductName",
                schema: "inventory",
                table: "stock_items");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                schema: "inventory",
                table: "stock_items");
        }
    }
}
