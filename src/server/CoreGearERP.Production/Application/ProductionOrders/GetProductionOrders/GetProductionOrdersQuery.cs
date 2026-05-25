using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.ProductionOrders.GetProductionOrders;

/// <summary>
/// Query to retrieve production orders for the current tenant.
/// </summary>
public record GetProductionOrdersQuery(string? Status = null) : IQuery<IReadOnlyList<ProductionOrderDto>>;

public record ProductionOrderDto(
    Guid Id,
    string OrderNumber,
    string FinishedProductCode,
    string FinishedProductName,
    string WorkCenterCode,
    decimal PlannedQuantity,
    decimal? ActualQuantity,
    string UnitCode,
    string Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt);