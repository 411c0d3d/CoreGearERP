using System.Text;
using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Host.Infrastructure;
using CoreGearERP.Host.Infrastructure.Behaviors;
using CoreGearERP.Host.Middleware;
using CoreGearERP.Inventory.Infrastructure.gRPC;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using CoreGearERP.Inventory.gRPC;

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
        services.AddScoped<IDispatcher, Dispatcher>();

        // Global exception handling with Problem Details responses.
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Pipeline behaviors run in registration order.
        // Logging wraps everything. Validation runs before the handler.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        // gRPC clients for Inventory module. The address is read from configuration with a fallback to localhost.
        services.AddGrpc();
        services.AddGrpcClient<InventoryCommands.InventoryCommandsClient>(options =>
        {
            options.Address = new Uri(configuration["Grpc:InventoryUrl"]
                                      ?? "http://localhost:5015");
        });

        services.AddGrpcClient<InventoryQueries.InventoryQueriesClient>(options =>
        {
            options.Address = new Uri(configuration["Grpc:InventoryUrl"]
                                      ?? "http://localhost:5015");
        });

        services.AddScoped<IInventoryCommandService, InventoryCommandGrpcClient>();
        services.AddScoped<IInventoryQueryService, InventoryQueryGrpcClient>();
        
        return services;
    }

    /// <summary>
    /// Registers middleware in the correct pipeline order.
    /// </summary>
    public static WebApplication UseHost(this WebApplication app)
    {
        app.UseExceptionHandler();
        
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) =>
                ex is null
                    ? Serilog.Events.LogEventLevel.Information
                    : Serilog.Events.LogEventLevel.Warning;
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantResolutionMiddleware>();

        return app;
    }
}