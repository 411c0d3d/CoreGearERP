using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.SalesOrders.GetSalesOrders;

/// <summary>
/// Handles GetSalesOrdersQuery. Returns sales orders with optional status filter.
/// </summary>
public class GetSalesOrdersQueryHandler : IQueryHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderDto>>
{
    private readonly SalesDbContext _context;

    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSalesOrdersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The sales database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetSalesOrdersQueryHandler(SalesDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<SalesOrderDto>> Handle(
        GetSalesOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = _context.SalesOrders
            .Include(o => o.Lines)
            .Where(o => o.TenantId == _currentTenant.TenantId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            orders = orders.Where(o => o.Status == query.Status);
        }

        return await orders
            .Select(o => new SalesOrderDto(
                o.Id,
                o.OrderNumber,
                o.CustomerName,
                o.Status,
                o.Lines.Count,
                o.CreatedAt,
                o.ConfirmedAt,
                o.CompletedAt))
            .ToListAsync(cancellationToken);
    }
}