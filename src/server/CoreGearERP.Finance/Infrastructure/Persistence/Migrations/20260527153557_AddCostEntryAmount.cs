using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGearERP.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostEntryAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "finance",
                table: "CostEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SourceDocumentNumber",
                schema: "finance",
                table: "CostEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "finance",
                table: "CostEntries",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                schema: "finance",
                table: "CostEntries",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "finance",
                table: "CostEntries");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                schema: "finance",
                table: "CostEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "finance",
                table: "CostEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SourceDocumentNumber",
                schema: "finance",
                table: "CostEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
