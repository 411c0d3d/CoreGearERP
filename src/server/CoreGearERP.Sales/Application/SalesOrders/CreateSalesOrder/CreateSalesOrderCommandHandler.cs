using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Sales.Domain.Entities;
using CoreGearERP.Sales.Domain.Enums;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.SalesOrders.CreateSalesOrder;

/// <summary>
/// Handles CreateSalesOrderCommand. Creates sales order with lines in Draft status.
/// </summary>
public class CreateSalesOrderCommandHandler : ICommandHandler<CreateSalesOrderCommand, Guid>
{
    private readonly SalesDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSalesOrderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The sales database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public CreateSalesOrderCommandHandler(
        SalesDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateSalesOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == command.CustomerId
                                   && c.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), command.CustomerId);
        }

        if (customer.Status != CustomerStatus.Active.ToString())
        {
            throw new DomainException("Cannot create a sales order for an inactive customer.");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var order = SalesOrder.Create(
            orderNumber:  orderNumber,
            customerId:   customer.Id,
            customerName: customer.Name,
            notes:        command.Notes,
            tenantId:     _currentTenant.TenantId,
            createdBy:    _currentUser.UserId);

        foreach (var lineDto in command.Lines)
        {
            var line = SalesOrderLine.Create(
                salesOrderId:    order.Id,
                productId:       lineDto.ProductId,
                productCode:     lineDto.ProductCode,
                productName:     lineDto.ProductName,
                quantityOrdered: new Quantity(lineDto.Quantity, lineDto.UnitCode),
                unitPrice:       new Money(lineDto.UnitPrice, lineDto.CurrencyCode),
                tenantId:        _currentTenant.TenantId,
                createdBy:       _currentUser.UserId);

            order.AddLine(line, _currentUser.UserId);
        }

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _context.SalesOrders
            .CountAsync(o => o.TenantId == _currentTenant.TenantId, cancellationToken);

        return $"SO-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}