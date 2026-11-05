using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;
using Ppdm = Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The role dashboards' figures are read from the rows that hold them, and a figure no row holds is null — not 0 and not
/// a copy of another figure (2026-10-10). Each arm hands the real service a data source holding known rows.
/// </summary>
public class RoleKpiFiguresTests
{
    private const string Connection = "PPDM39";

    private sealed class Rows
    {
        public Mock<IDMEEditor> Editor { get; } = new();
        public Mock<IDataSource> Source { get; } = new();

        public Rows()
        {
            Editor.Setup(e => e.GetDataSource(Connection)).Returns(Source.Object);
        }

        public Rows In(string table, params object[] rows)
        {
            Source.Setup(s => s.GetEntityAsync(table, It.IsAny<List<AppFilter>>())).ReturnsAsync(rows);
            return this;
        }

        public Rows InWhen(string table, Func<List<AppFilter>, bool> when, params object[] rows)
        {
            Source.Setup(s => s.GetEntityAsync(table, It.Is<List<AppFilter>>(filters => when(filters)))).ReturnsAsync(rows);
            return this;
        }
    }

    private static bool Names(List<AppFilter> filters, string column, string value) =>
        filters.Any(f => f.FieldName == column && f.FilterValue == value);

    private static HSEKPIService KpiService(Rows rows) => new(
        rows.Editor.Object, new Mock<ICommonColumnHandler>().Object, new Mock<IPPDM39DefaultsRepository>().Object,
        new Mock<IPPDMMetadataRepository>().Object, Connection, NullLogger<HSEKPIService>.Instance);

