using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.ProductionOrders.StartProductionOrder;

/// <summary>Handles StartProductionOrderCommand. Transitions order from Confirmed to InProgress.</summary>
public class StartProductionOrderCommandHandler : ICommandHandler<StartProductionOrderCommand, Unit>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartProductionOrderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public StartProductionOrderCommandHandler(
        ProductionDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        StartProductionOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == command.ProductionOrderId
                                      && o.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(ProductionOrder), command.ProductionOrderId);
        }

        order.Start(_currentUser.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}