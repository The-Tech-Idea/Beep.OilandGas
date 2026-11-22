using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Work;

/// <summary>One maintenance plan with its schedule and its derived compliance state (WRK-03).</summary>
public sealed record OgMaintenancePlan(
    string Id,
    string AssetId,
    string Kind,
    string Status,
    int? Frequency,
    string Unit,
    DateTime? LastDone,
    DateTime? NextDue,
    string Compliance,
    string? Tasks,
    string? EditedBy,
    DateTime? EditedDate)
{
    public bool IsScheduled => Frequency is > 0 && !string.IsNullOrWhiteSpace(Unit);

    public bool IsDue => Compliance is WorkMaintenanceRules.Due or WorkMaintenanceRules.Overdue;

    /// <summary>
    /// Days past the due date, counted against the application clock. It used the machine's local date
    /// (<see cref="DateTime.Today"/>) while the rest of the product records and compares UTC, so a workstation east of
    /// Greenwich reported a plan as a day overdue for the first hours after local midnight - and a test with a UTC
    /// fixture failed at exactly that rollover.
    /// </summary>
    public int DaysOverdue => NextDue is { } due && due.Date < DateTime.UtcNow.Date
        ? (DateTime.UtcNow.Date - due.Date).Days
        : 0;

    public string FrequencyText => IsScheduled
        ? Frequency!.Value.ToString(CultureInfo.InvariantCulture) + " " + Unit.ToLowerInvariant() +
          (Frequency == 1 ? string.Empty : "s")
        : "not scheduled";

    public string NextDueText => NextDue is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
          (Compliance == WorkMaintenanceRules.Overdue
              ? " (" + DaysOverdue.ToString(CultureInfo.InvariantCulture) + " day(s) overdue)"
              : string.Empty)
        : "no next due date";

    public string Describe() =>
        AssetId + " · " + Kind + " · " + FrequencyText + " · last done " +
        (LastDone?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(never)") +
        " · next due " + NextDueText + " · " + Compliance;
}

/// <summary>One generated service job (WRK-03, AC-3).</summary>
public sealed record OgServiceJob(
    string Id,
    string AssetId,
    string JobType,
    string Status,
    string? PlanId,
    DateTime? GeneratedOn,
    DateTime? PlannedStart)
{
    public string Describe() =>
        AssetId + " · " + JobType + " · " + Status.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(PlanId) ? " · raised by hand" : " · from plan " + PlanId) +
        (GeneratedOn is { } generated
            ? " · generated " + generated.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : string.Empty);
}

/// <summary>One inspection with its result and its findings (WRK-03, AC-2).</summary>
public sealed record OgInspectionRow(
    string Id,
    string WorkOrderId,
    string Kind,
    string Result,
    string? Findings,
    string? Inspector,
    DateTime? InspectedAt,
    DateTime? DueDate,
    bool HasFindings,
    bool HasLinkedAction,
    bool IsClosed)
{
    /// <summary>AC-2: an inspection with findings cannot close without a linked corrective action.</summary>
    public bool IsBlocked => HasFindings && !HasLinkedAction && !IsClosed;

    public string StateText => IsClosed
        ? "closed"
        : IsBlocked
            ? "open - findings need an action"
            : HasFindings
                ? "open - action linked"
                : "open";

    public string Describe() =>
        Kind + " · " + Result.ToLowerInvariant() + " · " + StateText +
        (DueDate is { } due ? " · due " + due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty) +
        (string.IsNullOrWhiteSpace(Inspector) ? string.Empty : " · " + Inspector);
}

