using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.ValueObjects;

namespace CoreGearERP.Procurement.Domain.Entities;

/// <summary>
/// Represents a single line on a GoodsReceipt. Immutable once created.
/// </summary>
public class GoodsReceiptLine : BaseEntity
{
    public Guid GoodsReceiptId { get; private set; }

    public Guid PurchaseOrderLineId { get; private set; }

    /// <summary>
    /// Product reference by Id. Cross-module -- no navigation property.
    /// </summary>
    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public Quantity QuantityReceived { get; private set; } = null!;

    /// <summary>
    /// Unit price copied from the PO line at receipt time. Immutable snapshot.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    private GoodsReceiptLine()
    {
    }

    /// <summary>
    /// Factory method. Creates an immutable receipt line snapshot.
    /// </summary>
    public static GoodsReceiptLine Create(
        Guid goodsReceiptId,
        Guid purchaseOrderLineId,
        Guid productId,
        string productCode,
        string productName,
        Quantity quantityReceived,
        Money unitPrice,
        Guid tenantId,
        Guid createdBy)
    {
        var line = new GoodsReceiptLine
        {
            GoodsReceiptId = goodsReceiptId,
            PurchaseOrderLineId = purchaseOrderLineId,
            ProductId = productId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            QuantityReceived = quantityReceived,
            UnitPrice = unitPrice
        };

        line.SetCreated(tenantId, createdBy);

        return line;
    }

    public Money LineTotal => UnitPrice.Multiply(QuantityReceived.Value);
}