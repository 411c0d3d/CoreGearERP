using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.Warehouses.GetWarehouses;

/// <summary>
/// Query to retrieve all active warehouses for the current tenant.
/// </summary>
public record GetWarehousesQuery : IQuery<IReadOnlyList<WarehouseDto>>;

/// <summary>
/// Read model returned to callers.
/// </summary>
public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Location,
    string Status);