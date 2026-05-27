using CoreGearERP.Common.Application.Events;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Finance.Domain.Entities;
using CoreGearERP.Finance.Domain.Enums;
using CoreGearERP.Finance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Finance.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes ProductionOrderCompletedEvent and creates a pending cost entry.
/// Amount is zero -- production costing requires reconciliation against component prices.
/// Idempotent via MassTransit inbox.
/// </summary>
public class ProductionOrderCompletedConsumer : IConsumer<ProductionOrderCompletedEvent>
{
    private readonly FinanceDbContext _context;

    /// <summary>
    /// Constructor with dependencies injected. Used for consuming ProductionOrderCompletedEvent.
    /// </summary>
    /// <param name="context">Database context for finance.</param>
    public ProductionOrderCompletedConsumer(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Consumes ProductionOrderCompletedEvent. Validates that an open financial period exists for the event date, then creates a pending cost entry. Throws if no open period is found.
    /// </summary>
    /// <param name="context">MassTransit consume context containing the ProductionOrderCompletedEvent message.</param>
    public async Task Consume(ConsumeContext<ProductionOrderCompletedEvent> context)
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

        var entry = CostEntry.Create(
            periodId: period.Id,
            sourceDocumentId: message.ProductionOrderId,
            sourceDocumentNumber: message.ProductionOrderNumber,
            sourceType: CostEntrySourceType.ProductionOrder,
            amount: new Money(0m, "EUR"),
            isPendingCosting: true,
            tenantId: message.TenantId,
            createdBy: Guid.Empty);

        _context.CostEntries.Add(entry);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}