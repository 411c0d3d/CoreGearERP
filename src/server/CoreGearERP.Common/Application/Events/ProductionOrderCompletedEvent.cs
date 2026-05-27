namespace CoreGearERP.Common.Application.Events;

/// <summary>Raised by Production when a production order is completed.</summary>
/// <param name="ProductionOrderId">The completed production order Id.</param>
/// <param name="ProductionOrderNumber">Human-facing order number.</param>
/// <param name="FinishedProductId">Product that was produced.</param>
/// <param name="FinishedProductCode">Denormalized product code at time of completion.</param>
/// <param name="PlannedQuantity">Originally planned production quantity.</param>
/// <param name="ActualQuantity">Actual quantity produced.</param>
/// <param name="UnitCode">Unit of measure for the quantities.</param>
/// <param name="TenantId">Tenant this event belongs to.</param>
/// <param name="OccurredAt">UTC timestamp when the order was completed.</param>
/// <remarks>
/// Production orders carry no monetary data -- component prices live in Procurement.
/// Finance creates a cost entry with Amount = 0 pending a costing reconciliation step.
/// </remarks>
public record ProductionOrderCompletedEvent(
    Guid ProductionOrderId,
    string ProductionOrderNumber,
    Guid FinishedProductId,
    string FinishedProductCode,
    decimal PlannedQuantity,
    decimal ActualQuantity,
    string UnitCode,
    Guid TenantId,
    DateTime OccurredAt);