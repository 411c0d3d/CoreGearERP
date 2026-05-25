using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Enums;

namespace CoreGearERP.Sales.Domain.Entities;

/// <summary>
/// Represents a sales order raised against a customer.
/// Stock is reserved on confirmation and released/consumed on shipment.
/// </summary>
public class SalesOrder : BaseEntity
{
    /// <summary>
    /// Human-facing order number. Unique per tenant.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public string Notes { get; private set; } = string.Empty;

    private readonly List<SalesOrderLine> _lines = [];
    public IReadOnlyList<SalesOrderLine> Lines => _lines.AsReadOnly();

    private SalesOrder() { }

    /// <summary>
    /// Factory method. Creates a SalesOrder in Draft status.
    /// </summary>
    public static SalesOrder Create(
        string orderNumber,
        Guid customerId,
        string customerName,
        string notes,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number cannot be empty.");
        }

        var order = new SalesOrder
        {
            OrderNumber  = orderNumber.Trim().ToUpperInvariant(),
            CustomerId   = customerId,
            CustomerName = customerName.Trim(),
            Notes        = notes.Trim()
        };

        order.Status = SalesOrderStatus.Draft.ToString();
        order.SetCreated(tenantId, createdBy);

        return order;
    }

    /// <summary>
    /// Adds a line item to the order. Only allowed in Draft status.
    /// </summary>
    public void AddLine(SalesOrderLine line, Guid modifiedBy)
    {
        if (Status != SalesOrderStatus.Draft.ToString())
        {
            throw new DomainException("Lines can only be added to a Draft sales order.");
        }

        _lines.Add(line);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Confirms the order. Stock reservations must be made before calling this.
    /// </summary>
    public void Confirm(Guid modifiedBy)
    {
        if (Status != SalesOrderStatus.Draft.ToString())
        {
            throw new DomainException("Only a Draft sales order can be confirmed.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("A sales order must have at least one line item before confirming.");
        }

        Status      = SalesOrderStatus.Confirmed.ToString();
        ConfirmedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Updates order status after a shipment.
    /// </summary>
    public void UpdateShipmentStatus(Guid modifiedBy)
    {
        if (Status != SalesOrderStatus.Confirmed.ToString() &&
            Status != SalesOrderStatus.PartiallyShipped.ToString())
        {
            throw new DomainException("Cannot update shipment status on this order.");
        }

        var allShipped = _lines.All(l => l.Status == SalesOrderLineStatus.Shipped.ToString());

        Status = allShipped
            ? SalesOrderStatus.Shipped.ToString()
            : SalesOrderStatus.PartiallyShipped.ToString();

        if (allShipped)
        {
            CompletedAt = DateTime.UtcNow;
        }

        SetModified(modifiedBy);
    }

    /// <summary>
    /// Cancels the order and releases all stock reservations.
    /// </summary>
    public void Cancel(Guid modifiedBy)
    {
        if (Status == SalesOrderStatus.Shipped.ToString())
        {
            throw new DomainException("Cannot cancel a fully shipped sales order.");
        }

        if (Status == SalesOrderStatus.Cancelled.ToString())
        {
            throw new DomainException("Sales order is already cancelled.");
        }

        Status      = SalesOrderStatus.Cancelled.ToString();
        CancelledAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }
}