using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Application.BillsOfMaterials.GetBillsOfMaterials;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.BillsOfMaterials;

/// <summary>
/// Handles GetBillsOfMaterialsQuery. Returns all BillsOfMaterials for the current tenant.
/// </summary>
public class
    GetBillsOfMaterialsQueryHandler : IQueryHandler<GetBillsOfMaterialsQuery, IReadOnlyList<BillOfMaterialsDto>>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBillsOfMaterialsQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetBillsOfMaterialsQueryHandler(ProductionDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<BillOfMaterialsDto>> Handle(
        GetBillsOfMaterialsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.BillsOfMaterials
            .Include(b => b.Lines)
            .Where(b => b.TenantId == _currentTenant.TenantId)
            .Select(b => new BillOfMaterialsDto(
                b.Id,
                b.FinishedProductId,
                b.FinishedProductCode,
                b.FinishedProductName,
                b.Version,
                b.Status,
                b.Lines.Count))
            .ToListAsync(cancellationToken);
    }
}