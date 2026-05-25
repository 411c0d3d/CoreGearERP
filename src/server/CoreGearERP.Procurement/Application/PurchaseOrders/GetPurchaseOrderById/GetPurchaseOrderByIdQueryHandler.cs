using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders.GetPurchaseOrderById;

/// <summary>
/// Handles GetPurchaseOrderByIdQuery. Returns PO with all line details.
/// </summary>
public class GetPurchaseOrderByIdQueryHandler
    : IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Constructor with dependencies injected. Used for retrieving a purchase order by ID.
    /// </summary>
    /// <param name="context">Database context for procurement.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    public GetPurchaseOrderByIdQueryHandler(
        ProcurementDbContext context,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<PurchaseOrderDetailDto> Handle(
        GetPurchaseOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == query.PurchaseOrderId
                                      && o.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(PurchaseOrder), query.PurchaseOrderId);
        }

        return new PurchaseOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.SupplierName,
            order.Status,
            order.Notes,
            order.CreatedAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.Lines.Select(l => new PurchaseOrderLineDto(
                l.Id,
                l.ProductId,
                l.ProductCode,
                l.ProductName,
                l.QuantityOrdered.Value,
                l.QuantityReceived.Value,
                l.QuantityOrdered.UnitCode,
                l.UnitPrice.Amount,
                l.UnitPrice.CurrencyCode,
                l.Status)).ToList());
    }
}