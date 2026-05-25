using CoreGearERP.Common.Domain.Entities;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Inventory.Domain.Enums;

namespace CoreGearERP.Inventory.Domain.Entities;

/// <summary>
/// Immutable record of every stock change. Never updated or deleted.
/// Every change to stock levels is represented as a new StockMovement.
/// The full stock history is the complete set of movements for a StockItem.
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid StockItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public Quantity Quantity { get; private set; } = null!;

    /// <summary>
    /// Reference to the source document. PO id, SalesOrder id etc.
    /// </summary>
    public Guid? ReferenceId { get; private set; }

    /// <summary>
    /// Human readable reference. PO number, order number etc.
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;

    public string Notes { get; private set; } = string.Empty;

    private StockMovement()
    {
    }

    /// <summary>
    /// Factory method. Creates an immutable stock movement record.
    /// </summary>
    public static StockMovement Create(
        Guid stockItemId,
        Guid productId,
        Guid warehouseId,
        StockMovementType movementType,
        Quantity quantity,
        Guid tenantId,
        Guid createdBy,
        Guid? referenceId = null,
        string referenceNumber = "",
        string notes = "")
    {
        var movement = new StockMovement
        {
            StockItemId = stockItemId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = movementType,
            Quantity = quantity,
            ReferenceId = referenceId,
            ReferenceNumber = referenceNumber.Trim(),
            Notes = notes.Trim()
        };

        movement.Status = "Posted";
        movement.SetCreated(tenantId, createdBy);

        return movement;
    }
}