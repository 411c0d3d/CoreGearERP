namespace CoreGearERP.Production.Domain.Enums;

/// <summary>
/// Valid status values for a ProductionOrder.
/// </summary>
public enum ProductionOrderStatus
{
    Draft,
    Confirmed,
    InProgress,
    Completed,
    Cancelled
}