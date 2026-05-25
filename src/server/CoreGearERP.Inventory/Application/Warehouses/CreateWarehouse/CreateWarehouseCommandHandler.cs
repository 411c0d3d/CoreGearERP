using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Warehouses.CreateWarehouse;

/// <summary>
/// Handles CreateWarehouseCommand. Validates uniqueness and persists the warehouse.
/// </summary>
public class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand, Guid>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the CreateWarehouseCommandHandler class with the specified dependencies.
    /// </summary>
    ///  <param name="context">
    /// The InventoryDbContext is injected to interact with the database.
    /// </param>
    /// <param name="currentTenant">
    /// The ICurrentTenant service is injected to access the current tenant's context, ensuring that the warehouse is created under the correct tenant and that uniqueness is validated within that tenant's scope.
    /// </param>
    /// <param name="currentUser">
    /// The ICurrentUser service is injected to access the current user's context, allowing the handler to set the CreatedBy property of the warehouse for auditing purposes.
    /// </param>
    public CreateWarehouseCommandHandler(
        InventoryDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateWarehouseCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Warehouses
            .AnyAsync(w => w.TenantId == _currentTenant.TenantId
                           && w.Code == command.Code.Trim().ToUpperInvariant(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException($"Warehouse with code '{command.Code}' already exists.");
        }

        var warehouse = Warehouse.Create(
            code:      command.Code,
            name:      command.Name,
            location:  command.Location,
            tenantId:  _currentTenant.TenantId,
            createdBy: _currentUser.UserId);

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}