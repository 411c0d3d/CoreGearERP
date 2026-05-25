using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Procurement.Domain.Entities;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Procurement.Application.Suppliers;

/// <summary>
/// Handles CreateSupplierCommand. Validates uniqueness and persists the supplier.
/// </summary>
public class CreateSupplierCommandHandler : ICommandHandler<CreateSupplierCommand, Guid>
{
    private readonly ProcurementDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CreateSupplierCommandHandler(
        ProcurementDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Suppliers
            .AnyAsync(s => s.TenantId == _currentTenant.TenantId
                           && s.Code == command.Code.Trim().ToUpperInvariant(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException($"Supplier with code '{command.Code}' already exists.");
        }

        var supplier = Supplier.Create(
            code:         command.Code,
            name:         command.Name,
            contactEmail: command.ContactEmail,
            contactPhone: command.ContactPhone,
            address:      command.Address,
            tenantId:     _currentTenant.TenantId,
            createdBy:    _currentUser.UserId);

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        return supplier.Id;
    }
}