/// <summary>The maintenance workspace (WRK-03).</summary>
public sealed record OgMaintenanceWorkspace(
    string? AssetFilter,
    IReadOnlyList<OgMaintenancePlan> Plans,
    IReadOnlyList<OgServiceJob> Jobs,
    IReadOnlyList<OgInspectionRow> Inspections,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int DueCount => Plans.Count(plan => plan.Compliance == WorkMaintenanceRules.Due);

    public int OverdueCount => Plans.Count(plan => plan.Compliance == WorkMaintenanceRules.Overdue);

    public int Unscheduled => Plans.Count(plan => !plan.IsScheduled);

    public int BlockedInspections => Inspections.Count(inspection => inspection.IsBlocked);

    public string CountsText =>
        Plans.Count.ToString(CultureInfo.InvariantCulture) + " plan(s) · " +
        DueCount.ToString(CultureInfo.InvariantCulture) + " due · " +
        OverdueCount.ToString(CultureInfo.InvariantCulture) + " overdue · " +
        Unscheduled.ToString(CultureInfo.InvariantCulture) + " not scheduled · " +
        Jobs.Count.ToString(CultureInfo.InvariantCulture) + " job(s) · " +
        Inspections.Count.ToString(CultureInfo.InvariantCulture) + " inspection(s) · " +
        BlockedInspections.ToString(CultureInfo.InvariantCulture) + " blocked by findings";

    public string PlansText => Plans.Count == 0
        ? "No maintenance plan is recorded for this asset."
        : string.Join(Environment.NewLine, Plans.Select(plan => plan.Describe()));

    public string JobsText => Jobs.Count == 0
        ? "No service job is recorded."
        : string.Join(Environment.NewLine, Jobs.Select(job => job.Describe()));

    public string InspectionsText => Inspections.Count == 0
        ? "No inspection is recorded."
        : string.Join(Environment.NewLine, Inspections.Select(row => row.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The maintenance rules WRK-03 states once.</summary>
public static class WorkMaintenanceRules
{
    public const string CodeNoConnection = "maintenance-no-connection";
    public const string CodeReadFailed = "maintenance-read-failed";
    public const string CodeWriteFailed = "maintenance-write-failed";
    public const string CodeActorRequired = "maintenance-actor-required";
    public const string CodeUnknownPlan = "maintenance-unknown-plan";
    public const string CodeFrequencyUnitRequired = "maintenance-frequency-unit-required";
    public const string CodeNotDue = "maintenance-not-due";
    public const string CodeUnknownInspection = "maintenance-unknown-inspection";
    public const string CodeFindingsNeedAction = "maintenance-findings-need-action";

    public const string Current = "current";
    public const string Due = "due";
    public const string Overdue = "overdue";
    public const string NotScheduled = "not scheduled";

    /// <summary>The units a frequency may be expressed in (AC-5).</summary>
    public static IReadOnlyList<string> Units { get; } = ["DAY", "WEEK", "MONTH", "YEAR"];

    /// <summary>
    /// AC-1: the next due date is last done plus the frequency in its unit, and it is computed - never stored (AC-4).
    /// </summary>
    public static DateTime? NextDue(DateTime? lastDone, int? frequency, string? unit)
    {
        if (lastDone is not { } last || frequency is not > 0 || string.IsNullOrWhiteSpace(unit))
        {
            return null;
        }

        return unit.Trim().ToUpperInvariant() switch
        {
            "DAY" => last.AddDays(frequency.Value),
            "WEEK" => last.AddDays(7 * frequency.Value),
            "MONTH" => last.AddMonths(frequency.Value),
            "YEAR" => last.AddYears(frequency.Value),
            _ => null,
        };
    }

    /// <summary>AC-4: the compliance state is derived from the computed next due date and today.</summary>
    public static string Compliance(DateTime? nextDue, bool isScheduled, DateTime asOf)
    {
        if (!isScheduled || nextDue is not { } due)
        {
            return NotScheduled;
        }

        if (due.Date < asOf.Date)
        {
            return Overdue;
        }

        return due.Date == asOf.Date ? Due : Current;
    }

    /// <summary>AC-5: a frequency always carries its unit, and a unit never stands without a frequency.</summary>
    public static (string Code, string Message)? ValidateFrequency(int? frequency, string? unit)
    {
        if (frequency is null or <= 0 && string.IsNullOrWhiteSpace(unit))
        {
            return null;
        }

        if (frequency is not > 0)
        {
            return (CodeFrequencyUnitRequired,
                "A frequency unit ('" + unit + "') needs a positive frequency; one without the other schedules nothing.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            return (CodeFrequencyUnitRequired,
                "A frequency of " + frequency + " needs its unit; one of " + string.Join(", ", Units) + " is required.");
        }

        return Units.Contains(unit.Trim().ToUpperInvariant(), StringComparer.Ordinal)
            ? null
            : (CodeFrequencyUnitRequired,
                "'" + unit + "' is not a frequency unit; use " + string.Join(", ", Units) + ".");
    }

    /// <summary>
    /// AC-2: an inspection with findings cannot close unless a corrective action is linked to its work order.
    /// </summary>
    public static (string Code, string Message)? CheckClosure(
        bool hasFindings,
        bool hasLinkedAction,
        string resultStatus)
    {
        if (!hasFindings)
        {
            return null;
        }

        return hasLinkedAction
            ? null
            : (CodeFindingsNeedAction,
                "The inspection records findings, so it cannot close as '" + resultStatus +
                "' until a corrective action is linked to its work order.");
    }

    /// <summary>AC-3: a job may be generated only when the plan is due or overdue.</summary>
    public static (string Code, string Message)? CheckGeneratable(OgMaintenancePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.IsDue
            ? null
            : (CodeNotDue,
                plan.AssetId + "'s plan is " + plan.Compliance +
                (plan.NextDue is { } due
                    ? " (next due " + due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")"
                    : string.Empty) +
                ", so no job is generated for it.");
    }

    /// <summary>The inspection kinds the owner records, from its own condition vocabulary.</summary>
    public static IReadOnlyList<string> InspectionKinds { get; } =
    [
        ConditionType.PreStart,
        ConditionType.Operational,
        ConditionType.CloseOut,
        ConditionType.Regulatory,
        ConditionType.SceVerify,
    ];
}

/// <summary>WRK-03: maintenance plans, service jobs and inspections.</summary>
public interface IOgMaintenance
{
    Task<OgResult<OgMaintenanceWorkspace>> LoadAsync(
        string? assetFilter,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Records the plan's schedule: the frequency and its unit, the last done date and the tasks.</summary>
    Task<OgResult<OgMaintenanceWorkspace>> EditPlanAsync(
        string planId,
        int? frequency,
        string? unit,
        DateTime? lastDone,
        string? tasks,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Generates a service job from a due plan, recording the plan and the generation date (AC-3).</summary>
    Task<OgResult<OgMaintenanceWorkspace>> GenerateJobAsync(
        string planId,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Records an inspection's result, refused on findings without a linked action (AC-2).</summary>
    Task<OgResult<OgMaintenanceWorkspace>> RecordInspectionAsync(
        string inspectionId,
        string resultStatus,
        string notes,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WRK-03 over the owner's <c>WELL_MAINTENANCE_PLAN</c> (the plan record, with its schedule columns), <c>WELL_SERVICE_JOB</c>
/// (with its generation columns), <c>WORK_ORDER_INSPECTION</c>, <c>WORK_ORDER</c> and the corrective-action projects linked to a
/// work order.
/// </summary>
public sealed class OgMaintenanceService : IOgMaintenance
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgMaintenanceService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgMaintenanceWorkspace>> LoadAsync(
        string? assetFilter,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "No connection is open, so the maintenance workspace cannot be read.",
                WorkMaintenanceRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the maintenance workspace", "the maintenance workspace is not shown",
                FailureSeverity.Error);

            return OgResult<OgMaintenanceWorkspace>.Failed(
                "The maintenance workspace could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkMaintenanceRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgMaintenanceWorkspace>> EditPlanAsync(
        string planId,
        int? frequency,
        string? unit,
        DateTime? lastDone,
        string? tasks,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "A plan edit must record who made it.", WorkMaintenanceRules.CodeActorRequired);
        }

        if (WorkMaintenanceRules.ValidateFrequency(frequency, unit) is { } refusal)
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "No connection is open, so the plan cannot be edited.", WorkMaintenanceRules.CodeNoConnection);
        }

        try
        {
            var unitOfWork = OgUow.For<WELL_MAINTENANCE_PLAN>(source);
            unitOfWork.PrimaryKey = nameof(WELL_MAINTENANCE_PLAN.MAINT_PLAN_ID);

            var rows = await OgUow.ReadAsync(
                unitOfWork,
                [OgUow.Equal(nameof(WELL_MAINTENANCE_PLAN.MAINT_PLAN_ID), planId)],
                cancellationToken).ConfigureAwait(false);

            var plan = rows.FirstOrDefault();

            if (plan is null)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    "Maintenance plan '" + planId + "' is not recorded.", WorkMaintenanceRules.CodeUnknownPlan);
            }

            var now = DateTime.UtcNow;

            plan.FREQUENCY = frequency;
            plan.FREQUENCY_UNIT = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim().ToUpperInvariant();
            plan.LAST_DONE_DATE = lastDone?.Date;
            plan.TASK_LIST = string.IsNullOrWhiteSpace(tasks) ? null : tasks.Trim();
            plan.SCHEDULE_EDITED_BY = actor.Trim();
            plan.SCHEDULE_EDITED_DATE = now;
            plan.ROW_CHANGED_BY = actor.Trim();
            plan.ROW_CHANGED_DATE = now;

            var updated = unitOfWork.Update(plan);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    OgUow.Message(updated, "The plan was not saved."), WorkMaintenanceRules.CodeWriteFailed);
            }

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    OgUow.Message(committed, "The plan was not saved."), WorkMaintenanceRules.CodeWriteFailed);
            }

            return await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "editing maintenance plan " + planId, "the plan was not saved", FailureSeverity.Error);

            return OgResult<OgMaintenanceWorkspace>.Failed(
                "The plan could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkMaintenanceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgMaintenanceWorkspace>> GenerateJobAsync(
        string planId,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "A generated job must record who generated it.", WorkMaintenanceRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "No connection is open, so no job can be generated.", WorkMaintenanceRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var plan = read.Value.Plans.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, planId, StringComparison.OrdinalIgnoreCase));

            if (plan is null)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    "Maintenance plan '" + planId + "' is not recorded.", WorkMaintenanceRules.CodeUnknownPlan);
            }

            if (WorkMaintenanceRules.CheckGeneratable(plan) is { } refusal)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var now = DateTime.UtcNow;

            var unitOfWork = OgUow.For<WELL_SERVICE_JOB>(source);
            unitOfWork.PrimaryKey = nameof(WELL_SERVICE_JOB.JOB_ID);

            unitOfWork.Add(new WELL_SERVICE_JOB
            {
                JOB_ID = Guid.NewGuid().ToString("N"),
                FDP_ID = string.Empty,
                UWI = plan.AssetId,
                JOB_TYPE = plan.Kind,
                JOB_STATUS = "PLANNED",
                PRIORITY = plan.Status,
                SERVICE_BA_ID = string.Empty,
                BA_SERVICE_TYPE = string.Empty,
                PLANNED_START_DATE = plan.NextDue,
                PLANNED_END_DATE = plan.NextDue,
                LINKED_ACTIVITY_ID = string.Empty,
                // AC-3: the job names the plan it came from and the date it was generated.
                MAINT_PLAN_ID = plan.Id,
                GENERATED_ON = now.Date,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    OgUow.Message(committed, "The job was not saved."), WorkMaintenanceRules.CodeWriteFailed);
            }

            return await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "generating a job from plan " + planId, "no job was generated", FailureSeverity.Error);

            return OgResult<OgMaintenanceWorkspace>.Failed(
                "The job could not be generated" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkMaintenanceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgMaintenanceWorkspace>> RecordInspectionAsync(
        string inspectionId,
        string resultStatus,
        string notes,
        string assetFilter,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "An inspection result must record who recorded it.", WorkMaintenanceRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMaintenanceWorkspace>.Failed(
                "No connection is open, so the inspection cannot be recorded.",
                WorkMaintenanceRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var row = read.Value.Inspections.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, inspectionId, StringComparison.OrdinalIgnoreCase));

            if (row is null)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    "Inspection '" + inspectionId + "' is not recorded.",
                    WorkMaintenanceRules.CodeUnknownInspection);
            }

            // AC-2: findings cannot close without a linked action.
            var hasFindings = !string.IsNullOrWhiteSpace(notes) ||
                              string.Equals(resultStatus, "FAIL", StringComparison.OrdinalIgnoreCase);

            if (WorkMaintenanceRules.CheckClosure(hasFindings, row.HasLinkedAction, resultStatus) is { } refused)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(refused.Message, refused.Code);
            }

            var unitOfWork = OgUow.For<WORK_ORDER_INSPECTION>(source);
            unitOfWork.PrimaryKey = nameof(WORK_ORDER_INSPECTION.INSPECTION_ID);

            var rows = await OgUow.ReadAsync(
                unitOfWork,
                [OgUow.Equal(nameof(WORK_ORDER_INSPECTION.INSPECTION_ID), inspectionId)],
                cancellationToken).ConfigureAwait(false);

            var inspection = rows.FirstOrDefault();

            if (inspection is null)
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    "Inspection '" + inspectionId + "' is not recorded.",
                    WorkMaintenanceRules.CodeUnknownInspection);
            }

            var now = DateTime.UtcNow;

            inspection.RESULT_STATUS = string.IsNullOrWhiteSpace(resultStatus)
                ? inspection.RESULT_STATUS
                : resultStatus.Trim().ToUpperInvariant();
            inspection.RESULT_NOTES = notes?.Trim() ?? string.Empty;
            inspection.INSPECTOR = actor.Trim();
            inspection.INSPECTED_AT_UTC = now;
            inspection.ROW_CHANGED_BY = actor.Trim();
            inspection.ROW_CHANGED_DATE = now;

            var updated = unitOfWork.Update(inspection);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    OgUow.Message(updated, "The inspection result was not saved."),
                    WorkMaintenanceRules.CodeWriteFailed);
            }

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgMaintenanceWorkspace>.Failed(
                    OgUow.Message(committed, "The inspection result was not saved."),
                    WorkMaintenanceRules.CodeWriteFailed);
            }

            return await ReadAsync(source, assetFilter, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording inspection " + inspectionId, "the inspection result was not saved",
                FailureSeverity.Error);

            return OgResult<OgMaintenanceWorkspace>.Failed(
                "The inspection result could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkMaintenanceRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgMaintenanceWorkspace>> ReadAsync(
        IDataSource source,
        string? assetFilter,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var plans = await OgUow.ReadAllAsync<WELL_MAINTENANCE_PLAN>(source).ConfigureAwait(false);
        var jobs = await OgUow.ReadAllAsync<WELL_SERVICE_JOB>(source).ConfigureAwait(false);
        var inspections = await OgUow.ReadAllAsync<WORK_ORDER_INSPECTION>(source).ConfigureAwait(false);
        var headers = await OgUow.ReadAllAsync<WORK_ORDER>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);
        var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var corrective = projects
            .Where(row => string.Equals(row.PROJECT_TYPE?.Trim(), "CORRECTIVE_ACTION", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PROJECT_ID)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // A work order has a linked action when one of its components points at a corrective-action project.
        var workOrdersWithAction = components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.PROJECT_ID) && corrective.Contains(row.PROJECT_ID!))
            .Select(row => row.WORK_ORDER_ID)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var planRows = plans
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(assetFilter) ||
                          string.Equals(row.UWI?.Trim(), assetFilter.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row =>
            {
                var nextDue = WorkMaintenanceRules.NextDue(row.LAST_DONE_DATE, row.FREQUENCY, row.FREQUENCY_UNIT);
                var scheduled = row.FREQUENCY is > 0 && !string.IsNullOrWhiteSpace(row.FREQUENCY_UNIT);

                return new OgMaintenancePlan(
                    row.MAINT_PLAN_ID,
                    row.UWI ?? string.Empty,
                    row.MAINTENANCE_TYPE ?? string.Empty,
                    row.MAINTENANCE_STATUS ?? string.Empty,
                    row.FREQUENCY,
                    row.FREQUENCY_UNIT ?? string.Empty,
                    row.LAST_DONE_DATE,
                    nextDue,
                    WorkMaintenanceRules.Compliance(nextDue, scheduled, asOf),
                    row.TASK_LIST,
                    row.SCHEDULE_EDITED_BY,
                    row.SCHEDULE_EDITED_DATE);
            })
            .OrderBy(plan => plan.NextDue ?? DateTime.MaxValue)
            .ThenBy(plan => plan.Id, StringComparer.Ordinal)
            .ToList();

        var jobRows = jobs
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(assetFilter) ||
                          string.Equals(row.UWI?.Trim(), assetFilter.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row => new OgServiceJob(
                row.JOB_ID,
                row.UWI ?? string.Empty,
                row.JOB_TYPE ?? string.Empty,
                row.JOB_STATUS ?? string.Empty,
                row.MAINT_PLAN_ID,
                row.GENERATED_ON,
                row.PLANNED_START_DATE))
            .OrderByDescending(job => job.GeneratedOn ?? DateTime.MinValue)
            .ToList();

        var inspectionRows = inspections
            .Where(row => string.IsNullOrWhiteSpace(assetFilter) ||
                          headers.Any(header => string.Equals(header.WORK_ORDER_ID, row.WORK_ORDER_ID,
                              StringComparison.OrdinalIgnoreCase) &&
                              components.Any(component =>
                                  string.Equals(component.WORK_ORDER_ID, header.WORK_ORDER_ID,
                                      StringComparison.OrdinalIgnoreCase) &&
                                  string.Equals(component.FIELD_ID?.Trim(), assetFilter.Trim(),
                                      StringComparison.OrdinalIgnoreCase))))
            .Select(row =>
            {
                var header = headers.FirstOrDefault(candidate =>
                    string.Equals(candidate.WORK_ORDER_ID, row.WORK_ORDER_ID, StringComparison.OrdinalIgnoreCase));

                var findings = string.IsNullOrWhiteSpace(row.RESULT_NOTES) ? null : row.RESULT_NOTES;
                var closed = string.Equals(row.RESULT_STATUS, "PASS", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(row.RESULT_STATUS, "CLOSED", StringComparison.OrdinalIgnoreCase);

                return new OgInspectionRow(
                    row.INSPECTION_ID,
                    row.WORK_ORDER_ID,
                    row.COND_TYPE ?? string.Empty,
                    string.IsNullOrWhiteSpace(row.RESULT_STATUS) ? "PENDING" : row.RESULT_STATUS!,
                    findings,
                    row.INSPECTOR,
                    row.INSPECTED_AT_UTC,
                    header?.DUE_DATE,
                    !string.IsNullOrWhiteSpace(findings) ||
                    string.Equals(row.RESULT_STATUS, "FAIL", StringComparison.OrdinalIgnoreCase),
                    workOrdersWithAction.Contains(row.WORK_ORDER_ID),
                    closed);
            })
            .OrderBy(row => row.DueDate ?? DateTime.MaxValue)
            .ToList();

        return OgResult<OgMaintenanceWorkspace>.Ok(new OgMaintenanceWorkspace(
            string.IsNullOrWhiteSpace(assetFilter) ? null : assetFilter.Trim(),
            planRows,
            jobRows,
            inspectionRows,
            asOf,
            DateTime.UtcNow));
    }
}
