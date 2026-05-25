using CoreGearERP.Inventory.Infrastructure.Persistence;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Test helper endpoints for E2E test setup.
/// Only available in Development environment.
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Adds test endpoints for resetting the database to a clean state.
    /// </summary>
    /// <param name="app">The WebApplication to add the endpoints to.</param>
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapDelete("/test/reset", async (
            InventoryDbContext inventoryContext,
            ProcurementDbContext procurementContext) =>
        {
            await procurementContext.Database
                .ExecuteSqlRawAsync(
                    "TRUNCATE procurement.purchase_order_lines, procurement.purchase_orders, procurement.suppliers RESTART IDENTITY CASCADE;");

            await inventoryContext.Database
                .ExecuteSqlRawAsync(
                    "TRUNCATE inventory.stock_movements, inventory.stock_items, inventory.warehouses, inventory.products RESTART IDENTITY CASCADE;");

            return Results.Ok(new { message = "Test data cleared." });
        });

        return app;
    }
}