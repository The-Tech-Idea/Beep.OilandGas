using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-02: the paging rule, the pre-production state, the unit rule, the water cut rule, the uptime rule, the field
/// status rule and the two source guards the feature document names (no figure that is not an argument, and no demo KPI
/// constant in the feature). The PPDM reads, the pool volumes and the failure path are proven in the gateway test.
/// </summary>
public sealed class FieldPoolRegisterTests
{
    private static readonly DateTime Day = new(2026, 10, 1);

    [Fact]
    public void The_register_pages_beyond_five_hundred_fields()
    {
        Assert.Equal(501, 501);

        Assert.Equal(6, FieldRegisterRules.PageCount(501, 100));
        Assert.Equal(5, FieldRegisterRules.PageCount(500, 100));
        Assert.Equal(0, FieldRegisterRules.PageCount(0, 100));
        Assert.Equal(2, FieldRegisterRules.PageCount(501, 500));

        // Beyond the last page the index is clamped, never extended.
        Assert.Equal(5, FieldRegisterRules.ClampPageIndex(9, 501, 100));
        Assert.Equal(0, FieldRegisterRules.ClampPageIndex(-3, 501, 100));

        Assert.Equal(FieldRegisterRules.DefaultPageSize, FieldRegisterRules.ClampPageSize(1));
        Assert.Equal(FieldRegisterRules.MaximumPageSize, FieldRegisterRules.ClampPageSize(10_000));

        var register = Register(total: 501, pageIndex: 5, pageSize: 100, page: [Field("F-501", "Field 501")]);

        Assert.True(register.HasPrevious);
        Assert.False(register.HasNext);
        Assert.Equal(6, register.PageCount);
        Assert.Contains("fields 501–501 of 501", register.PagingText, StringComparison.Ordinal);
        Assert.Contains("page 6 of 6", register.PagingText, StringComparison.Ordinal);

        var single = Register(total: 3, pageIndex: 0, pageSize: 100, page: [Field("F-1", "Field 1")]);

        Assert.False(single.HasPrevious);
        Assert.False(single.HasNext);
        Assert.Equal("3 field(s)", single.PagingText);
    }

    [Fact]
    public void A_field_that_has_not_produced_shows_an_em_dash_and_never_zero()
    {
        var preProduction = Field("F-1", "Alpha", firstProduction: null);
        var producing = Field(
            "F-2",
            "Bravo",
            firstProduction: Day.AddYears(-2),
            oil: 100m,
            oilUnit: "bbl",
            waterCut: 0.25m);

        Assert.True(preProduction.IsPreProduction);
        Assert.Equal("—", preProduction.OilText);
        Assert.Equal("—", preProduction.GasText);
        Assert.Equal("—", preProduction.WaterText);
        Assert.Equal("—", preProduction.WaterCutText);
        Assert.Equal("—", preProduction.FirstProductionText);
        Assert.Equal("pre_production", preProduction.StatusText);
        Assert.Contains("oil —", preProduction.Describe(), StringComparison.Ordinal);

        Assert.False(producing.IsPreProduction);
        Assert.Equal("100 bbl", producing.OilText);
        Assert.Equal("25%", producing.WaterCutText);
        Assert.Equal("2024-10-01", producing.FirstProductionText);
        Assert.Equal("producing", producing.StatusText);
    }

    [Fact]
    public void A_missing_unit_is_named_and_never_shown_as_a_bare_number()
    {
        var noUnit = Field("F-1", "Alpha", firstProduction: Day.AddYears(-2), oil: 100m, oilUnit: null);

        Assert.Equal("100 (unit not recorded)", noUnit.OilText);

        var excluded = Field("F-2", "Bravo", excludedRows: 3);

        Assert.Contains("3 row(s) excluded", excluded.ExcludedText, StringComparison.Ordinal);
        Assert.Contains("no unit recorded", excluded.ExcludedText, StringComparison.Ordinal);
        Assert.Equal("no row excluded for an unknown unit", Field("F-3", "Charlie").ExcludedText);

        var register = Register(total: 2, page: [excluded, Field("F-4", "Delta", excludedRows: 2)]);

        Assert.Equal(5, register.ExcludedUnitRows);
    }

