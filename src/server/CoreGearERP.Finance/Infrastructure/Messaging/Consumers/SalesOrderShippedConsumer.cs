using CoreGearERP.Common.Application.Events;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Finance.Domain.Entities;
using CoreGearERP.Finance.Domain.Enums;
using CoreGearERP.Finance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Finance.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes SalesOrderShippedEvent and creates a cost entry for the shipment revenue.
/// Idempotent via MassTransit inbox.
/// </summary>
public class SalesOrderShippedConsumer : IConsumer<SalesOrderShippedEvent>
{
    private readonly FinanceDbContext _context;

    /// <summary>
    /// Constructor with dependencies injected. Used for consuming SalesOrderShippedEvent and creating cost entries.
    /// </summary>
    /// <param name="context">Database context for finance.</param>
    public SalesOrderShippedConsumer(FinanceDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Consumes SalesOrderShippedEvent. Validates there's an open financial period for the shipment date,
    /// then creates a cost entry for the shipment revenue. Throws if no open period is found, which should prevent processing until a period is opened. Idempotent via MassTransit inbox.
    /// </summary>
    /// <param name="context">MassTransit consume context containing the SalesOrderShippedEvent message.</param>
    public async Task Consume(ConsumeContext<SalesOrderShippedEvent> context)
    {
        var message = context.Message;

        var period = await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => p.TenantId == message.TenantId
                                   && p.Status == FinancialPeriodStatus.Open.ToString()
                                   && p.StartDate <= message.OccurredAt
                                   && p.EndDate >= message.OccurredAt,
                context.CancellationToken);

        if (period is null)
        {
            throw new InvalidOperationException(
                $"No open financial period found for tenant {message.TenantId} covering {message.OccurredAt:yyyy-MM-dd}. " +
                "Open a period before processing cost entries.");
        }

        var amount = new Money(message.TotalAmount, message.CurrencyCode);

        var entry = CostEntry.Create(
            periodId:             period.Id,
            sourceDocumentId:     message.ShipmentId,
            sourceDocumentNumber: message.ShipmentNumber,
            sourceType:           CostEntrySourceType.Shipment,
            amount:               amount,
            isPendingCosting:     false,
            tenantId:             message.TenantId,
            createdBy:            Guid.Empty);

        _context.CostEntries.Add(entry);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}