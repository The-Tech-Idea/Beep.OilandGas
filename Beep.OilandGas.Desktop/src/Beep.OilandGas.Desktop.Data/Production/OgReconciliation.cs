using System.Globalization;
using System.Text;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One step of the waterfall (PRD-05): a stage's volume, its unit and the difference from the stage before it.</summary>
public sealed record OgReconciliationStep(
    string Stage,
    decimal? Value,
    string? Unit,
    decimal? Difference,
    decimal? DifferencePercent,
    bool Available,
    string? Note)
{
    /// <summary>The state table's "missing stage data": the step is an em dash and the waterfall stops there.</summary>
    public string ValueText => Available ? OgProductionDay.Format(Value, Unit) : "—";

    public string DifferenceText => Available && Difference is { } difference
        ? (difference > 0m ? "+" : string.Empty) + OgProductionDay.Format(difference, Unit)
        : "—";

    public string PercentText => DifferencePercent is { } percent
        ? (percent * 100m).ToString("0.##", CultureInfo.InvariantCulture) + "%"
        : "—";

    public string Describe() => $"{Stage.ToLowerInvariant()} {ValueText} ({DifferenceText})";
}

/// <summary>One reconciliation line: a well, or the field itself (PRD-05), matching the plan's <c>OgReconciliationLine</c>.</summary>
public sealed record OgReconciliationLine(
    string Dimension,
    string? Name,
    bool IsField,
    decimal? Produced,
    decimal? Measured,
    decimal? Allocated,
    decimal? Sold,
    decimal? Booked,
    string? Unit,
    IReadOnlyList<OgReconciliationStep> Steps,
    string? ReasonCode,
    string? ReasonNote,
    bool IsMaterial,
    string? UnitMismatch)
{
    /// <summary>AC-2: a material variance without a reason is what blocks the export.</summary>
    public bool NeedsReason => IsMaterial && string.IsNullOrWhiteSpace(ReasonCode);

    public string NameText => Name ?? Dimension;

    public string ReasonText => string.IsNullOrWhiteSpace(ReasonCode)
        ? NeedsReason ? "needs a reason" : "no material variance"
        : $"{ReasonCode}{(string.IsNullOrWhiteSpace(ReasonNote) ? string.Empty : $" ({ReasonNote!.Trim()})")}";

    public string StepsText => Steps.Count == 0
        ? "no stage is recorded"
        : string.Join(" → ", Steps.Select(step => $"{step.Stage.ToLowerInvariant()} {step.ValueText}"));

    public string DifferencesText => Steps
        .Where(step => step.Difference is not null)
        .Select(step => $"{step.Stage.ToLowerInvariant()} {step.DifferenceText} ({step.PercentText})")
        .DefaultIfEmpty("no difference is computable")
        .Aggregate((left, right) => left + " · " + right);

    public string ProducedText => StageText(ReconciliationRules.Produced);
    public string MeasuredText => StageText(ReconciliationRules.Measured);
    public string AllocatedText => StageText(ReconciliationRules.Allocated);
    public string SoldText => StageText(ReconciliationRules.Sold);
    public string BookedText => StageText(ReconciliationRules.Booked);

    public string MaterialText => IsMaterial ? "material" : "within tolerance";

    private string StageText(string stage) =>
        Steps.FirstOrDefault(step => string.Equals(step.Stage, stage, StringComparison.Ordinal))?.ValueText ?? "—";

    public string Describe() =>
        $"{NameText} · {StepsText} · {DifferencesText}" +
        (UnitMismatch is null ? string.Empty : $" · {UnitMismatch}") +
        (IsMaterial ? $" · material variance: {ReasonText}" : string.Empty);
}

/// <summary>One day of the month, so the monthly total can be shown beside its own rows (PRD-05 AC-5).</summary>
public sealed record OgReconciliationDay(
    DateTime Date,
    decimal? Produced,
    decimal? Measured,
    decimal? Allocated,
    decimal? Sold,
    decimal? Booked,
    string? Unit)
{
    public bool HasData => Produced is not null || Measured is not null || Allocated is not null
                           || Sold is not null || Booked is not null;

    public string DateText => Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string ProducedText => OgProductionDay.Format(Produced, Unit);
    public string MeasuredText => OgProductionDay.Format(Measured, Unit);
    public string AllocatedText => OgProductionDay.Format(Allocated, Unit);
    public string SoldText => OgProductionDay.Format(Sold, Unit);
    public string BookedText => OgProductionDay.Format(Booked, Unit);

    public string Text =>
        $"{DateText} · produced {ProducedText} · measured {MeasuredText} · allocated {AllocatedText} · " +
        $"sold {SoldText} · booked {BookedText}";
}

/// <summary>The month's totals, which are the sum of the day rows (PRD-05 AC-5).</summary>
public sealed record OgReconciliationTotals(
    decimal? Produced,
    decimal? Measured,
    decimal? Allocated,
    decimal? Sold,
    decimal? Booked,
    string? Unit)
{
    public IReadOnlyList<OgReconciliationStep> Steps => ReconciliationRules.Steps(this);

    public string Text =>
        string.Join(" → ", Steps.Select(step => $"{step.Stage.ToLowerInvariant()} {step.ValueText}"));
}

