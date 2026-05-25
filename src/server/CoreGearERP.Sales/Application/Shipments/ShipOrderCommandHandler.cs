using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Sales.Domain.Entities;
using CoreGearERP.Sales.Domain.Enums;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.Shipments;

/// <summary>
/// Handles ShipOrderCommand.
/// Creates shipment, updates sales order lines, releases reservations, creates stock movements.
/// Currently, in-process. Replaced with gRPC at M4.
/// </summary>
public class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand, Guid>
{
    private readonly SalesDbContext _context;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShipOrderCommandHandler(
        SalesDbContext context,
        IInventoryCommandService inventoryCommandService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context                 = context;
        _inventoryCommandService = inventoryCommandService;
        _currentTenant           = currentTenant;
        _currentUser             = currentUser;
    }

    public async Task<Guid> Handle(ShipOrderCommand command, CancellationToken cancellationToken = default)
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

        if (order.Status != SalesOrderStatus.Confirmed.ToString() &&
            order.Status != SalesOrderStatus.PartiallyShipped.ToString())
        {
            throw new DomainException("Goods can only be shipped against a Confirmed or PartiallyShipped order.");
        }

        var shipmentNumber = await GenerateShipmentNumberAsync(cancellationToken);

        var shipment = Shipment.Create(
            salesOrderId:   order.Id,
            shipmentNumber: shipmentNumber,
            notes:          command.Notes,
            tenantId:       _currentTenant.TenantId,
            createdBy:      _currentUser.UserId);

        foreach (var lineDto in command.Lines)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.Id == lineDto.SalesOrderLineId);

            if (orderLine is null)
            {
                throw new NotFoundException(nameof(SalesOrderLine), lineDto.SalesOrderLineId);
            }

            if (orderLine.Status == SalesOrderLineStatus.Shipped.ToString())
            {
                throw new DomainException($"Line for product '{lineDto.ProductCode}' is already fully shipped.");
            }

            var qty = new Quantity(lineDto.Quantity, lineDto.UnitCode);

            // Update sales order line shipped quantity.
            orderLine.Ship(qty, _currentUser.UserId);

            var shipmentLine = ShipmentLine.Create(
                shipmentId:       shipment.Id,
                salesOrderLineId: orderLine.Id,
                productId:        lineDto.ProductId,
                productCode:      lineDto.ProductCode,
                quantityShipped:  qty,
                tenantId:         _currentTenant.TenantId,
                createdBy:        _currentUser.UserId);

            shipment.AddLine(shipmentLine, _currentUser.UserId);
        }

        shipment.Ship(_currentUser.UserId);
        order.UpdateShipmentStatus(_currentUser.UserId);

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync(cancellationToken);

        // Release reservations and remove stock per line.
        foreach (var lineDto in command.Lines)
        {
            await _inventoryCommandService.ReleaseReservationAsync(
                productId:         lineDto.ProductId,
                warehouseId:       command.WarehouseId,
                quantity:          lineDto.Quantity,
                unitCode:          lineDto.UnitCode,
                tenantId:          _currentTenant.TenantId,
                userId:            _currentUser.UserId,
                cancellationToken: cancellationToken);

            await _inventoryCommandService.ShipStockAsync(
                productId:         lineDto.ProductId,
                warehouseId:       command.WarehouseId,
                quantity:          lineDto.Quantity,
                unitCode:          lineDto.UnitCode,
                referenceId:       shipment.Id,
                referenceNumber:   shipment.ShipmentNumber,
                tenantId:          _currentTenant.TenantId,
                userId:            _currentUser.UserId,
                cancellationToken: cancellationToken);
        }

        return shipment.Id;
    }

    private async Task<string> GenerateShipmentNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _context.Shipments
            .CountAsync(s => s.TenantId == _currentTenant.TenantId, cancellationToken);

        return $"SHP-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}