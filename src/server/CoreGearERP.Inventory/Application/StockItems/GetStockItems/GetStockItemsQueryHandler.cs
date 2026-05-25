using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.StockItems.GetStockItems;

/// <summary>
/// Handles GetStockItemsQuery. Returns stock levels with product and warehouse details.
/// </summary>
public class GetStockItemsQueryHandler : IQueryHandler<GetStockItemsQuery, IReadOnlyList<StockItemDto>>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    /// <param name="context">InventoryDbContext is used to query the StockItems, Products, and Warehouses tables.</param>
    /// <param name="currentTenant">ICurrentTenant is used to filter stock items by the current tenant.</param>
    public GetStockItemsQueryHandler(InventoryDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<StockItemDto>> Handle(
        GetStockItemsQuery query,
        CancellationToken cancellationToken = default)
    {

        return await _context.StockItems
            .Where(s => s.TenantId == _currentTenant.TenantId)
            .Select(s => new StockItemDto(
                s.Id,
                s.ProductId,
                s.ProductCode,
                s.ProductName,
                s.WarehouseId,
                s.WarehouseCode,
                s.QuantityOnHand.Value,
                s.QuantityReserved.Value,
                s.QuantityOnHand.Value - s.QuantityReserved.Value,
                s.QuantityOnHand.UnitCode))
            .ToListAsync(cancellationToken);
    }
}