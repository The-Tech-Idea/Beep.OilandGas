using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.OilGas.Drilling;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-09: the owner's analysis on a known report (AC-1 and AC-2 at the source), the unclassified bucket (AC-3), the CSV layout the
/// importer depends on (AC-4's precondition), the offset parser (AC-5) and the export's agreement with the rows (AC-6) - the parts
/// that need no database. The reports-versus-curve cross-check and the import are proven in the gateway test.
/// </summary>
public sealed class DrillingPerformanceTests
{
    private const string PageFileName = "DrillingPerformancePageView.cs";

    private static readonly DateTime Day1 = new(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);

    /// <summary>Six hours drilling 250 m, six hours tripping, eight hours of a pump failure that is flagged NPT.</summary>
    private static List<ReportActivity> Report() =>
    [
        new() { Start = Day1, End = Day1.AddHours(6), Code = "2", DepthFrom = 1000, DepthTo = 1250, Description = "drilled" },
        new() { Start = Day1.AddHours(6), End = Day1.AddHours(12), Code = "6", DepthFrom = 1250, DepthTo = 1250 },
        new() { Start = Day1.AddHours(12), End = Day1.AddHours(20), Code = "7", DepthFrom = 1250, DepthTo = 1250, Npt = true },
    ];

    [Fact]
    public void The_owner_curve_and_totals_are_the_sum_of_the_lines()
    {
        // AC-1 at the source: one curve point per line, ending at the depth the last line reached.
        var result = DrillingPerformance.Analyze(Report());

        Assert.Equal(4, result.TimeDepth.Count);
        Assert.Equal((0d, 1000d), result.TimeDepth[0]);
        Assert.Equal(0.25, result.TimeDepth[1].days, 6);
        Assert.Equal(1250, result.TimeDepth[1].depth);
        Assert.Equal(20d / 24d, result.TimeDepth[3].days, 6);
        Assert.Equal(1250, result.TimeDepth[3].depth);

        // AC-2 at the source: the non-productive hours are the flagged line and nothing else.
        Assert.Equal(20d, result.TotalHours, 6);
        Assert.Equal(8d, result.NptHours, 6);
        Assert.Equal(0.4, result.NptFraction, 6);

        // Six hours of drilling made 250 m, which is the on-bottom rate.
        Assert.Equal(250d / 6d, result.OnBottomRop!.Value, 6);

        // The codes are the configured IADC ones, and the time per code adds up to the reported time.
        Assert.Equal(3, result.ByCode.Count);
        Assert.Equal("7", result.ByCode[0].Code);
        Assert.Equal("Service / maintain rig", result.ByCode[0].Name);
        Assert.Equal(8d, result.ByCode[0].Hours, 6);
        Assert.Equal(result.TotalHours, result.ByCode.Sum(code => code.Hours), 6);
        Assert.Empty(result.Notes);
    }

    [Fact]
    public void A_gap_an_overlap_and_an_unknown_code_are_all_noted_with_their_time()
    {
        var activities = Report();

        activities.Add(new ReportActivity
        {
            Start = Day1.AddHours(23),
            End = Day1.AddHours(26),
            Code = "99",
            DepthFrom = 1250,
            DepthTo = 1250,
        });

        // The fourth line starts three hours after the third ended, and its code is not configured.
        activities[2] = new ReportActivity
        {
            Start = Day1.AddHours(12),
            End = Day1.AddHours(20),
            Code = "7",
            DepthFrom = 1250,
            DepthTo = 1250,
            Npt = true,
        };

        var result = DrillingPerformance.Analyze(activities);

        Assert.Contains(result.Notes, note => note.Contains("3 h not reported", StringComparison.Ordinal));
        Assert.Contains(result.Notes, note => note.Contains("code '99' is not in drilling.operationCodes", StringComparison.Ordinal));

        // An overlap is noted too, and the hours are still summed as reported rather than silently clipped.
        activities.Add(new ReportActivity
        {
            Start = Day1.AddHours(25),
            End = Day1.AddHours(28),
            Code = "2",
            DepthFrom = 1250,
            DepthTo = 1300,
        });

        var overlapping = DrillingPerformance.Analyze(activities);

        Assert.Contains(overlapping.Notes, note => note.Contains("overlaps the previous line by", StringComparison.Ordinal));
    }

