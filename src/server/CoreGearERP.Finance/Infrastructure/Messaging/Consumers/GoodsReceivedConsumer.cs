using CoreGearERP.Common.Application.Events;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Finance.Domain.Entities;
using CoreGearERP.Finance.Domain.Enums;
using CoreGearERP.Finance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Finance.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes GoodsReceivedEvent and creates a cost entry for the goods receipt.
/// Idempotent via MassTransit inbox -- duplicate messages are discarded automatically.
/// </summary>
public class GoodsReceivedConsumer : IConsumer<GoodsReceivedEvent>
{
    private readonly FinanceDbContext _context;

    /// <summary>
    /// Constructor with dependencies injected. Used for consuming GoodsReceivedEvent and creating cost entries.
    /// </summary>
    /// <param name="context">Database context for finance.</param>
    public GoodsReceivedConsumer(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Consumes GoodsReceivedEvent. Validates that an open financial period exists for the event date, then creates a cost entry for the goods receipt. Throws if no open period is found.
    /// </summary>
    /// <param name="context">MassTransit consume context containing the GoodsReceivedEvent message.</param>
    public async Task Consume(ConsumeContext<GoodsReceivedEvent> context)
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
            periodId: period.Id,
            sourceDocumentId: message.PurchaseOrderId,
            sourceDocumentNumber: message.PurchaseOrderNumber,
            sourceType: CostEntrySourceType.GoodsReceipt,
            amount: amount,
            isPendingCosting: false,
            tenantId: message.TenantId,
            createdBy: Guid.Empty);

        _context.CostEntries.Add(entry);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
} 