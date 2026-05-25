using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Sales.Domain.Entities;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.Customers.CreateCustomer;

/// <summary>
/// Handles CreateCustomerCommand. Validates uniqueness and persists the customer.
/// </summary>
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly SalesDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCustomerCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The sales database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public CreateCustomerCommandHandler(
        SalesDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Customers
            .AnyAsync(c => c.TenantId == _currentTenant.TenantId
                           && c.Code == command.Code.Trim().ToUpperInvariant(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException($"Customer with code '{command.Code}' already exists.");
        }

        var customer = Customer.Create(
            code:         command.Code,
            name:         command.Name,
            contactEmail: command.ContactEmail,
            contactPhone: command.ContactPhone,
            address:      command.Address,
            tenantId:     _currentTenant.TenantId,
            createdBy:    _currentUser.UserId);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}