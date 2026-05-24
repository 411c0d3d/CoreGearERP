using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Application.Queries.Product;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.ProductEndpoint;

/// <summary>
/// Handles GetProductsQuery. Returns all non-deleted products for the current tenant.
/// </summary>
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public GetProductsQueryHandler(InventoryDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => p.TenantId == _currentTenant.TenantId)
            .Select(p => new ProductDto(
                p.Id,
                p.Code,
                p.Name,
                p.UnitCode,
                p.Description,
                p.Status))
            .ToListAsync(cancellationToken);
    }
}