    [Fact]
    public void Uptime_is_the_share_of_the_window_that_reported()
    {
        Assert.Null(FieldRegisterRules.Uptime(0));
        Assert.Equal(0.5m, FieldRegisterRules.Uptime(15));
        Assert.Equal(1m, FieldRegisterRules.Uptime(30));
        Assert.Equal(1m, FieldRegisterRules.Uptime(40));

        Assert.Equal("50%", Field("F-1", "Alpha", uptime: 0.5m).UptimeText);
        Assert.Equal("92.5%", Field("F-2", "Bravo", uptime: 0.925m).UptimeText);
        Assert.Equal("not recorded", Field("F-3", "Charlie", uptime: null).UptimeText);
    }

    [Fact]
    public void The_status_says_pre_production_producing_or_inactive()
    {
        Assert.Equal("PRE_PRODUCTION", FieldRegisterRules.Status(false, "Y"));
        Assert.Equal("PRE_PRODUCTION", FieldRegisterRules.Status(false, "N"));
        Assert.Equal("PRODUCING", FieldRegisterRules.Status(true, "Y"));
        Assert.Equal("PRODUCING", FieldRegisterRules.Status(true, null));
        Assert.Equal("INACTIVE", FieldRegisterRules.Status(true, "N"));
    }

    [Fact]
    public void A_water_cut_needs_oil_and_water_in_the_same_unit()
    {
        Assert.Equal(0.2m, FieldRegisterRules.WaterCut(80m, 20m, "bbl", "bbl"));
        Assert.Null(FieldRegisterRules.WaterCut(80m, 20m, "bbl", "m3"));
        Assert.Null(FieldRegisterRules.WaterCut(null, 20m, "bbl", "bbl"));

        Assert.Equal(
            "not computable",
            Field(
                "F-1",
                "Alpha",
                firstProduction: Day.AddYears(-2),
                oil: 80m,
                water: 20m,
                oilUnit: "bbl",
                waterUnit: "m3",
                waterCut: null).WaterCutText);
    }

    [Fact]
    public void The_dominant_unit_is_the_unit_most_rows_use()
    {
        Assert.Equal("bbl", FieldRegisterRules.DominantUnit(["bbl", "bbl", "m3"]));
        Assert.Null(FieldRegisterRules.DominantUnit([null, " "]));
    }

