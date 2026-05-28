using System.Net;
using System.Net.Http.Json;
using CoreGearERP.Tests.Infrastructure;
using CoreGearERP.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Xunit;

namespace CoreGearERP.Tests.Modules.Finance;

/// <summary>
/// Integration tests for Finance periods and cost entry endpoints.
/// </summary>
public sealed class FinanceTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared integration test fixture.</param>
    public FinanceTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetPeriods_AfterReset_CurrentPeriodExists()
    {
        var response = await Client.GetAsync("/finance/periods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PeriodResponse>>();
        body.Should().NotBeNull();
        body.Should().Contain(p => p.Name == DateTime.UtcNow.ToString("yyyy-MM"));
    }

    [Fact]
    public async Task CreatePeriod_ValidRequest_Returns201WithId()
    {
        var response = await Client.PostAsJsonAsync("/finance/periods", new
        {
            name = "2099-01",
            startDate = new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            endDate = new DateTime(2099, 1, 31, 23, 59, 59, DateTimeKind.Utc),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<PeriodCreatedResponse>();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("2099-01");
    }

    [Fact]
    public async Task CreatePeriod_DuplicateName_Returns409()
    {
        await Client.PostAsJsonAsync("/finance/periods", new
        {
            name = "2099-02",
            startDate = new DateTime(2099, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            endDate = new DateTime(2099, 2, 28, 23, 59, 59, DateTimeKind.Utc),
        });

        var response = await Client.PostAsJsonAsync("/finance/periods", new
        {
            name = "2099-02",
            startDate = new DateTime(2099, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            endDate = new DateTime(2099, 2, 28, 23, 59, 59, DateTimeKind.Utc),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCostEntriesBySource_UnknownSourceId_ReturnsEmptyList()
    {
        var response = await Client.GetAsync($"/finance/cost-entries/by-source/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<object>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCostEntries_FilterByUnknownSourceType_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/finance/cost-entries?sourceType=UnknownType");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<CostEntryResponse>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCostEntries_NoFilter_ReturnsEmptyListOnFreshDatabase()
    {
        var response = await Client.GetAsync("/finance/cost-entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<CostEntryResponse>>();
        body.Should().BeEmpty();
    }

    private sealed record PeriodResponse(Guid Id, string Name, DateTime StartDate, DateTime EndDate);
    private sealed record PeriodCreatedResponse(Guid Id, string Name);
    private sealed record CostEntryResponse(Guid Id, string SourceType, decimal Amount, bool IsPendingCosting);
}