    private static Rows ThreeIncidents() => new Rows()
        .In("HSE_INCIDENT",
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "A", ACTIVE_IND = "Y" },
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "B", ACTIVE_IND = "Y" },
            // INCIDENT_CLASS_ID is written by nothing in the system; the old Tier 1 count read it.
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "C", ACTIVE_IND = "Y", INCIDENT_CLASS_ID = "TIER_1" })
        .In("HSE_INCIDENT_DETAIL",
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "A", DETAIL_OBS_NO = 2, DETAIL_TYPE = "SUPPLEMENTARY", INCIDENT_SEVERITY_ID = "TIER_3" },
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "A", DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = "TIER_1" },
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "B", DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = IncidentType.PseTier2 })
        .In("HSE_INCIDENT_RESPONSE",
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Reported, EFFECTIVE_DATE = new DateTime(2026, 9, 1) },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 2, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Closed, EFFECTIVE_DATE = new DateTime(2026, 9, 20) },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "B", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.UnderInvestigation, EFFECTIVE_DATE = new DateTime(2026, 9, 5) });

    [Fact]
    public async Task IncidentCounts_TakeEachTierFromThePrimaryDetail_AndOpenFromTheLatestStatus()
    {
        var counts = await KpiService(ThreeIncidents()).CountIncidentsAsync(null, null, null);

        Assert.Equal(3, counts.Total);
        // A is closed; B is under investigation; C has no status recorded, so it stands as reported.
        Assert.Equal(2, counts.Open);
        Assert.Equal(1, counts.Tier1);
        Assert.Equal(1, counts.Tier2);
        Assert.Equal(0, counts.Tier3);
        // C records no detail, so its tier is unrecorded (4) whatever its class column says.
        Assert.Equal(1, counts.Tier4);
    }

    [Fact]
    public async Task IncidentCounts_ForAField_CountOnlyTheIncidentsItsComponentsName()
    {
        var rows = ThreeIncidents().InWhen("HSE_INCIDENT_COMPONENT", f => Names(f, "FIELD_ID", "FIELD-7"),
            new Ppdm.HSE_INCIDENT_COMPONENT { INCIDENT_ID = "B", COMPONENT_OBS_NO = 1, FIELD_ID = "FIELD-7" });

        var counts = await KpiService(rows).CountIncidentsAsync("FIELD-7", null, null);

        Assert.Equal(1, counts.Total);
        Assert.Equal(1, counts.Open);
        Assert.Equal(1, counts.Tier2);
        Assert.Equal(0, counts.Tier1);
    }

    private static DrillingEngineerAggregationService DrillingService(Rows rows) => new(
        rows.Editor.Object, new Mock<ICommonColumnHandler>().Object, new Mock<IPPDM39DefaultsRepository>().Object,
        new Mock<IPPDMMetadataRepository>().Object, Connection);

    [Fact]
    public async Task DrillingKpi_CountsProducingWellsByStatus_AndReadsAfeAndNptFromTheProgrammesAndTheirReports()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var rows = new Rows()
            .In("WELL",
                new Ppdm.WELL { UWI = "W1", CURRENT_STATUS = "PRODUCING" },
                new Ppdm.WELL { UWI = "W2", CURRENT_STATUS = "Active" },
                new Ppdm.WELL { UWI = "W3", CURRENT_STATUS = "DRILLING" })
            .In("DRILLING_PROGRAM",
                new DRILLING_PROGRAM { PROGRAM_ID = "P1", AFE_COST = 1_000_000m, AFE_CURRENCY = "USD" },
                // No AFE: neither its budget nor its spend is added to the AFE figures.
                new DRILLING_PROGRAM { PROGRAM_ID = "P2" })
            .In("DRILLING_DAILY_REPORT",
                new DRILLING_DAILY_REPORT { PROGRAM_ID = "P1", REPORT_NUMBER = 1, REPORT_DATE = monthStart, DAILY_COST = 100_000m, CUMULATIVE_COST = 100_000m, NPT_HOURS = 3m },
                new DRILLING_DAILY_REPORT { PROGRAM_ID = "P1", REPORT_NUMBER = 2, REPORT_DATE = monthStart.AddHours(24), DAILY_COST = 160_000m, CUMULATIVE_COST = 260_000m, NPT_HOURS = 2m },
                new DRILLING_DAILY_REPORT { PROGRAM_ID = "P1", REPORT_NUMBER = 0, REPORT_DATE = monthStart.AddDays(-1), NPT_HOURS = 10m },
                new DRILLING_DAILY_REPORT { PROGRAM_ID = "P2", REPORT_NUMBER = 1, REPORT_DATE = monthStart, DAILY_COST = 50_000m, NPT_HOURS = 1m });

        var kpi = await DrillingService(rows).GetKpiAsync();

        Assert.Equal(3, kpi.TotalWells);
        Assert.Equal(1, kpi.ProducingWells);
        Assert.Equal(1_000_000m, kpi.AfeBudget);
        Assert.Equal(260_000m, kpi.AfeSpent);
        Assert.Equal("USD", kpi.AfeCurrency);
        // This month's reports only: 3 + 2 for P1 and 1 for P2; last month's 10 hours are not this month's.
        Assert.Equal(6m, kpi.NptHours);
    }

    [Fact]
    public async Task DrillingKpi_WithNoProgrammeRecords_LeavesAfeAndNptNull()
    {
        var rows = new Rows()
            .In("WELL", new Ppdm.WELL { UWI = "W1", CURRENT_STATUS = "DRILLING" })
            .In("DRILLING_PROGRAM")
            .In("DRILLING_DAILY_REPORT");

        var kpi = await DrillingService(rows).GetKpiAsync();

        Assert.Equal(1, kpi.TotalWells);
        Assert.Equal(0, kpi.ProducingWells);
        Assert.Null(kpi.AfeBudget);
        Assert.Null(kpi.AfeSpent);
        Assert.Null(kpi.NptHours);
    }

    [Fact]
    public async Task DrillingKpi_DoesNotAddAfeCostsInDifferentCurrencies()
    {
        var rows = new Rows()
            .In("WELL")
            .In("DRILLING_PROGRAM",
                new DRILLING_PROGRAM { PROGRAM_ID = "P1", AFE_COST = 1_000_000m, AFE_CURRENCY = "USD" },
                new DRILLING_PROGRAM { PROGRAM_ID = "P2", AFE_COST = 2_000_000m, AFE_CURRENCY = "CAD" })
            .In("DRILLING_DAILY_REPORT");

        var kpi = await DrillingService(rows).GetKpiAsync();

        Assert.Null(kpi.AfeBudget);
        Assert.Null(kpi.AfeSpent);
    }

    [Fact]
    public async Task DrillingKpi_ForAField_AsksForThatFieldsWellsAndProgrammes()
    {
        var rows = new Rows()
            .InWhen("WELL", f => Names(f, "ASSIGNED_FIELD", "FIELD-7"), new Ppdm.WELL { UWI = "W1", CURRENT_STATUS = "PRODUCING" })
            .InWhen("WELL", f => !f.Any(x => x.FieldName == "ASSIGNED_FIELD"),
                new Ppdm.WELL { UWI = "W1" }, new Ppdm.WELL { UWI = "W2" })
            .InWhen("DRILLING_PROGRAM", f => Names(f, "FIELD_ID", "FIELD-7"))
            .In("DRILLING_DAILY_REPORT");

        var kpi = await DrillingService(rows).GetKpiAsync("FIELD-7");

        Assert.Equal(1, kpi.TotalWells);
        Assert.Equal(1, kpi.ProducingWells);
    }

    [Fact]
    public async Task ReservoirKpi_CountsWellsOnInjection_AndLeavesTheUncomputedRecoveryFactorNull()
    {
        var rows = new Rows()
            .In("POOL",
                new Ppdm.POOL { POOL_ID = "POOL-1", DISCOVERY_DATE = new DateTime(2019, 4, 1) },
                new Ppdm.POOL { POOL_ID = "POOL-2" })
            .In("WELL", new Ppdm.WELL { UWI = "W1", ASSIGNED_FIELD = "FIELD-7" });
        var injection = new Mock<IEnhancedRecoveryService>(MockBehavior.Strict);
        injection.Setup(s => s.GetInjectionOperationsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InjectionOperation>
            {
                new() { OperationId = "I-1", WellUWI = "W1" },
                new() { OperationId = "I-2", WellUWI = "W1" },
                new() { OperationId = "I-3", WellUWI = "W2" },
            });
        var cch = new Mock<ICommonColumnHandler>().Object;
        var defaults = new Mock<IPPDM39DefaultsRepository>().Object;
        var metadata = new Mock<IPPDMMetadataRepository>().Object;
        var service = new ReservoirEngineerAggregationService(rows.Editor.Object, cch, defaults, metadata, injection.Object,
            new WellServices(rows.Editor.Object, cch, defaults, metadata, Connection), Connection);

        var everyField = await service.GetKpiAsync();
        var oneField = await service.GetKpiAsync("FIELD-7");

        Assert.Equal(2, everyField.ActivePools);
        Assert.Equal(1, everyField.PoolsWithDiscovery);
        Assert.Equal(2, everyField.PressureMaintenanceWells);
        Assert.Null(everyField.RecoveryFactor);
        // In one field, only that field's wells on injection count.
        Assert.Equal(1, oneField.PressureMaintenanceWells);
    }
}
