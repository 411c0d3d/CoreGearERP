using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.SalesOrders.GetSalesOrders;

/// <summary>
/// Query to retrieve sales orders for the current tenant.
/// </summary>
public record GetSalesOrdersQuery(string? Status = null) : IQuery<IReadOnlyList<SalesOrderDto>>;

public record SalesOrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string Status,
    int LineCount,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt);