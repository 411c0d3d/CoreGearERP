using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Procurement.Domain.Enums;

namespace CoreGearERP.Procurement.Domain.Entities;

/// <summary>
/// Represents a purchase order raised against a supplier.
/// Owns PurchaseOrderLines. Status progresses through the receipt flow.
/// </summary>
public class PurchaseOrder : BaseEntity
{
    /// <summary>
    /// Human-facing PO number. Unique per tenant.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid SupplierId { get; private set; }

    public string SupplierName { get; private set; } = string.Empty;

    public string Notes { get; private set; } = string.Empty;

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyList<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
    }

    /// <summary>
    /// Factory method. Creates a PO in Draft status.
    /// </summary>
    public static PurchaseOrder Create(
        string orderNumber,
        Guid supplierId,
        string supplierName,
        string notes,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number cannot be empty.");
        }

        var order = new PurchaseOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            SupplierId = supplierId,
            SupplierName = supplierName.Trim(),
            Notes = notes.Trim()
        };

        order.Status = PurchaseOrderStatus.Draft.ToString();
        order.SetCreated(tenantId, createdBy);

        return order;
    }

    /// <summary>
    /// Adds a line item to the PO. Only allowed in Draft status.
    /// </summary>
    public void AddLine(PurchaseOrderLine line, Guid modifiedBy)
    {
        if (Status != PurchaseOrderStatus.Draft.ToString())
        {
            throw new DomainException("Lines can only be added to a Draft purchase order.");
        }

        _lines.Add(line);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Confirms the PO. Requires at least one line item.
    /// </summary>
    public void Confirm(Guid modifiedBy)
    {
        if (Status != PurchaseOrderStatus.Draft.ToString())
        {
            throw new DomainException("Only a Draft purchase order can be confirmed.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("A purchase order must have at least one line item before confirming.");
        }

        Status = PurchaseOrderStatus.Confirmed.ToString();
        ConfirmedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Updates PO status after a goods receipt.
    /// Called after one or more lines have been received.
    /// </summary>
    public void UpdateReceiptStatus(Guid modifiedBy)
    {
        if (Status != PurchaseOrderStatus.Confirmed.ToString() &&
            Status != PurchaseOrderStatus.PartiallyReceived.ToString())
        {
            throw new DomainException(
                "Cannot update receipt status on a PO that is not Confirmed or PartiallyReceived.");
        }

        var allReceived = _lines.All(l => l.Status == PurchaseOrderLineStatus.Received.ToString());

        Status = allReceived
            ? PurchaseOrderStatus.Received.ToString()
            : PurchaseOrderStatus.PartiallyReceived.ToString();

        if (allReceived)
        {
            CompletedAt = DateTime.UtcNow;
        }

        SetModified(modifiedBy);
    }

    /// <summary>
    /// Cancels the PO. Cannot cancel if any goods have been received.
    /// </summary>
    public void Cancel(Guid modifiedBy)
    {
        if (Status == PurchaseOrderStatus.PartiallyReceived.ToString() ||
            Status == PurchaseOrderStatus.Received.ToString())
        {
            throw new DomainException("Cannot cancel a purchase order that has received goods.");
        }

        if (Status == PurchaseOrderStatus.Cancelled.ToString())
        {
            throw new DomainException("Purchase order is already cancelled.");
        }

        Status = PurchaseOrderStatus.Cancelled.ToString();
        CancelledAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }
}