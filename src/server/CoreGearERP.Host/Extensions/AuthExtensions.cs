using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoreGearERP.Common.Application.Interfaces;
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
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapPost("/dev/token", (IConfiguration config, IWebHostEnvironment env) =>
        {
            var secretKey = config["Auth:SecretKey"]!;
            var issuer = config["Auth:Issuer"]!;
            var audience = config["Auth:Audience"]!;

            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var claims = new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("email", "dev@coregear.local"),
                new Claim("tenant_id", tenantId.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Dev gets a 30-day token so you never hit expiry during development.
            // The endpoint does not exist outside Development so this never reaches prod.
            var expiry = DateTime.UtcNow.AddDays(30);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                token = tokenString,
                tenantId,
                userId,
                expires = expiry
            });
        });

        app.MapGet("/me", (ICurrentUser user, ICurrentTenant tenant) =>
        {
            return Results.Ok(new
            {
                userId   = user.UserId,
                email    = user.Email,
                tenantId = tenant.TenantId
            });
        }).RequireAuthorization();
        
        return app;
    }
}