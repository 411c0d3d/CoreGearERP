using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Command to create a new purchase order with line items in Draft status.
/// </summary>
public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    string Notes,
    IReadOnlyList<CreatePurchaseOrderLineDto> Lines) : ICommand<Guid>;

/// <summary>
/// Line item input for purchase order creation.
/// </summary>
public record CreatePurchaseOrderLineDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal Quantity,
    string UnitCode,
    decimal UnitPrice,
    string CurrencyCode);