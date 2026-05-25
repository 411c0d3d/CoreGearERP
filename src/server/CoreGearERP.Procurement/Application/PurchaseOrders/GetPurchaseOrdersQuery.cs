using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Query to retrieve purchase orders for the current tenant.
/// </summary>
public record GetPurchaseOrdersQuery(string? Status = null) : IQuery<IReadOnlyList<PurchaseOrderDto>>;

public record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    string SupplierName,
    string Status,
    int LineCount,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt);