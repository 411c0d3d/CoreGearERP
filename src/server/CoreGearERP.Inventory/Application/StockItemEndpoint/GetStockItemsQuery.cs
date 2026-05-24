using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.StockItemEndpoint;

/// <summary>
/// Query to retrieve stock levels for the current tenant.
/// </summary>
public record GetStockItemsQuery(Guid? WarehouseId = null) : IQuery<IReadOnlyList<StockItemDto>>;

/// <summary>
/// Read model for stock level display.
/// </summary>
public record StockItemDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    string UnitCode);