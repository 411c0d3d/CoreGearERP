namespace CoreGearERP.Inventory.Domain.Enums;

/// <summary>
/// Defines the direction and reason for a stock movement.
/// </summary>
public enum StockMovementType
{
    /// <summary>
    /// Stock in from a purchase order.
    /// </summary>
    GoodsReceipt,

    /// <summary>
    /// Stock out to production.
    /// </summary>
    GoodsIssue,

    /// <summary>
    /// Stock out to a customer.
    /// </summary>
    SalesShipment,

    /// <summary>
    /// Stock in from completed production order.
    /// </summary>
    ProductionReceipt,

    /// <summary>
    /// Manual stock correction.
    /// </summary>
    Adjustment,

    /// <summary>
    /// Stock returned from a customer.
    /// </summary>
    Return
}