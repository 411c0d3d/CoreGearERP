using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Sales.Application.Customers.GetCustomers;

/// <summary>
/// Handles GetCustomersQuery. Returns all active customers for the current tenant.
/// </summary>
public class GetCustomersQueryHandler : IQueryHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly SalesDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCustomersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The sales database context.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public GetCustomersQueryHandler(SalesDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(
        GetCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => c.TenantId == _currentTenant.TenantId)
            .Select(c => new CustomerDto(
                c.Id,
                c.Code,
                c.Name,
                c.ContactEmail,
                c.ContactPhone,
                c.Address,
                c.Status))
            .ToListAsync(cancellationToken);
    }
}