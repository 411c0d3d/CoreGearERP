using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Handles GetPurchaseOrdersQuery. Returns purchase orders with optional status filter.
/// </summary>
public class GetPurchaseOrdersQueryHandler : IQueryHandler<GetPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderDto>>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Constructor with dependencies injected. Used for retrieving purchase orders.
    /// </summary>
    /// <param name="context">Database context for procurement.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    public GetPurchaseOrdersQueryHandler(ProcurementDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<PurchaseOrderDto>> Handle(
        GetPurchaseOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = _context.PurchaseOrders
            .Include(o => o.Lines)
            .Where(o => o.TenantId == _currentTenant.TenantId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            orders = orders.Where(o => o.Status == query.Status);
        }

        return await orders
            .Select(o => new PurchaseOrderDto(
                o.Id,
                o.OrderNumber,
                o.SupplierName,
                o.Status,
                o.Lines.Count,
                o.CreatedAt,
                o.ConfirmedAt,
                o.CompletedAt))
            .ToListAsync(cancellationToken);
    }
}