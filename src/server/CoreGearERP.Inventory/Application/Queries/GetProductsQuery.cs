using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Inventory.Application.Queries;

/// <summary>
/// Query to retrieve all active products for the current tenant.
/// </summary>
public record GetProductsQuery : IQuery<IReadOnlyList<ProductDto>>;

/// <summary>
/// Read model returned to callers. Never exposes the domain entity directly.
/// </summary>
public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    string UnitCode,
    string Description,
    string Status);