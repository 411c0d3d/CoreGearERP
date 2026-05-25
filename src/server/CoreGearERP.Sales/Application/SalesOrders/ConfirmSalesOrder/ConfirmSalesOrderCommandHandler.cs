using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Entities;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.SalesOrders.ConfirmSalesOrder;

/// <summary>
/// Handles ConfirmSalesOrderCommand.
/// Checks stock availability and reserves stock via IInventoryCommandService.
/// WarehouseId is optional -- if not provided auto-finds the warehouse with most available stock.
/// Currently, in-process. Replaced with gRPC at M4.
/// </summary>
public class ConfirmSalesOrderCommandHandler : ICommandHandler<ConfirmSalesOrderCommand, Unit>
{
    private readonly SalesDbContext _context;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly IInventoryQueryService _inventoryQueryService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor with dependencies injected. Used for confirming a sales order, which involves checking stock and reserving it.
    /// </summary>
    /// <param name="context">Database context for sales.</param>
    /// <param name="inventoryCommandService">Service to execute inventory commands, such as reserving stock.</param>
    /// <param name="inventoryQueryService">Service to query inventory data, such as available stock and warehouse info.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    /// <param name="currentUser">Service to access current user information.</param>
    public ConfirmSalesOrderCommandHandler(
        SalesDbContext context,
        IInventoryCommandService inventoryCommandService,
        IInventoryQueryService inventoryQueryService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context                 = context;
        _inventoryCommandService = inventoryCommandService;
        _inventoryQueryService   = inventoryQueryService;
        _currentTenant           = currentTenant;
        _currentUser             = currentUser;
    }

    public async Task<Unit> Handle(
        ConfirmSalesOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == command.SalesOrderId
                                   && o.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(SalesOrder), command.SalesOrderId);
        }

        // Resolve warehouse per line -- explicit if provided, auto-find if not.
        var lineWarehouseMap = new Dictionary<Guid, Guid>();

        foreach (var line in order.Lines)
        {
            var warehouseId = command.WarehouseId ?? await _inventoryQueryService.FindBestWarehouseAsync(
                productId:         line.ProductId,
                quantity:          line.QuantityOrdered.Value,
                tenantId:          _currentTenant.TenantId,
                cancellationToken: cancellationToken);

            if (warehouseId == Guid.Empty)
            {
                throw new DomainException(
                    $"No warehouse with sufficient stock found for product '{line.ProductCode}'. " +
                    $"Required: {line.QuantityOrdered.Value} {line.QuantityOrdered.UnitCode}.");
            }

            lineWarehouseMap[line.Id] = warehouseId;
        }

        // Check availability for all lines before reserving any.
        foreach (var line in order.Lines)
        {
            var warehouseId = lineWarehouseMap[line.Id];

            var available = await _inventoryQueryService.GetAvailableStockInWarehouseAsync(
                productId:         line.ProductId,
                warehouseId:       warehouseId,
                tenantId:          _currentTenant.TenantId,
                cancellationToken: cancellationToken);

            if (available < line.QuantityOrdered.Value)
            {
                throw new DomainException(
                    $"Insufficient stock for product '{line.ProductCode}'. " +
                    $"Required: {line.QuantityOrdered.Value} {line.QuantityOrdered.UnitCode}, " +
                    $"Available: {available} {line.QuantityOrdered.UnitCode}.");
            }
        }

        // All lines available -- reserve stock per line using resolved warehouse.
        foreach (var line in order.Lines)
        {
            await _inventoryCommandService.ReserveStockInWarehouseAsync(
                productId:         line.ProductId,
                warehouseId:       lineWarehouseMap[line.Id],
                quantity:          line.QuantityOrdered.Value,
                unitCode:          line.QuantityOrdered.UnitCode,
                tenantId:          _currentTenant.TenantId,
                userId:            _currentUser.UserId,
                cancellationToken: cancellationToken);
        }

        order.Confirm(_currentUser.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}