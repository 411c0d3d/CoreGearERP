using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Sales.Application.Customers.CreateCustomer;
using CoreGearERP.Sales.Application.Customers.GetCustomers;
using CoreGearERP.Sales.Application.SalesOrders.ConfirmSalesOrder;
using CoreGearERP.Sales.Application.SalesOrders.CreateSalesOrder;
using CoreGearERP.Sales.Application.SalesOrders.GetSalesOrderById;
using CoreGearERP.Sales.Application.SalesOrders.GetSalesOrders;
using CoreGearERP.Sales.Application.Shipments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreGearERP.Sales.Extensions;

/// <summary>
/// Registers all Sales module HTTP endpoints.
/// </summary>
public static class SalesEndpoints
{
    /// <summary>
    /// Maps all Sales-related endpoints to the provided route builder. This includes endpoints for managing customers and other sales operations.
    /// </summary>
    /// <param name="app">The endpoint route builder to which the sales endpoints will be mapped.</param>
    /// <returns>The updated endpoint route builder with sales endpoints mapped.</returns>
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sales").RequireAuthorization();

        group.MapPost("/customers", async (CreateCustomerCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/sales/customers/{result.Value}", new { id = result.Value });
        });

        group.MapGet("/customers", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<CustomerDto>>(new GetCustomersQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/orders", async (CreateSalesOrderCommand command, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(command);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/sales/orders/{result.Value}", new { id = result.Value });
        });

        group.MapPut("/orders/{id}/confirm", async (
            Guid id,
            ConfirmSalesOrderCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Unit>(
                command with { SalesOrderId = id });
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok();
        });

        group.MapGet("/orders", async (string? status, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<IReadOnlyList<SalesOrderDto>>(
                new GetSalesOrdersQuery(status));
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapGet("/orders/{id}", async (Guid id, IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendQuery<SalesOrderDetailDto>(
                new GetSalesOrderByIdQuery(id));
            if (!result.IsSuccess)
            {
                return result.ErrorType == ResultErrorType.NotFound
                    ? Results.NotFound(new { error = result.Error })
                    : Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/orders/{id}/ship", async (
            Guid id,
            ShipOrderCommand command,
            IDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendCommand<Guid>(
                command with { SalesOrderId = id });
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/sales/shipments/{result.Value}", new { id = result.Value });
        });

        return app;
    }
}