using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Finance.Domain.Entities;
using CoreGearERP.Finance.Domain.Enums;
using CoreGearERP.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreGearERP.Finance.Extensions;

/// <summary>
/// HTTP endpoint registrations for the Finance module.
/// </summary>
public static class FinanceEndpoints
{
    /// <summary>
    /// Registers finance-related API endpoints under the "/finance" route prefix. All endpoints require authentication.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder to which the finance endpoints will be added.</param>
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/finance").RequireAuthorization();

        group.MapPost("/periods", CreatePeriod);
        group.MapGet("/periods", GetPeriods);
        group.MapGet("/cost-entries", GetCostEntries);
        group.MapGet("/cost-entries/by-source/{sourceDocumentId:guid}", GetCostEntriesBySource);

        return app;
    }

    private static async Task<IResult> CreatePeriod(
        CreatePeriodRequest request,
        FinanceDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var existing = await context.FinancialPeriods
            .AnyAsync(p => p.TenantId == currentTenant.TenantId
                           && p.Name == request.Name,
                ct);

        if (existing)
        {
            return Results.Conflict($"A financial period named '{request.Name}' already exists.");
        }

        var period = FinancialPeriod.Create(
            name: request.Name,
            startDate: request.StartDate,
            endDate: request.EndDate,
            tenantId: currentTenant.TenantId,
            createdBy: currentUser.UserId);

        context.FinancialPeriods.Add(period);
        await context.SaveChangesAsync(ct);

        return Results.Created($"/finance/periods/{period.Id}", new { period.Id, period.Name });
    }

    private static async Task<IResult> GetPeriods(
        FinanceDbContext context,
        ICurrentTenant currentTenant,
        CancellationToken ct)
    {
        var periods = await context.FinancialPeriods
            .Where(p => p.TenantId == currentTenant.TenantId)
            .AsNoTracking()
            .OrderByDescending(p => p.StartDate)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.StartDate,
                p.EndDate,
                p.Status
            })
            .ToListAsync(ct);

        return Results.Ok(periods);
    }

    private static async Task<IResult> GetCostEntries(
        FinanceDbContext context,
        ICurrentTenant currentTenant,
        CancellationToken ct,
        string? sourceType = null)
    {
        var query = context.CostEntries
            .AsNoTracking()
            .Where(e => e.TenantId == currentTenant.TenantId);

        if (!string.IsNullOrWhiteSpace(sourceType)
            && Enum.TryParse<CostEntrySourceType>(sourceType, ignoreCase: true, out var parsed))
        {
            query = query.Where(e => e.SourceType == parsed);
        }

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.PeriodId,
                e.SourceDocumentId,
                e.SourceDocumentNumber,
                SourceType = e.SourceType.ToString(),
                e.Amount.Amount,
                e.Amount.CurrencyCode,
                e.IsPendingCosting,
                e.IsReversal,
                e.CreatedAt
            })
            .ToListAsync(ct);

        return Results.Ok(entries);
    }

    private static async Task<IResult> GetCostEntriesBySource(
        Guid sourceDocumentId,
        FinanceDbContext context,
        ICurrentTenant currentTenant,
        CancellationToken ct)
    {
        var entries = await context.CostEntries
            .AsNoTracking() // EF Core cannot track owned types without their owner in the same query shape.
            .Where(e => e.TenantId == currentTenant.TenantId
                        && e.SourceDocumentId == sourceDocumentId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.PeriodId,
                e.SourceDocumentId,
                e.SourceDocumentNumber,
                SourceType = e.SourceType.ToString(),
                e.Amount.Amount,
                e.Amount.CurrencyCode,
                e.IsPendingCosting,
                e.IsReversal,
                e.CreatedAt
            })
            .ToListAsync(ct);

        return Results.Ok(entries);
    }
}

/// <summary>
/// Request body for creating a financial period.
/// </summary>
/// <param name="Name">The name of the financial period. Must be unique within the tenant.</param>
/// <param name="StartDate">The start date of the financial period.</param>
/// <param name="EndDate">The end date of the financial period. Must be after the start date.</param>
public record CreatePeriodRequest(string Name, DateTime StartDate, DateTime EndDate);