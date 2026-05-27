namespace CoreGearERP.Common.Application.Events;

/// <summary>Raised by Procurement when a goods receipt is posted against a purchase order.</summary>
/// <param name="GoodsReceiptId">The Id of the goods receipt that triggered this event.</param>
/// <param name="PurchaseOrderId">The purchase order being received against.</param>
/// <param name="PurchaseOrderNumber">Human-facing PO number.</param>
/// <param name="PurchaseOrderLineId">The specific line being received.</param>
/// <param name="ProductId">Product received.</param>
/// <param name="ProductCode">Denormalized product code at time of receipt.</param>
/// <param name="QuantityReceived">Quantity received in this receipt.</param>
/// <param name="UnitCode">Unit of measure for the quantity.</param>
/// <param name="UnitPrice">Unit price locked on the PO line.</param>
/// <param name="TotalAmount">Pre-calculated total: UnitPrice * QuantityReceived.</param>
/// <param name="CurrencyCode">ISO 4217 currency code from the PO line unit price.</param>
/// <param name="TenantId">Tenant this event belongs to.</param>
/// <param name="OccurredAt">UTC timestamp when the receipt was posted.</param>
public record GoodsReceivedEvent(
    Guid GoodsReceiptId,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid PurchaseOrderLineId,
    Guid ProductId,
    string ProductCode,
    decimal QuantityReceived,
    string UnitCode,
    decimal UnitPrice,
    decimal TotalAmount,
    string CurrencyCode,
    Guid TenantId,
    DateTime OccurredAt);