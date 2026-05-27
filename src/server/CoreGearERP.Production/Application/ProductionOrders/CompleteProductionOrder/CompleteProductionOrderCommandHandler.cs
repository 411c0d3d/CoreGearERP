using CoreGearERP.Common.Application.Events;
using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Messaging.Infrastructure.Persistence;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Domain.Enums;
using CoreGearERP.Production.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreGearERP.Production.Application.ProductionOrders.CompleteProductionOrder;

/// <summary>
/// Handles CompleteProductionOrderCommand.
/// Releases component reservations, consumes actual stock, adds finished goods,
/// then publishes ProductionOrderCompletedEvent atomically via shared outbox transaction.
/// </summary>
public class CompleteProductionOrderCommandHandler : ICommandHandler<CompleteProductionOrderCommand, Unit>
{
    private readonly ProductionDbContext _context;
    private readonly OutboxDbContext _outboxContext;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes handler with production context, shared outbox context, and supporting services.
    /// </summary>
    /// <param name="context">The ProductionDbContext for accessing production data.</param>
    /// <param name="outboxContext">The OutboxDbContext for managing the outbox pattern.</param>
    /// <param name="inventoryCommandService">The service for executing inventory commands.</param>
    /// <param name="publishEndpoint">The MassTransit publish endpoint for publishing events.</param>
    /// <param name="currentTenant">The current tenant context for multi-tenancy support.</param>
    /// <param name="currentUser">The current user context for auditing purposes.</param>
    public CompleteProductionOrderCommandHandler(
        ProductionDbContext context,
        OutboxDbContext outboxContext,
        IInventoryCommandService inventoryCommandService,
        IPublishEndpoint publishEndpoint,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _outboxContext = outboxContext;
        _inventoryCommandService = inventoryCommandService;
        _publishEndpoint = publishEndpoint;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        CompleteProductionOrderCommand command,
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

        if (order.Status != ProductionOrderStatus.InProgress.ToString())
        {
            throw new DomainException("Only an InProgress production order can be completed.");
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

        // Validate all components have warehouse assignments.
        foreach (var line in billOfMaterials.Lines)
        {
            var assignment = command.ComponentWarehouses
                .FirstOrDefault(w => w.ComponentProductId == line.ComponentProductId);

            if (assignment is null)
            {
                throw new DomainException(
                    $"No warehouse assignment provided for component '{line.ComponentProductCode}'.");
            }
        }

        var actualQuantity = new Quantity(command.ActualQuantity, order.PlannedQuantity.UnitCode);

        foreach (var line in billOfMaterials.Lines)
        {
            var assignment = command.ComponentWarehouses.First(w => w.ComponentProductId == line.ComponentProductId);
            var plannedComponentQty = line.QuantityRequired.Value * order.PlannedQuantity.Value;
            var actualComponentQty = line.QuantityRequired.Value * command.ActualQuantity;

            // Release the planned reservation.
            await _inventoryCommandService.ReleaseReservationAsync(
                productId: line.ComponentProductId,
                warehouseId: assignment.WarehouseId,
                quantity: plannedComponentQty,
                unitCode: line.QuantityRequired.UnitCode,
                tenantId: _currentTenant.TenantId,
                userId: _currentUser.UserId,
                cancellationToken: cancellationToken);

            // Consume actual component quantity.
            await _inventoryCommandService.IssueStockAsync(
                productId: line.ComponentProductId,
                warehouseId: assignment.WarehouseId,
                quantity: actualComponentQty,
                unitCode: line.QuantityRequired.UnitCode,
                referenceId: order.Id,
                referenceNumber: order.OrderNumber,
                tenantId: _currentTenant.TenantId,
                userId: _currentUser.UserId,
                cancellationToken: cancellationToken);
        }

        // Add finished goods to warehouse.
        await _inventoryCommandService.AddStockFromProductionAsync(
            productId: billOfMaterials.FinishedProductId,
            warehouseId: command.FinishedGoodsWarehouseId,
            quantity: command.ActualQuantity,
            unitCode: order.PlannedQuantity.UnitCode,
            referenceId: order.Id,
            referenceNumber: order.OrderNumber,
            tenantId: _currentTenant.TenantId,
            userId: _currentUser.UserId,
            cancellationToken: cancellationToken);

        order.Complete(actualQuantity, _currentUser.UserId);

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await _context.Database.UseTransactionAsync(transaction, cancellationToken);
        _outboxContext.Database.SetDbConnection(connection);
        await _outboxContext.Database.UseTransactionAsync(transaction, cancellationToken);

        await _publishEndpoint.Publish(
            new ProductionOrderCompletedEvent(
                order.Id,
                order.OrderNumber,
                billOfMaterials.FinishedProductId,
                order.FinishedProductCode,
                order.PlannedQuantity.Value,
                command.ActualQuantity,
                order.PlannedQuantity.UnitCode,
                _currentTenant.TenantId,
                DateTime.UtcNow),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await _outboxContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}