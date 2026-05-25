using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_orders",
                schema: "production",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bill_of_materials_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    finished_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    work_center_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_center_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    planned_qty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    planned_unit_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    actual_qty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    actual_unit_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_production_orders_bom_id",
                schema: "production",
                table: "production_orders",
                column: "bill_of_materials_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_orders_tenant_id",
                schema: "production",
                table: "production_orders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_orders_tenant_order_number",
                schema: "production",
                table: "production_orders",
                columns: new[] { "tenant_id", "order_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "production_orders",
                schema: "production");
        }
    }
}
