using CoreGearERP.Common.Application.Events;
using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Domain.Enums;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders.ReceiveGoods;

/// <summary>
/// Handles ReceiveGoodsCommand.
/// Creates a GoodsReceipt, updates PO line and status, adds stock via inventory service,
/// then publishes GoodsReceivedEvent atomically via shared outbox transaction.
/// </summary>
public class ReceiveGoodsCommandHandler : ICommandHandler<ReceiveGoodsCommand, Unit>
{
    private readonly ProcurementDbContext _context;
    private readonly IInventoryCommandService _inventoryCommandService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor with dependencies injected. Used for receiving goods against a purchase order line.
    /// </summary>
    /// <param name="context">Database context for procurement.</param>
    /// <param name="inventoryCommandService">Service to execute inventory commands, such as adding stock.</param>
    /// <param name="publishEndpoint">MassTransit publish endpoint for publishing domain events.</param>
    /// <param name="currentTenant">Service to access current tenant information.</param>
    /// <param name="currentUser">Service to access current user information.</param>
    public ReceiveGoodsCommandHandler(
        ProcurementDbContext context,
        IInventoryCommandService inventoryCommandService,
        IPublishEndpoint publishEndpoint,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _inventoryCommandService = inventoryCommandService;
        _publishEndpoint = publishEndpoint;
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

        line.Receive(quantity, _currentUser.UserId);
        order.UpdateReceiptStatus(_currentUser.UserId);

        var receipt = GoodsReceipt.Create(
            purchaseOrderId: order.Id,
            purchaseOrderNumber: order.OrderNumber,
            warehouseId: command.WarehouseId,
            tenantId: _currentTenant.TenantId,
            createdBy: _currentUser.UserId);

        var receiptLine = GoodsReceiptLine.Create(
            goodsReceiptId: receipt.Id,
            purchaseOrderLineId: line.Id,
            productId: line.ProductId,
            productCode: line.ProductCode,
            productName: line.ProductName,
            quantityReceived: quantity,
            unitPrice: line.UnitPrice,
            tenantId: _currentTenant.TenantId,
            createdBy: _currentUser.UserId);

        receipt.AddLine(receiptLine);
        _context.GoodsReceipts.Add(receipt);

        await _inventoryCommandService.AddStockAsync(
            productId: line.ProductId,
            warehouseId: command.WarehouseId,
            quantity: command.Quantity,
            unitCode: line.QuantityOrdered.UnitCode,
            referenceId: receipt.Id,
            referenceNumber: order.OrderNumber,
            tenantId: _currentTenant.TenantId,
            userId: _currentUser.UserId,
            cancellationToken: cancellationToken);

        await _publishEndpoint.Publish(
            new GoodsReceivedEvent(
                receipt.Id,
                order.Id,
                order.OrderNumber,
                line.Id,
                line.ProductId,
                line.ProductCode,
                command.Quantity,
                line.QuantityOrdered.UnitCode,
                line.UnitPrice.Amount,
                line.UnitPrice.Amount * command.Quantity,
                line.UnitPrice.CurrencyCode,
                _currentTenant.TenantId,
                DateTime.UtcNow),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}