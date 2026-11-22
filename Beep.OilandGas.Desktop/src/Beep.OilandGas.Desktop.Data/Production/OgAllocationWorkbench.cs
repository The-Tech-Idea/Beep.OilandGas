using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One row of an allocation run: what the well measured, what it was allocated, and why (PRD-04).</summary>
public sealed record OgAllocationRow(
    string WellId,
    string? WellName,
    decimal? Measured,
    decimal? Allocated,
    string? Unit,
    decimal? Factor,
    decimal? Residual,
    string Method,
    string Status,
    string Basis,
    string? OverrideReason,
    string? OverrideBy)
{
    public const string AllocatedStatus = "ALLOCATED";
    public const string OverriddenStatus = "OVERRIDDEN";
    public const string UnallocatedStatus = "UNALLOCATED";

    /// <summary>The state table's "no method" row: it is unallocated, never a zero.</summary>
    public bool IsUnallocated => string.Equals(Status, UnallocatedStatus, StringComparison.OrdinalIgnoreCase);

    public bool IsOverridden => string.Equals(Status, OverriddenStatus, StringComparison.OrdinalIgnoreCase);

    public string NameText => WellName ?? WellId;

    public string MeasuredText => OgProductionDay.Format(Measured, Unit);

    /// <summary>AC-1: an unallocated row says so; it does not show a zero that reads as "allocated nothing".</summary>
    public string AllocatedText => IsUnallocated
        ? "unallocated"
        : OgProductionDay.Format(Allocated, Unit);

    public string FactorText => Factor is { } factor
        ? (factor * 100m).ToString("0.###", CultureInfo.InvariantCulture) + "%"
        : "no factor";

    public string ResidualText => IsUnallocated
        ? "no residual"
        : (Residual is { } residual && residual > 0m ? "+" : string.Empty) + OgProductionDay.Format(Residual, Unit);

    public string StatusText => Status.ToLowerInvariant();

    public string Describe() =>
        $"{NameText} · measured {MeasuredText} · allocated {AllocatedText} · factor {FactorText} · " +
        $"residual {ResidualText} · {Method.ToLowerInvariant()} ({StatusText})" +
        (string.IsNullOrWhiteSpace(Basis) ? string.Empty : $" · {Basis}") +
        (IsOverridden
            ? $" · override by {OverrideBy ?? "(unknown)"}: {OverrideReason ?? "(no reason)"}"
            : string.Empty);
}

