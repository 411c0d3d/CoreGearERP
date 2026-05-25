using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.ProductionOrders.GetProductionOrders;

/// <summary>
/// Handles GetProductionOrdersQuery.
/// </summary>
public class GetProductionOrdersQueryHandler
    : IQueryHandler<GetProductionOrdersQuery, IReadOnlyList<ProductionOrderDto>>
{
    private readonly ProductionDbContext _context;

    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductionOrdersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetProductionOrdersQueryHandler(ProductionDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<ProductionOrderDto>> Handle(
        GetProductionOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = _context.ProductionOrders
            .Where(o => o.TenantId == _currentTenant.TenantId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            orders = orders.Where(o => o.Status == query.Status);
        }

        return await orders
            .Select(o => new ProductionOrderDto(
                o.Id,
                o.OrderNumber,
                o.FinishedProductCode,
                o.FinishedProductName,
                o.WorkCenterCode,
                o.PlannedQuantity.Value,
                o.ActualQuantity == null ? null : o.ActualQuantity.Value,
                o.PlannedQuantity.UnitCode,
                o.Status,
                o.CreatedAt,
                o.ConfirmedAt,
                o.CompletedAt))
            .ToListAsync(cancellationToken);
    }
}