    [Fact]
    public void An_npt_line_with_no_cause_is_its_own_visible_bucket()
    {
        // AC-3: nothing is spread across the causes that do have a name.
        Assert.Equal("pump failure", OgPerformanceRules.CauseOf(" pump failure "));
        Assert.Equal(OgPerformanceRules.Unclassified, OgPerformanceRules.CauseOf(null));
        Assert.Equal(OgPerformanceRules.Unclassified, OgPerformanceRules.CauseOf("  "));

        var named = new OgReportLine(1, "R-1", "7", "Service / maintain rig", Day1, Day1.AddHours(4), 4,
            1250, 1250, true, "pump failure", null);
        var unnamed = named with { Line = 2, Cause = null };

        Assert.Equal("pump failure", named.CauseText);
        Assert.Equal(OgPerformanceRules.Unclassified, unnamed.CauseText);
        Assert.Contains("NPT unclassified", unnamed.Describe("m"), StringComparison.Ordinal);

        // A productive line has no cause at all, which is not the same as unclassified.
        Assert.Equal(string.Empty, (named with { Npt = false }).CauseText);
    }

    [Fact]
    public void The_csv_layout_the_importer_depends_on_is_the_one_the_owner_reads()
    {
        // AC-4's precondition: the columns, their order and what a bad row does.
        Assert.Contains("start, end, code, depth_from, depth_to, description, npt", OgPerformanceRules.CsvLayout,
            StringComparison.Ordinal);

        var warnings = new List<string>();

        var parsed = DrillingPerformance.FromCsv(
            "start,end,code,depth_from,depth_to,description,npt\n" +
            "2026-10-01T06:00:00Z,2026-10-01T12:00:00Z,2,1000,1250,drilled,false\n" +
            "not a date,2026-10-01T18:00:00Z,6,1250,1250,trip,false\n",
            warnings);

        var line = Assert.Single(parsed);

        Assert.Equal("2", line.Code);
        Assert.Equal(1000, line.DepthFrom);
        Assert.Equal(1250, line.DepthTo);
        Assert.Equal("drilled", line.Description);
        Assert.False(line.Npt);

        // The owner rejects the unreadable line and names its number - which is the number the page shows.
        Assert.Single(warnings);
        Assert.Contains("line 3:", warnings[0], StringComparison.Ordinal);
    }

    [Fact]
    public void The_cost_apportionment_and_the_incompleteness_rule_are_stated_not_assumed()
    {
        // There is no recorded NPT cost anywhere, so the share is the report's cost by hours - and nothing when there is no cost.
        Assert.Equal(25000d, OgPerformanceRules.Apportion(100000, 6, 24), 6);
        Assert.Equal(0d, OgPerformanceRules.Apportion(0, 6, 24));
        Assert.Equal(0d, OgPerformanceRules.Apportion(100000, 6, 0));

        // A report without a period, or whose lines do not cover its period, is incomplete.
        Assert.True(OgPerformanceRules.Incomplete(null, Day1, 6));
        Assert.True(OgPerformanceRules.Incomplete(Day1, Day1.AddHours(24), 6));
        Assert.False(OgPerformanceRules.Incomplete(Day1, Day1.AddHours(6), 6));
        Assert.False(OgPerformanceRules.Incomplete(Day1, Day1.AddHours(24), 24));
    }

    [Fact]
    public void The_curve_and_the_report_headers_are_compared_rather_than_smoothed()
    {
        // AC-1: equal within a hundredth, and a real difference is reported as one.
        Assert.True(OgPerformanceRules.CurveMatches(2000, 2000));
        Assert.True(OgPerformanceRules.CurveMatches(2000.005, 2000));
        Assert.False(OgPerformanceRules.CurveMatches(2000, 2010));
    }

