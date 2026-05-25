using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Application.BillsOfMaterials;
using CoreGearERP.Production.Application.BillsOfMaterials.CreateBillOfMaterials;
using CoreGearERP.Production.Application.BillsOfMaterials.GetBillsOfMaterials;
using CoreGearERP.Production.Application.ProductionOrders;
using CoreGearERP.Production.Application.ProductionOrders.CompleteProductionOrder;
using CoreGearERP.Production.Application.ProductionOrders.ConfirmProductionOrder;
using CoreGearERP.Production.Application.ProductionOrders.CreateProductionOrder;
using CoreGearERP.Production.Application.ProductionOrders.GetProductionOrders;
using CoreGearERP.Production.Application.ProductionOrders.StartProductionOrder;
using CoreGearERP.Production.Application.WorkCenters;
using CoreGearERP.Production.Application.WorkCenters.CreateWorkCenter;
using CoreGearERP.Production.Application.WorkCenters.GetWorkCenters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreGearERP.Production.Extensions;

/// <summary>
/// Registers all Production module HTTP endpoints.
/// </summary>
public static class ProductionEndpoints
{
    /// <summary>
    /// Defines all HTTP endpoints for the Production module, grouped under /production and protected by authorization.
    /// </summary>
    /// <param name="app">The endpoint route builder to add endpoints to.</param>
    /// <returns>The modified endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/production").RequireAuthorization();

        group.MapPost("/work-centers", async (CreateWorkCenterCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/production/work-centers/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/work-centers", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<WorkCenterDto>>(new GetWorkCentersQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/bills-of-materials", async (
            CreateBillOfMaterialsCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/production/bills-of-materials/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/bills-of-materials", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<BillOfMaterialsDto>>(
                new GetBillsOfMaterialsQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/orders", async (CreateProductionOrderCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/production/orders/{result.Value}", new { id = result.Value });
        });

        group.MapPut("/orders/{id}/confirm", async (
            Guid id,
            ConfirmProductionOrderCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(
                command with { ProductionOrderId = id });
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        group.MapGet("/orders", async (string? status, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<ProductionOrderDto>>(
                new GetProductionOrdersQuery(status));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPut("/orders/{id}/start", async (Guid id, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(new StartProductionOrderCommand(id));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        group.MapPut("/orders/{id}/complete", async (
            Guid id,
            CompleteProductionOrderCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(
                command with { ProductionOrderId = id });
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        return app;
    }
}