using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.Customers.GetCustomers;

/// <summary>
/// Query to retrieve all active customers for the current tenant.
/// </summary>
public record GetCustomersQuery : IQuery<IReadOnlyList<CustomerDto>>;

/// <summary>
/// Read model for customer display.
/// </summary>
public record CustomerDto(
    Guid Id,
    string Code,
    string Name,
    string ContactEmail,
    string ContactPhone,
    string Address,
    string Status);