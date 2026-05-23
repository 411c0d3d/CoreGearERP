namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Provides current tenant context throughout the request pipeline.
/// Resolved from the JWT claim in the Host layer.
/// Injected into DbContexts to scope all queries automatically.
/// </summary>
public interface ICurrentTenant
{
    Guid TenantId { get; }
}