using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.Suppliers;

/// <summary>
/// Query to retrieve all active suppliers for the current tenant.
/// </summary>
public record GetSuppliersQuery : IQuery<IReadOnlyList<SupplierDto>>;

/// <summary>
/// Read model for supplier display.
/// </summary>
public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string ContactEmail,
    string ContactPhone,
    string Address,
    string Status);