using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Queries;

/// <summary>
/// Handles GetWarehousesQuery. Returns all active warehouses for the current tenant.
/// </summary>
public class GetWarehousesQueryHandler : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseDto>>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="context">InventoryDbContext is used to query the Warehouses table.</param>
    /// <param name="currentTenant">ICurrentTenant is used to filter warehouses by the current tenant.</param>
    public GetWarehousesQueryHandler(InventoryDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<WarehouseDto>> Handle(
        GetWarehousesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .Where(w => w.TenantId == _currentTenant.TenantId)
            .Select(w => new WarehouseDto(
                w.Id,
                w.Code,
                w.Name,
                w.Location,
                w.Status))
            .ToListAsync(cancellationToken);
    }
}