/// <summary>One allocation run: the inputs, the rows, the totals, the residual and the input hash (PRD-04).</summary>
public sealed record OgAllocationRun(
    string Id,
    string? FieldId,
    string? FacilityId,
    DateTime Period,
    string Method,
    IReadOnlyList<OgAllocationRow> Rows,
    decimal? TotalMeasured,
    decimal? TotalAllocated,
    decimal? Residual,
    string? Unit,
    string Hash,
    string State,
    decimal TolerancePercent,
    bool Acknowledged,
    string? CommittedBy,
    DateTime? CommittedAt,
    string MeasuredSource)
{
    public const string DraftState = "DRAFT";
    public const string CommittedState = "COMMITTED";

    public bool IsCommitted => string.Equals(State, CommittedState, StringComparison.OrdinalIgnoreCase);

    public int RowCount => Rows.Count;

    public int UnallocatedCount => Rows.Count(row => row.IsUnallocated);

    public int OverriddenCount => Rows.Count(row => row.IsOverridden);

    /// <summary>The residual that is still acceptable for this run, as an absolute volume.</summary>
    public decimal? ToleranceValue => TotalMeasured is { } measured
        ? Math.Abs(measured) * TolerancePercent / 100m
        : null;

    /// <summary>AC-2: the run is flagged when the allocated total is not the measured total within tolerance.</summary>
    public bool IsBeyondTolerance =>
        Residual is { } residual && ToleranceValue is { } tolerance && Math.Abs(residual) > tolerance;

    public string StateText => State.ToLowerInvariant();

    public string TotalMeasuredText => OgProductionDay.Format(TotalMeasured, Unit);

    public string TotalAllocatedText => OgProductionDay.Format(TotalAllocated, Unit);

    public string ResidualText =>
        (Residual is { } residual && residual > 0m ? "+" : string.Empty) + OgProductionDay.Format(Residual, Unit);

    public string ToleranceText => ToleranceValue is { } tolerance
        ? $"{tolerance.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Unit) ? string.Empty : Unit!.Trim()).Trim() +
          $" ({TolerancePercent.ToString("0.###", CultureInfo.InvariantCulture)}% of measured)"
        : "no tolerance: nothing was measured";

    public string PeriodText => Period.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>The flags the page shows; an acknowledged residual is still a flag.</summary>
    public IReadOnlyList<string> Flags
    {
        get
        {
            var flags = new List<string>();

            if (IsBeyondTolerance)
            {
                flags.Add(
                    $"the residual {ResidualText} is beyond the tolerance {ToleranceText}" +
                    (Acknowledged ? " (acknowledged before the commit)" : " and has not been acknowledged"));
            }

            if (UnallocatedCount > 0)
            {
                flags.Add($"{UnallocatedCount.ToString(CultureInfo.InvariantCulture)} row(s) are unallocated");
            }

            if (OverriddenCount > 0)
            {
                flags.Add($"{OverriddenCount.ToString(CultureInfo.InvariantCulture)} row(s) were manually overridden");
            }

            return flags;
        }
    }

    public string FlagsText => Flags.Count == 0
        ? "no flag: every row is allocated and the residual is within tolerance"
        : string.Join(" · ", Flags);

    public string BasisText => Rows.Count == 0
        ? "no well is measured for this period"
        : string.Join(
            " · ",
            Rows.Select(row =>
                $"{row.NameText}: {row.Method.ToLowerInvariant()} - {row.Basis}" +
                (row.IsOverridden
                    ? $" (override by {row.OverrideBy ?? "the operator"}: {row.OverrideReason ?? "(no reason)"})"
                    : string.Empty)));

    public string Summary =>
        $"{PeriodText} · {Method.ToLowerInvariant()} · {RowCount.ToString(CultureInfo.InvariantCulture)} row(s) · " +
        $"measured {TotalMeasuredText} · allocated {TotalAllocatedText} · residual {ResidualText} · " +
        $"tolerance {ToleranceText} · {StateText}";
}

/// <summary>One facility that has a measurement, so the run can be pinned to it (PRD-04).</summary>
public sealed record OgAllocationFacility(string Id, string Label);

/// <summary>One manual override, as the page enters it (AC-3).</summary>
public sealed record OgAllocationOverride(string WellId, decimal Allocated, string? Reason);

/// <summary>The inputs of a run; the same inputs produce the same hash (AC-6).</summary>
public sealed record OgAllocationInputs(
    string? FieldId,
    string? FacilityId,
    DateTime Period,
    string Method,
    decimal TolerancePercent,
    IReadOnlyList<OgAllocationOverride> Overrides);

/// <summary>The workbench: the run being examined or committed, and the committed history.</summary>
public sealed record OgAllocationWorkspace(
    string FieldName,
    string? FieldId,
    DateTime Period,
    string Method,
    decimal TolerancePercent,
    IReadOnlyList<string> Methods,
    IReadOnlyList<OgAllocationFacility> Facilities,
    OgAllocationRun? Run,
    IReadOnlyList<OgAllocationRun> History)
{
    public bool HasRun => Run is not null;

    public string HistoryText => History.Count == 0
        ? "No allocation run is committed for this field."
        : string.Join(
            " · ",
            History.Select(run =>
                $"{run.PeriodText} {run.Method.ToLowerInvariant()} " +
                $"{run.TotalAllocatedText} ({run.Hash[..Math.Min(12, run.Hash.Length)]})"));

    public string MethodsText => string.Join(", ", Methods);
}

/// <summary>The allocation rules, stated once so the page, the service and the tests agree.</summary>
public static class AllocationRules
{
    public const string MeasuredShare = "MEASURED_SHARE";
    public const string PriorPeriod = "PRIOR_PERIOD";
    public const string Equal = "EQUAL";
    public const string NoMethod = "UNALLOCATED";

    public const decimal DefaultTolerancePercent = 0.5m;

