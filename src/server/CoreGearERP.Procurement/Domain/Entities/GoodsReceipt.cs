using CoreGearERP.Common.Domain.Entities;

namespace CoreGearERP.Procurement.Domain.Entities;

/// <summary>
/// Represents a physical goods receipt against a PurchaseOrder.
/// Immutable once created. Each receipt -- including partial -- is a distinct document.
/// Triggers a StockMovement in Inventory on creation.
/// </summary>
public class GoodsReceipt : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }

    public string PurchaseOrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Warehouse where stock physically landed.
    /// </summary>
    public Guid WarehouseId { get; private set; }

    /// <summary>
    /// UTC timestamp when the receipt was posted.
    /// </summary>
    public DateTime ReceivedAt { get; private set; }

    private readonly List<GoodsReceiptLine> _lines = [];
    public IReadOnlyList<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    private GoodsReceipt() { }

    /// <summary>
    /// Factory method. Creates a GoodsReceipt in Posted status.
    /// </summary>
    public static GoodsReceipt Create(
        Guid purchaseOrderId,
        string purchaseOrderNumber,
        Guid warehouseId,
        Guid tenantId,
        Guid createdBy)
    {
        var receipt = new GoodsReceipt
        {
            PurchaseOrderId     = purchaseOrderId,
            PurchaseOrderNumber = purchaseOrderNumber.Trim().ToUpperInvariant(),
            WarehouseId         = warehouseId,
            ReceivedAt          = DateTime.UtcNow
        };

        receipt.Status = "Posted";
        receipt.SetCreated(tenantId, createdBy);

        return receipt;
    }

    /// <summary>
    /// Adds a receipt line. Only called during construction -- receipt is immutable after save.
    /// </summary>
    public void AddLine(GoodsReceiptLine line)
    {
        _lines.Add(line);
    }
}