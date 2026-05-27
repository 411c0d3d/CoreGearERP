using CoreGearERP.Finance.Extensions;
using CoreGearERP.Host.Extensions;
using CoreGearERP.Inventory.Extensions;
using CoreGearERP.Inventory.Infrastructure.gRPC;
using CoreGearERP.Messaging.Extensions;
using CoreGearERP.Procurement.Extensions;
using CoreGearERP.Production.Extensions;
using CoreGearERP.Sales.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CoreGearERP");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    builder.Services
        .AddHost(builder.Configuration)
        .AddInventoryModule(builder.Configuration)
        .AddProcurementModule(builder.Configuration)
        .AddProductionModule(builder.Configuration)
        .AddSalesModule(builder.Configuration)
        .AddFinance(builder.Configuration)
        .AddMessaging(builder.Configuration)
        .AddBus(builder.Configuration);

    var app = builder.Build();

    app.UseHost();

    app.MapGrpcService<InventoryCommandGrpcService>();
    app.MapGrpcService<InventoryQueryGrpcService>();

    app.MapGet("/", () => "CoreGearERP");
    app.MapDevTokenEndpoint();
    app.MapTestEndpoints();
    app.MapInventoryEndpoints();
    app.MapProcurementEndpoints();
    app.MapProductionEndpoints();
    app.MapSalesEndpoints();
    app.MapFinanceEndpoints();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var urls = string.Join(", ", app.Urls);
        Log.Information("CoreGearERP running on {Environment} | Listening on {Urls}",
            app.Environment.EnvironmentName,
            urls);
    });
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

public partial class Program { }