    [Fact]
    public void A_pool_that_has_not_produced_shows_an_em_dash_and_says_what_is_not_recorded()
    {
        var preProduction = Pool("P-1", "Upper", lastProduction: null, drive: null, reservoir: null);
        var producing = Pool("P-2", "Lower", lastProduction: Day, rate: 120m, cumulative: 9000m);

        Assert.True(preProduction.IsPreProduction);
        Assert.Equal("—", preProduction.RateText);
        Assert.Equal("—", preProduction.CumulativeText);
        Assert.Equal("pre-production", preProduction.StatusText);
        Assert.Equal("not recorded", preProduction.DriveText);
        Assert.Equal("not recorded", preProduction.ReservoirText);

        Assert.False(producing.IsPreProduction);
        Assert.Equal("120 bbl", producing.RateText);
        Assert.Equal("9,000 bbl", producing.CumulativeText);
        Assert.Equal("water drive", producing.DriveText);
        Assert.Contains("cumulative 9,000 bbl", producing.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_trend_names_a_month_without_production()
    {
        Assert.Equal("2026-09 not recorded", new OgMonthPoint(new DateTime(2026, 9, 1), null, "bbl").Text);
        Assert.Equal("2026-09 120 bbl", new OgMonthPoint(new DateTime(2026, 9, 1), 120m, "bbl").Text);
        Assert.Equal("2026-09", new OgMonthPoint(new DateTime(2026, 9, 1), null, null).Label);
    }

    [Fact]
    public void The_register_reports_its_pre_production_fields_and_its_selection()
    {
        var register = new OgFieldRegister(
            "Alpha field",
            "F-1",
            Day,
            [Field("F-1", "Alpha", firstProduction: null), Field("F-2", "Bravo", firstProduction: Day)],
            2,
            0,
            100,
            1,
            Field("F-1", "Alpha", firstProduction: null),
            [Pool("P-1", "Upper", lastProduction: Day)],
            "P-1",
            Pool("P-1", "Upper", lastProduction: Day),
            [new OgMonthPoint(new DateTime(2026, 9, 1), 10m, "bbl")],
            "Alpha field · monthly oil · 2026-09 10 bbl");

        Assert.Contains("2 field(s)", register.Summary, StringComparison.Ordinal);
        Assert.Contains("1 pre-production", register.Summary, StringComparison.Ordinal);
        Assert.Contains("1 pool(s) of Alpha", register.SelectedPoolsText, StringComparison.Ordinal);

        var noPools = register with { Pools = [], SelectedPool = null };

        Assert.Contains("has no pool recorded", noPools.SelectedPoolsText, StringComparison.Ordinal);
        Assert.Equal("Choose a field to see its pools.", (register with { Selected = null }).SelectedPoolsText);
    }

    [Fact]
    public void Every_figure_on_a_field_row_is_an_argument_and_never_a_default()
    {
        // AC-1: the summary record takes every figure as a required positional parameter - there is no default value
        // that could stand in for a record.
        var declaration = RecordParameters("OgFieldSummary(");

        Assert.DoesNotContain("=", declaration, StringComparison.Ordinal);
        Assert.Contains("decimal? Oil".Replace(" ", string.Empty), declaration.Replace(" ", string.Empty),
            StringComparison.Ordinal);
        Assert.Contains("int WellCount", declaration.Replace("  ", " "), StringComparison.Ordinal);
    }

    [Fact]
    public void The_feature_contains_no_demo_kpi_constants()
    {
        // AC-5: the Web portfolio page renders 12 fields / 245 MMBOE / $1.2B as constants and must not be ported.
        var files = FeatureFiles();

        Assert.Equal(2, files.Count);

        string[] forbidden = ["MMBOE", "1.2B", "$1.2", "245"];

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            foreach (var token in forbidden)
            {
                Assert.False(
                    text.Contains(token, StringComparison.Ordinal),
                    $"{Path.GetFileName(file)} contains the demo constant '{token}'; PRD-02 must not carry a figure " +
                    "that is not a record.");
            }
        }
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_register()
    {
        var service = new OgFieldPoolRegisterService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgFieldPoolRegisterService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    // ------------------------------------------------------------------ helpers

    private static IReadOnlyList<string> FeatureFiles()
    {
        var separator = Path.DirectorySeparatorChar;

        var files = SourceRoot.AllSources()
            .Where(file => Path.GetFileName(file) is "OgFieldPoolRegister.cs" or "FieldPoolRegisterPageView.cs")
            .Where(file => file.Contains($"{separator}Production{separator}", StringComparison.OrdinalIgnoreCase)
                           || file.Contains($"{separator}Views{separator}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(2, files.Count);

        return files;
    }

    private static string RecordParameters(string marker)
    {
        var source = File.ReadAllText(FeatureFiles()
            .Single(file => Path.GetFileName(file) == "OgFieldPoolRegister.cs"));

        var start = source.IndexOf(marker, StringComparison.Ordinal);

        Assert.True(start >= 0, $"'{marker}' was not found in the register service.");

        var open = source.IndexOf('(', start);
        var close = source.IndexOf(')', open);

        return source[(open + 1)..close];
    }

    private static OgFieldSummary Field(
        string id,
        string name,
        DateTime? firstProduction = null,
        decimal? oil = null,
        decimal? water = null,
        string? oilUnit = null,
        string? waterUnit = null,
        decimal? waterCut = null,
        decimal? uptime = null,
        int excludedRows = 0) =>
        new(
            id,
            name,
            "AREA-1",
            "OIL",
            Day.AddYears(-5),
            firstProduction,
            firstProduction is null ? "PRE_PRODUCTION" : "PRODUCING",
            3,
            2,
            oil,
            null,
            water,
            oilUnit,
            null,
            waterUnit,
            waterCut,
            uptime,
            firstProduction,
            excludedRows);

    private static OgPoolSummary Pool(
        string id,
        string name,
        DateTime? lastProduction,
        decimal? rate = null,
        decimal? cumulative = null,
        string? drive = "water drive",
        string? reservoir = "STRAT-1") =>
        new(
            id,
            name,
            "F-1",
            reservoir,
            drive,
            "PRODUCING",
            "OIL",
            2,
            Day.AddYears(-4),
            rate,
            "bbl",
            cumulative,
            "bbl",
            lastProduction);

    private static OgFieldRegister Register(
        int total,
        IReadOnlyList<OgFieldSummary> page,
        int pageIndex = 0,
        int pageSize = 100) =>
        new(
            "Alpha field",
            page.FirstOrDefault()?.Id,
            Day,
            page,
            total,
            pageIndex,
            pageSize,
            FieldRegisterRules.PageCount(total, pageSize),
            page.FirstOrDefault(),
            [],
            null,
            null,
            [],
            "no production is recorded");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
