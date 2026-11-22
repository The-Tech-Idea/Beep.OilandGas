using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-06: the report value rules, the NPT reference rule, the depth continuation rule and the derived totals - the
/// parts that do not need a database. The duplicate period, the correction, the transfer and the lost acknowledgement are
/// proven in the gateway test.
/// </summary>
public sealed class DrillingExecutionTests
{
    private static readonly DateTime Day = new(2026, 8, 1);

    [Fact]
    public void A_report_needs_a_day_a_shift_a_depth_run_and_a_unit()
    {
        Assert.Equal(
            OgDrillingExecutionService.CodeActivityInvalid,
            OgDrillingExecutionService.ValidateReport(
                new OgDailyReportValues(null, "DAY", null, null, 0m, 500m, "m", 100m, "USD", null, null, null))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeActivityInvalid,
            OgDrillingExecutionService.ValidateReport(Values(shift: "AFTERNOON"))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeDepthInvalid,
            OgDrillingExecutionService.ValidateReport(Values(start: null))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeDepthInvalid,
            OgDrillingExecutionService.ValidateReport(Values(start: 900m, end: 500m))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeDepthUnitRequired,
            OgDrillingExecutionService.ValidateReport(Values(unit: null))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeCostInvalid,
            OgDrillingExecutionService.ValidateReport(Values(cost: -1m))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeCurrencyRequired,
            OgDrillingExecutionService.ValidateReport(Values(cost: 10m, currency: null))!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeActivityInvalid,
            OgDrillingExecutionService.ValidateReport(
                Values(periodStart: Day.AddHours(12), periodEnd: Day.AddHours(6)))!.Value.Code);

        Assert.Null(OgDrillingExecutionService.ValidateReport(Values()));
    }

    [Fact]
    public void An_npt_line_needs_a_code_from_the_reference_set()
    {
        var codes = new[] { "EQ-FAIL", "WEATHER" };

        Assert.Equal(
            OgDrillingExecutionService.CodeActivityInvalid,
            OgDrillingExecutionService.ValidateActivity(Activity(hours: 0m), codes)!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeActivityInvalid,
            OgDrillingExecutionService.ValidateActivity(Activity(code: null), codes)!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeNptCodeRequired,
            OgDrillingExecutionService.ValidateActivity(Activity(isNpt: true, nptCode: null), codes)!.Value.Code);

        Assert.Equal(
            OgDrillingExecutionService.CodeNptCodeUnknown,
            OgDrillingExecutionService.ValidateActivity(Activity(isNpt: true, nptCode: "MADE-UP"), codes)!.Value.Code);

        Assert.Null(OgDrillingExecutionService.ValidateActivity(Activity(isNpt: true, nptCode: "eq-fail"), codes));

        // With no reference set recorded, an NPT line is refused instead of accepting free text.
        Assert.Equal(
            OgDrillingExecutionService.CodeNptReferenceMissing,
            OgDrillingExecutionService.ValidateActivity(Activity(isNpt: true, nptCode: "EQ-FAIL"), [])!.Value.Code);

        // A productive line needs no NPT code at all.
        Assert.Null(OgDrillingExecutionService.ValidateActivity(Activity(isNpt: false, nptCode: null), []));
    }

