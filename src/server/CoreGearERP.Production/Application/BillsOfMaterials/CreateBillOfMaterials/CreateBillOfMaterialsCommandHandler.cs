using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.BillsOfMaterials.CreateBillOfMaterials;

/// <summary>
/// Handles CreateBillOfMaterialsCommand. Creates BillOfMaterials with component lines.
/// </summary>
public class CreateBillOfMaterialsCommandHandler : ICommandHandler<CreateBillOfMaterialsCommand, Guid>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBillOfMaterialsCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public CreateBillOfMaterialsCommandHandler(
        ProductionDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateBillOfMaterialsCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.BillsOfMaterials
            .AnyAsync(b => b.TenantId == _currentTenant.TenantId
                        && b.FinishedProductId == command.FinishedProductId
                        && b.Version == command.Version.Trim(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException(
                $"A bill of materials for product '{command.FinishedProductCode}' version '{command.Version}' already exists.");
        }

        var billOfMaterials = BillOfMaterials.Create(
            finishedProductId:   command.FinishedProductId,
            finishedProductCode: command.FinishedProductCode,
            finishedProductName: command.FinishedProductName,
            version:             command.Version,
            notes:               command.Notes,
            tenantId:            _currentTenant.TenantId,
            createdBy:           _currentUser.UserId);

        foreach (var lineDto in command.Lines)
        {
            var line = BillOfMaterialsLine.Create(
                billOfMaterialsId:    billOfMaterials.Id,
                componentProductId:   lineDto.ComponentProductId,
                componentProductCode: lineDto.ComponentProductCode,
                componentProductName: lineDto.ComponentProductName,
                quantityRequired:     new Quantity(lineDto.Quantity, lineDto.UnitCode),
                tenantId:             _currentTenant.TenantId,
                createdBy:            _currentUser.UserId);

            billOfMaterials.AddLine(line, _currentUser.UserId);
        }

        billOfMaterials.ValidateForProduction();

        _context.BillsOfMaterials.Add(billOfMaterials);
        await _context.SaveChangesAsync(cancellationToken);

        return billOfMaterials.Id;
    }
}