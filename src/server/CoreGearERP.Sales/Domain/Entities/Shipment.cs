using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Enums;

namespace CoreGearERP.Sales.Domain.Entities;

/// <summary>
/// Represents a physical shipment against a SalesOrder.
/// Triggers stock movement in Inventory on shipping.
/// </summary>
public class Shipment : BaseEntity
{
    public Guid SalesOrderId { get; private set; }

    public string ShipmentNumber { get; private set; } = string.Empty;

    public string Notes { get; private set; } = string.Empty;

    private readonly List<ShipmentLine> _lines = [];
    public IReadOnlyList<ShipmentLine> Lines => _lines.AsReadOnly();

    private Shipment() { }

    /// <summary>
    /// Factory method. Creates a Shipment in Pending status.
    /// </summary>
    public static Shipment Create(
        Guid salesOrderId,
        string shipmentNumber,
        string notes,
        Guid tenantId,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(shipmentNumber))
        {
            throw new DomainException("Shipment number cannot be empty.");
        }

        var shipment = new Shipment
        {
            SalesOrderId    = salesOrderId,
            ShipmentNumber  = shipmentNumber.Trim().ToUpperInvariant(),
            Notes           = notes.Trim()
        };

        shipment.Status = ShipmentStatus.Pending.ToString();
        shipment.SetCreated(tenantId, createdBy);

        return shipment;
    }

    /// <summary>
    /// Adds a line to the shipment.
    /// </summary>
    public void AddLine(ShipmentLine line, Guid modifiedBy)
    {
        _lines.Add(line);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Marks the shipment as shipped.
    /// </summary>
    public void Ship(Guid modifiedBy)
    {
        if (Status != ShipmentStatus.Pending.ToString())
        {
            throw new DomainException("Only a Pending shipment can be shipped.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("A shipment must have at least one line before shipping.");
        }

        Status      = ShipmentStatus.Shipped.ToString();
        CompletedAt = DateTime.UtcNow;
        SetModified(modifiedBy);
    }
}