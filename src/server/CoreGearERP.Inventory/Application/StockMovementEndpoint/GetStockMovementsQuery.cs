using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.StockMovementEndpoint;

/// <summary>
/// Query to retrieve stock movement history for a stock item.
/// </summary>
public record GetStockMovementsQuery(Guid StockItemId) : IQuery<IReadOnlyList<StockMovementDto>>;

/// <summary>
/// Read model for stock movement history.
/// </summary>
public record StockMovementDto(
    Guid Id,
    string MovementType,
    decimal Quantity,
    string UnitCode,
    string? ReferenceNumber,
    string? Notes,
    DateTime CreatedAt);