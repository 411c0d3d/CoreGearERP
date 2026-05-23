using System.Text;
using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Host.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Registers host-level services and configures the request pipeline.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Registers infrastructure services common to the entire host.
    /// </summary>
    public static IServiceCollection AddHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, CurrentTenantService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        var secretKey = configuration["Auth:SecretKey"]
                        ?? throw new InvalidOperationException("Auth:SecretKey is not configured.");

        var issuer = configuration["Auth:Issuer"];
        var audience = configuration["Auth:Audience"];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey))
                };
            });

        services.AddAuthorization();
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }

    /// <summary>
    /// Registers middleware in the correct pipeline order.
    /// </summary>
    public static WebApplication UseHost(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantResolutionMiddleware>();

        return app;
    }
}