/// <summary>The reconciliation of one field and month (PRD-05).</summary>
public sealed record OgReconciliationWorkspace(
    string FieldName,
    string? FieldId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string PeriodLabel,
    string State,
    bool IsClosed,
    string? ClosedBy,
    DateTime? ClosedAt,
    decimal MaterialityPercent,
    IReadOnlyList<OgReconciliationDay> Days,
    IReadOnlyList<OgReconciliationLine> Lines,
    OgReconciliationLine? FieldLine,
    OgReconciliationTotals Monthly,
    IReadOnlyList<string> Reasons,
    bool ReasonReferenceMissing,
    string UnitMismatch,
    string JoinKeysText)
{
    public const string OpenState = "OPEN";
    public const string ClosedState = "CLOSED";

    public bool IsEmpty => Lines.Count == 0;

    public int MaterialCount => Lines.Count(line => line.IsMaterial);

    public int NeedsReasonCount => Lines.Count(line => line.NeedsReason);

    public bool ExportReady => string.IsNullOrEmpty(UnitMismatch) && NeedsReasonCount == 0;

    /// <summary>AC-2 and AC-3: why the export is blocked, in the page's own words.</summary>
    public string ExportBlockedReason => !string.IsNullOrEmpty(UnitMismatch)
        ? UnitMismatch
        : NeedsReasonCount > 0
            ? $"{NeedsReasonCount.ToString(CultureInfo.InvariantCulture)} material variance(s) have no reason, so the " +
              "report cannot be exported"
            : string.Empty;

    /// <summary>AC-1: the waterfall walks one stage at a time and each step is the previous one plus its difference.</summary>
    public bool IsConsistent => ReconciliationRules.IsConsistent(Monthly);

    public string WaterfallText => FieldLine?.StepsText ?? Monthly.Text;

    public string Summary =>
        $"{PeriodLabel} · {Lines.Count.ToString(CultureInfo.InvariantCulture)} line(s) · " +
        $"{MaterialCount.ToString(CultureInfo.InvariantCulture)} material · {State.ToLowerInvariant()} · " +
        $"materiality {MaterialityPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";

    public string ReasonsText => ReasonReferenceMissing
        ? "No reconciliation reason code is recorded, so no variance can be reasoned. Add the closing categories to " +
          "R_RECONCILIATION_REASON."
        : string.Join(", ", Reasons);

    public string DaysText => Days.Count == 0
        ? "no day in the month"
        : string.Join(Environment.NewLine, Days.Select(day => day.Text));

    /// <summary>AC-5 as the page states it: the month total is the sum of the day rows.</summary>
    public string MonthlyText =>
        $"{Monthly.Text} · from {Days.Count(day => day.HasData).ToString(CultureInfo.InvariantCulture)} day(s) with data";
}

/// <summary>One variance reason, as the page assigns it.</summary>
public sealed record OgReconciliationReasonValues(
    string? FieldId,
    DateTime? Period,
    string? Dimension,
    string? Stage,
    string? ReasonCode,
    string? Note);

/// <summary>The exported report (PRD-05): the preview text, the CSV and what blocked or allowed it.</summary>
public sealed record OgReconciliationReport(string Text, string Csv, int Lines, string PeriodLabel);

/// <summary>The reconciliation rules, stated once so the page, the service and the tests agree.</summary>
public static class ReconciliationRules
{
    public const string Produced = "PRODUCED";
    public const string Measured = "MEASURED";
    public const string Allocated = "ALLOCATED";
    public const string Sold = "SOLD";
    public const string Booked = "BOOKED";

    public const decimal DefaultMaterialityPercent = 1m;

    public const string CodeNoConnection = "reconciliation-no-connection";
    public const string CodeReadFailed = "reconciliation-read-failed";
    public const string CodeWriteFailed = "reconciliation-write-failed";
    public const string CodeActorRequired = "reconciliation-actor-required";
    public const string CodePeriodRequired = "reconciliation-period-required";
    public const string CodeDimensionRequired = "reconciliation-dimension-required";
    public const string CodeStageInvalid = "reconciliation-stage-invalid";
    public const string CodeReasonRequired = "reconciliation-reason-required";
    public const string CodeReasonReferenceMissing = "reconciliation-reason-reference-missing";
    public const string CodeReasonUnknown = "reconciliation-reason-unknown";
    public const string CodePeriodClosed = "reconciliation-period-closed";
    public const string CodeUnitMismatch = "reconciliation-unit-mismatch";
    public const string CodeMaterialVarianceWithoutReason = "reconciliation-material-variance-without-reason";
    public const string CodeNothingToExport = "reconciliation-nothing-to-export";
    public const string CodeMaterialityInvalid = "reconciliation-materiality-invalid";

    /// <summary>The waterfall's stages, in order.</summary>
    public static IReadOnlyList<string> Stages { get; } = [Produced, Measured, Allocated, Sold, Booked];

    /// <summary>The first day of the month a date falls in.</summary>
    public static DateTime MonthOf(DateTime date) => new(date.Year, date.Month, 1);

    public static DateTime MonthEnd(DateTime month) => MonthOf(month).AddMonths(1).AddDays(-1);

