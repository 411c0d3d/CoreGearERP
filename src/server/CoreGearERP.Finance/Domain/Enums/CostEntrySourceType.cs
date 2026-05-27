namespace CoreGearERP.Finance.Domain.Enums;

/// <summary>
/// Identifies the source document type that originated a CostEntry.
/// </summary>
public enum CostEntrySourceType
{
    GoodsReceipt,
    ProductionOrder,
    Shipment
}