    [Fact]
    public void The_offset_list_is_trimmed_and_deduplicated_or_refused()
    {
        // AC-5: the same wells are compared once, whatever the typing.
        var offsets = DrillingPerformancePageView.Offsets(" W-2, w-2 ;W-3 ", out var problem);

        Assert.Null(problem);
        Assert.Equal(["W-2", "W-3"], offsets);

        Assert.Empty(DrillingPerformancePageView.Offsets(" ", out var emptyProblem));
        Assert.Contains("needs at least one offset well", emptyProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_export_is_the_grid_rows_and_the_cause_table_field_for_field()
    {
        // AC-6: both exports come from the collections the screen binds, so a row cannot differ from what is shown.
        var workspace = Workspace();

        var rows = workspace.RowsCsv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("line,report,start,end,code,name,hours,from,to,npt,cause,description", rows[0]);
        Assert.Equal(1 + workspace.Rows.Count, rows.Length);
        Assert.Contains("\"R-1\"", rows[1], StringComparison.Ordinal);
        Assert.Contains("\"2\"", rows[1], StringComparison.Ordinal);
        Assert.Contains(",1000,1250,N,", rows[1], StringComparison.Ordinal);

        // The NPT row carries its unclassified cause in the export, not an empty field.
        Assert.Contains("\"unclassified\"", rows[2], StringComparison.Ordinal);
        Assert.Contains(",Y,", rows[2], StringComparison.Ordinal);

        var causes = workspace.CausesCsv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("cause,hours,fraction,lines,cost_impact,currency", causes[0]);
        Assert.Equal(1 + workspace.NptByCause.Count, causes.Length);
        Assert.StartsWith("\"unclassified\",8,", causes[1], StringComparison.Ordinal);
        Assert.Contains("\"USD\"", causes[1], StringComparison.Ordinal);

        // The screen states the definitions the comparison uses, which is what makes the offset table comparable.
        Assert.Contains("apportioned from each report's cost by its hours", workspace.CostText, StringComparison.Ordinal);
        Assert.Equal(50000d, workspace.TotalCost!.Value, 6);
    }

    [Fact]
    public async Task A_read_is_refused_without_a_well_or_a_connection_and_an_import_without_an_actor()
    {
        var offline = new OgDrillingPerformanceService(() => null, new Quiet());

        var noWell = await offline.LoadAsync(" ");

        Assert.False(noWell.Success);
        Assert.Equal(OgPerformanceRules.CodeWellRequired, noWell.Reference);

        var noConnection = await offline.LoadAsync("W-1");

        Assert.False(noConnection.Success);
        Assert.Equal(OgPerformanceRules.CodeNoConnection, noConnection.Reference);

        var noActor = await offline.ImportCsvAsync("W-1", "start,end,code\n", " ");

        Assert.False(noActor.Success);
        Assert.Equal(OgPerformanceRules.CodeActorRequired, noActor.Reference);

        var noCsv = await offline.ImportCsvAsync("W-1", "  ", "sara");

        Assert.False(noCsv.Success);
        Assert.Equal(OgPerformanceRules.CodeNoCsv, noCsv.Reference);

        var noOffsets = await offline.OffsetsAsync("W-1", []);

        Assert.False(noOffsets.Success);
        Assert.Equal(OgPerformanceRules.CodeNoOffsets, noOffsets.Reference);
    }

    [Fact]
    public void The_page_draws_the_owners_time_depth_view_and_clears_it_on_a_failure()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The feature's source component is the owner's own chart, hosted rather than redrawn.
        Assert.Contains("TimeDepthView", page, StringComparison.Ordinal);
        Assert.Contains("_timeDepth.Result = workspace.Owner", page, StringComparison.Ordinal);
        Assert.Contains("_host.Surface = _timeDepth", page, StringComparison.Ordinal);
        Assert.Contains("_host.Redraw()", page, StringComparison.Ordinal);

        Assert.Contains("workspace.RowsCsv()", page, StringComparison.Ordinal);
        Assert.Contains("workspace.CausesCsv()", page, StringComparison.Ordinal);
        Assert.Contains("workspace.NptText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.RopText", page, StringComparison.Ordinal);
        Assert.Contains("outcome.Value.RejectedText", page, StringComparison.Ordinal);

        // A failed read clears the curve rather than leaving the last well's performance on screen.
        Assert.Contains("_timeDepth.Result = null", page, StringComparison.Ordinal);
    }

    private static OgPerformanceWorkspace Workspace()
    {
        var owner = DrillingPerformance.Analyze(Report());

        return new OgPerformanceWorkspace(
            "W-1",
            "m",
            2,
            0,
            [],
            20d / 24d,
            1250,
            250,
            20,
            8,
            0.4,
            250d / 6d,
            [
                new OgReportLine(1, "R-1", "2", "Drilling", Day1, Day1.AddHours(6), 6, 1000, 1250, false, null, "drilled"),
                new OgReportLine(2, "R-1", "7", "Service / maintain rig", Day1.AddHours(12), Day1.AddHours(20), 8,
                    1250, 1250, true, null, "pump failed"),
            ],
            [new OgNptCause(OgPerformanceRules.Unclassified, 8, 1, 20000, 1)],
            [new OgRopInterval(1, 1000, 1250, 250, 6, 250d / 6d)],
            [new OgCodeTime("2", "Drilling", 6, 0.3)],
            [new OgTimeDepthPoint(0, 1000)],
            owner,
            50000,
            "USD",
            20000,
            1250,
            1250,
            true,
            [],
            Day1.AddDays(1));
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
