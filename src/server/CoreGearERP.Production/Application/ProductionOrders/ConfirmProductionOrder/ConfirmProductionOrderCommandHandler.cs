using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.ProductionOrders.ConfirmProductionOrder;

/// <summary>
/// Handles ConfirmProductionOrderCommand.
/// Checks component availability and reserves stock via IInventoryCommandService.
/// Currently, in-process. Replaced with gRPC at M4.
/// </summary>
public class ConfirmProductionOrderCommandHandler : ICommandHandler<ConfirmProductionOrderCommand, Unit>
{
    private readonly ProductionDbContext _context;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly IInventoryQueryService _inventoryQueryService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmProductionOrderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="inventoryCommandService">The inventory command service for reserving stock.</param>
    /// <param name="inventoryQueryService">The inventory query service for checking stock availability.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public ConfirmProductionOrderCommandHandler(
        ProductionDbContext context,
        IInventoryCommandService inventoryCommandService,
        IInventoryQueryService inventoryQueryService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _inventoryCommandService = inventoryCommandService;
        _inventoryQueryService = inventoryQueryService;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        ConfirmProductionOrderCommand command,
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

        var billOfMaterials = await _context.BillsOfMaterials
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == order.BillOfMaterialsId
                                      && b.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (billOfMaterials is null)
        {
            throw new NotFoundException(nameof(BillOfMaterials), order.BillOfMaterialsId);
        }

        // Validate all BOM components have a warehouse assignment.
        foreach (var line in billOfMaterials.Lines)
        {
            var assignment = command.ComponentWarehouses
                .FirstOrDefault(w => w.ComponentProductId == line.ComponentProductId);

            if (assignment is null)
            {
                throw new DomainException(
                    $"No warehouse assignment provided for component '{line.ComponentProductCode}'. " +
                    "All components must have an explicit warehouse assignment.");
            }
        }

        var scaledRequirements = billOfMaterials.Lines
            .Select(line => new
            {
                Line = line,
                Warehouse = command.ComponentWarehouses.First(w => w.ComponentProductId == line.ComponentProductId),
                RequiredQty = line.QuantityRequired.Value * order.PlannedQuantity.Value
            })
            .ToList();

        // Check availability for all components before reserving any.
        foreach (var req in scaledRequirements)
        {
            var available = await _inventoryQueryService.GetAvailableStockInWarehouseAsync(
                productId: req.Line.ComponentProductId,
                warehouseId: req.Warehouse.WarehouseId,
                tenantId: _currentTenant.TenantId,
                cancellationToken: cancellationToken);

            if (available < req.RequiredQty)
            {
                throw new DomainException(
                    $"Insufficient stock for component '{req.Line.ComponentProductCode}' in warehouse '{req.Warehouse.WarehouseId}'. " +
                    $"Required: {req.RequiredQty} {req.Line.QuantityRequired.UnitCode}, " +
                    $"Available: {available} {req.Line.QuantityRequired.UnitCode}.");
            }
        }

        // All components available -- reserve stock per component per warehouse.
        foreach (var req in scaledRequirements)
        {
            await _inventoryCommandService.ReserveStockInWarehouseAsync(
                productId: req.Line.ComponentProductId,
                warehouseId: req.Warehouse.WarehouseId,
                quantity: req.RequiredQty,
                unitCode: req.Line.QuantityRequired.UnitCode,
                tenantId: _currentTenant.TenantId,
                userId: _currentUser.UserId,
                cancellationToken: cancellationToken);
        }

        order.Confirm(_currentUser.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}