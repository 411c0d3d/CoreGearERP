using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.StockMovements;

/// <summary>
/// Handles GetStockMovementsQuery. Returns movement history for a stock item.
/// </summary>
public class GetStockMovementsQueryHandler : IQueryHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementDto>>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
     /// <param name="context">InventoryDbContext is used to access the StockMovements table.</param>
     /// <param name="currentTenant">ICurrentTenant is used to filter by tenant.</param>
    public GetStockMovementsQueryHandler(InventoryDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<StockMovementDto>> Handle(
        GetStockMovementsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Where(m => m.TenantId == _currentTenant.TenantId
                        && m.StockItemId == query.StockItemId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new StockMovementDto(
                m.Id,
                m.MovementType.ToString(),
                m.Quantity.Value,
                m.Quantity.UnitCode,
                m.ReferenceNumber,
                m.Notes,
                m.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}