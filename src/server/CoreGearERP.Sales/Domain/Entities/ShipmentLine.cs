using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;

namespace CoreGearERP.Sales.Domain.Entities;

/// <summary>
/// Represents a single line item in a Shipment.
/// </summary>
public class ShipmentLine : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public Guid SalesOrderLineId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public Quantity QuantityShipped { get; private set; } = null!;

    private ShipmentLine() { }

    /// <summary>
    /// Factory method. Creates a shipment line.
    /// </summary>
    public static ShipmentLine Create(
        Guid shipmentId,
        Guid salesOrderLineId,
        Guid productId,
        string productCode,
        Quantity quantityShipped,
        Guid tenantId,
        Guid createdBy)
    {
        if (quantityShipped.Value <= 0)
        {
            throw new DomainException("Shipment line quantity must be greater than zero.");
        }

        var line = new ShipmentLine
        {
            ShipmentId       = shipmentId,
            SalesOrderLineId = salesOrderLineId,
            ProductId        = productId,
            ProductCode      = productCode.Trim().ToUpperInvariant(),
            QuantityShipped  = quantityShipped
        };

        line.Status = "Active";
        line.SetCreated(tenantId, createdBy);

        return line;
    }
}