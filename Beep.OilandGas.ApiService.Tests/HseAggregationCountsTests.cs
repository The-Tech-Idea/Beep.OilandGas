using System;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The HSE aggregations answer the KPI service's counts — the one owner of an incident's tier and state — rather than
/// counting HSE_INCIDENT.INCIDENT_CLASS_ID, which nothing writes (2026-10-10).
/// </summary>
public class HseAggregationCountsTests
{
    [Fact]
    public async Task IncidentSummary_AnswersTheKpiServiceTierCounts_ForTheFieldAndRangeAsked()
    {
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 9, 30);
        var incidents = new Mock<IHSEKPIService>(MockBehavior.Strict);
        incidents.Setup(s => s.CountIncidentsAsync("FIELD-7", start, end))
            .ReturnsAsync(new IncidentCounts(Total: 9, Open: 4, Tier1: 2, Tier2: 3, Tier3: 1, Tier4: 3));

        var summary = await new HseAggregationService(incidents.Object).GetIncidentSummaryAsync("FIELD-7", start, end);

        Assert.Equal(9, summary.Total);
        Assert.Equal(2, summary.Tier1);
        Assert.Equal(3, summary.Tier2);
        incidents.VerifyAll();
    }

    [Fact]
    public async Task HseKpi_TotalIsThisYearsIncidents_AndOpenIsEveryIncidentNotClosedOrCancelled()
    {
        var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
        var incidents = new Mock<IHSEKPIService>(MockBehavior.Strict);
        incidents.Setup(s => s.CountIncidentsAsync(null, yearStart, null))
            .ReturnsAsync(new IncidentCounts(Total: 5, Open: 1, Tier1: 0, Tier2: 0, Tier3: 0, Tier4: 5));
        // An incident reported last year and still under investigation is open: the open count is over every incident.
        incidents.Setup(s => s.CountIncidentsAsync(null, null, null))
            .ReturnsAsync(new IncidentCounts(Total: 12, Open: 3, Tier1: 0, Tier2: 0, Tier3: 0, Tier4: 12));

        var kpi = await new HseEngineerAggregationService(incidents.Object).GetKpiAsync();

        Assert.Equal(5, kpi.TotalIncidents);
        Assert.Equal(3, kpi.OpenIncidents);
        incidents.VerifyAll();
    }
}
