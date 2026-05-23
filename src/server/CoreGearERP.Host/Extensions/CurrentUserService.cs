using System.Security.Claims;
using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Resolves current user from JWT claims.
/// </summary>
public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The Http Context Accessor</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // ASP.NET Core maps 'sub' to ClaimTypes.NameIdentifier internally.
            // Check both to be safe.
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user?.FindFirst("sub");

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
            {
                throw new InvalidOperationException("User context is not available.");
            }

            return userId;
        }
    }

    public string Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.FindFirst(ClaimTypes.Email)?.Value
                   ?? user?.FindFirst("email")?.Value
                   ?? throw new InvalidOperationException("User email claim is not available.");
        }
    }
}