    public const string CodeNoConnection = "allocation-no-connection";
    public const string CodeReadFailed = "allocation-read-failed";
    public const string CodeWriteFailed = "allocation-write-failed";
    public const string CodeActorRequired = "allocation-actor-required";
    public const string CodeMethodInvalid = "allocation-method-invalid";
    public const string CodeToleranceInvalid = "allocation-tolerance-invalid";
    public const string CodePeriodRequired = "allocation-period-required";
    public const string CodeOverrideReasonRequired = "allocation-override-reason-required";
    public const string CodeResidualBeyondTolerance = "allocation-residual-beyond-tolerance";
    public const string CodeNoRows = "allocation-no-rows";

    public static IReadOnlyList<string> Methods { get; } = [MeasuredShare, PriorPeriod, Equal];

    /// <summary>AC-3: a manual override needs a reason, and the reason is recorded with it.</summary>
    public static (string Code, string Message)? Validate(OgAllocationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Period == default)
        {
            return (CodePeriodRequired, "An allocation run needs the period it allocates.");
        }

        if (!Methods.Contains((inputs.Method ?? string.Empty).Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeMethodInvalid,
                $"'{inputs.Method}' is not an allocation method; use {string.Join(", ", Methods)}.");
        }

        if (inputs.TolerancePercent is < 0m or > 100m)
        {
            return (CodeToleranceInvalid, "The tolerance must be a percentage between 0 and 100.");
        }

        var missingReason = (inputs.Overrides ?? [])
            .Where(item => string.IsNullOrWhiteSpace(item.Reason))
            .Select(item => item.WellId)
            .FirstOrDefault();

        return missingReason is null
            ? null
            : (CodeOverrideReasonRequired,
                $"The manual override for '{missingReason}' needs a reason; an override is recorded with why it was made.");
    }

    /// <summary>The residual of a row: allocated minus measured.</summary>
    public static decimal? Residual(decimal? allocated, decimal? measured) =>
        allocated is { } allocatedValue && measured is { } measuredValue
            ? allocatedValue - measuredValue
            : null;

    /// <summary>AC-2: the tolerance is a percentage of the measured total.</summary>
    public static decimal? ToleranceValue(decimal? totalMeasured, decimal tolerancePercent) =>
        totalMeasured is { } measured ? Math.Abs(measured) * tolerancePercent / 100m : null;

    public static bool IsBeyondTolerance(decimal? residual, decimal? totalMeasured, decimal tolerancePercent) =>
        residual is { } value
        && ToleranceValue(totalMeasured, tolerancePercent) is { } tolerance
        && Math.Abs(value) > tolerance;

    /// <summary>A row's factor, or null when there is no basis to share out.</summary>
    public static decimal? Factor(decimal? basis, decimal? totalBasis) =>
        basis is { } value && totalBasis is { } total && total > 0m ? value / total : null;

    /// <summary>
    /// AC-6: a stable hash of the run's inputs - what was measured, how it was shared out and what was overridden -
    /// so two runs can be compared and the same run is never posted twice (AC-4).
    /// </summary>
    public static string Hash(OgAllocationInputs inputs, IReadOnlyList<OgAllocationRow> rows)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(rows);

        var text = new StringBuilder()
            .Append(inputs.FieldId ?? string.Empty).Append('|')
            .Append(inputs.FacilityId ?? string.Empty).Append('|')
            .Append(inputs.Period.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('|')
            .Append((inputs.Method ?? string.Empty).Trim().ToUpperInvariant()).Append('|')
            .Append(inputs.TolerancePercent.ToString("0.####", CultureInfo.InvariantCulture)).Append('|');

        foreach (var row in rows
                     .OrderBy(item => item.WellId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.Method, StringComparer.Ordinal))
        {
            text
                .Append(row.WellId).Append('=')
                .Append(Number(row.Measured)).Append(':')
                .Append(Number(row.Allocated)).Append(':')
                .Append(row.Unit ?? string.Empty).Append(':')
                .Append(row.Method).Append(':')
                .Append(Number(row.Residual)).Append(':')
                .Append(row.OverrideReason ?? string.Empty).Append(';');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()))).ToLowerInvariant();
    }

    private static string Number(decimal? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture) ?? "-";
}

