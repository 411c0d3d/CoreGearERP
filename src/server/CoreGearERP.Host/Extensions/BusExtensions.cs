using CoreGearERP.Finance.Infrastructure.Messaging.Consumers;
using CoreGearERP.Finance.Infrastructure.Persistence;
using CoreGearERP.Messaging.Infrastructure.Persistence;
using CoreGearERP.Procurement.Infrastructure.Persistence;
using CoreGearERP.Production.Infrastructure.Persistence;
using CoreGearERP.Sales.Infrastructure.Persistence;
using MassTransit;

namespace CoreGearERP.Host.Extensions;

/// <summary>
/// Provides extension methods for configuring MassTransit with RabbitMQ transport and outbox.
/// </summary>
public static class BusExtensions
{
    /// <summary>
    /// Configures MassTransit with RabbitMQ transport and outbox.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static IServiceCollection AddBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<OutboxDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.AddConsumer<GoodsReceivedConsumer>();
            x.AddConsumer<ProductionOrderCompletedConsumer>();
            x.AddConsumer<SalesOrderShippedConsumer>();

            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                if (name is nameof(GoodsReceivedConsumer)
                    or nameof(ProductionOrderCompletedConsumer)
                    or nameof(SalesOrderShippedConsumer))
                {
                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(30)));

                    cfg.UseEntityFrameworkOutbox<FinanceDbContext>(context);
                }
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var port = configuration.GetValue<ushort>("RabbitMq:Port", 5672);
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";
                var vhost = configuration["RabbitMq:VirtualHost"] ?? "/";

                cfg.Host(host, port, vhost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}