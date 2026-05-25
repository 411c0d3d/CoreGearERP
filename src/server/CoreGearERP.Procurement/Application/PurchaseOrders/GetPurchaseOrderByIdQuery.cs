using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Query to retrieve a single purchase order with its lines by Id.
/// </summary>
public record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId) : IQuery<PurchaseOrderDetailDto>;

public record PurchaseOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string SupplierName,
    string Status,
    string Notes,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public record PurchaseOrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    string UnitCode,
    decimal UnitPrice,
    string CurrencyCode,
    string Status);