    /// <summary>AC-2: a variance is material when it exceeds the threshold, as a percentage of the stage before it.</summary>
    public static bool IsMaterial(decimal? difference, decimal? previous, decimal percent)
    {
        if (difference is not { } value || previous is not { } before || before <= 0m)
        {
            return false;
        }

        return Math.Abs(value) > Math.Abs(before) * percent / 100m;
    }

    /// <summary>
    /// Builds the five steps from the stage values. A stage with no record makes the waterfall stop: the step is
    /// unavailable (an em dash) and the stages after it are not compared.
    /// </summary>
    public static IReadOnlyList<OgReconciliationStep> Steps(OgReconciliationTotals totals)
    {
        ArgumentNullException.ThrowIfNull(totals);

        var values = new (string Stage, decimal? Value)[]
        {
            (Produced, totals.Produced),
            (Measured, totals.Measured),
            (Allocated, totals.Allocated),
            (Sold, totals.Sold),
            (Booked, totals.Booked),
        };

        var steps = new List<OgReconciliationStep>();
        decimal? previous = null;
        var stopped = false;

        foreach (var (stage, value) in values)
        {
            if (stopped || value is null)
            {
                steps.Add(new OgReconciliationStep(
                    stage, value, totals.Unit, null, null, false, stopped ? "the waterfall stopped earlier" : "not recorded"));
                stopped = true;
                continue;
            }

            var difference = previous is { } before ? value - before : (decimal?)null;

            var percent = previous is { } baseValue && baseValue > 0m && difference is { } delta
                ? delta / baseValue
                : (decimal?)null;

            steps.Add(new OgReconciliationStep(stage, value, totals.Unit, difference, percent, true, null));
            previous = value;
        }

        return steps;
    }

    /// <summary>
    /// AC-1: every available step equals the previous one plus the step's own difference, and the first step has none.
    /// </summary>
    public static bool IsConsistent(OgReconciliationTotals totals)
    {
        ArgumentNullException.ThrowIfNull(totals);

        var steps = Steps(totals);
        decimal? previous = null;
        var checkedAny = false;

        foreach (var step in steps)
        {
            if (!step.Available || step.Value is not { } value)
            {
                break;
            }

            if (previous is { } before)
            {
                if (step.Difference is not { } difference || before + difference != value)
                {
                    return false;
                }

                checkedAny = true;
            }

            previous = value;
        }

        return checkedAny || steps.Count(step => step.Available) <= 1;
    }

    /// <summary>AC-3: the stages must be stated in one unit, or the comparison is refused.</summary>
    public static string UnitMismatch(IReadOnlyList<OgReconciliationStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var units = steps
            .Where(step => step.Available && !string.IsNullOrWhiteSpace(step.Unit))
            .Select(step => step.Unit!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(unit => unit, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return units.Count <= 1
            ? string.Empty
            : $"the stages are stated in more than one unit ({string.Join(", ", units)}), so the comparison is refused";
    }

    public static (string Code, string Message)? Validate(OgReconciliationReasonValues values, IReadOnlyCollection<string> reasons)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(reasons);

        if (values.Period is null)
        {
            return (CodePeriodRequired, "Choose the month the variance belongs to.");
        }

        if (string.IsNullOrWhiteSpace(values.Dimension))
        {
            return (CodeDimensionRequired, "Choose the line the reason belongs to.");
        }

        var stage = (values.Stage ?? string.Empty).Trim().ToUpperInvariant();

        if (!Stages.Contains(stage, StringComparer.Ordinal))
        {
            return (CodeStageInvalid, $"'{values.Stage}' is not a waterfall stage; use {string.Join(", ", Stages)}.");
        }

        if (string.IsNullOrWhiteSpace(values.ReasonCode))
        {
            return (CodeReasonRequired,
                reasons.Count == 0
                    ? "A variance needs a reason code, and no code is recorded in the reference set yet."
                    : $"A variance needs a reason code; use {string.Join(", ", reasons)}.");
        }

        if (reasons.Count == 0)
        {
            return (CodeReasonReferenceMissing,
                "No reconciliation reason code is recorded in the reference set, so a variance cannot be reasoned.");
        }

        return reasons.Contains(values.ReasonCode.Trim(), StringComparer.OrdinalIgnoreCase)
            ? null
            : (CodeReasonUnknown,
                $"'{values.ReasonCode}' is not in the reconciliation reason set; use {string.Join(", ", reasons)}.");
    }

    /// <summary>The report text and CSV for a workspace, or the refusal that blocks them.</summary>
    public static (string Code, string Message)? ValidateExport(OgReconciliationWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (workspace.IsEmpty)
        {
            return (CodeNothingToExport, "There is no line to report for this month.");
        }

        if (!string.IsNullOrEmpty(workspace.UnitMismatch))
        {
            return (CodeUnitMismatch, workspace.UnitMismatch);
        }

        return workspace.NeedsReasonCount > 0
            ? (CodeMaterialVarianceWithoutReason, workspace.ExportBlockedReason)
            : null;
    }
}

/// <summary>PRD-05: volume reconciliation and reporting.</summary>
public interface IOgReconciliation
{
    Task<OgResult<OgReconciliationWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? period = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default);

