using System.Globalization;
using System.Text;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One point of the time-depth curve: days since the first line, and the hole depth then (ENG-09).</summary>
public sealed record OgTimeDepthPoint(double Days, double Depth);

/// <summary>Non-productive time by cause (ENG-09, AC-2 and AC-3).</summary>
/// <param name="Cause">The recorded cause, or <see cref="OgPerformanceRules.Unclassified"/> when an NPT line has none.</param>
/// <param name="CostImpact">The share of the reports' recorded cost that this cause's hours carry, apportioned by hours.</param>
public sealed record OgNptCause(string Cause, double Hours, double Fraction, double CostImpact, int Lines)
{
    public string Describe(string currency) =>
        Cause + " · " + Hours.ToString("0.##", CultureInfo.InvariantCulture) + " h · " +
        (Fraction * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % · " +
        Lines.ToString(CultureInfo.InvariantCulture) + " line(s) · cost " +
        CostImpact.ToString("0", CultureInfo.InvariantCulture) + " " + currency;
}

/// <summary>One drilling run: consecutive drilling-code lines, with the footage and hours they made (ENG-09).</summary>
public sealed record OgRopInterval(int Index, double From, double To, double Footage, double Hours, double? Rop)
{
    public string Describe(string unit) =>
        "run " + Index.ToString(CultureInfo.InvariantCulture) + " · " +
        From.ToString("0.#", CultureInfo.InvariantCulture) + "-" + To.ToString("0.#", CultureInfo.InvariantCulture) + " " +
        unit + " · " + Footage.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + " in " +
        Hours.ToString("0.##", CultureInfo.InvariantCulture) + " h · " +
        (Rop is { } rop ? rop.ToString("0.##", CultureInfo.InvariantCulture) + " " + unit + "/h" : "no footage");
}

/// <summary>Time per operation code (ENG-09).</summary>
public sealed record OgCodeTime(string Code, string Name, double Hours, double Fraction)
{
    public string Describe() =>
        Code + " " + Name + " · " + Hours.ToString("0.##", CultureInfo.InvariantCulture) + " h · " +
        (Fraction * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %";
}

/// <summary>One activity line as the screen shows it (ENG-09).</summary>
public sealed record OgReportLine(
    int Line,
    string ReportId,
    string Code,
    string Name,
    DateTime Start,
    DateTime End,
    double Hours,
    double? From,
    double? To,
    bool Npt,
    string? Cause,
    string? Description)
{
    public string CauseText => Npt ? Cause ?? OgPerformanceRules.Unclassified : string.Empty;

    public string Describe(string unit) =>
        Start.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " → " +
        End.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture) + " · " + Code + " " + Name + " · " +
        Hours.ToString("0.##", CultureInfo.InvariantCulture) + " h" +
        (From is { } from && To is { } to
            ? " · " + from.ToString("0.#", CultureInfo.InvariantCulture) + "-" + to.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit
            : string.Empty) +
        (Npt ? " · NPT " + CauseText : string.Empty);
}

/// <summary>One well on the offset comparison (ENG-09, AC-5).</summary>
public sealed record OgOffsetRow(
    string WellId,
    string Unit,
    int Reports,
    double Days,
    double Depth,
    double NptHours,
    double NptPercent,
    double? Rop,
    bool HasReports)
{
    public string Describe() => HasReports
        ? WellId + " · " + Reports.ToString(CultureInfo.InvariantCulture) + " report(s) · " +
          Days.ToString("0.##", CultureInfo.InvariantCulture) + " day(s) · TD " +
          Depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · NPT " +
          NptPercent.ToString("0.#", CultureInfo.InvariantCulture) + " % · ROP " +
          (Rop is { } rop ? rop.ToString("0.##", CultureInfo.InvariantCulture) + " " + Unit + "/h" : "not computed")
        : WellId + " · no activity line is recorded, so it is not compared";
}

/// <summary>One rejected CSV line (ENG-09, AC-4).</summary>
public sealed record OgCsvRejection(int Line, string Text, string Reason)
{
    public string Describe() =>
        (Line > 0 ? "line " + Line.ToString(CultureInfo.InvariantCulture) : "line unknown") + ": " + Reason +
        " (" + Text + ")";
}

/// <summary>The result of a CSV import (ENG-09, AC-4).</summary>
public sealed record OgCsvImport(
    int Accepted,
    int Attached,
    int Unattached,
    IReadOnlyList<OgCsvRejection> Rejected,
    IReadOnlyList<string> Warnings)
{
    public string Describe() =>
        Accepted.ToString(CultureInfo.InvariantCulture) + " line(s) imported · " +
        Attached.ToString(CultureInfo.InvariantCulture) + " attached to a daily report · " +
        Unattached.ToString(CultureInfo.InvariantCulture) + " unattached · " +
        Rejected.Count.ToString(CultureInfo.InvariantCulture) + " line(s) rejected";

    public string RejectedText => Rejected.Count == 0
        ? "Every line was read."
        : string.Join(Environment.NewLine, Rejected.Select(rejection => "✗ " + rejection.Describe()));
}

/// <summary>The drilling-performance workspace (ENG-09).</summary>
public sealed record OgPerformanceWorkspace(
    string WellId,
    string Unit,
    int ReportCount,
    int IncompleteReports,
    IReadOnlyList<string> IncompleteReportIds,
    double Days,
    double Depth,
    double Footage,
    double TotalHours,
    double NptHours,
    double? NptFraction,
    double? OnBottomRop,
    IReadOnlyList<OgReportLine> Rows,
    IReadOnlyList<OgNptCause> NptByCause,
    IReadOnlyList<OgRopInterval> Rop,
    IReadOnlyList<OgCodeTime> ByCode,
    IReadOnlyList<OgTimeDepthPoint> TimeDepth,
    PerformanceResult Owner,
    double? TotalCost,
    string? Currency,
    double? NptCost,
    double ReportsDepth,
    double? CurveDepth,
    bool CurveMatchesReports,
    IReadOnlyList<string> Warnings,
    DateTime ReadAt)
{
    public bool Empty => Rows.Count == 0;

    public string CountsText =>
        ReportCount.ToString(CultureInfo.InvariantCulture) + " report(s) · " +
        (IncompleteReports == 0 ? "all complete" : IncompleteReports.ToString(CultureInfo.InvariantCulture) + " incomplete") +
        (IncompleteReports == 0
            ? string.Empty
            : " (" + string.Join(", ", IncompleteReportIds) + ")") +
        " · " + Rows.Count.ToString(CultureInfo.InvariantCulture) + " line(s) · " +
        Days.ToString("0.##", CultureInfo.InvariantCulture) + " day(s) · " +
        Depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " TD · NPT " +
        NptHours.ToString("0.##", CultureInfo.InvariantCulture) + " h (" +
        ((NptFraction ?? 0d) * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %)";

    public string SummaryText => Empty
        ? "No activity line is recorded for this well, so there is nothing to analyse. Record the daily reports (DEV-06) " +
          "and their activity lines, or import a CSV."
        : "On-bottom ROP " + (OnBottomRop is { } rop
            ? rop.ToString("0.##", CultureInfo.InvariantCulture) + " " + Unit + "/h"
            : "not computed (no drilling footage)") +
          " · " + Footage.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " drilled over " +
          TotalHours.ToString("0.##", CultureInfo.InvariantCulture) + " reported hour(s)." +
          (IncompleteReports == 0
              ? string.Empty
              : " The totals cover " + IncompleteReports.ToString(CultureInfo.InvariantCulture) +
                " incomplete report(s), so they are partial.");

    public string NptText => NptByCause.Count == 0
        ? "No non-productive time is recorded."
        : string.Join(Environment.NewLine, NptByCause.Select(cause => cause.Describe(Currency ?? string.Empty)));

    public string RopText => Rop.Count == 0
        ? "No drilling run is recorded, so no rate of penetration is computed."
        : string.Join(Environment.NewLine, Rop.Select(run => run.Describe(Unit)));

    public string CodeText => ByCode.Count == 0
        ? "No operation code is recorded."
        : string.Join(Environment.NewLine, ByCode.Select(code => code.Describe()));

    public string CurveText => TimeDepth.Count == 0
        ? "No time-depth curve: no line is recorded."
        : CurveMatchesReports
            ? "The curve ends at " + (CurveDepth ?? 0d).ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit +
              ", which is the deepest depth the reports record."
            : "The curve ends at " + (CurveDepth ?? 0d).ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit +
              " but the reports record " + ReportsDepth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit +
              " - the difference is stated rather than smoothed.";

    public string CostText => TotalCost is null
        ? "No cost is recorded on the daily reports, so no cost impact is stated."
        : "Recorded cost " + TotalCost.Value.ToString("0", CultureInfo.InvariantCulture) + " " + (Currency ?? string.Empty) +
          " · NPT share " + (NptCost ?? 0d).ToString("0", CultureInfo.InvariantCulture) + " " + (Currency ?? string.Empty) +
          ", apportioned from each report's cost by its hours - it is not a separately recorded NPT cost.";

    public string WarningsText => Warnings.Count == 0
        ? "The analysis raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>AC-6: the activity rows, field for field, as the grid holds them.</summary>
    public string RowsCsv()
    {
        var text = new StringBuilder();
        text.AppendLine("line,report,start,end,code,name,hours,from,to,npt,cause,description");

        foreach (var row in Rows)
        {
            text.AppendLine(string.Join(",",
                row.Line.ToString(CultureInfo.InvariantCulture),
                Quote(row.ReportId),
                Quote(row.Start.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
                Quote(row.End.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
                Quote(row.Code),
                Quote(row.Name),
                Figure(row.Hours),
                Figure(row.From),
                Figure(row.To),
                row.Npt ? "Y" : "N",
                Quote(row.CauseText),
                Quote(row.Description ?? string.Empty)));
        }

        return text.ToString();
    }

    /// <summary>AC-6 and AC-2: the non-productive time by cause, as the summary table holds it.</summary>
    public string CausesCsv()
    {
        var text = new StringBuilder();
        text.AppendLine("cause,hours,fraction,lines,cost_impact,currency");

        foreach (var cause in NptByCause)
        {
            text.AppendLine(string.Join(",",
                Quote(cause.Cause),
                Figure(cause.Hours),
                Figure(cause.Fraction),
                cause.Lines.ToString(CultureInfo.InvariantCulture),
                Figure(cause.CostImpact),
                Quote(Currency ?? string.Empty)));
        }

        return text.ToString();
    }

    public static string Figure(double? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>The drilling-performance rules ENG-09 states once.</summary>
public static class OgPerformanceRules
{
    public const string CodeNoConnection = "performance-no-connection";
    public const string CodeReadFailed = "performance-read-failed";
    public const string CodeWriteFailed = "performance-write-failed";
    public const string CodeWellRequired = "performance-well-required";
    public const string CodeActorRequired = "performance-actor-required";
    public const string CodeNoCsv = "performance-no-csv";
    public const string CodeNoLines = "performance-no-lines";
    public const string CodeNoOffsets = "performance-no-offsets";

    /// <summary>AC-3: the bucket an NPT line with no cause falls into.</summary>
    public const string Unclassified = "unclassified";

    /// <summary>The report id an imported line carries when no recorded report covers it.</summary>
    public const string Unattached = "UNATTACHED";

    /// <summary>The owner's CSV column order, documented here because the importer depends on it.</summary>
    public const string CsvLayout =
        "start, end, code, depth_from, depth_to, description, npt - ISO dates, invariant numbers, one header line";

    /// <summary>AC-3: the cause of an NPT line, named as unclassified when none is recorded.</summary>
    public static string CauseOf(string? cause) =>
        string.IsNullOrWhiteSpace(cause) ? Unclassified : cause.Trim();

    /// <summary>
    /// The cost a set of hours carries out of a report's recorded cost, apportioned by hours. There is no recorded NPT cost
    /// anywhere, so this is stated as an apportionment rather than presented as a measured figure.
    /// </summary>
    public static double Apportion(double reportCost, double hours, double reportHours) =>
        reportHours > 1e-9 && reportCost > 0d ? reportCost * hours / reportHours : 0d;

    /// <summary>A report is incomplete when its period is not recorded or its lines do not cover the period.</summary>
    public static bool Incomplete(DateTime? start, DateTime? end, double lineHours) =>
        start is null || end is null || end <= start || Math.Abs(lineHours - (end.Value - start.Value).TotalHours) > 0.25;

    /// <summary>AC-1: the curve and the report headers must agree; the difference is stated, never smoothed.</summary>
    public static bool CurveMatches(double curveDepth, double reportsDepth) =>
        Math.Abs(curveDepth - reportsDepth) <= 0.01;
}

/// <summary>ENG-09: the drilling performance of a well, its documents and its offset comparison.</summary>
public interface IOgDrillingPerformance
{
    /// <summary>Reads the well's daily reports and activity lines and analyses them.</summary>
    Task<OgResult<OgPerformanceWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports report lines from a CSV, rejecting every line whose operation code is not configured (AC-4) and naming the line.
    /// </summary>
    Task<OgResult<OgCsvImport>> ImportCsvAsync(
        string wellId,
        string csv,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-5: the well and its offsets, every row from the same analysis.</summary>
    Task<OgResult<IReadOnlyList<OgOffsetRow>>> OffsetsAsync(
        string wellId,
        IReadOnlyList<string> offsetWells,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-09 over the DrillingAndConstruction-owned <c>DEVELOPMENT_DAILY_REPORT</c> (DEV-06) and the added
/// <c>DEVELOPMENT_REPORT_ACTIVITY</c>, using the tools' <see cref="DrillingPerformance"/> and <see cref="TimeDepthView"/>.
/// </summary>
/// <remarks>
/// <para>
/// The analysis is the owner's: <see cref="DrillingPerformance.Analyze"/> builds the time-depth curve, the hours per code, the
/// non-productive time and the on-bottom rate of penetration, and its own notes name overlapping lines, unreported gaps and unknown
/// codes. <see cref="DrillingPerformance.FromCsv"/> reads a CSV with the column order this service documents.
/// </para>
/// <para>
/// What the desktop adds is the data the owner's model needs and no table carried - the operation code and the non-productive cause
/// on each line - together with the cost apportionment, the drilling runs, the offset table and the export.
/// </para>
/// </remarks>
public sealed class OgDrillingPerformanceService : IOgDrillingPerformance
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDrillingPerformanceService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPerformanceWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgPerformanceWorkspace>.Failed(
                "Drilling performance belongs to one well; none is named.", OgPerformanceRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPerformanceWorkspace>.Failed(
                "No connection is open, so the drilling performance cannot be read.", OgPerformanceRules.CodeNoConnection);
        }

        try
        {
            return await AnalyseAsync(source, wellId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the drilling performance for " + wellId, "the analysis is not shown", FailureSeverity.Error);

            return OgResult<OgPerformanceWorkspace>.Failed(
                "The drilling performance could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgPerformanceRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgCsvImport>> ImportCsvAsync(
        string wellId,
        string csv,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCsvImport>.Failed(
                "An import records who made it.", OgPerformanceRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgCsvImport>.Failed(
                "An import belongs to one well; none is named.", OgPerformanceRules.CodeWellRequired);
        }

        if (string.IsNullOrWhiteSpace(csv))
        {
            return OgResult<OgCsvImport>.Failed(
                "The CSV is empty, so there is nothing to import.", OgPerformanceRules.CodeNoCsv);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCsvImport>.Failed(
                "No connection is open, so the lines cannot be imported.", OgPerformanceRules.CodeNoConnection);
        }

        try
        {
            var codes = DrillingToolbox.Settings.OperationCodes;
            var warnings = new List<string>();

            // The owner's parser reads the columns and reports its own structural problems with their line numbers.
            var parsed = DrillingPerformance.FromCsv(csv, warnings);

            // The line numbers come from the same acceptance test the owner applies, so a rejected row is never mis-numbered and a
            // row it accepted is never counted twice.
            var lines = LineNumbersOf(csv, parsed);

            var rejected = new List<OgCsvRejection>();
            var accepted = new List<(ReportActivity Activity, int Line)>();

            for (var index = 0; index < parsed.Count; index++)
            {
                var activity = parsed[index];
                var line = index < lines.Count ? lines[index] : 0;

                if (!codes.ContainsKey(activity.Code))
                {
                    rejected.Add(new OgCsvRejection(
                        line,
                        activity.Code,
                        "'" + activity.Code + "' is not an operation code in drilling.operationCodes, so the line is rejected"));

                    continue;
                }

                accepted.Add((activity, line));
            }

            // The structural warnings the owner raised are rejections too: a line it could not read was not imported.
            foreach (var warning in warnings)
            {
                rejected.Add(new OgCsvRejection(LineOf(warning), warning, warning));
            }

            if (accepted.Count == 0)
            {
                return OgResult<OgCsvImport>.Failed(
                    "No line could be imported: " + (rejected.Count == 0
                        ? "the CSV holds a header and nothing else."
                        : rejected.Count + " line(s) were rejected, the first being " + rejected[0].Describe()),
                    OgPerformanceRules.CodeNoLines);
            }

            accepted.Sort((left, right) => left.Activity.Start.CompareTo(right.Activity.Start));

            var unit = WellboreEnvironment.Current.Units.DepthUnit;
            var unitOfWork = OgUow.For<DEVELOPMENT_REPORT_ACTIVITY>(source);
            unitOfWork.PrimaryKey = nameof(DEVELOPMENT_REPORT_ACTIVITY.ACTIVITY_ID);

            var reports = await OgUow.ReadAllAsync<DEVELOPMENT_DAILY_REPORT>(source).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            var attached = 0;

            foreach (var (activity, _) in accepted)
            {
                var report = reports.FirstOrDefault(row =>
                    string.Equals(row.WELL_ID?.Trim(), wellId.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    row.PERIOD_START is { } from && row.PERIOD_END is { } to &&
                    activity.Start >= from && activity.Start <= to);

                if (report is not null)
                {
                    attached++;
                }

                unitOfWork.Add(new DEVELOPMENT_REPORT_ACTIVITY
                {
                    ACTIVITY_ID = Guid.NewGuid().ToString("N"),
                    REPORT_ID = report?.REPORT_ID ?? OgPerformanceRules.Unattached,
                    WELL_ID = wellId.Trim(),
                    START_UTC = activity.Start,
                    END_UTC = activity.End,
                    CODE = activity.Code.Trim(),
                    DEPTH_FROM = activity.DepthFrom is { } depthFrom ? (decimal)depthFrom : null,
                    DEPTH_TO = activity.DepthTo is { } depthTo ? (decimal)depthTo : null,
                    DEPTH_OUOM = report?.DEPTH_OUOM ?? unit,
                    DESCRIPTION = string.IsNullOrWhiteSpace(activity.Description) ? null : activity.Description.Trim(),
                    NPT = activity.Npt is { } npt ? (npt ? "Y" : "N") : null,
                    NPT_CAUSE = null,
                    ACTOR = actor.Trim(),
                    RECORDED_AT_UTC = now,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCsvImport>.Failed(
                    OgUow.Message(committed, "The lines were not imported."), OgPerformanceRules.CodeWriteFailed);
            }

            return OgResult<OgCsvImport>.Ok(new OgCsvImport(
                accepted.Count, attached, accepted.Count - attached, rejected, warnings));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "importing drilling report lines for " + wellId, "the lines are not imported", FailureSeverity.Error);

            return OgResult<OgCsvImport>.Failed(
                "The lines could not be imported" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgPerformanceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<IReadOnlyList<OgOffsetRow>>> OffsetsAsync(
        string wellId,
        IReadOnlyList<string> offsetWells,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<IReadOnlyList<OgOffsetRow>>.Failed(
                "An offset comparison is made against one well; none is named.", OgPerformanceRules.CodeWellRequired);
        }

        if (offsetWells.Count == 0)
        {
            return OgResult<IReadOnlyList<OgOffsetRow>>.Failed(
                "An offset comparison needs at least one offset well; none is named.", OgPerformanceRules.CodeNoOffsets);
        }

        var rows = new List<OgOffsetRow>();

        // AC-5: every row comes from the same LoadAsync, so the definitions cannot differ between this well and its offsets.
        foreach (var candidate in new[] { wellId }.Concat(offsetWells)
                     .Select(id => id.Trim())
                     .Where(id => id.Length > 0)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var loaded = await LoadAsync(candidate, cancellationToken).ConfigureAwait(false);

            if (!loaded.Success || loaded.Value is null)
            {
                return OgResult<IReadOnlyList<OgOffsetRow>>.Failed(
                    loaded.Message ?? "The offset well could not be read.",
                    loaded.Reference ?? OgPerformanceRules.CodeReadFailed);
            }

            var workspace = loaded.Value;

            rows.Add(new OgOffsetRow(
                workspace.WellId,
                workspace.Unit,
                workspace.ReportCount,
                workspace.Days,
                workspace.Depth,
                workspace.NptHours,
                (workspace.NptFraction ?? 0d) * 100d,
                workspace.OnBottomRop,
                !workspace.Empty));
        }

        return OgResult<IReadOnlyList<OgOffsetRow>>.Ok(rows);
    }

    // ------------------------------------------------------------------ the analysis

    private async Task<OgResult<OgPerformanceWorkspace>> AnalyseAsync(
        IDataSource source,
        string wellId,
        CancellationToken cancellationToken)
    {
        var reports = await OgUow.ReadAllAsync<DEVELOPMENT_DAILY_REPORT>(source).ConfigureAwait(false);
        var activities = await OgUow.ReadAllAsync<DEVELOPMENT_REPORT_ACTIVITY>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var mine = reports
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_ID?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var lines = activities
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_ID?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.START_UTC)
            .ToList();

        var unit = lines.FirstOrDefault()?.DEPTH_OUOM?.Trim()
                   ?? mine.FirstOrDefault()?.DEPTH_OUOM?.Trim()
                   ?? WellboreEnvironment.Current.Units.DepthUnit;

        var owner = DrillingPerformance.Analyze(lines.Select(row => new ReportActivity
        {
            Start = row.START_UTC,
            End = row.END_UTC,
            Code = row.CODE ?? string.Empty,
            DepthFrom = row.DEPTH_FROM is { } from ? (double)from : null,
            DepthTo = row.DEPTH_TO is { } to ? (double)to : null,
            Description = row.DESCRIPTION,
            Npt = string.Equals(row.NPT?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
                ? true
                : string.Equals(row.NPT?.Trim(), "N", StringComparison.OrdinalIgnoreCase)
                    ? false
                    : null,
        }).ToList());

        var names = DrillingToolbox.Settings.OperationCodes;

        var rows = lines.Select((row, index) => new OgReportLine(
            index + 1,
            row.REPORT_ID ?? string.Empty,
            row.CODE ?? string.Empty,
            names.TryGetValue(row.CODE ?? string.Empty, out var name) ? name : row.CODE ?? string.Empty,
            row.START_UTC,
            row.END_UTC,
            (row.END_UTC - row.START_UTC).TotalHours,
            row.DEPTH_FROM is { } fromDepth ? (double)fromDepth : null,
            row.DEPTH_TO is { } toDepth ? (double)toDepth : null,
            string.Equals(row.NPT?.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ||
            (row.NPT is null && DrillingToolbox.Settings.NptCodes.Contains(row.CODE ?? string.Empty, StringComparer.OrdinalIgnoreCase)),
            row.NPT_CAUSE,
            row.DESCRIPTION)).ToList();

        // AC-2 and AC-3: the NPT is grouped by cause, and an NPT line with no cause is its own visible bucket.
        var costs = CostsOf(mine, lines);

        var nptByCause = rows
            .Where(row => row.Npt)
            .GroupBy(row => OgPerformanceRules.CauseOf(row.Cause), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Sum(row => row.Hours))
            .Select(group => new OgNptCause(
                group.Key,
                group.Sum(row => row.Hours),
                owner.NptHours > 0d ? group.Sum(row => row.Hours) / owner.NptHours : 0d,
                group.Sum(row => CostOf(row, costs)),
                group.Count()))
            .ToList();

        var rop = RunsOf(lines);

        var timeDepth = owner.TimeDepth.Select(point => new OgTimeDepthPoint(point.days, point.depth)).ToList();
        var curveDepth = timeDepth.Count == 0 ? 0d : timeDepth[^1].Depth;
        var reportsDepth = mine.Count == 0
            ? 0d
            : mine.Max(row => (double)row.DEPTH_END);

        var days = owner.TotalHours / 24d;

        if (timeDepth.Count > 0)
        {
            days = timeDepth[^1].Days;
        }

        var warnings = new List<string>(owner.Notes);

        if (!OgPerformanceRules.CurveMatches(curveDepth, reportsDepth) && mine.Count > 0)
        {
            warnings.Add(
                "The time-depth curve ends at " + curveDepth.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
                " while the daily report headers record " + reportsDepth.ToString("0.#", CultureInfo.InvariantCulture) + " " +
                unit + ": the activity lines and the report footages disagree by " +
                Math.Abs(curveDepth - reportsDepth).ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + ".");
        }

        if (nptByCause.Any(cause => string.Equals(cause.Cause, OgPerformanceRules.Unclassified, StringComparison.OrdinalIgnoreCase)))
        {
            warnings.Add(
                "Some non-productive time has no recorded cause and is shown as '" + OgPerformanceRules.Unclassified +
                "' rather than being spread across the causes that have one.");
        }

        var incompleteIds = mine
            .Where(row => OgPerformanceRules.Incomplete(
                row.PERIOD_START,
                row.PERIOD_END,
                lines.Where(line => string.Equals(line.REPORT_ID, row.REPORT_ID, StringComparison.OrdinalIgnoreCase))
                    .Sum(line => (line.END_UTC - line.START_UTC).TotalHours)))
            .Select(row => row.REPORT_ID)
            .ToList();

        var incomplete = incompleteIds.Count;

        var totalCost = mine.Count == 0 ? (double?)null : mine.Sum(row => (double?)row.COST ?? 0d);
        var currency = mine.Select(row => row.CURRENCY?.Trim()).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return OgResult<OgPerformanceWorkspace>.Ok(new OgPerformanceWorkspace(
            wellId,
            unit,
            mine.Count,
            incomplete,
            incompleteIds,
            days,
            curveDepth,
            rows.Where(row => string.Equals(row.Code, DrillingToolbox.Settings.DrillingCode, StringComparison.OrdinalIgnoreCase))
                .Sum(row => (row.To ?? 0d) - (row.From ?? 0d)),
            owner.TotalHours,
            owner.NptHours,
            owner.TotalHours > 0d ? owner.NptFraction : null,
            owner.OnBottomRop,
            rows,
            nptByCause,
            rop,
            owner.ByCode.Select(code => new OgCodeTime(code.Code, code.Name, code.Hours, code.Fraction)).ToList(),
            timeDepth,
            owner,
            totalCost,
            currency,
            nptByCause.Sum(cause => cause.CostImpact),
            reportsDepth,
            curveDepth,
            OgPerformanceRules.CurveMatches(curveDepth, reportsDepth),
            warnings,
            DateTime.UtcNow));
    }

    /// <summary>The drilling runs: consecutive drilling-code lines, so a rate of penetration belongs to a real interval.</summary>
    private static IReadOnlyList<OgRopInterval> RunsOf(IReadOnlyList<DEVELOPMENT_REPORT_ACTIVITY> lines)
    {
        var runs = new List<OgRopInterval>();
        var drilling = DrillingToolbox.Settings.DrillingCode;
        var depth = lines.FirstOrDefault(row => row.DEPTH_FROM is not null)?.DEPTH_FROM is { } first ? (double)first : 0d;

        double? from = null;
        var to = 0d;
        var footage = 0d;
        var hours = 0d;

        void Close()
        {
            if (hours > 0d || footage > 0d)
            {
                runs.Add(new OgRopInterval(
                    runs.Count + 1,
                    from ?? to,
                    to,
                    footage,
                    hours,
                    hours > 0d && footage > 0d ? footage / hours : null));
            }

            from = null;
            to = 0d;
            footage = 0d;
            hours = 0d;
        }

        foreach (var line in lines)
        {
            if (!string.Equals(line.CODE?.Trim(), drilling, StringComparison.OrdinalIgnoreCase))
            {
                Close();

                if (line.DEPTH_TO is { } passed)
                {
                    depth = Math.Max(depth, (double)passed);
                }

                continue;
            }

            var start = line.DEPTH_FROM is { } lineFrom ? (double)lineFrom : depth;
            var end = line.DEPTH_TO is { } lineTo ? (double)lineTo : start;

            from ??= start;
            to = end;
            footage += Math.Max(0d, end - start);
            hours += (line.END_UTC - line.START_UTC).TotalHours;
            depth = Math.Max(depth, end);
        }

        Close();

        return runs;
    }

    /// <summary>Each report's cost, its period hours and its currency, which the apportionment needs.</summary>
    private static IReadOnlyDictionary<string, (double Cost, double Hours, string? Currency)> CostsOf(
        IReadOnlyList<DEVELOPMENT_DAILY_REPORT> reports,
        IReadOnlyList<DEVELOPMENT_REPORT_ACTIVITY> lines)
    {
        var costs = new Dictionary<string, (double, double, string?)>(StringComparer.OrdinalIgnoreCase);

        foreach (var report in reports)
        {
            var lineHours = lines
                .Where(line => string.Equals(line.REPORT_ID, report.REPORT_ID, StringComparison.OrdinalIgnoreCase))
                .Sum(line => (line.END_UTC - line.START_UTC).TotalHours);

            var periodHours = report.PERIOD_START is { } start && report.PERIOD_END is { } end && end > start
                ? (end - start).TotalHours
                : lineHours;

            costs[report.REPORT_ID] = ((double)report.COST, Math.Max(periodHours, lineHours), report.CURRENCY);
        }

        return costs;
    }

    private static double CostOf(
        OgReportLine row,
        IReadOnlyDictionary<string, (double Cost, double Hours, string? Currency)> costs) =>
        costs.TryGetValue(row.ReportId, out var report)
            ? OgPerformanceRules.Apportion(report.Cost, row.Hours, report.Hours)
            : 0d;

    /// <summary>
    /// The data lines the owner's parser accepted, numbered as a reader counts them. The acceptance test is the owner's own, so a
    /// row it rejected is never given a number as if it had been imported.
    /// </summary>
    private static IReadOnlyList<int> LineNumbersOf(string csv, IReadOnlyList<ReportActivity> parsed)
    {
        // The same acceptance test the owner applies, so a line it rejected is never numbered as if it had been read.
        var readable = new List<(int Line, string Code)>();
        var number = 1;

        foreach (var line in csv.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').Skip(1))
        {
            number++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = line.Split(',');

            if (fields.Length >= 3 &&
                DateTime.TryParse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _) &&
                DateTime.TryParse(fields[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _))
            {
                readable.Add((number, fields[2].Trim()));
            }
        }

        // Matched by code in order, so a rejected row in the middle does not cost the rows after it their line numbers.
        var numbers = new List<int>(parsed.Count);
        var cursor = 0;

        foreach (var activity in parsed)
        {
            var found = -1;

            for (var index = cursor; index < readable.Count; index++)
            {
                if (string.Equals(readable[index].Code, activity.Code, StringComparison.OrdinalIgnoreCase))
                {
                    found = index;
                    break;
                }
            }

            if (found < 0)
            {
                numbers.Add(0);
                continue;
            }

            numbers.Add(readable[found].Line);
            cursor = found + 1;
        }

        return numbers;
    }

    private static int LineOf(string warning)
    {
        var start = warning.IndexOf("line ", StringComparison.OrdinalIgnoreCase);

        if (start < 0)
        {
            return 0;
        }

        var digits = new string(warning[(start + 5)..].TakeWhile(char.IsDigit).ToArray());

        return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var line) ? line : 0;
    }
}
