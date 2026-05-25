namespace CoreGearERP.Procurement.Domain.Enums;

/// <summary>
/// Valid status values for a PurchaseOrder.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft,
    Confirmed,
    PartiallyReceived,
    Received,
    Cancelled
}