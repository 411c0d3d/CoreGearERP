using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Entities;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.SalesOrders.GetSalesOrderById;

/// <summary>
/// Handles GetSalesOrderByIdQuery. Returns sales order with all line details.
/// </summary>
public class GetSalesOrderByIdQueryHandler : IQueryHandler<GetSalesOrderByIdQuery, SalesOrderDetailDto>
{
    private readonly SalesDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSalesOrderByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The sales database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetSalesOrderByIdQueryHandler(SalesDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<SalesOrderDetailDto> Handle(
        GetSalesOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == query.SalesOrderId
                                      && o.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(SalesOrder), query.SalesOrderId);
        }

        return new SalesOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.Status,
            order.Notes,
            order.CreatedAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.Lines.Select(l => new SalesOrderLineDto(
                l.Id,
                l.ProductId,
                l.ProductCode,
                l.ProductName,
                l.QuantityOrdered.Value,
                l.QuantityShipped.Value,
                l.QuantityOrdered.UnitCode,
                l.UnitPrice.Amount,
                l.UnitPrice.CurrencyCode,
                l.Status)).ToList());
    }
}