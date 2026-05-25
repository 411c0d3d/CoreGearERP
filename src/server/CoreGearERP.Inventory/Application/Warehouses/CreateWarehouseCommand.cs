using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.Warehouses;

/// <summary>
/// Command to create a new warehouse.
/// </summary>
public record CreateWarehouseCommand(
    string Code,
    string Name,
    string Location = "") : ICommand<Guid>;