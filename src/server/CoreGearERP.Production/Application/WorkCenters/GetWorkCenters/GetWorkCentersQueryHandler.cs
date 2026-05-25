using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.WorkCenters.GetWorkCenters;

/// <summary>
/// Handles GetWorkCentersQuery. Returns all active work centers for the current tenant.
/// </summary>
public class GetWorkCentersQueryHandler : IQueryHandler<GetWorkCentersQuery, IReadOnlyList<WorkCenterDto>>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWorkCentersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetWorkCentersQueryHandler(ProductionDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<WorkCenterDto>> Handle(
        GetWorkCentersQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkCenters
            .Where(w => w.TenantId == _currentTenant.TenantId)
            .Select(w => new WorkCenterDto(
                w.Id,
                w.Code,
                w.Name,
                w.Description,
                w.CapacityPerHour,
                w.Status))
            .ToListAsync(cancellationToken);
    }
}