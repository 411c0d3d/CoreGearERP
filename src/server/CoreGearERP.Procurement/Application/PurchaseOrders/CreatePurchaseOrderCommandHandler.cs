using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Domain.Enums;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.PurchaseOrders;

/// <summary>
/// Handles CreatePurchaseOrderCommand. Creates PO with lines in Draft status.
/// </summary>
public class CreatePurchaseOrderCommandHandler : ICommandHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(
        ProcurementDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == command.SupplierId
                                   && s.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundException(nameof(Supplier), command.SupplierId);
        }

        if (supplier.Status != SupplierStatus.Active.ToString())
        {
            throw new DomainException("Cannot create a purchase order for an inactive supplier.");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var order = PurchaseOrder.Create(
            orderNumber:  orderNumber,
            supplierId:   supplier.Id,
            supplierName: supplier.Name,
            notes:        command.Notes,
            tenantId:     _currentTenant.TenantId,
            createdBy:    _currentUser.UserId);

        foreach (var lineDto in command.Lines)
        {
            var line = PurchaseOrderLine.Create(
                purchaseOrderId: order.Id,
                productId:       lineDto.ProductId,
                productCode:     lineDto.ProductCode,
                productName:     lineDto.ProductName,
                quantityOrdered: new Quantity(lineDto.Quantity, lineDto.UnitCode),
                unitPrice:       new Money(lineDto.UnitPrice, lineDto.CurrencyCode),
                tenantId:        _currentTenant.TenantId,
                createdBy:       _currentUser.UserId);

            order.AddLine(line, _currentUser.UserId);
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _context.PurchaseOrders
            .CountAsync(o => o.TenantId == _currentTenant.TenantId, cancellationToken);

        return $"PO-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}