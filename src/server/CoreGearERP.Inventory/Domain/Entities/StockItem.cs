using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;

namespace CoreGearERP.Inventory.Domain.Entities;

/// <summary>
/// Represents the current stock level of a Product in a Warehouse.
/// Stock is never modified directly -- StockMovements drive all changes.
/// </summary>
public class StockItem : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }

    /// <summary>
    /// Current quantity on hand. Updated by applying StockMovements.
    /// </summary>
    public Quantity QuantityOnHand { get; private set; } = null!;

    /// <summary>
    /// Reserved quantity against open sales orders.
    /// </summary>
    public Quantity QuantityReserved { get; private set; } = null!;

    /// <summary>
    /// Available quantity. QuantityOnHand minus QuantityReserved.
    /// </summary>
    public Quantity QuantityAvailable =>
        new(QuantityOnHand.Value - QuantityReserved.Value, QuantityOnHand.UnitCode);

    private StockItem() { }

    /// <summary>
    /// Factory method. Creates a StockItem with zero initial quantities.
    /// </summary>
    public static StockItem Create(
        Guid productId,
        Guid warehouseId,
        string unitCode,
        Guid tenantId,
        Guid createdBy)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId cannot be empty.");
        }

        if (warehouseId == Guid.Empty)
        {
            throw new DomainException("WarehouseId cannot be empty.");
        }

        var item = new StockItem
        {
            ProductId         = productId,
            WarehouseId       = warehouseId,
            QuantityOnHand    = Quantity.Zero(unitCode),
            QuantityReserved  = Quantity.Zero(unitCode)
        };

        item.Status = "Active";
        item.SetCreated(tenantId, createdBy);

        return item;
    }

    /// <summary>
    /// Increases stock on hand. Called when goods are received.
    /// </summary>
    public void AddStock(Quantity quantity, Guid modifiedBy)
    {
        QuantityOnHand = QuantityOnHand.Add(quantity);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Decreases stock on hand. Called when goods are consumed or shipped.
    /// </summary>
    public void RemoveStock(Quantity quantity, Guid modifiedBy)
    {
        if (!QuantityOnHand.IsSufficientFor(quantity))
        {
            throw new DomainException(
                $"Insufficient stock. Available: {QuantityOnHand}, Requested: {quantity}.");
        }

        QuantityOnHand = QuantityOnHand.Subtract(quantity);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Reserves quantity against an open sales order.
    /// </summary>
    public void Reserve(Quantity quantity, Guid modifiedBy)
    {
        if (!QuantityAvailable.IsSufficientFor(quantity))
        {
            throw new DomainException(
                $"Insufficient available stock to reserve. Available: {QuantityAvailable}, Requested: {quantity}.");
        }

        QuantityReserved = QuantityReserved.Add(quantity);
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Releases a previously held reservation.
    /// </summary>
    public void ReleaseReservation(Quantity quantity, Guid modifiedBy)
    {
        QuantityReserved = QuantityReserved.Subtract(quantity);
        SetModified(modifiedBy);
    }
}