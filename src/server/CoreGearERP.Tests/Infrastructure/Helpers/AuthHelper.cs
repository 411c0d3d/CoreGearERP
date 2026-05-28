using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoreGearERP.Tests.Infrastructure.Helpers;

/// <summary>
/// Generates signed JWTs for authenticating against the test application host.
/// </summary>
public static class AuthHelper
{
    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal const string TestSigningKey = "test-integration-signing-key-32ch!!";
    internal const string Issuer = "coregear-api";
    internal const string Audience = "coregear-client";

    /// <summary>
    /// Creates a valid Bearer token string for the shared test tenant and user.
    /// </summary>
    public static string CreateBearerToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, TestUserId.ToString()),
            new Claim("email", "dev@coregear.local"),
            new Claim("tenant_id", TestTenantId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Returns a formatted Authorization header value.
    /// </summary>
    public static string BearerHeaderValue() => $"Bearer {CreateBearerToken()}";
}