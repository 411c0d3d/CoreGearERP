using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shipments",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipment_lines",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qty_shipped = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unit_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_shipment_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipment_lines_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalSchema: "sales",
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_shipment_id",
                schema: "sales",
                table: "shipment_lines",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_tenant_id",
                schema: "sales",
                table: "shipment_lines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_sales_order_id",
                schema: "sales",
                table: "shipments",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tenant_id",
                schema: "sales",
                table: "shipments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tenant_shipment_number",
                schema: "sales",
                table: "shipments",
                columns: new[] { "tenant_id", "shipment_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipment_lines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "shipments",
                schema: "sales");
        }
    }
}
