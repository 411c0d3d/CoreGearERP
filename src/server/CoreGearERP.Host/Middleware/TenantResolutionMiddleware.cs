using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Host.Middleware;

/// <summary>Resolves tenant context from JWT claims on every authenticated request.</summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The Next Request Delegate.</param>
    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id");

            if (tenantClaim is null || !Guid.TryParse(tenantClaim.Value, out _))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant context is missing or invalid." });
                return;
            }
        }

        await _next(context);
    }
}