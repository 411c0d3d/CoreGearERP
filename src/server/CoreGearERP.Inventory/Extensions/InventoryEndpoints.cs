using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Application.Products;
using CoreGearERP.Inventory.Application.StockItems;
using CoreGearERP.Inventory.Application.StockMovements;
using CoreGearERP.Inventory.Application.Warehouses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreGearERP.Inventory.Extensions;

/// <summary>
/// Registers all Inventory module HTTP endpoints.
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>
    /// Maps all Inventory-related endpoints to the provided route builder. This includes endpoints for managing products, stock levels, and other inventory operations.
    /// </summary>
    /// <param name="app">The endpoint route builder to which the inventory endpoints will be mapped.</param>
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/inventory").RequireAuthorization();

        group.MapPost("/products", async (CreateProductCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/inventory/products/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/products", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<ProductDto>>(new GetProductsQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/warehouses", async (CreateWarehouseCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/inventory/warehouses/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/warehouses", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<WarehouseDto>>(new GetWarehousesQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/stock-items", async (CreateStockItemCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/inventory/stock-items/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/stock-items", async (Guid? warehouseId, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<StockItemDto>>(
                new GetStockItemsQuery(warehouseId));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapGet("/stock-items/{stockItemId}/movements", async (
            Guid stockItemId,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<StockMovementDto>>(
                new GetStockMovementsQuery(stockItemId));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        return app;
    }
}