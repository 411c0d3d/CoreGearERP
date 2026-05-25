using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Domain.Enums;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders.ReceiveGoods;

/// <summary>
/// Handles ReceiveGoodsCommand.
/// Updates PO line and status, then adds stock via IInventoryCommandService.
/// Currently, in-process. Replaced with gRPC at M4.
/// </summary>
public class ReceiveGoodsCommandHandler : ICommandHandler<ReceiveGoodsCommand, Unit>
{
    private readonly ProcurementDbContext _context;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor with dependencies injected. Used for receiving goods against a purchase order line.
    /// </summary>
    /// <param name="context">Database context for procurement.</param>
    /// <param name="inventoryCommandService">Service to execute inventory commands, such as adding stock.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    /// <param name="currentUser">Service to access current user information.</param>
    public ReceiveGoodsCommandHandler(
        ProcurementDbContext context,
        IInventoryCommandService inventoryCommandService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _inventoryCommandService = inventoryCommandService;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        ReceiveGoodsCommand command,
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

        if (order.Status != PurchaseOrderStatus.Confirmed.ToString() &&
            order.Status != PurchaseOrderStatus.PartiallyReceived.ToString())
        {
            throw new DomainException("Goods can only be received against a Confirmed or PartiallyReceived order.");
        }

        var line = order.Lines.FirstOrDefault(l => l.Id == command.PurchaseOrderLineId);

        if (line is null)
        {
            throw new NotFoundException(nameof(PurchaseOrderLine), command.PurchaseOrderLineId);
        }

        if (line.Status == PurchaseOrderLineStatus.Received.ToString())
        {
            throw new DomainException("This purchase order line has already been fully received.");
        }

        var quantity = new Quantity(command.Quantity, line.QuantityOrdered.UnitCode);

        // Update PO line and status -- not saved yet.
        line.Receive(quantity, _currentUser.UserId);
        order.UpdateReceiptStatus(_currentUser.UserId);

        // Add stock via contract -- if this throws, PO changes are not saved.
        await _inventoryCommandService.AddStockAsync(
            productId: line.ProductId,
            warehouseId: command.WarehouseId,
            quantity: command.Quantity,
            unitCode: line.QuantityOrdered.UnitCode,
            referenceId: order.Id,
            referenceNumber: order.OrderNumber,
            tenantId: _currentTenant.TenantId,
            userId: _currentUser.UserId,
            cancellationToken: cancellationToken);

        // Only save PO changes after inventory succeeds.
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}