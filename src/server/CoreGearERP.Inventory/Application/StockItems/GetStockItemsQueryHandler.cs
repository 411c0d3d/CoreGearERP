using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.StockItems;

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
        var stockItems = _context.StockItems
            .Where(s => s.TenantId == _currentTenant.TenantId);

        if (query.WarehouseId.HasValue)
        {
            stockItems = stockItems.Where(s => s.WarehouseId == query.WarehouseId.Value);
        }

        return await stockItems
            .Join(_context.Products,
                s => s.ProductId,
                p => p.Id,
                (s, p) => new { StockItem = s, Product = p })
            .Join(_context.Warehouses,
                sp => sp.StockItem.WarehouseId,
                w => w.Id,
                (sp, w) => new StockItemDto(
                    sp.StockItem.Id,
                    sp.StockItem.ProductId,
                    sp.Product.Code,
                    sp.Product.Name,
                    sp.StockItem.WarehouseId,
                    w.Code,
                    sp.StockItem.QuantityOnHand.Value,
                    sp.StockItem.QuantityReserved.Value,
                    sp.StockItem.QuantityOnHand.Value - sp.StockItem.QuantityReserved.Value,
                    sp.StockItem.QuantityOnHand.UnitCode))
            .ToListAsync(cancellationToken);
    }
}