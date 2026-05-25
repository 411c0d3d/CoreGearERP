using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Handles ConfirmPurchaseOrderCommand. Transitions PO from Draft to Confirmed.
/// </summary>
public class ConfirmPurchaseOrderCommandHandler : ICommandHandler<ConfirmPurchaseOrderCommand, Unit>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor with dependencies injected. Used for confirming a purchase order.
    /// </summary>
    /// <param name="context">Database context for procurement.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    /// <param name="currentUser">Service to access current user information.</param>
    public ConfirmPurchaseOrderCommandHandler(
        ProcurementDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        ConfirmPurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId
                                      && o.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(PurchaseOrder), command.PurchaseOrderId);
        }

        order.Confirm(_currentUser.UserId);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}