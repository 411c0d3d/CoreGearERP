using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillOfMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bills_of_materials",
                schema: "production",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    finished_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_bills_of_materials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_of_materials_lines",
                schema: "production",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_of_materials_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    component_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    qty_required = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_bill_of_materials_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_bill_of_materials_lines_bills_of_materials_bill_of_material~",
                        column: x => x.bill_of_materials_id,
                        principalSchema: "production",
                        principalTable: "bills_of_materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bill_of_materials_lines_bom_id",
                schema: "production",
                table: "bill_of_materials_lines",
                column: "bill_of_materials_id");

            migrationBuilder.CreateIndex(
                name: "ix_bill_of_materials_lines_tenant_id",
                schema: "production",
                table: "bill_of_materials_lines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_bills_of_materials_tenant_id",
                schema: "production",
                table: "bills_of_materials",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_bills_of_materials_tenant_product_version",
                schema: "production",
                table: "bills_of_materials",
                columns: new[] { "tenant_id", "finished_product_id", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_of_materials_lines",
                schema: "production");

            migrationBuilder.DropTable(
                name: "bills_of_materials",
                schema: "production");
        }
    }
}
