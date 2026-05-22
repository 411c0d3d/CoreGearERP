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
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("sub");

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
            return _httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value
                   ?? throw new InvalidOperationException("User email claim is not available.");
        }
    }
}