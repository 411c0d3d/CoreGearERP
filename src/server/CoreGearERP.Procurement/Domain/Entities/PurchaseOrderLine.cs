using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Procurement.Domain.Enums;

namespace CoreGearERP.Procurement.Domain.Entities;

/// <summary>
/// Represents a single line item on a PurchaseOrder.
/// References a Product by Id only -- no navigation property across module boundary.
/// Unit price is locked at creation and never changes.
/// </summary>
public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }

    /// <summary>
    /// Product reference by Id. Cross-module -- no navigation property.
    /// </summary>
    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public Quantity QuantityOrdered { get; private set; } = null!;

    public Quantity QuantityReceived { get; private set; } = null!;

    /// <summary>
    /// Unit price locked at PO creation. Supplier price changes do not affect open orders.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    private PurchaseOrderLine() { }

    /// <summary>
    /// Factory method. Creates a PO line with locked unit price.
    /// </summary>
    public static PurchaseOrderLine Create(
        Guid purchaseOrderId,
        Guid productId,
        string productCode,
        string productName,
        Quantity quantityOrdered,
        Money unitPrice,
        Guid tenantId,
        Guid createdBy)
    {
        if (quantityOrdered.Value <= 0)
        {
            throw new DomainException("Purchase order line quantity must be greater than zero.");
        }

        var line = new PurchaseOrderLine
        {
            PurchaseOrderId  = purchaseOrderId,
            ProductId        = productId,
            ProductCode      = productCode.Trim().ToUpperInvariant(),
            ProductName      = productName.Trim(),
            QuantityOrdered  = quantityOrdered,
            QuantityReceived = Quantity.Zero(quantityOrdered.UnitCode),
            UnitPrice        = unitPrice
        };

        line.Status = PurchaseOrderLineStatus.Open.ToString();
        line.SetCreated(tenantId, createdBy);

        return line;
    }

    /// <summary>
    /// Records a goods receipt against this line. Updates received quantity and status.
    /// </summary>
    public void Receive(Quantity quantity, Guid modifiedBy)
    {
        var remaining = QuantityOrdered.Subtract(QuantityReceived);

        if (!remaining.IsSufficientFor(quantity))
        {
            throw new DomainException(
                $"Receipt quantity {quantity} exceeds remaining quantity {remaining} on line.");
        }

        QuantityReceived = QuantityReceived.Add(quantity);

        Status = QuantityReceived.Value >= QuantityOrdered.Value
            ? PurchaseOrderLineStatus.Received.ToString()
            : PurchaseOrderLineStatus.PartiallyReceived.ToString();

        SetModified(modifiedBy);
    }

    public Money LineTotal => UnitPrice.Multiply(QuantityOrdered.Value);
}