    [Fact]
    public void A_start_depth_that_does_not_continue_the_previous_report_is_flagged()
    {
        Assert.Null(OgDrillingExecutionService.DepthFlag(500m, null, "m"));
        Assert.Null(OgDrillingExecutionService.DepthFlag(500m, 500m, "m"));
        Assert.Null(OgDrillingExecutionService.DepthFlag(500.001m, 500m, "m"));

        var flag = OgDrillingExecutionService.DepthFlag(950m, 900m, "m");

        Assert.NotNull(flag);
        Assert.Contains("950", flag!, StringComparison.Ordinal);
        Assert.Contains("900", flag!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_report_derives_its_run_npt_and_line_count()
    {
        var report = Report(
            start: 1000m,
            end: 1250m,
            activities:
            [
                Line(Activity(hours: 8m), 1),
                Line(Activity(hours: 3.5m, isNpt: true, nptCode: "EQ-FAIL"), 2),
                Line(Activity(hours: 0.5m, isNpt: true, nptCode: "WEATHER"), 3),
            ]);

        Assert.Equal(250m, report.MetresDrilled);
        Assert.Equal("250 m", report.MetresText);
        Assert.Equal(4m, report.NptHours);
        Assert.Equal(3, report.ActivityCount);
        Assert.Contains("NPT 4 h", report.Describe(), StringComparison.Ordinal);
        Assert.Equal("1000–1250 m", report.DepthText);
    }

    [Fact]
    public void A_flagged_report_says_so_in_its_sentence()
    {
        var report = Report(start: 1000m, end: 1100m) with { DepthFlag = "starts at 1000 but the previous report ended at 900 m" };

        Assert.True(report.DepthFlagged);
        Assert.Contains("previous report ended at 900", report.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_execution_header_shows_a_partial_cost_when_a_report_has_none()
    {
        var complete = Header(costToDate: 1000m, complete: true);
        var partial = Header(costToDate: 1000m, complete: false);

        Assert.Equal("1,000 USD", complete.CostText);
        Assert.Contains("partial", partial.CostText, StringComparison.Ordinal);
        Assert.Contains("no cost recorded", partial.CostText, StringComparison.Ordinal);
        var plotted = Header(0m, true) with { TimeDepth = [new OgTimeDepthPoint(Day, 500m)] };

        Assert.Contains("points only", plotted.TimeDepthText, StringComparison.Ordinal);
        Assert.Contains("no report to plot", Header(0m, true).TimeDepthText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_execution()
    {
        var service = new OgDrillingExecutionService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgDrillingExecutionService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_start_a_report_and_a_transfer_all_need_an_actor()
    {
        var service = new OgDrillingExecutionService(() => null, new Quiet());

        Assert.Equal(
            OgDrillingExecutionService.CodeActorRequired,
            (await service.StartAsync("C-1", "P-1", Day, "sara", "COMPANY_MAN", " ")).Reference);

        Assert.Equal(
            OgDrillingExecutionService.CodeActorRequired,
            (await service.CreateReportAsync("E-1", Values(), " ")).Reference);

        Assert.Equal(
            OgDrillingExecutionService.CodeActorRequired,
            (await service.TransferResponsibilityAsync("E-1", "omar", "COMPANY_MAN", "handover", " ")).Reference);

        Assert.Equal(
            OgDrillingExecutionService.CodeReasonRequired,
            (await service.TransferResponsibilityAsync("E-1", "omar", "COMPANY_MAN", " ", "sara")).Reference);

        Assert.Equal(
            OgDrillingExecutionService.CodeReasonRequired,
            (await service.ReviseReportAsync("R-1", " ", "sara")).Reference);

        Assert.Equal(
            OgDrillingExecutionService.CodeNothingPending,
            (await service.RetrySubmitAsync()).Reference);
    }

    private static OgDailyReportValues Values(
        DateTime? date = null,
        string? shift = "DAY",
        decimal? start = 0m,
        decimal? end = 500m,
        string? unit = "m",
        decimal? cost = 100m,
        string? currency = "USD",
        DateTime? periodStart = null,
        DateTime? periodEnd = null) =>
        new(date ?? Day, shift, periodStart, periodEnd, start, end, unit, cost, currency, null, null, null);

    private static OgDailyActivityValues Activity(
        string? code = "DRILL",
        decimal? hours = 8m,
        bool isNpt = false,
        string? nptCode = null) =>
        new(code, hours, "line", isNpt, nptCode, 500m);

    private static OgDailyActivity Line(OgDailyActivityValues values, int sequence) =>
        new(
            "A-" + sequence,
            sequence,
            values.Code,
            values.Hours,
            values.Description,
            values.IsNpt,
            values.NptCode,
            values.DepthAtStart);

    private static OgDailyReport Report(
        decimal start,
        decimal end,
        IReadOnlyList<OgDailyActivity>? activities = null) =>
        new(
            "R-1",
            "E-1",
            "P-1",
            "WELL-1",
            Day,
            "DAY",
            1,
            OgDailyReport.Draft,
            start,
            end,
            "m",
            100m,
            "USD",
            null,
            null,
            null,
            "sara",
            null,
            null,
            null,
            null,
            activities ?? [],
            null);

    private static OgExecutionHeader Header(decimal costToDate, bool complete) =>
        new(
            "E-1",
            "P-1",
            "F-1",
            "WELL-1",
            "ACTIVE",
            1,
            Day,
            500m,
            "m",
            "DRILL",
            1,
            costToDate,
            complete,
            "USD",
            "sara",
            "COMPANY_MAN",
            []);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
