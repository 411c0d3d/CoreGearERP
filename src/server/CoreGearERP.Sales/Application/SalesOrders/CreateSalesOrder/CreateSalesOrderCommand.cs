using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.SalesOrders.CreateSalesOrder;

/// <summary>
/// Command to create a new sales order with line items in Draft status.
/// </summary>
public record CreateSalesOrderCommand(
    Guid CustomerId,
    string Notes,
    IReadOnlyList<CreateSalesOrderLineDto> Lines) : ICommand<Guid>;

/// <summary>
/// Line item input for sales order creation.
/// </summary>
public record CreateSalesOrderLineDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal Quantity,
    string UnitCode,
    decimal UnitPrice,
    string CurrencyCode);