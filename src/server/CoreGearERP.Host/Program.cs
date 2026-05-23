using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Host.Extensions;
using CoreGearERP.Inventory.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CoreGearERP");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    builder.Services
        .AddHost(builder.Configuration)
        .AddInventoryModule(builder.Configuration);

    var app = builder.Build();

    app.UseHost();
    app.UseSerilogRequestLogging();

    app.MapGet("/", () => "CoreGearERP");
    app.MapDevTokenEndpoint();

    app.MapGet("/me", (ICurrentUser user, ICurrentTenant tenant) =>
    {
        return Results.Ok(new
        {
            userId   = user.UserId,
            email    = user.Email,
            tenantId = tenant.TenantId
        });
    }).RequireAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CoreGearERP failed to start");
}
finally
{
    Log.CloseAndFlush();
}