using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Commands;

/// <summary>
/// Handles the CreateProductCommand. Validates uniqueness and persists the product.
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the CreateProductCommandHandler class with the specified dependencies.
    /// </summary>
    /// <param name="context">
    /// The InventoryDbContext is injected to interact with the database.
    /// </param>
    /// <param name="currentTenant">
    /// The ICurrentTenant service is injected to access the current tenant's context, ensuring that the product is created under the correct tenant and that uniqueness is validated within that tenant's scope.
    /// </param>
    /// <param name="currentUser">
    /// The ICurrentUser service is injected to access the current user's context, allowing the handler to set the CreatedBy property of the product for auditing purposes.
    /// </param>
    public CreateProductCommandHandler(
        InventoryDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.TenantId == _currentTenant.TenantId
                           && p.Code == command.Code.Trim().ToUpperInvariant(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException($"Product with code '{command.Code}' already exists.");
        }

        var product = Product.Create(
            code: command.Code,
            name: command.Name,
            unitCode: command.UnitCode,
            tenantId: _currentTenant.TenantId,
            createdBy: _currentUser.UserId,
            description: command.Description);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}