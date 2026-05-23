using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Temporary token generation for local development and M1 testing only.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Maps a test token generation endpoint. Remove before production.
    /// </summary>
    public static WebApplication MapDevTokenEndpoint(this WebApplication app)
    {
        app.MapPost("/dev/token", (IConfiguration config) =>
        {
            var secretKey = config["Auth:SecretKey"]!;
            var issuer = config["Auth:Issuer"]!;
            var audience = config["Auth:Audience"]!;

            var tenantId = Guid.Parse(config["Dev:TenantId"]
                                      ?? throw new InvalidOperationException("Dev:TenantId is not configured."));

            var userId = Guid.Parse(config["Dev:UserId"]
                                    ?? throw new InvalidOperationException("Dev:UserId is not configured."));

            var email = config["Dev:Email"]
                        ?? throw new InvalidOperationException("Dev:Email is not configured.");

            var claims = new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("email", email),
                new Claim("tenant_id", tenantId.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new { token = tokenString, tenantId, userId });
        });

        return app;
    }
}