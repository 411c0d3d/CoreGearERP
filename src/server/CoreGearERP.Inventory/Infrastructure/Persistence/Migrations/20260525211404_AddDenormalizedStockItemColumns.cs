using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDenormalizedStockItemColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WarehouseCode",
                schema: "inventory",
                table: "stock_items",
                newName: "warehouse_code");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                schema: "inventory",
                table: "stock_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                schema: "inventory",
                table: "stock_items",
                newName: "product_code");

            // Backfill any rows that were created before denormalization was in place.
            migrationBuilder.Sql(@"
        UPDATE inventory.stock_items si
        SET product_code   = p.code,
            product_name   = p.name,
            warehouse_code = w.code
        FROM inventory.products p,
             inventory.warehouses w
        WHERE si.product_id   = p.id
          AND si.warehouse_id = w.id
          AND (si.product_code = '' OR si.warehouse_code = '');
    ");

            migrationBuilder.AlterColumn<string>(
                name: "warehouse_code",
                schema: "inventory",
                table: "stock_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "product_name",
                schema: "inventory",
                table: "stock_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "product_code",
                schema: "inventory",
                table: "stock_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "warehouse_code",
                schema: "inventory",
                table: "stock_items",
                newName: "WarehouseCode");

            migrationBuilder.RenameColumn(
                name: "product_name",
                schema: "inventory",
                table: "stock_items",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_code",
                schema: "inventory",
                table: "stock_items",
                newName: "ProductCode");

            migrationBuilder.AlterColumn<string>(
                name: "WarehouseCode",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                schema: "inventory",
                table: "stock_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
