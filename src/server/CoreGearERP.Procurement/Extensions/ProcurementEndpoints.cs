using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Procurement.Application.PurchaseOrders;
using CoreGearERP.Procurement.Application.Suppliers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreGearERP.Procurement.Extensions;

/// <summary>
/// Registers all Procurement module HTTP endpoints.
/// </summary>
public static class ProcurementEndpoints
{
    /// <summary>
    /// Maps all Procurement-related endpoints to the provided route builder. This includes endpoints for managing purchase orders, suppliers, and other procurement operations.
    /// </summary>
    /// <param name="app">The endpoint route builder to which the procurement endpoints will be mapped.</param>
    /// <returns>The updated endpoint route builder with procurement endpoints mapped.</returns>
    public static IEndpointRouteBuilder MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/procurement").RequireAuthorization();

        group.MapPost("/suppliers", async (CreateSupplierCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/procurement/suppliers/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/suppliers", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<SupplierDto>>(new GetSuppliersQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/orders", async (CreatePurchaseOrderCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/procurement/orders/{result.Value}", new { id = result.Value });
        });

        group.MapPut("/orders/{id}/confirm", async (Guid id, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(new ConfirmPurchaseOrderCommand(id));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        group.MapGet("/orders", async (string? status, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<PurchaseOrderDto>>(
                new GetPurchaseOrdersQuery(status));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/orders/{id}/receive", async (
            Guid id,
            ReceiveGoodsCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(
                command with { PurchaseOrderId = id });
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        group.MapGet("/orders/{id}", async (Guid id, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<PurchaseOrderDetailDto>(
                new GetPurchaseOrderByIdQuery(id));
            if (!result.IsSuccess)
            {
                return result.ErrorType == ResultErrorType.NotFound
                    ? Results.NotFound(new { error = result.Error })
                    : Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        return app;
    }
}