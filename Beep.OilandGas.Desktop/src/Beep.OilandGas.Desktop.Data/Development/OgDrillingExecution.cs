using System.Globalization;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One activity interval of a daily report (DEV-06); the breakdown lives in the module's DRILLING_ACTIVITY.</summary>
public sealed record OgDailyActivity(
    string Id,
    int Sequence,
    string? Code,
    decimal? Hours,
    string? Description,
    bool IsNpt,
    string? NptCode,
    decimal? DepthAtStart)
{
    public string Describe() =>
        $"{Code ?? "(no code)"} · {Hours?.ToString("0.##", CultureInfo.InvariantCulture) ?? "-"} h" +
        (string.IsNullOrWhiteSpace(Description) ? string.Empty : $" · {Description}") +
        (IsNpt ? $" · NPT {NptCode ?? "(no code)"}" : string.Empty);
}

/// <summary>One daily report (DEV-06).</summary>
public sealed record OgDailyReport(
    string Id,
    string ExecutionId,
    string ProgramId,
    string WellId,
    DateTime? Date,
    string? Shift,
    int Revision,
    string State,
    decimal StartDepth,
    decimal EndDepth,
    string? DepthUnit,
    decimal? Cost,
    string? Currency,
    string? HseEvents,
    string? Personnel,
    string? Equipment,
    string? Author,
    string? SubmittedBy,
    DateTime? SubmittedAt,
    string? CorrectsReportId,
    string? CorrectionReason,
    IReadOnlyList<OgDailyActivity> Activities,
    string? DepthFlag)
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";

    public bool IsDraft => string.Equals(State, Draft, StringComparison.OrdinalIgnoreCase);

    public bool IsSubmitted => string.Equals(State, Submitted, StringComparison.OrdinalIgnoreCase);

    public string StateText => IsSubmitted ? "submitted" : "draft";

    /// <summary>The metres the report claims; a negative run is not a depth (the service refuses it).</summary>
    public decimal MetresDrilled => EndDepth - StartDepth;

    public string MetresText =>
        $"{MetresDrilled.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit not recorded)"}";

    public decimal NptHours => Activities.Where(activity => activity.IsNpt).Sum(activity => activity.Hours ?? 0m);

    public int ActivityCount => Activities.Count;

    public string DepthText =>
        $"{StartDepth.ToString("0.##", CultureInfo.InvariantCulture)}–" +
        $"{EndDepth.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit not recorded)"}";

    public string CostText => Cost is { } cost
        ? $"{cost.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no cost recorded";

    public string ShiftText => $"{Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)"} " +
                               (Shift ?? "(no shift)");

    /// <summary>AC-6: a start depth that does not match the previous end depth is flagged, not corrected.</summary>
    public bool DepthFlagged => DepthFlag is not null;

    public string Describe() =>
        $"{ShiftText} · rev {Revision.ToString(CultureInfo.InvariantCulture)} · {StateText} · {DepthText} · " +
        $"{CostText}" +
        (Activities.Count == 0 ? " · no activity line" : $" · {Activities.Count.ToString(CultureInfo.InvariantCulture)} line(s)") +
        (NptHours > 0 ? $" · NPT {NptHours.ToString("0.##", CultureInfo.InvariantCulture)} h" : string.Empty) +
        (DepthFlagged ? $" · {DepthFlag}" : string.Empty);
}

/// <summary>One reporting-responsibility period (DEV-06 AC-4).</summary>
public sealed record OgResponsibility(
    string Id,
    string ExecutionId,
    string AssignedTo,
    string? Role,
    DateTime? From,
    DateTime? To,
    string? Reason,
    string? Actor,
    bool Current)
{
    public string Describe() =>
        $"{AssignedTo} ({Role ?? "no role"}) from " +
        $"{From?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "?"}" +
        (To is { } to ? $" to {to.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}" : " · current") +
        (string.IsNullOrWhiteSpace(Reason) ? string.Empty : $" · {Reason}") +
        (string.IsNullOrWhiteSpace(Actor) ? string.Empty : $" · by {Actor}");
}

