using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.Suppliers;

/// <summary>
/// Handles GetSuppliersQuery. Returns all active suppliers for the current tenant.
/// </summary>
public class GetSuppliersQueryHandler : IQueryHandler<GetSuppliersQuery, IReadOnlyList<SupplierDto>>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public GetSuppliersQueryHandler(ProcurementDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<SupplierDto>> Handle(
        GetSuppliersQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .Where(s => s.TenantId == _currentTenant.TenantId)
            .Select(s => new SupplierDto(
                s.Id,
                s.Code,
                s.Name,
                s.ContactEmail,
                s.ContactPhone,
                s.Address,
                s.Status))
            .ToListAsync(cancellationToken);
    }
}