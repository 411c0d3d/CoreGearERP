using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.SalesOrders.GetSalesOrderById;

/// <summary>
/// Query to retrieve a single sales order with its lines by Id.
/// </summary>
public record GetSalesOrderByIdQuery(Guid SalesOrderId) : IQuery<SalesOrderDetailDto>;

public record SalesOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string Status,
    string Notes,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt,
    IReadOnlyList<SalesOrderLineDto> Lines);

public record SalesOrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal QuantityOrdered,
    decimal QuantityShipped,
    string UnitCode,
    decimal UnitPrice,
    string CurrencyCode,
    string Status);