/// <summary>The execution header, derived from the reports (DEV-06 section 3).</summary>
public sealed record OgExecutionHeader(
    string Id,
    string ProgramId,
    string? FieldId,
    string WellId,
    string Status,
    int ProgramVersion,
    DateTime? SpudDate,
    decimal? CurrentDepth,
    string? DepthUnit,
    string? CurrentActivity,
    int? DaysOnWell,
    decimal CostToDate,
    bool CostIsComplete,
    string? Currency,
    string? Responsible,
    string? ResponsibleRole,
    IReadOnlyList<OgTimeDepthPoint> TimeDepth)
{
    public string DepthText => CurrentDepth is { } depth
        ? $"{depth.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit not recorded)"}"
        : "no depth reported";

    public string CostText => CostIsComplete
        ? $"{CostToDate.ToString("#,0.###", CultureInfo.InvariantCulture)} {Currency ?? "(currency not recorded)"}".TrimEnd()
        : $"partial {CostToDate.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          $"{Currency ?? "(currency not recorded)"}".TrimEnd() + " (a report has no cost recorded)";

    public string ResponsibilityText => Responsible is null
        ? "no reporting responsibility recorded"
        : $"{Responsible} ({ResponsibleRole ?? "no role"})";

    /// <summary>ENG-09's time-depth chart is not built, so the series is shown as points and says so.</summary>
    public string TimeDepthText => TimeDepth.Count == 0
        ? "no report to plot"
        : string.Join(" · ", TimeDepth.Select(point =>
            $"{point.Date:yyyy-MM-dd} {point.Depth.ToString("0.##", CultureInfo.InvariantCulture)}")) +
          " (points only; the ENG-09 chart is not built)";
}

/// <summary>One point of the time-depth series.</summary>
public sealed record OgTimeDepthPoint(DateTime Date, decimal Depth);

/// <summary>One approved program the page may start an execution of.</summary>
public sealed record OgExecutionProgramOption(
    string Id,
    string? Name,
    string WellId,
    string State,
    bool HasExecution)
{
    public string Label => $"{Name ?? Id} · {WellId} · {State.ToLowerInvariant()}" +
                           (HasExecution ? " · execution recorded" : string.Empty);
}

/// <summary>The execution workspace: the executions of the field and the selected one's reports.</summary>
public sealed record OgExecutionWorkspace(
    string FieldName,
    IReadOnlyList<OgExecutionHeader> Executions,
    OgExecutionHeader? Selected,
    IReadOnlyList<OgDailyReport> Reports,
    IReadOnlyList<OgResponsibility> Responsibilities,
    IReadOnlyList<string> NptCodes,
    bool NptReferenceMissing,
    IReadOnlyList<OgExecutionProgramOption> Programs)
{
    public bool IsEmpty => Executions.Count == 0;

    public string? CurrentHolder => Selected?.Responsible;

    /// <summary>The approved programs that have no execution yet.</summary>
    public IReadOnlyList<OgExecutionProgramOption> StartablePrograms =>
        Programs.Where(program => !program.HasExecution).ToList();

    public IReadOnlyList<OgDailyReport> DraftReports => Reports.Where(report => report.IsDraft).ToList();
}

/// <summary>One report, as the page saves it.</summary>
public sealed record OgDailyReportValues(
    DateTime? Date,
    string? Shift,
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    decimal? StartDepth,
    decimal? EndDepth,
    string? DepthUnit,
    decimal? Cost,
    string? Currency,
    string? HseEvents,
    string? Personnel,
    string? Equipment);

/// <summary>One activity line, as the page adds it.</summary>
public sealed record OgDailyActivityValues(
    string? Code,
    decimal? Hours,
    string? Description,
    bool IsNpt,
    string? NptCode,
    decimal? DepthAtStart);

/// <summary>DEV-06: running the program - daily reports, the activity breakdown and reporting responsibility.</summary>
public interface IOgDrillingExecution
{
    Task<OgResult<OgExecutionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? executionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Starts an execution of an approved program.</summary>
    Task<OgResult<OgExecutionWorkspace>> StartAsync(
        string companyId,
        string programId,
        DateTime? spudDate,
        string responsible,
        string? role,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a draft report; a duplicate date+shift is refused (AC-1) and only the current holder may write.</summary>
    Task<OgResult<OgExecutionWorkspace>> CreateReportAsync(
        string executionId,
        OgDailyReportValues values,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgExecutionWorkspace>> SaveReportAsync(
        string reportId,
        OgDailyReportValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Adds an activity line; an NPT line needs a code from the reference set (AC-5).</summary>
    Task<OgResult<OgExecutionWorkspace>> AddActivityAsync(
        string reportId,
        OgDailyActivityValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Submits a draft; a second submission for the same date+shift is refused (AC-1).</summary>
    Task<OgResult<OgExecutionWorkspace>> SubmitReportAsync(
        string reportId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Creates the correcting revision of a submitted report, with a reason (AC-2).</summary>
    Task<OgResult<OgExecutionWorkspace>> ReviseReportAsync(
        string reportId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the current responsibility and opens a new one, recording actor and reason (AC-4).</summary>
    Task<OgResult<OgExecutionWorkspace>> TransferResponsibilityAsync(
        string executionId,
        string assignedTo,
        string? role,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The submit that was not acknowledged, or null (AC-3).</summary>
    OgReportSubmit? Pending { get; }

    Task<OgResult<OgExecutionWorkspace>> RetrySubmitAsync(CancellationToken cancellationToken = default);
}

/// <summary>One submit, retained when the acknowledgement was lost.</summary>
public sealed record OgReportSubmit(string ReportId, string Actor);

/// <summary>
/// The execution over the DrillingAndConstruction-owned <c>DEVELOPMENT_DRILLING_EXECUTION</c>,
/// <c>DEVELOPMENT_DAILY_REPORT</c>, <c>DEVELOPMENT_REPORT_RESPONSIBILITY</c> and <c>R_DRILLING_NPT_CODE</c>, with the
/// breakdown in the module's <c>DRILLING_ACTIVITY</c> rows keyed by the report's id, all through a unit of work.
/// </summary>
/// <remarks>
/// The module's <c>DRILLING_OPERATION</c> and <c>DRILLING_DAILY_REPORT</c> are the company execution records: their
/// schemas require the program review instance, the execution-start proof and the recorder's role/persona. This screen
/// runs the plan-stage execution until that governance gate is met, and never writes the company row set.
/// </remarks>
public sealed class OgDrillingExecutionService : IOgDrillingExecution
{
    public const string CodeNoConnection = "drill-execution-no-connection";
    public const string CodeReadFailed = "drill-execution-read-failed";
    public const string CodeWriteFailed = "drill-execution-write-failed";
    public const string CodeUnknown = "drill-execution-unknown";
    public const string CodeActorRequired = "drill-execution-actor-required";
    public const string CodeProgramRequired = "drill-execution-program-required";
    public const string CodeProgramNotApproved = "drill-execution-program-not-approved";
    public const string CodeAlreadyRunning = "drill-execution-already-running";
    public const string CodeDuplicatePeriod = "drill-execution-duplicate-period";
    public const string CodeReportLocked = "drill-execution-report-locked";
    public const string CodeNotResponsible = "drill-execution-not-responsible";
    public const string CodeDepthUnitRequired = "drill-execution-depth-unit-required";
    public const string CodeDepthInvalid = "drill-execution-depth-invalid";
    public const string CodeCostInvalid = "drill-execution-cost-invalid";
    public const string CodeCurrencyRequired = "drill-execution-currency-required";
    public const string CodeActivityInvalid = "drill-execution-activity-invalid";
    public const string CodeNptCodeRequired = "drill-execution-npt-code-required";
    public const string CodeNptCodeUnknown = "drill-execution-npt-code-unknown";
    public const string CodeNptReferenceMissing = "drill-execution-npt-reference-missing";
    public const string CodeReasonRequired = "drill-execution-reason-required";
    public const string CodeCorrectionNeedsSubmitted = "drill-execution-correction-needs-submitted";
    public const string CodeSameHolder = "drill-execution-same-holder";
    public const string CodeNothingPending = "drill-execution-nothing-pending";

    /// <summary>The shifts a report can cover.</summary>
    public static IReadOnlyList<string> Shifts { get; } = ["DAY", "NIGHT"];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDrillingExecutionService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgReportSubmit? Pending { get; private set; }

    /// <summary>The shared report rules. A depth always carries its unit and never runs backwards.</summary>
    public static (string Code, string Message)? ValidateReport(OgDailyReportValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Date is null)
        {
            return (CodeActivityInvalid, "A daily report needs the day it covers.");
        }

        if (Shifts.Contains(values.Shift?.Trim().ToUpperInvariant() ?? string.Empty) is false)
        {
            return (CodeActivityInvalid, $"A daily report needs a shift; use {string.Join(" or ", Shifts)}.");
        }

        if (values.StartDepth is null || values.EndDepth is null)
        {
            return (CodeDepthInvalid, "A daily report needs the depth at the start and at the end of the period.");
        }

        if (values.StartDepth < 0m || values.EndDepth < 0m)
        {
            return (CodeDepthInvalid, "A depth cannot be negative.");
        }

        if (values.EndDepth < values.StartDepth)
        {
            return (CodeDepthInvalid,
                $"The period ends at {values.EndDepth.Value.ToString("0.##", CultureInfo.InvariantCulture)}, above its " +
                $"start at {values.StartDepth.Value.ToString("0.##", CultureInfo.InvariantCulture)}.");
        }

        if (string.IsNullOrWhiteSpace(values.DepthUnit))
        {
            return (CodeDepthUnitRequired, "A depth needs the unit it is measured in (m or ft).");
        }

        if (values.Cost is < 0m)
        {
            return (CodeCostInvalid, "A report cost cannot be negative.");
        }

        if (values.Cost is > 0m && string.IsNullOrWhiteSpace(values.Currency))
        {
            return (CodeCurrencyRequired, "A report cost needs the currency it is in.");
        }

        if (values.PeriodStart is { } start && values.PeriodEnd is { } end && end <= start)
        {
            return (CodeActivityInvalid, "The period must end after it starts.");
        }

        return null;
    }

    /// <summary>AC-5: an NPT line is only accepted with a code the reference set holds.</summary>
    public static (string Code, string Message)? ValidateActivity(
        OgDailyActivityValues values,
        IReadOnlyCollection<string> nptCodes)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(nptCodes);

        if (values.Hours is not > 0m)
        {
            return (CodeActivityInvalid, "An activity line needs hours greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(values.Code))
        {
            return (CodeActivityInvalid, "An activity line needs its activity code.");
        }

        if (values.DepthAtStart is < 0m)
        {
            return (CodeActivityInvalid, "An activity's depth cannot be negative.");
        }

        if (!values.IsNpt)
        {
            return null;
        }

        if (nptCodes.Count == 0)
        {
            return (CodeNptReferenceMissing,
                "No NPT reason code is recorded in the reference table, so a non-productive time line cannot be accepted.");
        }

        if (string.IsNullOrWhiteSpace(values.NptCode))
        {
            return (CodeNptCodeRequired, "A non-productive time line needs a reason code from the reference set.");
        }

        return !nptCodes.Contains(values.NptCode.Trim(), StringComparer.OrdinalIgnoreCase)
            ? (CodeNptCodeUnknown,
                $"'{values.NptCode}' is not in the NPT reference set; use {string.Join(", ", nptCodes)}.")
            : null;
    }

    /// <summary>
    /// AC-6: the report's start depth must equal the previous report's end depth; when it does not, the message names both
    /// numbers and the report is flagged rather than adjusted.
    /// </summary>
    public static string? DepthFlag(decimal start, decimal? previousEnd, string? unit)
    {
        if (previousEnd is not { } previous)
        {
            return null;
        }

        return Math.Abs(previous - start) < 0.005m
            ? null
            : $"starts at {start.ToString("0.##", CultureInfo.InvariantCulture)} but the previous report ended at " +
              $"{previous.ToString("0.##", CultureInfo.InvariantCulture)} {unit ?? string.Empty}".TrimEnd();
    }

    public async Task<OgResult<OgExecutionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? executionId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the drilling execution cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var reports = BuildReports(model);
            var responsibilities = model.Responsibilities
                .OrderByDescending(row => row.FROM_AT)
                .Select(row => new OgResponsibility(
                    row.RESPONSIBILITY_ID ?? string.Empty,
                    row.EXECUTION_ID ?? string.Empty,
                    row.ASSIGNED_TO ?? string.Empty,
                    Blank(row.ROLE),
                    row.FROM_AT,
                    row.TO_AT,
                    Blank(row.REASON),
                    Blank(row.ACTOR),
                    !string.Equals(row.CURRENT_IND, "N", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var headers = model.Executions
                .Where(execution => FieldMatches(execution.FIELD_ID, fieldId, fieldName))
                .OrderByDescending(execution => execution.SPUD_DATE)
                .ThenBy(execution => execution.EXECUTION_ID, StringComparer.Ordinal)
                .Select(execution => Header(execution, reports, responsibilities))
                .ToList();

            var selected = string.IsNullOrWhiteSpace(executionId)
                ? headers.FirstOrDefault()
                : headers.FirstOrDefault(header =>
                    string.Equals(header.Id, executionId.Trim(), StringComparison.OrdinalIgnoreCase));

            var running = model.Executions
                .Select(execution => execution.PROGRAM_ID ?? string.Empty)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var programs = model.Programs
                .Where(program => string.Equals(program.STATE, OgDrillProgram.Approved, StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(program.STATE, OgDrillProgram.ApprovedWithComments,
                                      StringComparison.OrdinalIgnoreCase))
                .Where(program => FieldMatches(program.FIELD_ID, fieldId, fieldName))
                .OrderBy(program => program.PROGRAM_NAME, StringComparer.OrdinalIgnoreCase)
                .Select(program => new OgExecutionProgramOption(
                    program.PROGRAM_ID,
                    Blank(program.PROGRAM_NAME),
                    program.WELL_ID,
                    program.STATE ?? string.Empty,
                    running.Contains(program.PROGRAM_ID)))
                .ToList();

            return OgResult<OgExecutionWorkspace>.Ok(new OgExecutionWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                headers,
                selected,
                selected is null
                    ? []
                    : reports
                        .Where(report => string.Equals(report.ExecutionId, selected.Id, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(report => report.Date)
                        .ThenBy(report => report.Shift, StringComparer.Ordinal)
                        .ToList(),
                selected is null
                    ? []
                    : responsibilities
                        .Where(row => string.Equals(row.ExecutionId, selected.Id, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                NptCodes(model),
                NptCodes(model).Count == 0,
                programs));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the drilling execution",
                "the execution is not shown",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The drilling execution could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> StartAsync(
        string companyId,
        string programId,
        DateTime? spudDate,
        string responsible,
        string? role,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(responsible))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "Starting an execution must record who runs it and who is responsible.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(programId))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "Choose the approved program the execution runs.", CodeProgramRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the execution cannot be started.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var program = model.Programs.FirstOrDefault(row =>
                string.Equals(row.PROGRAM_ID, programId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (program is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            // Only an approved program may be run.
            if (!string.Equals(program.STATE, OgDrillProgram.Approved, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(program.STATE, OgDrillProgram.ApprovedWithComments, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Program '{programId}' is {program.STATE}; only an approved program can be executed.",
                    CodeProgramNotApproved);
            }

            if (model.Executions.Any(execution =>
                    string.Equals(execution.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Program '{programId}' already has an execution.", CodeAlreadyRunning);
            }

            var now = DateTime.UtcNow;
            var executionId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_DRILLING_EXECUTION>(source);
            unit.PrimaryKey = "EXECUTION_ID";

            unit.Add(new DEVELOPMENT_DRILLING_EXECUTION
            {
                EXECUTION_ID = executionId,
                PROGRAM_ID = program.PROGRAM_ID!,
                FIELD_ID = program.FIELD_ID,
                WELL_ID = program.WELL_ID,
                COMPANY_ID = Blank(companyId),
                STATUS = "ACTIVE",
                PROGRAM_VERSION = program.VERSION,
                SPUD_DATE = spudDate?.Date,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The execution was not started."), CodeWriteFailed);
            }

            // The opening responsibility period is part of starting the execution.
            var responsibility = OgUow.For<DEVELOPMENT_REPORT_RESPONSIBILITY>(source);
            responsibility.PrimaryKey = "RESPONSIBILITY_ID";

            responsibility.Add(new DEVELOPMENT_REPORT_RESPONSIBILITY
            {
                RESPONSIBILITY_ID = Guid.NewGuid().ToString("N"),
                EXECUTION_ID = executionId,
                ASSIGNED_TO = responsible.Trim(),
                ROLE = Blank(role)?.Trim().ToUpperInvariant(),
                FROM_AT = now,
                CURRENT_IND = "Y",
                ACTOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var responsibilityCommitted = await responsibility.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(responsibilityCommitted))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The opening responsibility of execution {executionId} was not written: " +
                        OgUow.Message(responsibilityCommitted, "the row was not inserted")),
                    "starting the drilling execution",
                    "the execution started but no reporting responsibility is recorded",
                    FailureSeverity.Degraded);
            }

            return await LoadAsync(null, string.Empty, executionId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"starting an execution of program {programId}",
                "the execution was not started",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The execution could not be started.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> CreateReportAsync(
        string executionId,
        OgDailyReportValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A daily report must record who wrote it.", CodeActorRequired);
        }

        if (ValidateReport(values) is { } refusal)
        {
            return OgResult<OgExecutionWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the report cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var execution = model.Executions.FirstOrDefault(row =>
                string.Equals(row.EXECUTION_ID, executionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (execution is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Execution '{executionId}' is not in this database.", CodeUnknown);
            }

            // The previous holder cannot write after a transfer: responsibility is checked before the report exists.
            var holder = CurrentHolder(model, execution.EXECUTION_ID!);

            if (holder is not null && !string.Equals(holder, actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Reporting responsibility for this well is held by {holder}, so {actor.Trim()} cannot add a report.",
                    CodeNotResponsible);
            }

            var date = values.Date!.Value.Date;
            var shift = values.Shift!.Trim().ToUpperInvariant();

            // AC-1 (at creation): one report per date and shift, whatever its state.
            if (model.Reports.Any(row =>
                    string.Equals(row.EXECUTION_ID, execution.EXECUTION_ID, StringComparison.OrdinalIgnoreCase)
                    && row.REPORT_DATE?.Date == date
                    && string.Equals((row.SHIFT ?? string.Empty).Trim(), shift, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"A report for {date:yyyy-MM-dd} {shift} already exists on this well; open it or correct it.",
                    CodeDuplicatePeriod);
            }

            var now = DateTime.UtcNow;
            var reportId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_DAILY_REPORT>(source);
            unit.PrimaryKey = "REPORT_ID";

            unit.Add(new DEVELOPMENT_DAILY_REPORT
            {
                REPORT_ID = reportId,
                EXECUTION_ID = execution.EXECUTION_ID!,
                PROGRAM_ID = execution.PROGRAM_ID ?? string.Empty,
                WELL_ID = execution.WELL_ID ?? string.Empty,
                REPORT_DATE = date,
                SHIFT = shift,
                REVISION = 1,
                PERIOD_START = values.PeriodStart,
                PERIOD_END = values.PeriodEnd,
                DEPTH_START = values.StartDepth!.Value,
                DEPTH_END = values.EndDepth!.Value,
                DEPTH_OUOM = Blank(values.DepthUnit)?.Trim(),
                COST = values.Cost,
                CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant(),
                HSE_EVENTS = Blank(values.HseEvents),
                PERSONNEL = Blank(values.Personnel),
                EQUIPMENT = Blank(values.Equipment),
                STATE = OgDailyReport.Draft,
                AUTHOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await LoadAsync(null, string.Empty, execution.EXECUTION_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The report was not created."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating a daily report on execution {executionId}",
                "the report was not created",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The report could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> SaveReportAsync(
        string reportId,
        OgDailyReportValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A report change must record who made it.", CodeActorRequired);
        }

        if (ValidateReport(values) is { } refusal)
        {
            return OgResult<OgExecutionWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the report cannot be saved.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<DEVELOPMENT_DAILY_REPORT>(source);
            unit.PrimaryKey = "REPORT_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("REPORT_ID", reportId.Trim())], cancellationToken)
                .ConfigureAwait(false);
            var report = rows.FirstOrDefault();

            if (report is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Report '{reportId}' is not in this database.", CodeUnknown);
            }

            // AC-2: a submitted report is read-only.
            if (string.Equals(report.STATE, OgDailyReport.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    "This report is submitted and read-only; record a correction with a reason instead.",
                    CodeReportLocked);
            }

            var date = values.Date!.Value.Date;
            var shift = values.Shift!.Trim().ToUpperInvariant();

            // Changing the date or shift must not collide with another report (AC-1).
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            if (model.Reports.Any(row =>
                    !string.Equals(row.REPORT_ID, report.REPORT_ID, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.EXECUTION_ID, report.EXECUTION_ID, StringComparison.OrdinalIgnoreCase)
                    && row.REPORT_DATE?.Date == date
                    && string.Equals((row.SHIFT ?? string.Empty).Trim(), shift, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"A report for {date:yyyy-MM-dd} {shift} already exists on this well.", CodeDuplicatePeriod);
            }

            report.REPORT_DATE = date;
            report.SHIFT = shift;
            report.PERIOD_START = values.PeriodStart;
            report.PERIOD_END = values.PeriodEnd;
            report.DEPTH_START = values.StartDepth!.Value;
            report.DEPTH_END = values.EndDepth!.Value;
            report.DEPTH_OUOM = Blank(values.DepthUnit)?.Trim();
            report.COST = values.Cost;
            report.CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant();
            report.HSE_EVENTS = Blank(values.HseEvents);
            report.PERSONNEL = Blank(values.Personnel);
            report.EQUIPMENT = Blank(values.Equipment);
            report.ROW_CHANGED_BY = actor.Trim();
            report.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(report);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(updated, "The report was not saved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await LoadAsync(null, string.Empty, report.EXECUTION_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The report was not saved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving daily report {reportId}",
                "the report was not saved",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The report could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> AddActivityAsync(
        string reportId,
        OgDailyActivityValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "An activity change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the activity cannot be added.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var report = model.Reports.FirstOrDefault(row =>
                string.Equals(row.REPORT_ID, reportId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (report is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Report '{reportId}' is not in this database.", CodeUnknown);
            }

            if (string.Equals(report.STATE, OgDailyReport.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    "This report is submitted and read-only; record a correction with a reason instead.",
                    CodeReportLocked);
            }

            if (ValidateActivity(values, NptCodes(model)) is { } refusal)
            {
                return OgResult<OgExecutionWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var existing = model.Activities
                .Where(row => string.Equals(row.DDR_ID, report.REPORT_ID, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.START_TIME)
                .ToList();

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DRILLING_ACTIVITY>(source);
            unit.PrimaryKey = "ACTIVITY_ID";

            var start = report.PERIOD_START ?? report.REPORT_DATE;

            unit.Add(new DRILLING_ACTIVITY
            {
                ACTIVITY_ID = Guid.NewGuid().ToString("N"),
                DDR_ID = report.REPORT_ID!,
                WELL_ID = report.WELL_ID,
                START_TIME = start?.AddHours(
                    (double)existing.Sum(row => row.DURATION_HOURS ?? 0m)),
                END_TIME = start?.AddHours(
                    (double)(existing.Sum(row => row.DURATION_HOURS ?? 0m) + values.Hours!.Value)),
                DURATION_HOURS = values.Hours,
                ACTIVITY_CODE = values.Code!.Trim().ToUpperInvariant(),
                NPT_IND = values.IsNpt ? "Y" : "N",
                NPT_CODE_IADC = values.IsNpt ? values.NptCode!.Trim() : null,
                DEPTH_AT_START_MD = values.DepthAtStart,
                DEPTH_OUOM = report.DEPTH_OUOM,
                ACTIVITY_DESCRIPTION = Blank(values.Description),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await LoadAsync(null, string.Empty, report.EXECUTION_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The activity was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"adding an activity to daily report {reportId}",
                "the activity was not recorded",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The activity could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> SubmitReportAsync(
        string reportId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var submitted = await SubmitCoreAsync(reportId, actor, cancellationToken).ConfigureAwait(false);

        if (submitted.Success)
        {
            Pending = null;
        }

        return submitted;
    }

    public async Task<OgResult<OgExecutionWorkspace>> ReviseReportAsync(
        string reportId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A correction must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A correction must record why it was made.", CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the correction cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var previous = model.Reports.FirstOrDefault(row =>
                string.Equals(row.REPORT_ID, reportId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (previous is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Report '{reportId}' is not in this database.", CodeUnknown);
            }

            // AC-2: a correction exists for a submitted report; a draft is edited in place.
            if (!string.Equals(previous.STATE, OgDailyReport.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    "Only a submitted report is corrected; a draft is edited in place.",
                    CodeCorrectionNeedsSubmitted);
            }

            var now = DateTime.UtcNow;
            var revision = model.Reports
                .Where(row => string.Equals(row.EXECUTION_ID, previous.EXECUTION_ID, StringComparison.OrdinalIgnoreCase)
                              && row.REPORT_DATE?.Date == previous.REPORT_DATE?.Date
                              && string.Equals((row.SHIFT ?? string.Empty).Trim(), (previous.SHIFT ?? string.Empty).Trim(),
                                  StringComparison.OrdinalIgnoreCase))
                .Select(row => row.REVISION)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var unit = OgUow.For<DEVELOPMENT_DAILY_REPORT>(source);
            unit.PrimaryKey = "REPORT_ID";

            var newId = Guid.NewGuid().ToString("N");

            unit.Add(new DEVELOPMENT_DAILY_REPORT
            {
                REPORT_ID = newId,
                EXECUTION_ID = previous.EXECUTION_ID,
                PROGRAM_ID = previous.PROGRAM_ID,
                WELL_ID = previous.WELL_ID,
                REPORT_DATE = previous.REPORT_DATE,
                SHIFT = previous.SHIFT,
                REVISION = revision,
                CORRECTS_REPORT_ID = previous.REPORT_ID,
                CORRECTION_REASON = reason.Trim(),
                PERIOD_START = previous.PERIOD_START,
                PERIOD_END = previous.PERIOD_END,
                DEPTH_START = previous.DEPTH_START,
                DEPTH_END = previous.DEPTH_END,
                DEPTH_OUOM = previous.DEPTH_OUOM,
                COST = previous.COST,
                CURRENCY = previous.CURRENCY,
                HSE_EVENTS = previous.HSE_EVENTS,
                PERSONNEL = previous.PERSONNEL,
                EQUIPMENT = previous.EQUIPMENT,
                STATE = OgDailyReport.Draft,
                AUTHOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The correction was not created."), CodeWriteFailed);
            }

            // The corrected report carries the same activity lines, so the revising author sees what was submitted.
            var activities = model.Activities
                .Where(row => string.Equals(row.DDR_ID, previous.REPORT_ID, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.START_TIME)
                .ToList();

            if (activities.Count > 0)
            {
                var activityUnit = OgUow.For<DRILLING_ACTIVITY>(source);
                activityUnit.PrimaryKey = "ACTIVITY_ID";

                foreach (var row in activities)
                {
                    activityUnit.Add(new DRILLING_ACTIVITY
                    {
                        ACTIVITY_ID = Guid.NewGuid().ToString("N"),
                        DDR_ID = newId,
                        WELL_ID = row.WELL_ID,
                        START_TIME = row.START_TIME,
                        END_TIME = row.END_TIME,
                        DURATION_HOURS = row.DURATION_HOURS,
                        ACTIVITY_CODE = row.ACTIVITY_CODE,
                        NPT_IND = row.NPT_IND,
                        NPT_CODE_IADC = row.NPT_CODE_IADC,
                        DEPTH_AT_START_MD = row.DEPTH_AT_START_MD,
                        DEPTH_OUOM = row.DEPTH_OUOM,
                        ACTIVITY_DESCRIPTION = row.ACTIVITY_DESCRIPTION,
                        ROW_CREATED_BY = actor.Trim(),
                        ROW_CREATED_DATE = now,
                        ROW_CHANGED_BY = actor.Trim(),
                        ROW_CHANGED_DATE = now,
                    });
                }

                var copied = await activityUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(copied))
                {
                    _failures.ReportHandled(
                        new InvalidOperationException(
                            $"The activity lines of report {previous.REPORT_ID} were not copied to the correction: " +
                            OgUow.Message(copied, "the rows were not inserted")),
                        $"copying the activity lines into the correction of {previous.REPORT_ID}",
                        "the correction exists but has no activity lines",
                        FailureSeverity.Degraded);
                }
            }

            return await LoadAsync(null, string.Empty, previous.EXECUTION_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"correcting daily report {reportId}",
                "the correction was not created",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The correction could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> TransferResponsibilityAsync(
        string executionId,
        string assignedTo,
        string? role,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(assignedTo))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A transfer must record who hands over and who receives.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "A transfer must record why it was made.", CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the transfer cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var execution = model.Executions.FirstOrDefault(row =>
                string.Equals(row.EXECUTION_ID, executionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (execution is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Execution '{executionId}' is not in this database.", CodeUnknown);
            }

            var current = CurrentHolder(model, execution.EXECUTION_ID!);

            if (current is not null
                && string.Equals(current, assignedTo.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"{assignedTo.Trim()} already holds reporting responsibility for this well.", CodeSameHolder);
            }

            var now = DateTime.UtcNow;

            // Close the open period first, so a failure cannot leave two holders.
            var open = model.Responsibilities
                .Where(row => string.Equals(row.EXECUTION_ID, execution.EXECUTION_ID, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.CURRENT_IND, "Y", StringComparison.OrdinalIgnoreCase)
                              || string.IsNullOrWhiteSpace(row.CURRENT_IND))
                .ToList();

            if (open.Count > 0)
            {
                var closeUnit = OgUow.For<DEVELOPMENT_REPORT_RESPONSIBILITY>(source);
                closeUnit.PrimaryKey = "RESPONSIBILITY_ID";

                var loaded = await OgUow.ReadAsync(
                    closeUnit,
                    [OgUow.Equal("EXECUTION_ID", execution.EXECUTION_ID)],
                    cancellationToken).ConfigureAwait(false);

                foreach (var row in loaded.Where(row =>
                             string.Equals(row.CURRENT_IND, "Y", StringComparison.OrdinalIgnoreCase)
                             || string.IsNullOrWhiteSpace(row.CURRENT_IND)))
                {
                    row.CURRENT_IND = "N";
                    row.TO_AT = now;
                    row.ROW_CHANGED_BY = actor.Trim();
                    row.ROW_CHANGED_DATE = now;
                    closeUnit.Update(row);
                }

                var closed = await closeUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(closed))
                {
                    return OgResult<OgExecutionWorkspace>.Failed(
                        OgUow.Message(closed, "The current responsibility was not closed."), CodeWriteFailed);
                }
            }

            var unit = OgUow.For<DEVELOPMENT_REPORT_RESPONSIBILITY>(source);
            unit.PrimaryKey = "RESPONSIBILITY_ID";

            unit.Add(new DEVELOPMENT_REPORT_RESPONSIBILITY
            {
                RESPONSIBILITY_ID = Guid.NewGuid().ToString("N"),
                EXECUTION_ID = execution.EXECUTION_ID!,
                ASSIGNED_TO = assignedTo.Trim(),
                ROLE = Blank(role)?.Trim().ToUpperInvariant(),
                FROM_AT = now,
                CURRENT_IND = "Y",
                REASON = reason.Trim(),
                ACTOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await LoadAsync(null, string.Empty, execution.EXECUTION_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The transfer was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"transferring reporting responsibility on execution {executionId}",
                "the transfer was not recorded",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The transfer could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgExecutionWorkspace>> RetrySubmitAsync(
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "There is no submit waiting to be retried.", CodeNothingPending);
        }

        var retried = await SubmitCoreAsync(pending.ReportId, pending.Actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    private async Task<OgResult<OgExecutionWorkspace>> SubmitCoreAsync(
        string reportId,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgExecutionWorkspace>.Failed("A submit must record who sent it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgExecutionWorkspace>.Failed(
                "No connection is open, so the report cannot be submitted.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var report = model.Reports.FirstOrDefault(row =>
                string.Equals(row.REPORT_ID, reportId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (report is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Report '{reportId}' is not in this database.", CodeUnknown);
            }

            if (string.Equals(report.STATE, OgDailyReport.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    "This report is already submitted.", CodeReportLocked);
            }

            var holder = CurrentHolder(model, report.EXECUTION_ID!);

            if (holder is not null && !string.Equals(holder, actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Reporting responsibility for this well is held by {holder}, so {actor.Trim()} cannot submit this report.",
                    CodeNotResponsible);
            }

            // AC-1: a second submission for the same date and shift is refused.
            var clash = model.Reports.FirstOrDefault(row =>
                !string.Equals(row.REPORT_ID, report.REPORT_ID, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.EXECUTION_ID, report.EXECUTION_ID, StringComparison.OrdinalIgnoreCase)
                && row.REPORT_DATE?.Date == report.REPORT_DATE?.Date
                && string.Equals((row.SHIFT ?? string.Empty).Trim(), (report.SHIFT ?? string.Empty).Trim(),
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.STATE, OgDailyReport.Submitted, StringComparison.OrdinalIgnoreCase));

            if (clash is not null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"{report.REPORT_DATE:yyyy-MM-dd} {report.SHIFT} is already submitted as report " +
                    $"{clash.REPORT_ID}; a correction is a new revision, not a second submission.",
                    CodeDuplicatePeriod);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_DAILY_REPORT>(source);
            unit.PrimaryKey = "REPORT_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("REPORT_ID", report.REPORT_ID)], cancellationToken)
                .ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgExecutionWorkspace>.Failed(
                    $"Report '{reportId}' is not in this database.", CodeUnknown);
            }

            row.STATE = OgDailyReport.Submitted;
            row.SUBMITTED_BY = actor.Trim();
            row.SUBMITTED_AT = now;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                // AC-3: the report is not shown as submitted; the original is retained for a retry.
                Pending = new OgReportSubmit(row.REPORT_ID!, actor.Trim());
                return OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(updated, "The submit was not acknowledged; retry sends the same report."),
                    CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgReportSubmit(row.REPORT_ID!, actor.Trim());
                return OgResult<OgExecutionWorkspace>.Failed(
                    OgUow.Message(committed, "The submit was not acknowledged; retry sends the same report."),
                    CodeWriteFailed);
            }

            return await LoadAsync(null, string.Empty, row.EXECUTION_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgReportSubmit(reportId.Trim(), actor.Trim());
            var reference = _failures.ReportHandled(
                ex,
                $"submitting daily report {reportId}",
                "the submit was not acknowledged; the report is not shown as submitted",
                FailureSeverity.Error);

            return OgResult<OgExecutionWorkspace>.Failed(
                "The report could not be submitted; retry sends the same report.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<ExecutionModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var executions = await OgUow.ReadAllAsync<DEVELOPMENT_DRILLING_EXECUTION>(source).ConfigureAwait(false);
        var reports = await OgUow.ReadAllAsync<DEVELOPMENT_DAILY_REPORT>(source).ConfigureAwait(false);
        var activities = await OgUow.ReadAllAsync<DRILLING_ACTIVITY>(source).ConfigureAwait(false);
        var responsibilities = await OgUow.ReadAllAsync<DEVELOPMENT_REPORT_RESPONSIBILITY>(source).ConfigureAwait(false);
        var nptCodes = await OgUow.ReadAllAsync<R_DRILLING_NPT_CODE>(source).ConfigureAwait(false);
        var programs = await OgUow.ReadAllAsync<DEVELOPMENT_DRILLING_PROGRAM>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ExecutionModel(executions, reports, activities, responsibilities, nptCodes, programs);
    }

    private static IReadOnlyList<string> NptCodes(ExecutionModel model) =>
        model.NptCodes
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.NPT_CODE))
            .Select(row => row.NPT_CODE!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? CurrentHolder(ExecutionModel model, string executionId) =>
        model.Responsibilities
            .Where(row => string.Equals(row.EXECUTION_ID, executionId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.CURRENT_IND, "Y", StringComparison.OrdinalIgnoreCase)
                          || string.IsNullOrWhiteSpace(row.CURRENT_IND))
            .OrderByDescending(row => row.FROM_AT)
            .Select(row => Blank(row.ASSIGNED_TO))
            .FirstOrDefault();

    /// <summary>The reports of the model, with the AC-6 depth check applied against the previous report.</summary>
    private static List<OgDailyReport> BuildReports(ExecutionModel model)
    {
        var result = new List<OgDailyReport>();

        foreach (var execution in model.Executions)
        {
            var ordered = model.Reports
                .Where(row => string.Equals(row.EXECUTION_ID, execution.EXECUTION_ID, StringComparison.OrdinalIgnoreCase))
                .Where(row => row.REPORT_DATE is not null)
                .OrderBy(row => row.REPORT_DATE)
                .ThenBy(row => row.SHIFT == "DAY" ? 0 : 1)
                .ThenBy(row => row.REVISION)
                .ToList();

            decimal? previousEnd = null;

            foreach (var row in ordered)
            {
                var activities = model.Activities
                    .Where(activity => string.Equals(activity.DDR_ID, row.REPORT_ID, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(activity => activity.START_TIME)
                    .Select((activity, index) => new OgDailyActivity(
                        activity.ACTIVITY_ID ?? string.Empty,
                        index + 1,
                        Blank(activity.ACTIVITY_CODE),
                        activity.DURATION_HOURS,
                        Blank(activity.ACTIVITY_DESCRIPTION),
                        string.Equals(activity.NPT_IND, "Y", StringComparison.OrdinalIgnoreCase),
                        Blank(activity.NPT_CODE_IADC),
                        activity.DEPTH_AT_START_MD))
                    .ToList();

                result.Add(new OgDailyReport(
                    row.REPORT_ID ?? string.Empty,
                    row.EXECUTION_ID ?? string.Empty,
                    row.PROGRAM_ID ?? string.Empty,
                    row.WELL_ID ?? string.Empty,
                    row.REPORT_DATE,
                    Blank(row.SHIFT),
                    row.REVISION,
                    Blank(row.STATE) ?? OgDailyReport.Draft,
                    row.DEPTH_START,
                    row.DEPTH_END,
                    Blank(row.DEPTH_OUOM),
                    row.COST,
                    Blank(row.CURRENCY),
                    Blank(row.HSE_EVENTS),
                    Blank(row.PERSONNEL),
                    Blank(row.EQUIPMENT),
                    Blank(row.AUTHOR),
                    Blank(row.SUBMITTED_BY),
                    row.SUBMITTED_AT,
                    Blank(row.CORRECTS_REPORT_ID),
                    Blank(row.CORRECTION_REASON),
                    activities,
                    DepthFlag(row.DEPTH_START, previousEnd, Blank(row.DEPTH_OUOM))));

                // Only the newest revision of a period sets the depth the next report must continue from.
                previousEnd = row.DEPTH_END;
            }
        }

        return result;
    }

    private static OgExecutionHeader Header(
        DEVELOPMENT_DRILLING_EXECUTION execution,
        IReadOnlyList<OgDailyReport> reports,
        IReadOnlyList<OgResponsibility> responsibilities)
    {
        var mine = reports
            .Where(report => string.Equals(report.ExecutionId, execution.EXECUTION_ID, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var newest = mine
            .OrderByDescending(report => report.Date)
            .ThenByDescending(report => report.Shift == "NIGHT" ? 1 : 0)
            .ThenByDescending(report => report.Revision)
            .FirstOrDefault();

        var current = responsibilities
            .Where(row => string.Equals(row.ExecutionId, execution.EXECUTION_ID, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(row => row.Current);

        var costs = mine.Select(report => report.Cost).ToList();

        var series = mine
            .Where(report => report.IsSubmitted)
            .GroupBy(report => report.Date)
            .Select(group => new OgTimeDepthPoint(
                group.Key!.Value.Date,
                group.OrderBy(report => report.Revision).Last().EndDepth))
            .OrderBy(point => point.Date)
            .ToList();

        var days = execution.SPUD_DATE is { } spud && newest?.Date is { } last
            ? Math.Max(0, (last.Date - spud.Date).Days + 1)
            : (int?)null;

        return new OgExecutionHeader(
            execution.EXECUTION_ID ?? string.Empty,
            execution.PROGRAM_ID ?? string.Empty,
            Blank(execution.FIELD_ID),
            execution.WELL_ID ?? string.Empty,
            Blank(execution.STATUS) ?? "ACTIVE",
            execution.PROGRAM_VERSION,
            execution.SPUD_DATE,
            newest?.EndDepth,
            newest?.DepthUnit,
            newest?.Activities.FirstOrDefault()?.Code,
            days,
            costs.Sum(cost => cost ?? 0m),
            mine.Count > 0 && mine.All(report => report.Cost is not null),
            newest?.Currency ?? mine.Select(report => report.Currency).FirstOrDefault(currency => currency is not null),
            current?.AssignedTo,
            current?.Role,
            series);
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record ExecutionModel(
        IReadOnlyList<DEVELOPMENT_DRILLING_EXECUTION> Executions,
        IReadOnlyList<DEVELOPMENT_DAILY_REPORT> Reports,
        IReadOnlyList<DRILLING_ACTIVITY> Activities,
        IReadOnlyList<DEVELOPMENT_REPORT_RESPONSIBILITY> Responsibilities,
        IReadOnlyList<R_DRILLING_NPT_CODE> NptCodes,
        IReadOnlyList<DEVELOPMENT_DRILLING_PROGRAM> Programs);
}
