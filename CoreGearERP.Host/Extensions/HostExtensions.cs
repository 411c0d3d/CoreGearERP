using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Host.Middleware;

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

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration["Auth:Authority"];
                options.Audience = configuration["Auth:Audience"];
            });

        services.AddAuthorization();

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