    /// <summary>Assigns a variance reason; a closed month refuses it (AC-4).</summary>
    Task<OgResult<OgReconciliationWorkspace>> AssignReasonAsync(
        OgReconciliationReasonValues values,
        string actor,
        string? fieldName = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the month, after which its reasons are read-only (AC-4).</summary>
    Task<OgResult<OgReconciliationWorkspace>> ClosePeriodAsync(
        DateTime period,
        string? fieldId,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Produces the report and its CSV. A material variance without a reason or a stage in another unit is refused
    /// (AC-2, AC-3).
    /// </summary>
    Task<OgResult<OgReconciliationReport>> BuildReportAsync(
        string? fieldId,
        string fieldName,
        DateTime? period = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The reconciliation over the PPDM <c>WELL</c>, <c>PDEN_WELL</c> and <c>PDEN_VOL_SUMMARY</c> measurements, the
/// ProductionOperations <c>FACILITY_MEASUREMENT</c>, the ProductionAccounting <c>RUN_TICKET</c>,
/// <c>PRODUCTION_ALLOCATION</c>, <c>ALLOCATION_RUN</c>/<c>ALLOCATION_RUN_ROW</c> (PRD-04's committed runs) and its own
/// variance, reason and period rows, read and written through their entity models and a unit of work.
/// </remarks>
public sealed class OgReconciliationService : IOgReconciliation
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgReconciliationService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgReconciliationWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? period = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgReconciliationWorkspace>.Failed(
                "No connection is open, so the reconciliation cannot be read.", ReconciliationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReconciliationWorkspace>.Ok(
                Build(model, fieldId, fieldName, period ?? DateTime.Today, materialityPercent));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the volume reconciliation",
                "the reconciliation is not shown",
                FailureSeverity.Error);

            return OgResult<OgReconciliationWorkspace>.Failed(
                "The reconciliation could not be read.", reference ?? ReconciliationRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgReconciliationWorkspace>> AssignReasonAsync(
        OgReconciliationReasonValues values,
        string actor,
        string? fieldName = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReconciliationWorkspace>.Failed(
                "A variance reason must record who assigned it.", ReconciliationRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReconciliationWorkspace>.Failed(
                "No connection is open, so the reason cannot be saved.", ReconciliationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var reasons = Reasons(model);

            if (ReconciliationRules.Validate(values, reasons) is { } refusal)
            {
                return OgResult<OgReconciliationWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var month = ReconciliationRules.MonthOf(values.Period!.Value);
            var field = Blank(values.FieldId);
            var dimension = values.Dimension!.Trim();
            var stage = values.Stage!.Trim().ToUpperInvariant();

            // AC-4: a closed month is read-only.
            if (IsClosed(model, field, month))
            {
                return OgResult<OgReconciliationWorkspace>.Failed(
                    $"{month:yyyy-MM} is closed; its variance reasons are read-only.",
                    ReconciliationRules.CodePeriodClosed);
            }

            var workspace = Build(model, field, fieldName ?? string.Empty, month, materialityPercent);

            var line = workspace.Lines.FirstOrDefault(candidate =>
                string.Equals(candidate.Dimension, dimension, StringComparison.OrdinalIgnoreCase));

            var step = line?.Steps.FirstOrDefault(candidate =>
                string.Equals(candidate.Stage, stage, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var existing = model.Variances.FirstOrDefault(row =>
                string.Equals(row.FIELD_ID, field, StringComparison.OrdinalIgnoreCase)
                && row.PERIOD_MONTH.Date == month
                && string.Equals(row.DIMENSION, dimension, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.STAGE, stage, StringComparison.OrdinalIgnoreCase));

            var unit = OgUow.For<RECONCILIATION_VARIANCE>(source);

            var isNew = existing is null;

            existing ??= new RECONCILIATION_VARIANCE
            {
                VARIANCE_ID = Guid.NewGuid().ToString("N"),
                FIELD_ID = field,
                PERIOD_MONTH = month,
                DIMENSION = dimension,
                STAGE = stage,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
            };

            existing.AMOUNT = step?.Difference;
            existing.VOLUME_OUOM = step?.Unit;
            existing.MATERIAL_IND = line?.IsMaterial == true ? "Y" : "N";
            existing.REASON_CODE = values.ReasonCode!.Trim().ToUpperInvariant();
            existing.NOTE = Blank(values.Note);
            existing.ACTOR = actor.Trim();
            existing.RECORDED_AT = now;
            existing.ROW_CHANGED_BY = actor.Trim();
            existing.ROW_CHANGED_DATE = now;

            unit.PrimaryKey = "VARIANCE_ID";

            // An existing reason is updated in place; a new line is added.
            if (isNew)
            {
                unit.Add(existing);
            }
            else
            {
                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgReconciliationWorkspace>.Failed(
                        OgUow.Message(updated, "The reason was not saved."), ReconciliationRules.CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReconciliationWorkspace>.Failed(
                    OgUow.Message(committed, "The reason was not saved."), ReconciliationRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReconciliationWorkspace>.Ok(
                Build(after, field, fieldName ?? string.Empty, month, materialityPercent));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"assigning a variance reason to {values.Dimension}",
                "the reason was not saved",
                FailureSeverity.Error);

            return OgResult<OgReconciliationWorkspace>.Failed(
                "The reason could not be saved.", reference ?? ReconciliationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReconciliationWorkspace>> ClosePeriodAsync(
        DateTime period,
        string? fieldId,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReconciliationWorkspace>.Failed(
                "Closing a month must record who closed it.", ReconciliationRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReconciliationWorkspace>.Failed(
                "No connection is open, so the month cannot be closed.", ReconciliationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var month = ReconciliationRules.MonthOf(period);
            var field = Blank(fieldId);

            if (IsClosed(model, field, month))
            {
                return OgResult<OgReconciliationWorkspace>.Failed(
                    $"{month:yyyy-MM} is already closed.", ReconciliationRules.CodePeriodClosed);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<RECONCILIATION_PERIOD>(source);
            unit.PrimaryKey = "PERIOD_ID";

            unit.Add(new RECONCILIATION_PERIOD
            {
                PERIOD_ID = Guid.NewGuid().ToString("N"),
                FIELD_ID = field,
                PERIOD_MONTH = month,
                STATE = OgReconciliationWorkspace.ClosedState,
                CLOSED_BY = actor.Trim(),
                CLOSED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReconciliationWorkspace>.Failed(
                    OgUow.Message(committed, "The month was not closed."), ReconciliationRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReconciliationWorkspace>.Ok(
                Build(after, field, fieldName ?? string.Empty, month, ReconciliationRules.DefaultMaterialityPercent));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"closing reconciliation month {period:yyyy-MM}",
                "the month was not closed",
                FailureSeverity.Error);

            return OgResult<OgReconciliationWorkspace>.Failed(
                "The month could not be closed.", reference ?? ReconciliationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReconciliationReport>> BuildReportAsync(
        string? fieldId,
        string fieldName,
        DateTime? period = null,
        decimal materialityPercent = ReconciliationRules.DefaultMaterialityPercent,
        CancellationToken cancellationToken = default)
    {
        var loaded = await LoadAsync(fieldId, fieldName, period, materialityPercent, cancellationToken)
            .ConfigureAwait(false);

        if (!loaded.Success || loaded.Value is null)
        {
            return OgResult<OgReconciliationReport>.Failed(
                loaded.Message ?? "The reconciliation could not be read.", loaded.Reference ?? ReconciliationRules.CodeReadFailed);
        }

        var workspace = loaded.Value;

        // AC-2 and AC-3: the report is refused, not trimmed.
        if (ReconciliationRules.ValidateExport(workspace) is { } refusal)
        {
            return OgResult<OgReconciliationReport>.Failed(refusal.Message, refusal.Code);
        }

        var text = new StringBuilder()
            .AppendLine($"Volume reconciliation · {workspace.FieldName} · {workspace.PeriodLabel}")
            .AppendLine($"materiality {workspace.MaterialityPercent.ToString("0.##", CultureInfo.InvariantCulture)}% · " +
                        $"{workspace.State.ToLowerInvariant()}")
            .AppendLine($"join keys: {workspace.JoinKeysText}")
            .AppendLine()
            .AppendLine("Waterfall (field):")
            .AppendLine("  " + workspace.WaterfallText);

        if (workspace.FieldLine is { } fieldLine)
        {
            text.AppendLine("  " + fieldLine.DifferencesText);
        }

        text.AppendLine()
            .AppendLine("Monthly totals equal the sum of the daily rows (AC-5):")
            .AppendLine("  " + workspace.MonthlyText);

        text.AppendLine().AppendLine("Lines:");

        foreach (var line in workspace.Lines)
        {
            text.AppendLine(
                $"  {line.NameText}: {line.StepsText} · {line.DifferencesText} · reason {line.ReasonText}");
        }

        var csv = new StringBuilder()
            .AppendLine("dimension,name,produced,measured,allocated,sold,booked,unit,material,reason,note");

        foreach (var line in workspace.Lines)
        {
            csv.AppendLine(string.Join(
                ",",
                Quote(line.Dimension),
                Quote(line.Name),
                Number(line.Produced),
                Number(line.Measured),
                Number(line.Allocated),
                Number(line.Sold),
                Number(line.Booked),
                Quote(line.Unit),
                line.IsMaterial ? "Y" : "N",
                Quote(line.ReasonCode),
                Quote(line.ReasonNote)));
        }

        return OgResult<OgReconciliationReport>.Ok(new OgReconciliationReport(
            text.ToString(),
            csv.ToString(),
            workspace.Lines.Count,
            workspace.PeriodLabel));
    }

    // ---------------------------------------------------------------- the build

    private static OgReconciliationWorkspace Build(
        WorkbenchModel model,
        string? fieldId,
        string fieldName,
        DateTime period,
        decimal materialityPercent)
    {
        var month = ReconciliationRules.MonthOf(period);
        var monthEnd = ReconciliationRules.MonthEnd(month);

        var wells = model.Wells
            .Where(row => !string.IsNullOrWhiteSpace(row.UWI))
            .Where(row => FieldMatches(row.ASSIGNED_FIELD, fieldId, fieldName))
            .GroupBy(row => row.UWI!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.WELL_NAME ?? row.UWI, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var wellIds = wells.Select(row => row.UWI!).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entityToWell = model.PdenWells
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && !string.IsNullOrWhiteSpace(row.PRIMARY_UWI))
            .Where(row => wellIds.Contains(row.PRIMARY_UWI!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().PRIMARY_UWI!, StringComparer.OrdinalIgnoreCase);

        var produced = ProducedByWell(model, entityToWell, month, monthEnd, out var measuredUnit);

        var runs = model.Runs
            .Where(row => string.Equals(row.STATE, OgAllocationRun.CommittedState, StringComparison.OrdinalIgnoreCase))
            .Where(row => row.PERIOD_DATE.Date >= month && row.PERIOD_DATE.Date <= monthEnd)
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .Select(row => row.ALLOCATION_RUN_ID ?? string.Empty)
            .Where(id => id.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var measuredByWell = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
        var allocatedByWell = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in model.RunRows.Where(row =>
                     row.ALLOCATION_RUN_ID is not null && runs.Contains(row.ALLOCATION_RUN_ID)))
        {
            if (string.IsNullOrWhiteSpace(row.WELL_ID))
            {
                continue;
            }

            measuredByWell[row.WELL_ID!] = (measuredByWell.GetValueOrDefault(row.WELL_ID!) ?? 0m)
                                           + (row.MEASURED_VOLUME ?? 0m);
            allocatedByWell[row.WELL_ID!] = (allocatedByWell.GetValueOrDefault(row.WELL_ID!) ?? 0m)
                                            + (row.ALLOCATED_VOLUME ?? 0m);
        }

        // The sold volumes are the PPDM disposition rows of this field's production entities.
        var dispositionRows = model.Dispositions
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals((row.ACTIVITY_TYPE ?? string.Empty).Trim(), "INJECTION",
                StringComparison.OrdinalIgnoreCase))
            .Where(row => DispositionDate(row) is { } date && date >= month && date <= monthEnd)
            .ToList();

        var bookedRows = model.ProductionAllocations
            .Where(row => row.ALLOCATION_DATE is { } date && date.Date >= month && date.Date <= monthEnd)
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // AC-3: each stage is stated in the unit its own record carries; a stage in another unit is refused, not added.
        var soldUnit = OperationsCentreRules.DominantUnit(
            dispositionRows.Select(row => Blank(row.VOLUME_OUOM) ?? Blank(row.VOLUME_UOM)));
        var bookedUnit = OperationsCentreRules.DominantUnit(bookedRows.Select(row => row.ALLOCATED_VOLUME_OUOM));
        var runUnit = OperationsCentreRules.DominantUnit(
            model.Runs.Where(row => runs.Contains(row.ALLOCATION_RUN_ID ?? string.Empty))
                .Select(row => row.VOLUME_OUOM));

        var mismatches = new List<string>();

        var measured = SameUnit(measuredUnit, runUnit, ReconciliationRules.Measured, mismatches);
        var allocated = SameUnit(measuredUnit, runUnit, ReconciliationRules.Allocated, mismatches);
        var sold = SameUnit(measuredUnit, soldUnit, ReconciliationRules.Sold, mismatches);
        var booked = SameUnit(measuredUnit, bookedUnit, ReconciliationRules.Booked, mismatches);

        // A run stated in another unit cannot be compared, so its measured and allocated rows are withheld together.
        if (!measured)
        {
            measuredByWell.Clear();
            allocatedByWell.Clear();
        }

        var soldByWell = sold
            ? dispositionRows
                .Where(row => string.Equals(
                    (Blank(row.VOLUME_OUOM) ?? Blank(row.VOLUME_UOM) ?? string.Empty).Trim(),
                    soldUnit ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => entityToWell[row.PDEN_ID!], StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (decimal?)group.Sum(row => row.VOLUME),
                    StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        var bookedByWell = booked
            ? SumByDimension(bookedRows, row => row.WELL_ID, row => row.ALLOCATED_VOLUME ?? row.TOTAL_PRODUCTION)
            : new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        var variances = model.Variances
            .Where(row => row.PERIOD_MONTH.Date == month)
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var lines = new List<OgReconciliationLine>();

        foreach (var well in wells)
        {
            lines.Add(BuildLine(
                well.UWI!,
                Blank(well.WELL_NAME),
                false,
                measuredUnit,
                produced.GetValueOrDefault(well.UWI!),
                measuredByWell.GetValueOrDefault(well.UWI!),
                allocatedByWell.GetValueOrDefault(well.UWI!),
                soldByWell.GetValueOrDefault(well.UWI!),
                bookedByWell.GetValueOrDefault(well.UWI!),
                variances,
                materialityPercent));
        }

        var fieldLine = BuildLine(
            Blank(fieldId) ?? "(field)",
            string.IsNullOrWhiteSpace(fieldName) ? Blank(fieldId) : fieldName.Trim(),
            true,
            measuredUnit,
            SumOrNull(produced.Values),
            SumOrNull(measuredByWell.Values),
            SumOrNull(allocatedByWell.Values),
            SumOrNull(soldByWell.Values),
            SumOrNull(bookedByWell.Values),
            variances,
            materialityPercent);

        var unitMismatch = mismatches.Count == 0
            ? string.Empty
            : "the stages are stated in more than one unit (" +
              string.Join("; ", mismatches.Distinct(StringComparer.OrdinalIgnoreCase)) +
              "), so the comparison is refused";

        var monthly = new OgReconciliationTotals(
            fieldLine.Produced,
            fieldLine.Measured,
            fieldLine.Allocated,
            fieldLine.Sold,
            fieldLine.Booked,
            measuredUnit);

        var days = Enumerable.Range(0, (monthEnd - month).Days + 1)
            .Select(offset => month.AddDays(offset))
            .Select(date => new OgReconciliationDay(
                date,
                ProducedOn(model, entityToWell, date, measuredUnit),
                measuredByWell.Count == 0 ? null : SumRunOn(model, runs, date, measured: true),
                allocatedByWell.Count == 0 ? null : SumRunOn(model, runs, date, measured: false),
                SumSoldOn(model, entityToWell, date),
                SumBookedOn(model, fieldId, date),
                measuredUnit))
            .ToList();

        var closed = model.Periods
            .Where(row => row.PERIOD_MONTH.Date == month)
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.STATE, OgReconciliationWorkspace.ClosedState, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.CLOSED_AT)
            .FirstOrDefault();

        var reasons = Reasons(model);

        return new OgReconciliationWorkspace(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            Blank(fieldId),
            month,
            monthEnd,
            month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            closed is null ? OgReconciliationWorkspace.OpenState : OgReconciliationWorkspace.ClosedState,
            closed is not null,
            Blank(closed?.CLOSED_BY),
            closed?.CLOSED_AT,
            materialityPercent,
            days,
            lines,
            fieldLine,
            monthly,
            reasons,
            reasons.Count == 0,
            unitMismatch,
            "produced = the wells' own PDEN_VOL_SUMMARY rows; measured and allocated = PRD-04's committed " +
            "ALLOCATION_RUN rows; sold = the PPDM PDEN_VOL_DISPOSITION rows; booked = " +
            "PRODUCTION_ALLOCATION.ALLOCATED_VOLUME; each joined on field + well + date. The accounting RUN_TICKET " +
            "cannot be read through an entity model (its compatibility partial duplicates the TEMPERATURE column), so " +
            "the sold volumes come from the PPDM disposition record of the same handover.");
    }

    private static OgReconciliationLine BuildLine(
        string dimension,
        string? name,
        bool isField,
        string? unit,
        decimal? produced,
        decimal? measured,
        decimal? allocated,
        decimal? sold,
        decimal? booked,
        IReadOnlyList<RECONCILIATION_VARIANCE> variances,
        decimal materialityPercent)
    {
        var totals = new OgReconciliationTotals(produced, measured, allocated, sold, booked, unit);
        var steps = ReconciliationRules.Steps(totals);

        // AC-2: the material variance is the first stage whose difference is above the threshold.
        var materialStep = steps.FirstOrDefault(step =>
            ReconciliationRules.IsMaterial(step.Difference, PreviousValue(steps, step.Stage), materialityPercent));

        var variance = materialStep is null
            ? null
            : variances.FirstOrDefault(row =>
                string.Equals(row.DIMENSION, dimension, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.STAGE, materialStep.Stage, StringComparison.OrdinalIgnoreCase));

        return new OgReconciliationLine(
            dimension,
            name,
            isField,
            produced,
            measured,
            allocated,
            sold,
            booked,
            unit,
            steps,
            Blank(variance?.REASON_CODE),
            Blank(variance?.NOTE),
            materialStep is not null,
            ReconciliationRules.UnitMismatch(steps));
    }

    /// <summary>
    /// AC-3: a stage may be compared with the produced volume only when its record states the same unit. A null stage
    /// unit means the stage recorded no unit at all, which is not a mismatch - it is simply unstated.
    /// </summary>
    private static bool SameUnit(string? producedUnit, string? stageUnit, string stage, List<string> mismatches)
    {
        if (string.IsNullOrWhiteSpace(stageUnit) || string.IsNullOrWhiteSpace(producedUnit))
        {
            return true;
        }

        if (string.Equals(stageUnit.Trim(), producedUnit.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        mismatches.Add($"{stage.ToLowerInvariant()} in {stageUnit.Trim()} against produced in {producedUnit.Trim()}");
        return false;
    }

    private static decimal? PreviousValue(IReadOnlyList<OgReconciliationStep> steps, string stage)
    {
        var index = steps.ToList().FindIndex(step => string.Equals(step.Stage, stage, StringComparison.Ordinal));

        return index <= 0 ? null : steps[index - 1].Value;
    }

    // ---------------------------------------------------------------- the reads

    private static async Task<WorkbenchModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);
        var dispositions = await OgUow.ReadAllAsync<PDEN_VOL_DISPOSITION>(source).ConfigureAwait(false);
        var productionAllocations = await OgUow.ReadAllAsync<PRODUCTION_ALLOCATION>(source).ConfigureAwait(false);
        var runs = await OgUow.ReadAllAsync<ALLOCATION_RUN>(source).ConfigureAwait(false);
        var runRows = await OgUow.ReadAllAsync<ALLOCATION_RUN_ROW>(source).ConfigureAwait(false);
        var variances = await OgUow.ReadAllAsync<RECONCILIATION_VARIANCE>(source).ConfigureAwait(false);
        var periods = await OgUow.ReadAllAsync<RECONCILIATION_PERIOD>(source).ConfigureAwait(false);
        var reasons = await OgUow.ReadAllAsync<R_RECONCILIATION_REASON>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new WorkbenchModel(
            wells, pdenWells, volumes, dispositions, productionAllocations, runs, runRows, variances, periods, reasons);
    }

    private static IReadOnlyList<string> Reasons(WorkbenchModel model) =>
        model.Reasons
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REASON_CODE))
            .Select(row => row.REASON_CODE!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool IsClosed(WorkbenchModel model, string? fieldId, DateTime month) =>
        model.Periods.Any(row =>
            row.PERIOD_MONTH.Date == month
            && string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(row.STATE, OgReconciliationWorkspace.ClosedState, StringComparison.OrdinalIgnoreCase));

    private static Dictionary<string, decimal?> ProducedByWell(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime month,
        DateTime monthEnd,
        out string? unit)
    {
        var rows = LatestProduction(model, entityToWell, month, monthEnd);

        var dominant = OperationsCentreRules.DominantUnit(rows.Select(row => row.OIL_VOLUME_OUOM));
        unit = dominant;

        return rows
            .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), dominant ?? string.Empty,
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => entityToWell[row.PDEN_ID!], StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (decimal?)group.Sum(row => row.OIL_VOLUME),
                StringComparer.OrdinalIgnoreCase);
    }

    private static List<PDEN_VOL_SUMMARY> LatestProduction(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime month,
        DateTime monthEnd) =>
        model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE is { } date && date.Date >= month && date.Date <= monthEnd)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

    private static decimal? ProducedOn(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime date,
        string? unit)
    {
        var rows = model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE?.Date == date)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), unit ?? string.Empty,
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

        return rows.Count == 0 ? null : rows.Sum(row => row.OIL_VOLUME);
    }

    private static decimal? SumRunOn(
        WorkbenchModel model,
        IReadOnlySet<string> runIds,
        DateTime date,
        bool measured)
    {
        var runsToday = model.Runs
            .Where(row => row.PERIOD_DATE.Date == date)
            .Where(row => runIds.Contains(row.ALLOCATION_RUN_ID ?? string.Empty))
            .Select(row => row.ALLOCATION_RUN_ID)
            .Where(id => id is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = model.RunRows
            .Where(row => row.ALLOCATION_RUN_ID is not null && runsToday.Contains(row.ALLOCATION_RUN_ID))
            .ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        return measured
            ? rows.Sum(row => row.MEASURED_VOLUME ?? 0m)
            : rows.Sum(row => row.ALLOCATED_VOLUME ?? 0m);
    }

    private static decimal? SumSoldOn(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime date)
    {
        var rows = model.Dispositions
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals((row.ACTIVITY_TYPE ?? string.Empty).Trim(), "INJECTION",
                StringComparison.OrdinalIgnoreCase))
            .Where(row => DispositionDate(row) == date)
            .ToList();

        return rows.Count == 0 ? null : rows.Sum(row => row.VOLUME);
    }

    /// <summary>A disposition row is dated by its volume period's start, or its end when only that is recorded.</summary>
    private static DateTime? DispositionDate(PDEN_VOL_DISPOSITION row) =>
        (row.VOLUME_START_DATE ?? row.VOLUME_END_DATE)?.Date;

    private static decimal? SumBookedOn(WorkbenchModel model, string? fieldId, DateTime date)
    {
        var rows = model.ProductionAllocations
            .Where(row => row.ALLOCATION_DATE?.Date == date)
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return rows.Count == 0 ? null : rows.Sum(row => row.ALLOCATED_VOLUME ?? row.TOTAL_PRODUCTION);
    }

    private static Dictionary<string, decimal?> SumByDimension<T>(
        IEnumerable<T> rows,
        Func<T, string?> dimension,
        Func<T, decimal> value)
    {
        var result = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var key = dimension(row);

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key!.Trim()] = (result.GetValueOrDefault(key.Trim()) ?? 0m) + value(row);
        }

        return result;
    }

    private static decimal? SumOrNull(IEnumerable<decimal?> values)
    {
        var list = values.Where(value => value is not null).Select(value => value!.Value).ToList();

        return list.Count == 0 ? null : list.Sum();
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string Number(decimal? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Quote(string? value) =>
        value is null ? string.Empty : "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private sealed record WorkbenchModel(
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<PDEN_VOL_DISPOSITION> Dispositions,
        IReadOnlyList<PRODUCTION_ALLOCATION> ProductionAllocations,
        IReadOnlyList<ALLOCATION_RUN> Runs,
        IReadOnlyList<ALLOCATION_RUN_ROW> RunRows,
        IReadOnlyList<RECONCILIATION_VARIANCE> Variances,
        IReadOnlyList<RECONCILIATION_PERIOD> Periods,
        IReadOnlyList<R_RECONCILIATION_REASON> Reasons);
}
