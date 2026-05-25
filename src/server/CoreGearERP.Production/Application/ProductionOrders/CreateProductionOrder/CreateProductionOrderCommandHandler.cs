using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Domain.Enums;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.ProductionOrders.CreateProductionOrder;

/// <summary>
/// Handles CreateProductionOrderCommand. Creates production order in Draft status.
/// </summary>
public class CreateProductionOrderCommandHandler : ICommandHandler<CreateProductionOrderCommand, Guid>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductionOrderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public CreateProductionOrderCommandHandler(
        ProductionDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var billOfMaterials = await _context.BillsOfMaterials
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == command.BillOfMaterialsId
                                   && b.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (billOfMaterials is null)
        {
            throw new NotFoundException(nameof(BillOfMaterials), command.BillOfMaterialsId);
        }

        billOfMaterials.ValidateForProduction();

        var workCenter = await _context.WorkCenters
            .FirstOrDefaultAsync(w => w.Id == command.WorkCenterId
                                   && w.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (workCenter is null)
        {
            throw new NotFoundException(nameof(WorkCenter), command.WorkCenterId);
        }

        if (workCenter.Status != WorkCenterStatus.Active.ToString())
        {
            throw new DomainException("Cannot create a production order for an inactive work center.");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var order = ProductionOrder.Create(
            orderNumber:         orderNumber,
            billOfMaterialsId:   billOfMaterials.Id,
            finishedProductCode: billOfMaterials.FinishedProductCode,
            finishedProductName: billOfMaterials.FinishedProductName,
            workCenterId:        workCenter.Id,
            workCenterCode:      workCenter.Code,
            plannedQuantity:     new Quantity(command.PlannedQuantity, command.UnitCode),
            notes:               command.Notes,
            tenantId:            _currentTenant.TenantId,
            createdBy:           _currentUser.UserId);

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _context.ProductionOrders
            .CountAsync(o => o.TenantId == _currentTenant.TenantId, cancellationToken);

        return $"PRD-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}