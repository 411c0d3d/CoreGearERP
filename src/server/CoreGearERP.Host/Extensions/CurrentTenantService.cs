using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Resolves current tenant from the JWT tenant_id claim.
/// </summary>
public class CurrentTenantService : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentTenantService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id");

            if (claim is null || !Guid.TryParse(claim.Value, out var tenantId))
            {
                throw new InvalidOperationException("Tenant context is not available.");
            }

            return tenantId;
        }
    }
}