using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Sales.Domain.Enums;

namespace CoreGearERP.Sales.Domain.Entities;

/// <summary>
/// Represents a single line item on a SalesOrder.
/// References a Product by Id only -- no navigation property across module boundary.
/// Unit price is locked at creation.
/// </summary>
public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; private set; }

    /// <summary>
    /// Product reference by Id. Cross-module -- no navigation property.
    /// </summary>
    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public Quantity QuantityOrdered { get; private set; } = null!;

    public Quantity QuantityShipped { get; private set; } = null!;

    /// <summary>
    /// Unit price locked at order creation.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    private SalesOrderLine() { }

    /// <summary>
    /// Factory method. Creates a sales order line with locked unit price.
    /// </summary>
    public static SalesOrderLine Create(
        Guid salesOrderId,
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
            throw new DomainException("Sales order line quantity must be greater than zero.");
        }

        var line = new SalesOrderLine
        {
            SalesOrderId     = salesOrderId,
            ProductId        = productId,
            ProductCode      = productCode.Trim().ToUpperInvariant(),
            ProductName      = productName.Trim(),
            QuantityOrdered  = quantityOrdered,
            QuantityShipped  = Quantity.Zero(quantityOrdered.UnitCode),
            UnitPrice        = unitPrice
        };

        line.Status = SalesOrderLineStatus.Open.ToString();
        line.SetCreated(tenantId, createdBy);

        return line;
    }

    /// <summary>
    /// Records a shipment against this line.
    /// </summary>
    public void Ship(Quantity quantity, Guid modifiedBy)
    {
        var remaining = QuantityOrdered.Subtract(QuantityShipped);

        if (!remaining.IsSufficientFor(quantity))
        {
            throw new DomainException(
                $"Shipment quantity {quantity} exceeds remaining quantity {remaining} on line.");
        }

        QuantityShipped = QuantityShipped.Add(quantity);

        Status = QuantityShipped.Value >= QuantityOrdered.Value
            ? SalesOrderLineStatus.Shipped.ToString()
            : SalesOrderLineStatus.PartiallyShipped.ToString();

        SetModified(modifiedBy);
    }

    public Money LineTotal => UnitPrice.Multiply(QuantityOrdered.Value);
}