/// <summary>PRD-04: the allocation workbench.</summary>
public interface IOgAllocationWorkbench
{
    /// <summary>
    /// Loads the field's measurements and the committed history, and returns a preview computed in memory (the dry run
    /// persists nothing, AC-5).
    /// </summary>
    Task<OgResult<OgAllocationWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? facilityId = null,
        DateTime? period = null,
        string? method = null,
        decimal tolerancePercent = AllocationRules.DefaultTolerancePercent,
        string? actor = null,
        CancellationToken cancellationToken = default);

    /// <summary>Recomputes the run with the overrides; still nothing is written (AC-5).</summary>
    Task<OgResult<OgAllocationWorkspace>> PreviewAsync(
        OgAllocationInputs inputs,
        string? fieldName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the run. A residual beyond tolerance is refused unless acknowledged (AC-2); repeating the same run
    /// returns the committed one instead of posting it twice (AC-4).
    /// </summary>
    Task<OgResult<OgAllocationWorkspace>> CommitAsync(
        OgAllocationInputs inputs,
        bool acknowledgeResidual,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default);

    /// <summary>The inputs retained when a commit was not acknowledged, or null.</summary>
    OgAllocationInputs? Pending { get; }

    /// <summary>Re-sends the retained commit after applying the acknowledgement.</summary>
    Task<OgResult<OgAllocationWorkspace>> RetryCommitAsync(
        bool acknowledgeResidual,
        string? fieldName = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The workbench over the PPDM <c>WELL</c>, <c>PDEN_WELL</c> and <c>PDEN_VOL_SUMMARY</c> measurements, the
/// ProductionOperations <c>FACILITY_MEASUREMENT</c> total and the ProductionAccounting <c>ALLOCATION_RUN</c> and
/// <c>ALLOCATION_RUN_ROW</c> rows, read and written through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The owner's single entry point for an allocation is <c>IAllocationService.AllocateAsync(RUN_TICKET, method, userId,
/// connectionName)</c>, which allocates a run ticket's volume into <c>ALLOCATION_RESULT</c> and <c>ALLOCATION_DETAIL</c>.
/// This screen keeps its own run pair, <c>ALLOCATION_RUN</c> and <c>ALLOCATION_RUN_ROW</c>, because the accounting result
/// carries no unit, residual, override reason, run state or input hash, and because the accounting entity declares both
/// <c>ALLOCATION_METHOD</c> and the <c>METHOD</c> enum, which the framework's field builder collapses onto one column name
/// (an entity read of it throws "An item with the same key has already been added. Key: METHOD"). The accounting flow
/// converts a committed run when the run is posted. The workbench's measured total is a facility measurement when one is
/// named, otherwise the field's own well measurements.
/// </remarks>
public sealed class OgAllocationWorkbenchService : IOgAllocationWorkbench
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgAllocationWorkbenchService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgAllocationInputs? Pending { get; private set; }

    public async Task<OgResult<OgAllocationWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? facilityId = null,
        DateTime? period = null,
        string? method = null,
        decimal tolerancePercent = AllocationRules.DefaultTolerancePercent,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgAllocationWorkspace>.Failed(
                "No connection is open, so the allocation workbench cannot be read.", AllocationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var inputs = new OgAllocationInputs(
                fieldId,
                string.IsNullOrWhiteSpace(facilityId) ? null : facilityId.Trim(),
                (period ?? DateTime.Today).Date,
                string.IsNullOrWhiteSpace(method) ? AllocationRules.MeasuredShare : method.Trim().ToUpperInvariant(),
                tolerancePercent,
                []);

            var run = Build(model, inputs, fieldId, fieldName);

            return OgResult<OgAllocationWorkspace>.Ok(
                Workspace(model, inputs, run, fieldName));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the allocation workbench",
                "the allocation workbench is not shown",
                FailureSeverity.Error);

            return OgResult<OgAllocationWorkspace>.Failed(
                "The allocation workbench could not be read.", reference ?? AllocationRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgAllocationWorkspace>> PreviewAsync(
        OgAllocationInputs inputs,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (AllocationRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgAllocationWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAllocationWorkspace>.Failed(
                "No connection is open, so the run cannot be previewed.", AllocationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var run = Build(model, inputs, inputs.FieldId, fieldName);

            // AC-5: a preview writes nothing; the run it returns is state DRAFT and has no id.
            return OgResult<OgAllocationWorkspace>.Ok(Workspace(model, inputs, run, fieldName));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "previewing an allocation run",
                "the run is not shown",
                FailureSeverity.Error);

            return OgResult<OgAllocationWorkspace>.Failed(
                "The run could not be previewed.", reference ?? AllocationRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgAllocationWorkspace>> CommitAsync(
        OgAllocationInputs inputs,
        bool acknowledgeResidual,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        var committed = await CommitCoreAsync(inputs, acknowledgeResidual, actor, fieldName, cancellationToken)
            .ConfigureAwait(false);

        if (committed.Success)
        {
            Pending = null;
        }

        return committed;
    }

    public async Task<OgResult<OgAllocationWorkspace>> RetryCommitAsync(
        bool acknowledgeResidual,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgAllocationWorkspace>.Failed(
                "There is no commit waiting to be retried.", "allocation-nothing-pending");
        }

        var retried = await CommitCoreAsync(pending, acknowledgeResidual, pending.FieldId ?? string.Empty, fieldName,
            cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    private async Task<OgResult<OgAllocationWorkspace>> CommitCoreAsync(
        OgAllocationInputs inputs,
        bool acknowledgeResidual,
        string actor,
        string? fieldName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAllocationWorkspace>.Failed(
                "A committed run must record who committed it.", AllocationRules.CodeActorRequired);
        }

        if (AllocationRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgAllocationWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAllocationWorkspace>.Failed(
                "No connection is open, so the run cannot be committed.", AllocationRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var run = Build(model, inputs, inputs.FieldId, fieldName);

            if (run.RowCount == 0)
            {
                return OgResult<OgAllocationWorkspace>.Failed(
                    "The run has no row, so there is nothing to commit.", AllocationRules.CodeNoRows);
            }

            // AC-4: the same run is recognised by its inputs hash and is never posted twice.
            var existing = model.Results.FirstOrDefault(row =>
                string.Equals(row.INPUT_HASH, run.Hash, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.STATE, OgAllocationRun.CommittedState, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                return OgResult<OgAllocationWorkspace>.Ok(
                    Workspace(model, inputs, ToRun(existing, model), fieldName));
            }

            // AC-2: a residual beyond tolerance is refused until it is acknowledged.
            if (run.IsBeyondTolerance && !acknowledgeResidual)
            {
                return OgResult<OgAllocationWorkspace>.Failed(
                    $"The run is not committed: its residual {run.ResidualText} is beyond the tolerance " +
                    $"{run.ToleranceText}. Acknowledge the residual to commit it.",
                    AllocationRules.CodeResidualBeyondTolerance);
            }

            var now = DateTime.UtcNow;
            var runId = Guid.NewGuid().ToString("N");

            var resultUnit = OgUow.For<ALLOCATION_RUN>(source);
            resultUnit.PrimaryKey = "ALLOCATION_RUN_ID";

            resultUnit.Add(new ALLOCATION_RUN
            {
                ALLOCATION_RUN_ID = runId,
                PERIOD_DATE = run.Period,
                METHOD = run.Method,
                FIELD_ID = inputs.FieldId,
                FACILITY_ID = inputs.FacilityId,
                VOLUME_OUOM = run.Unit,
                STATE = OgAllocationRun.CommittedState,
                INPUT_HASH = run.Hash,
                TOLERANCE_PERCENT = run.TolerancePercent,
                ACKNOWLEDGED_IND = run.IsBeyondTolerance && acknowledgeResidual ? "Y" : "N",
                TOTAL_MEASURED = run.TotalMeasured,
                TOTAL_ALLOCATED = run.TotalAllocated,
                RESIDUAL = run.Residual,
                MEASURED_SOURCE = run.MeasuredSource,
                COMMITTED_BY = actor.Trim(),
                COMMITTED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var detailUnit = OgUow.For<ALLOCATION_RUN_ROW>(source);
            detailUnit.PrimaryKey = "ALLOCATION_RUN_ROW_ID";

            foreach (var row in run.Rows)
            {
                detailUnit.Add(new ALLOCATION_RUN_ROW
                {
                    ALLOCATION_RUN_ROW_ID = Guid.NewGuid().ToString("N"),
                    ALLOCATION_RUN_ID = runId,
                    WELL_ID = row.WellId,
                    WELL_NAME = row.WellName,
                    ALLOCATED_VOLUME = row.Allocated,
                    FACTOR = row.Factor,
                    MEASURED_VOLUME = row.Measured,
                    RESIDUAL = row.Residual,
                    VOLUME_OUOM = row.Unit,
                    METHOD = row.Method,
                    STATUS = row.Status,
                    BASIS_TEXT = row.Basis,
                    OVERRIDE_REASON = row.OverrideReason,
                    OVERRIDE_BY = row.OverrideBy,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committedDetails = await detailUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedDetails))
            {
                Pending = inputs;
                return OgResult<OgAllocationWorkspace>.Failed(
                    OgUow.Message(committedDetails, "The run's rows were not written; retry sends the same run."),
                    AllocationRules.CodeWriteFailed);
            }

            var committedResult = await resultUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedResult))
            {
                Pending = inputs;
                return OgResult<OgAllocationWorkspace>.Failed(
                    OgUow.Message(committedResult, "The run was not acknowledged; retry sends the same run."),
                    AllocationRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAllocationWorkspace>.Ok(
                Workspace(after, inputs, ToRun(
                    after.Results.First(row => string.Equals(row.ALLOCATION_RUN_ID, runId, StringComparison.OrdinalIgnoreCase)),
                    after),
                    fieldName));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = inputs;

            var reference = _failures.ReportHandled(
                ex,
                $"committing an allocation run for {inputs.FieldId}",
                "the run was not committed; retry sends the same run",
                FailureSeverity.Error);

            return OgResult<OgAllocationWorkspace>.Failed(
                "The run could not be committed; retry sends the same run.", reference ?? AllocationRules.CodeWriteFailed);
        }
    }

    // ---------------------------------------------------------------- the run

    private static OgAllocationRun Build(
        WorkbenchModel model,
        OgAllocationInputs inputs,
        string? fieldId,
        string? fieldName)
    {
        var period = inputs.Period.Date;
        var method = (inputs.Method ?? AllocationRules.MeasuredShare).Trim().ToUpperInvariant();

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

        var measured = MeasuredByWell(model, entityToWell, period, out var unit);

        // The measured total: a named facility's own measurement, otherwise the field's wells.
        var facilityRows = inputs.FacilityId is { } facilityId
            ? model.FacilityMeasurements
                .Where(row => string.Equals(row.FACILITY_ID, facilityId, StringComparison.OrdinalIgnoreCase))
                .Where(row => row.MEASURED_DATE?.Date == period)
                .ToList()
            : [];

        var facilityUnit = OperationsCentreRules.DominantUnit(facilityRows.Select(row => row.MEASURED_UOM));

        decimal? totalMeasured;
        string? runUnit;
        string measuredSource;

        if (facilityUnit is not null)
        {
            totalMeasured = facilityRows
                .Where(row => string.Equals((row.MEASURED_UOM ?? string.Empty).Trim(), facilityUnit,
                    StringComparison.OrdinalIgnoreCase))
                .Sum(row => row.MEASURED_VALUE);

            runUnit = facilityUnit;
            measuredSource = $"the measurement of facility {inputs.FacilityId} on {period:yyyy-MM-dd}";
        }
        else
        {
            runUnit = unit;
            totalMeasured = measured.Count == 0 ? null : measured.Values.Sum(value => value ?? 0m);
            measuredSource = inputs.FacilityId is null
                ? "the field's own well measurements"
                : $"the field's own well measurements (facility {inputs.FacilityId} has no measurement on " +
                  $"{period:yyyy-MM-dd})";
        }

        var bases = Baselines(model, entityToWell, period, method, measured);

        var totalBasis = bases.Values.Where(value => value is > 0m).Sum(value => value ?? 0m);

        var overrides = (inputs.Overrides ?? [])
            .GroupBy(item => item.WellId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

        var rows = new List<OgAllocationRow>();

        foreach (var well in wells)
        {
            measured.TryGetValue(well.UWI!, out var wellMeasured);
            bases.TryGetValue(well.UWI!, out var basis);

            var factor = AllocationRules.Factor(basis, totalBasis);
            var allocated = factor is { } shareFactor && totalMeasured is { } total
                ? Math.Round(shareFactor * total, 3, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            var rowMethod = factor is null ? AllocationRules.NoMethod : method;
            var status = factor is null ? OgAllocationRow.UnallocatedStatus : OgAllocationRow.AllocatedStatus;
            var basisText = BasisText(method, basis, wellMeasured, runUnit);
            decimal? final = allocated;
            string? overrideReason = null;
            string? overrideBy = null;

            if (overrides.TryGetValue(well.UWI!, out var manual) && factor is not null)
            {
                final = manual.Allocated;
                status = OgAllocationRow.OverriddenStatus;
                overrideReason = manual.Reason?.Trim();
                overrideBy = manual.Reason?.Trim() is null ? null : "the operator";
                basisText = $"manual override: {overrideReason ?? "(no reason)"}";
            }

            rows.Add(new OgAllocationRow(
                well.UWI!,
                Blank(well.WELL_NAME),
                wellMeasured,
                factor is null ? null : final,
                runUnit,
                factor,
                factor is null ? null : AllocationRules.Residual(final, wellMeasured),
                rowMethod,
                status,
                basisText,
                overrideReason,
                overrideBy));
        }

        var totalAllocated = rows.Count == 0
            ? (decimal?)null
            : rows.Sum(row => row.Allocated ?? 0m);

        var residual = AllocationRules.Residual(totalAllocated, totalMeasured);

        var hashInputs = inputs with { FieldId = fieldId };

        return new OgAllocationRun(
            string.Empty,
            fieldId,
            inputs.FacilityId,
            period,
            method,
            rows,
            totalMeasured,
            totalAllocated,
            residual,
            runUnit,
            AllocationRules.Hash(hashInputs, rows),
            OgAllocationRun.DraftState,
            inputs.TolerancePercent,
            false,
            null,
            null,
            measuredSource);
    }

    private static Dictionary<string, decimal?> MeasuredByWell(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime period,
        out string? unit)
    {
        var rows = model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE?.Date == period)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

        var dominant = OperationsCentreRules.DominantUnit(rows.Select(row => row.OIL_VOLUME_OUOM));
        unit = dominant;

        var result = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in rows.GroupBy(row => entityToWell[row.PDEN_ID!], StringComparer.OrdinalIgnoreCase))
        {
            var matching = group
                .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), dominant ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            result[group.Key] = dominant is null || matching.Count == 0
                ? null
                : matching.Sum(row => row.OIL_VOLUME);
        }

        return result;
    }

    /// <summary>The basis each well is shared out on, per the method; a well with none is unallocated.</summary>
    private static Dictionary<string, decimal?> Baselines(
        WorkbenchModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime period,
        string method,
        IReadOnlyDictionary<string, decimal?> measured)
    {
        var result = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

        if (string.Equals(method, AllocationRules.Equal, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var (well, value) in measured)
            {
                result[well] = value is > 0m ? 1m : null;
            }

            return result;
        }

        if (string.Equals(method, AllocationRules.MeasuredShare, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var (well, value) in measured)
            {
                result[well] = value is > 0m ? value : null;
            }

            return result;
        }

        // PRIOR_PERIOD: the well's mean over the seven recorded days before the period, falling back to its measurement.
        var previous = model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE is { } date
                          && date.Date < period
                          && date.Date >= period.AddDays(-7))
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

        foreach (var group in previous.GroupBy(row => entityToWell[row.PDEN_ID!], StringComparer.OrdinalIgnoreCase))
        {
            var daily = group
                .GroupBy(row => row.VOLUME_DATE!.Value.Date)
                .Select(day => day.Sum(row => row.OIL_VOLUME))
                .ToList();

            result[group.Key] = daily.Count == 0 ? null : Math.Round(daily.Average(), 3, MidpointRounding.AwayFromZero);
        }

        foreach (var (well, value) in measured)
        {
            if (!result.ContainsKey(well) || result[well] is not > 0m)
            {
                result[well] = value is > 0m ? value : null;
            }
        }

        return result;
    }

    private static string BasisText(string method, decimal? basis, decimal? measured, string? unit)
    {
        if (basis is not > 0m)
        {
            return "no measured volume and no prior period to share out, so the row is unallocated";
        }

        return method switch
        {
            AllocationRules.Equal => "equal share of the measured total",
            AllocationRules.PriorPeriod =>
                $"prior period: the mean of the last seven recorded days " +
                $"({OgProductionDay.Format(basis, unit)})",
            _ => $"measured share: the well measured {OgProductionDay.Format(basis, unit)}" +
                 (measured is null ? " (no measurement on the period, so the fallback is used)" : string.Empty),
        };
    }

    // ----------------------------------------------------------------- reads

    private static async Task<WorkbenchModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);
        var facilityMeasurements = await OgUow.ReadAllAsync<FACILITY_MEASUREMENT>(source).ConfigureAwait(false);
        var results = await OgUow.ReadAllAsync<ALLOCATION_RUN>(source).ConfigureAwait(false);
        var details = await OgUow.ReadAllAsync<ALLOCATION_RUN_ROW>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new WorkbenchModel(wells, pdenWells, volumes, facilityMeasurements, results, details);
    }

    private static OgAllocationWorkspace Workspace(
        WorkbenchModel model,
        OgAllocationInputs inputs,
        OgAllocationRun run,
        string? fieldName)
    {
        var history = model.Results
            .Where(row => string.Equals(row.STATE, OgAllocationRun.CommittedState, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(inputs.FieldId)
                          || string.Equals(row.FIELD_ID, inputs.FieldId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.PERIOD_DATE)
            .ThenByDescending(row => row.COMMITTED_AT)
            .Take(20)
            .Select(row => ToRun(row, model))
            .ToList();

        var facilities = model.FacilityMeasurements
            .Where(row => !string.IsNullOrWhiteSpace(row.FACILITY_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.FACILITY_ID!, StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgAllocationFacility(
                group.Key,
                $"{group.Key} · {group.Count().ToString(CultureInfo.InvariantCulture)} measurement(s)"))
            .OrderBy(facility => facility.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new OgAllocationWorkspace(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            inputs.FieldId,
            inputs.Period,
            run.Method,
            inputs.TolerancePercent,
            AllocationRules.Methods,
            facilities,
            run,
            history);
    }

    private static OgAllocationRun ToRun(ALLOCATION_RUN row, WorkbenchModel model)
    {
        var rows = model.Details
            .Where(detail => string.Equals(
                detail.ALLOCATION_RUN_ID, row.ALLOCATION_RUN_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(detail => detail.WELL_NAME ?? detail.WELL_ID, StringComparer.OrdinalIgnoreCase)
            .Select(detail => new OgAllocationRow(
                detail.WELL_ID ?? string.Empty,
                Blank(detail.WELL_NAME),
                detail.MEASURED_VOLUME,
                detail.ALLOCATED_VOLUME,
                Blank(detail.VOLUME_OUOM),
                detail.FACTOR,
                detail.RESIDUAL,
                Blank(detail.METHOD) ?? AllocationRules.NoMethod,
                Blank(detail.STATUS) ?? OgAllocationRow.UnallocatedStatus,
                Blank(detail.BASIS_TEXT) ?? string.Empty,
                Blank(detail.OVERRIDE_REASON),
                Blank(detail.OVERRIDE_BY)))
            .ToList();

        return new OgAllocationRun(
            row.ALLOCATION_RUN_ID ?? string.Empty,
            Blank(row.FIELD_ID),
            Blank(row.FACILITY_ID),
            row.PERIOD_DATE.Date,
            Blank(row.METHOD) ?? string.Empty,
            rows,
            row.TOTAL_MEASURED,
            row.TOTAL_ALLOCATED,
            row.RESIDUAL,
            Blank(row.VOLUME_OUOM),
            Blank(row.INPUT_HASH) ?? string.Empty,
            Blank(row.STATE) ?? OgAllocationRun.CommittedState,
            row.TOLERANCE_PERCENT,
            string.Equals(row.ACKNOWLEDGED_IND, "Y", StringComparison.OrdinalIgnoreCase),
            Blank(row.COMMITTED_BY),
            row.COMMITTED_AT,
            Blank(row.MEASURED_SOURCE) ?? "the committed run's own rows");
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record WorkbenchModel(
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<FACILITY_MEASUREMENT> FacilityMeasurements,
        IReadOnlyList<ALLOCATION_RUN> Results,
        IReadOnlyList<ALLOCATION_RUN_ROW> Details);
}
