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

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    builder.Services
        .AddHost(builder.Configuration)
        .AddInventoryModule(builder.Configuration);

    var app = builder.Build();

    app.UseHost();

    app.MapGet("/", () => "CoreGearERP");
    app.MapDevTokenEndpoint();
    app.MapInventoryEndpoints();
    
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var urls = string.Join(", ", app.Urls);
        Log.Information("CoreGearERP running on {Environment} | Listening on {Urls}",
            app.Environment.EnvironmentName,
            urls);
    });    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CoreGearERP failed to start");
}
finally
{
    Log.CloseAndFlush();
}