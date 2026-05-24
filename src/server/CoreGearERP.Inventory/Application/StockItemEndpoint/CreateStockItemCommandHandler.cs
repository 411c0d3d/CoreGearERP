using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Domain.Enums;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.StockItemEndpoint;

/// <summary>
/// Handles CreateStockItemCommand. Validates product and warehouse exist then creates the stock item.
/// </summary>
public class CreateStockItemCommandHandler : ICommandHandler<CreateStockItemCommand, Guid>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor with dependencies injected.
    /// </summary>
    ///  <param name="context">InventoryDbContext is used to access the Products, Warehouses, and StockItems tables.</param>
    /// <param name="currentTenant">ICurrentTenant is used to filter by tenant and set tenantId on the new stock item.</param>
    /// <param name="currentUser">ICurrentUser is used to set the createdBy field on the new stock item.</param>
    public CreateStockItemCommandHandler(
        InventoryDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateStockItemCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId
                                   && p.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), command.ProductId);
        }

        if (product.Status != ProductStatus.Active.ToString())
        {
            throw new DomainException("Cannot create a stock item for an inactive or discontinued product.");
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == command.WarehouseId
                                   && w.TenantId == _currentTenant.TenantId,
                cancellationToken);

        if (warehouse is null)
        {
            throw new NotFoundException(nameof(Warehouse), command.WarehouseId);
        }

        if (warehouse.Status != WarehouseStatus.Active.ToString())
        {
            throw new DomainException("Cannot create a stock item in an inactive warehouse.");
        }

        var exists = await _context.StockItems
            .AnyAsync(s => s.TenantId == _currentTenant.TenantId
                        && s.ProductId == command.ProductId
                        && s.WarehouseId == command.WarehouseId,
                cancellationToken);

        if (exists)
        {
            throw new DomainException("A stock item for this product and warehouse already exists.");
        }

        var stockItem = StockItem.Create(
            productId:   command.ProductId,
            warehouseId: command.WarehouseId,
            unitCode:    command.UnitCode,
            tenantId:    _currentTenant.TenantId,
            createdBy:   _currentUser.UserId);

        _context.StockItems.Add(stockItem);
        await _context.SaveChangesAsync(cancellationToken);

        return stockItem.Id;
    }
}