using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.StockItems;

/// <summary>
/// Command to create a StockItem linking a Product to a Warehouse.
/// </summary>
public record CreateStockItemCommand(
    Guid ProductId,
    Guid WarehouseId,
    string UnitCode) : ICommand<Guid>;