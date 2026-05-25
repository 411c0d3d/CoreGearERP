namespace CoreGearERP.Sales.Domain.Enums;

/// <summary>
/// Valid status values for a SalesOrder.
/// </summary>
public enum SalesOrderStatus
{
    Draft,
    Confirmed,
    PartiallyShipped,
    Shipped,
    Cancelled
}