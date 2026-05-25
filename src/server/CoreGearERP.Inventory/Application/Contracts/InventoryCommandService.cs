using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Domain.Enums;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Contracts;

/// <summary>
/// In-process implementation of IInventoryCommandService.
/// Called by Procurement and Sales to mutate inventory state.
/// Replaced with gRPC service implementation at M4.
/// </summary>
public class InventoryCommandService : IInventoryCommandService
{
    private readonly InventoryDbContext _context;

    public InventoryCommandService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                      && s.WarehouseId == warehouseId
                                      && s.TenantId == tenantId,
                cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException("StockItem", $"Product {productId} in Warehouse {warehouseId}");
        }

        var qty = new Quantity(quantity, unitCode);

        stockItem.AddStock(qty, userId);

        var movement = StockMovement.Create(
            stockItemId: stockItem.Id,
            productId: productId,
            warehouseId: warehouseId,
            movementType: StockMovementType.GoodsReceipt,
            quantity: qty,
            tenantId: tenantId,
            createdBy: userId,
            referenceId: referenceId,
            referenceNumber: referenceNumber);

        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReserveStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                      && s.WarehouseId == warehouseId
                                      && s.TenantId == tenantId,
                cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException("StockItem", $"Product {productId} in Warehouse {warehouseId}");
        }

        stockItem.Reserve(new Quantity(quantity, unitCode), userId);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseReservationAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                      && s.WarehouseId == warehouseId
                                      && s.TenantId == tenantId,
                cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException("StockItem", $"Product {productId} in Warehouse {warehouseId}");
        }

        stockItem.ReleaseReservation(new Quantity(quantity, unitCode), userId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}