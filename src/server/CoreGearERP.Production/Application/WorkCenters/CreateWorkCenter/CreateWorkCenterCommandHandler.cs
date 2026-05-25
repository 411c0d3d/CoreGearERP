using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Production.Domain.Entities;
using CoreGearERP.Production.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Production.Application.WorkCenters.CreateWorkCenter;

/// <summary>
/// Handles CreateWorkCenterCommand. Validates uniqueness and persists the work center.
/// </summary>
public class CreateWorkCenterCommandHandler : ICommandHandler<CreateWorkCenterCommand, Guid>
{
    private readonly ProductionDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkCenterCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The production database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    /// <param name="currentUser">The current user context.</param>
    public CreateWorkCenterCommandHandler(
        ProductionDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateWorkCenterCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _context.WorkCenters
            .AnyAsync(w => w.TenantId == _currentTenant.TenantId
                           && w.Code == command.Code.Trim().ToUpperInvariant(),
                cancellationToken);

        if (exists)
        {
            throw new DomainException($"Work center with code '{command.Code}' already exists.");
        }

        var workCenter = WorkCenter.Create(
            code: command.Code,
            name: command.Name,
            capacityPerHour: command.CapacityPerHour,
            tenantId: _currentTenant.TenantId,
            createdBy: _currentUser.UserId,
            description: command.Description);

        _context.WorkCenters.Add(workCenter);
        await _context.SaveChangesAsync(cancellationToken);

        return workCenter.Id;
    }
}