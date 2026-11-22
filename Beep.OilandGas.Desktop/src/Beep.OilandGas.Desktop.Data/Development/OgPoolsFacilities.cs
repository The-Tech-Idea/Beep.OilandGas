using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One pool of the development plan, with its numbers and its facility routing (DEV-04).</summary>
public sealed record OgPoolPlan(
    string Id,
    string PlanId,
    string FieldId,
    int PlanVersion,
    string? PoolId,
    string Name,
    string? Reservoir,
    string? DriveMechanism,
    decimal? Ooip,
    string? OoipUnit,
    decimal RecoveryFactor,
    int PlannedWells,
    string? FacilityId)
{
    /// <summary>AC-1: a volume without its unit is not a volume.</summary>
    public bool HasOoipUnit => !string.IsNullOrWhiteSpace(OoipUnit);

    /// <summary>OOIP x recovery factor, or null when either is missing; the unit travels with it.</summary>
    public decimal? Recoverable =>
        Ooip is { } ooip && ooip > 0m ? ooip * RecoveryFactor : null;

    public string OoipText => Ooip is { } ooip
        ? $"{ooip.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (HasOoipUnit ? OoipUnit!.Trim() : "(unit not recorded)")
        : "no OOIP recorded";

    public string RecoveryText => $"{(RecoveryFactor * 100m).ToString("0.##", CultureInfo.InvariantCulture)}%";

    public string RecoverableText => Recoverable is { } volume
        ? $"{volume.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (HasOoipUnit ? OoipUnit!.Trim() : "(unit not recorded)")
        : "not computable";

    public string Describe() =>
        $"{Name} · {Reservoir ?? "(no reservoir)"} · {DriveMechanism ?? "(no drive mechanism)"} · " +
        $"OOIP {OoipText} · RF {RecoveryText} · recoverable {RecoverableText} · " +
        $"{PlannedWells.ToString(CultureInfo.InvariantCulture)} well(s) · " +
        $"to {FacilityId ?? "(no facility)"}";
}

/// <summary>One facility of the development plan (DEV-04).</summary>
public sealed record OgFacilityPlan(
    string Id,
    string PlanId,
    int PlanVersion,
    string? FacilityId,
    string Name,
    string? Kind,
    string? Phase,
    decimal? Capacity,
    string? CapacityUnit,
    decimal? Investment,
    string? Currency,
    DateTime? StartDate,
    DateTime? CompletionDate,
    string State,
    bool ExistsInPpdm)
{
    public const string Planned = "PLANNED";
    public const string InConstruction = "IN_CONSTRUCTION";
    public const string Operating = "OPERATING";
    public const string Abandoned = "ABANDONED";

    /// <summary>AC-3: only a facility that exists and operates belongs on an operational page.</summary>
    public bool IsOperating => string.Equals(State, Operating, StringComparison.OrdinalIgnoreCase);

    public bool IsPlanned => string.Equals(State, Planned, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-3: a planned or abandoned facility is excluded from the operational pages (PRD-02).</summary>
    public bool ExcludedFromOperationalPages => !IsOperating;

    public string StateText => State.ToLowerInvariant();

    public string CapacityText => Capacity is { } capacity
        ? $"{capacity.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(CapacityUnit) ? "(unit not recorded)" : CapacityUnit!.Trim())
        : "no capacity recorded";

    public string InvestmentText => Investment is { } investment
        ? $"{investment.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no investment recorded";

    public string InServiceText =>
        CompletionDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        ?? StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        ?? "(no date)";

    public string Describe() =>
        $"{Name} · {Kind ?? "(no type)"} · {StateText} · capacity {CapacityText} · {InvestmentText} · " +
        $"in service {InServiceText}";
}

/// <summary>One fluid's planned rate against the plan's capacity for that unit (AC-1, AC-2).</summary>
public sealed record OgCapacityLine(
    string Fluid,
    string Unit,
    decimal PlannedRate,
    int WellsWithRate,
    int WellsWithoutRate,
    decimal? Capacity,
    int FacilitiesCount,
    IReadOnlyList<string> NotComparable,
    decimal? Headroom)
{
    public bool HasCapacity => Capacity is > 0m;

    /// <summary>AC-2: a shortfall is its own number, never a clamp to zero.</summary>
    public decimal? Shortfall => Headroom is < 0m ? -Headroom.Value : null;

    public string Text
    {
        get
        {
            var planned = $"{PlannedRate.ToString("#,0.###", CultureInfo.InvariantCulture)} {Unit} planned " +
                          $"({WellsWithRate.ToString(CultureInfo.InvariantCulture)} well(s)";

            planned += WellsWithoutRate == 0
                ? ")"
                : $", {WellsWithoutRate.ToString(CultureInfo.InvariantCulture)} without a rate)";

            if (!HasCapacity)
            {
                return $"{planned} · no capacity is recorded in {Unit}";
            }

            var capacity = $"{Capacity!.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} {Unit} capacity " +
                           $"({FacilitiesCount.ToString(CultureInfo.InvariantCulture)} facility/facilities)";

            var answer = Shortfall is { } shortfall
                ? $" · shortfall {shortfall.ToString("#,0.###", CultureInfo.InvariantCulture)} {Unit}"
                : $" · headroom {Headroom!.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} {Unit}";

            return NotComparable.Count == 0
                ? $"{planned} · {capacity}{answer}"
                : $"{planned} · {capacity}{answer} · not comparable: {string.Join(", ", NotComparable)}";
        }
    }
}

/// <summary>The capacity check for the plan, one line per fluid it plans.</summary>
public sealed record OgCapacityCheck(IReadOnlyList<OgCapacityLine> Lines)
{
    public bool HasShortfall => Lines.Any(line => line.Shortfall is not null);

    public string Text => Lines.Count == 0
        ? "No planned rate is recorded, so there is nothing to compare with capacity."
        : string.Join(" · ", Lines.Select(line => line.Text));
}

/// <summary>How the plan's pools map onto one facility (the allocation preview).</summary>
public sealed record OgAllocationLine(
    string FacilityId,
    string FacilityName,
    IReadOnlyList<string> Pools,
    decimal? Total,
    string? Unit,
    IReadOnlyList<string> UnitMismatch)
{
    public string Text
    {
        get
        {
            if (Pools.Count == 0)
            {
                return $"{FacilityName}: no pool is routed to it";
            }

            var pools = $"{FacilityName}: {string.Join(", ", Pools)}";

            if (UnitMismatch.Count > 0)
            {
                // AC-1: two volume units are never added; the mismatch is stated instead of a total.
                return $"{pools} · recoverable volumes recorded in {string.Join(" and ", UnitMismatch)}, " +
                       "so no total is produced";
            }

            return Total is { } total
                ? $"{pools} · recoverable {total.ToString("#,0.###", CultureInfo.InvariantCulture)} {Unit}"
                : $"{pools} · recoverable volume not computable";
        }
    }
}

/// <summary>The allocation preview and the pools that route nowhere.</summary>
public sealed record OgAllocation(
    IReadOnlyList<OgAllocationLine> Facilities,
    IReadOnlyList<string> UnroutedPools)
{
    public string Text
    {
        get
        {
            var lines = Facilities.Select(line => line.Text).ToList();

            if (UnroutedPools.Count > 0)
            {
                lines.Add($"routed to no facility: {string.Join(", ", UnroutedPools)}");
            }

            return lines.Count == 0
                ? "No pool is recorded, so there is no allocation to preview."
                : string.Join(" · ", lines);
        }
    }
}

/// <summary>The pool and facility plan of one development plan version.</summary>
public sealed record OgPoolsFacilitiesSnapshot(
    string FieldName,
    string? PlanId,
    string? PlanName,
    int PlanVersion,
    IReadOnlyList<OgPoolPlan> Pools,
    IReadOnlyList<OgFacilityPlan> Facilities,
    OgCapacityCheck Capacity,
    OgAllocation Allocation,
    DateTime AsOf)
{
    public bool HasPlan => !string.IsNullOrWhiteSpace(PlanId);

    public bool IsEmpty => Pools.Count == 0 && Facilities.Count == 0;

    public int PlannedWells => Pools.Sum(pool => pool.PlannedWells);

    public int OperatingFacilities => Facilities.Count(facility => facility.IsOperating);

    /// <summary>AC-1: pools whose OOIP has no unit, so the page can call them out.</summary>
    public int PoolsWithoutUnit => Pools.Count(pool => !pool.HasOoipUnit);

    public string Summary =>
        $"{Pools.Count.ToString(CultureInfo.InvariantCulture)} pool(s) · " +
        $"{PlannedWells.ToString(CultureInfo.InvariantCulture)} well(s) planned · " +
        $"{Facilities.Count.ToString(CultureInfo.InvariantCulture)} facilit(y/ies) " +
        $"({OperatingFacilities.ToString(CultureInfo.InvariantCulture)} operating)";
}

/// <summary>One pool, as the page saves it.</summary>
public sealed record OgPoolValues(
    string Name,
    string? PoolId,
    string? Reservoir,
    string? DriveMechanism,
    decimal? Ooip,
    string? OoipUnit,
    decimal RecoveryFactor,
    int PlannedWells,
    string? FacilityId);

/// <summary>One facility, as the page saves it.</summary>
public sealed record OgFacilityValues(
    string? FacilityId,
    string Name,
    string? Kind,
    string? Phase,
    decimal? Capacity,
    string? CapacityUnit,
    decimal? Investment,
    string? Currency,
    DateTime? StartDate,
    DateTime? CompletionDate);

/// <summary>DEV-04: pools, facility capacity and the facility investment a plan depends on.</summary>
public interface IOgPoolsFacilities
{
    Task<OgResult<OgPoolsFacilitiesSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? planId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Adds or updates one pool of the plan; the row carries the plan and its version (AC-4).</summary>
    Task<OgResult<OgPoolsFacilitiesSnapshot>> SavePoolAsync(
        string planId,
        OgPoolValues values,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Adds or updates one facility investment of the plan (AC-4).</summary>
    Task<OgResult<OgPoolsFacilitiesSnapshot>> SaveFacilityAsync(
        string planId,
        OgFacilityValues values,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The pool and facility plan over the DevelopmentPlanning-owned <c>DEVELOPMENT_POOL_PLAN</c> and
/// <c>FACILITY_INVESTMENT</c>, the PPDM <c>POOL</c> and <c>FACILITY</c> rows they refer to, and the plan's
/// <c>DEVELOPMENT_WELL_SCHEDULE</c> rates, read through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// A rate is compared with a capacity only when the units agree; two units are never added or converted (AC-1). A
/// shortfall is reported as its own number (AC-2). A facility's state is derived from the PPDM <c>FACILITY</c> row and
/// the planned dates, so a planned facility is visible as planned and excluded from the operational pages (AC-3).
/// The pool rows and the facility investments both carry the plan and the plan version they belong to (AC-4).
/// </remarks>
public sealed class OgPoolsFacilitiesService : IOgPoolsFacilities
{
    public const string CodeNoConnection = "pools-facilities-no-connection";
    public const string CodeReadFailed = "pools-facilities-read-failed";
    public const string CodeWriteFailed = "pools-facilities-write-failed";
    public const string CodeNoPlan = "pools-facilities-no-plan";
    public const string CodePlanUnknown = "pools-facilities-plan-unknown";
    public const string CodeActorRequired = "pools-facilities-actor-required";
    public const string CodePoolNameRequired = "pools-facilities-pool-name-required";
    public const string CodeVolumeUnitRequired = "pools-facilities-volume-unit-required";
    public const string CodeRecoveryInvalid = "pools-facilities-recovery-invalid";
    public const string CodeWellsInvalid = "pools-facilities-wells-invalid";
    public const string CodeFacilityNameRequired = "pools-facilities-facility-name-required";
    public const string CodeCapacityUnitRequired = "pools-facilities-capacity-unit-required";
    public const string CodeCurrencyRequired = "pools-facilities-currency-required";
    public const string CodeDateOrder = "pools-facilities-date-order";
    public const string CodeFacilityUnknown = "pools-facilities-facility-unknown";

    /// <summary>The oil rate unit the schedule's <c>INITIAL_RATE_OIL_BOPD</c> column is stated in.</summary>
    public const string OilUnit = "BOPD";

    /// <summary>The gas rate unit the schedule's <c>INITIAL_RATE_GAS_MMSCFD</c> column is stated in.</summary>
    public const string GasUnit = "MMSCFD";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPoolsFacilitiesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPoolsFacilitiesSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? planId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "No connection is open, so the pool and facility plan cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var plan = ResolvePlan(model.Plans, fieldId, fieldName, planId);

            if (plan is null)
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Ok(new OgPoolsFacilitiesSnapshot(
                    string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                    null,
                    null,
                    0,
                    [],
                    [],
                    new OgCapacityCheck([]),
                    new OgAllocation([], []),
                    asOf.Date));
            }

            var snapshot = Build(model, plan, asOf.Date);
            return OgResult<OgPoolsFacilitiesSnapshot>.Ok(snapshot);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the pool and facility plan",
                "the pools and facilities are not shown",
                FailureSeverity.Error);

            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "The pool and facility plan could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPoolsFacilitiesSnapshot>> SavePoolAsync(
        string planId,
        OgPoolValues values,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "A pool plan change must record who made it.", CodeActorRequired);
        }

        if (ValidatePool(values) is { } refusal)
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "No connection is open, so the pool plan cannot be saved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var plan = model.Plans.FirstOrDefault(row =>
                string.Equals(row.FDP_ID, planId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (plan is null)
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                    $"Development plan '{planId}' is not in this database.", CodePlanUnknown);
            }

            // A pool routed to a facility this plan does not have is a broken allocation, so it is refused.
            if (!string.IsNullOrWhiteSpace(values.FacilityId)
                && !model.Facilities.Any(facility =>
                    string.Equals(facility.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(facility.FACILITY_ID, values.FacilityId!.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                    $"Facility '{values.FacilityId}' is not part of this plan, so the pool cannot be routed to it.",
                    CodeFacilityUnknown);
            }

            var existing = model.Pools.FirstOrDefault(row =>
                string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(values.PoolId)
                    ? string.Equals(row.POOL_NAME, values.Name.Trim(), StringComparison.OrdinalIgnoreCase)
                    : string.Equals(row.POOL_ID, values.PoolId!.Trim(), StringComparison.OrdinalIgnoreCase)));

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_POOL_PLAN>(source);
            unit.PrimaryKey = "POOL_PLAN_ID";

            if (existing is null)
            {
                unit.Add(new DEVELOPMENT_POOL_PLAN
                {
                    POOL_PLAN_ID = Guid.NewGuid().ToString("N"),
                    FDP_ID = plan.FDP_ID!,
                    PLAN_VERSION = plan.FDP_VERSION,
                    FIELD_ID = plan.FIELD_ID ?? string.Empty,
                    POOL_ID = Blank(values.PoolId),
                    POOL_NAME = values.Name.Trim(),
                    RESERVOIR = Blank(values.Reservoir),
                    DRIVE_MECHANISM = Blank(values.DriveMechanism),
                    OOIP = values.Ooip ?? 0m,
                    OOIP_OUOM = Blank(values.OoipUnit)?.Trim(),
                    RECOVERY_FACTOR = values.RecoveryFactor,
                    PLANNED_WELLS = values.PlannedWells,
                    FACILITY_ID = Blank(values.FacilityId),
                    ACTOR = actor.Trim(),
                    RECORDED_AT = now,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                existing.PLAN_VERSION = plan.FDP_VERSION;
                existing.POOL_ID = Blank(values.PoolId);
                existing.POOL_NAME = values.Name.Trim();
                existing.RESERVOIR = Blank(values.Reservoir);
                existing.DRIVE_MECHANISM = Blank(values.DriveMechanism);
                existing.OOIP = values.Ooip ?? 0m;
                existing.OOIP_OUOM = Blank(values.OoipUnit)?.Trim();
                existing.RECOVERY_FACTOR = values.RecoveryFactor;
                existing.PLANNED_WELLS = values.PlannedWells;
                existing.FACILITY_ID = Blank(values.FacilityId);
                existing.ACTOR = actor.Trim();
                existing.RECORDED_AT = now;
                existing.ROW_CHANGED_BY = actor.Trim();
                existing.ROW_CHANGED_DATE = now;

                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                        OgUow.Message(updated, "The pool plan was not saved."), CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                    OgUow.Message(committed, "The pool plan was not saved."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var savedPlan = after.Plans.First(row =>
                string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase));

            return OgResult<OgPoolsFacilitiesSnapshot>.Ok(Build(after, savedPlan, asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving a pool plan for {planId}",
                "the pool plan was not saved",
                FailureSeverity.Error);

            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "The pool plan could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPoolsFacilitiesSnapshot>> SaveFacilityAsync(
        string planId,
        OgFacilityValues values,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "A facility change must record who made it.", CodeActorRequired);
        }

        if (ValidateFacility(values) is { } refusal)
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "No connection is open, so the facility cannot be saved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var plan = model.Plans.FirstOrDefault(row =>
                string.Equals(row.FDP_ID, planId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (plan is null)
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                    $"Development plan '{planId}' is not in this database.", CodePlanUnknown);
            }

            var existing = model.Facilities.FirstOrDefault(row =>
                string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(values.FacilityId)
                && string.Equals(row.FACILITY_ID, values.FacilityId!.Trim(), StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var unit = OgUow.For<FACILITY_INVESTMENT>(source);
            unit.PrimaryKey = "FACILITY_INV_ID";

            if (existing is null)
            {
                unit.Add(new FACILITY_INVESTMENT
                {
                    FACILITY_INV_ID = Guid.NewGuid().ToString("N"),
                    FDP_ID = plan.FDP_ID!,
                    FIELD_ID = plan.FIELD_ID ?? string.Empty,
                    FACILITY_ID = Blank(values.FacilityId),
                    FACILITY_NAME = values.Name.Trim(),
                    FACILITY_TYPE = Blank(values.Kind),
                    INVESTMENT_PHASE = Blank(values.Phase),
                    CAPACITY_PLANNED_BOPD = values.Capacity,
                    CAPACITY_OUOM = Blank(values.CapacityUnit)?.Trim(),
                    CAPEX_PLANNED_MM = values.Investment,
                    COST_CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant(),
                    PLANNED_START_DATE = values.StartDate,
                    PLANNED_COMPLETION_DATE = values.CompletionDate,
                    PLAN_VERSION = plan.FDP_VERSION,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                existing.FACILITY_NAME = values.Name.Trim();
                existing.FACILITY_TYPE = Blank(values.Kind);
                existing.INVESTMENT_PHASE = Blank(values.Phase);
                existing.CAPACITY_PLANNED_BOPD = values.Capacity;
                existing.CAPACITY_OUOM = Blank(values.CapacityUnit)?.Trim();
                existing.CAPEX_PLANNED_MM = values.Investment;
                existing.COST_CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant();
                existing.PLANNED_START_DATE = values.StartDate;
                existing.PLANNED_COMPLETION_DATE = values.CompletionDate;
                existing.PLAN_VERSION = plan.FDP_VERSION;
                existing.ROW_CHANGED_BY = actor.Trim();
                existing.ROW_CHANGED_DATE = now;

                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                        OgUow.Message(updated, "The facility was not saved."), CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                    OgUow.Message(committed, "The facility was not saved."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var savedPlan = after.Plans.First(row =>
                string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase));

            return OgResult<OgPoolsFacilitiesSnapshot>.Ok(Build(after, savedPlan, asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving a facility for {planId}",
                "the facility was not saved",
                FailureSeverity.Error);

            return OgResult<OgPoolsFacilitiesSnapshot>.Failed(
                "The facility could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    /// <summary>The shared pool rules; a volume or a factor outside its bounds is refused.</summary>
    public static (string Code, string Message)? ValidatePool(OgPoolValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.Name))
        {
            return (CodePoolNameRequired, "A pool plan needs a pool name.");
        }

        if (values.Ooip is > 0m && string.IsNullOrWhiteSpace(values.OoipUnit))
        {
            return (CodeVolumeUnitRequired,
                "An OOIP needs the unit it is measured in (for example MMbbl, MMstb or Bcf).");
        }

        if (values.Ooip is < 0m)
        {
            return (CodeVolumeUnitRequired, "An OOIP cannot be negative.");
        }

        if (values.RecoveryFactor is < 0m or > 1m)
        {
            return (CodeRecoveryInvalid,
                $"The recovery factor {values.RecoveryFactor.ToString(CultureInfo.InvariantCulture)} must be a " +
                "fraction between 0 and 1.");
        }

        if (values.PlannedWells < 0)
        {
            return (CodeWellsInvalid, "The planned well count cannot be negative.");
        }

        return null;
    }

    /// <summary>The shared facility rules.</summary>
    public static (string Code, string Message)? ValidateFacility(OgFacilityValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.Name))
        {
            return (CodeFacilityNameRequired, "A facility plan needs a facility name.");
        }

        if (values.Capacity is > 0m && string.IsNullOrWhiteSpace(values.CapacityUnit))
        {
            return (CodeCapacityUnitRequired,
                "A capacity needs the unit it is measured in (for example BOPD or MMSCFD).");
        }

        if (values.Capacity is < 0m)
        {
            return (CodeCapacityUnitRequired, "A capacity cannot be negative.");
        }

        if (values.Investment is > 0m && string.IsNullOrWhiteSpace(values.Currency))
        {
            return (CodeCurrencyRequired, "An investment needs the currency it is in.");
        }

        if (values.StartDate is { } start && values.CompletionDate is { } end && end < start)
        {
            return (CodeDateOrder,
                $"The completion date {end:yyyy-MM-dd} is before the planned start {start:yyyy-MM-dd}.");
        }

        return null;
    }

    /// <summary>
    /// AC-1/AC-2: the planned rates per fluid against the plan's capacity in the same unit. A facility whose capacity
    /// unit differs is named as not comparable rather than converted or dropped silently.
    /// </summary>
    public static OgCapacityCheck CheckCapacity(
        IReadOnlyList<DEVELOPMENT_WELL_SCHEDULE> schedules,
        IReadOnlyList<OgFacilityPlan> facilities)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(facilities);

        var live = facilities
            .Where(facility => !string.Equals(facility.State, OgFacilityPlan.Abandoned, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var lines = new List<OgCapacityLine>();

        foreach (var (fluid, unit, rate) in new[]
                 {
                     ("Oil", OilUnit, schedules.Sum(row => row.INITIAL_RATE_OIL_BOPD ?? 0m)),
                     ("Gas", GasUnit, schedules.Sum(row => row.INITIAL_RATE_GAS_MMSCFD ?? 0m)),
                 })
        {
            var withRate = fluid == "Oil"
                ? schedules.Count(row => row.INITIAL_RATE_OIL_BOPD is not null)
                : schedules.Count(row => row.INITIAL_RATE_GAS_MMSCFD is not null);

            var withoutRate = schedules.Count - withRate;

            if (rate <= 0m && withRate == 0)
            {
                continue;
            }

            var matching = live
                .Where(facility => !string.IsNullOrWhiteSpace(facility.CapacityUnit)
                                   && string.Equals(facility.CapacityUnit!.Trim(), unit, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // A facility with a capacity but a different unit cannot be compared with this rate, so it is named.
            var incomparable = live
                .Where(facility => facility.Capacity is > 0m)
                .Where(facility => string.IsNullOrWhiteSpace(facility.CapacityUnit)
                                   || !string.Equals(facility.CapacityUnit!.Trim(), unit, StringComparison.OrdinalIgnoreCase))
                .Select(facility =>
                    $"{facility.Name} ({facility.CapacityUnit?.Trim() ?? "no unit"})")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            decimal? capacity = matching.Count == 0
                ? null
                : matching.Sum(facility => facility.Capacity ?? 0m);

            lines.Add(new OgCapacityLine(
                fluid,
                unit,
                rate,
                withRate,
                withoutRate,
                capacity,
                matching.Count,
                incomparable,
                capacity is { } available ? available - rate : null));
        }

        return new OgCapacityCheck(lines);
    }

    /// <summary>
    /// AC-1: the recoverable volume the plan's pools route to each facility. Two volume units are never added; the
    /// mismatch is reported per facility instead.
    /// </summary>
    public static OgAllocation BuildAllocation(
        IReadOnlyList<OgPoolPlan> pools,
        IReadOnlyList<OgFacilityPlan> facilities)
    {
        ArgumentNullException.ThrowIfNull(pools);
        ArgumentNullException.ThrowIfNull(facilities);

        var lines = new List<OgAllocationLine>();

        foreach (var facility in facilities)
        {
            var routed = pools
                .Where(pool => !string.IsNullOrWhiteSpace(pool.FacilityId)
                               && string.Equals(pool.FacilityId, facility.FacilityId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var units = routed
                .Where(pool => pool.Recoverable is not null)
                .Select(pool => pool.OoipUnit?.Trim() ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var mismatch = units.Count > 1
                ? units.OrderBy(unit => unit, StringComparer.OrdinalIgnoreCase).ToList()
                : [];

            var total = units.Count == 1
                ? routed.Where(pool => pool.Recoverable is not null).Sum(pool => pool.Recoverable!.Value)
                : (decimal?)null;

            lines.Add(new OgAllocationLine(
                facility.FacilityId ?? string.Empty,
                facility.Name,
                routed.Select(pool => pool.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList(),
                total,
                units.Count == 1 ? units[0] : null,
                mismatch));
        }

        var known = facilities
            .Select(facility => facility.FacilityId ?? string.Empty)
            .Where(id => id.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unrouted = pools
            .Where(pool => string.IsNullOrWhiteSpace(pool.FacilityId) || !known.Contains(pool.FacilityId!))
            .Select(pool => pool.Name)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new OgAllocation(lines, unrouted);
    }

    /// <summary>AC-3: planned, in construction, operating or abandoned, from the PPDM row and the planned dates.</summary>
    public static string FacilityState(
        FACILITY? existing,
        DateTime? start,
        DateTime asOf)
    {
        if (existing is not null)
        {
            if (existing.ABANDONED_DATE is { } abandoned && abandoned.Date <= asOf.Date)
            {
                return OgFacilityPlan.Abandoned;
            }

            if (string.Equals(existing.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase)
                && existing.INACTIVE_DATE is { } inactive
                && inactive.Date <= asOf.Date)
            {
                return OgFacilityPlan.Abandoned;
            }

            if (string.Equals(existing.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)
                || existing.ACTIVE_DATE is { } active && active.Date <= asOf.Date)
            {
                return OgFacilityPlan.Operating;
            }
        }

        // No PPDM row means no evidence the facility exists: a planned date can only make it "in construction",
        // never "operating". Only the PPDM FACILITY row above can report an operating facility (AC-3).
        return start is { } plannedStart && plannedStart.Date <= asOf.Date
            ? OgFacilityPlan.InConstruction
            : OgFacilityPlan.Planned;
    }

    // ------------------------------------------------------------------ reads

    private async Task<PoolFacilityModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);
        var pools = await OgUow.ReadAllAsync<DEVELOPMENT_POOL_PLAN>(source).ConfigureAwait(false);
        var facilities = await OgUow.ReadAllAsync<FACILITY_INVESTMENT>(source).ConfigureAwait(false);
        var schedules = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_SCHEDULE>(source).ConfigureAwait(false);
        var ppdmFacilities = await ReadPpdmFacilitiesAsync(source, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new PoolFacilityModel(plans, pools, facilities, schedules, ppdmFacilities);
    }

    /// <summary>Reads the PPDM facility rows the plan's facilities refer to, tolerating a database without them.</summary>
    private async Task<List<FACILITY>> ReadPpdmFacilitiesAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        try
        {
            return (await OgUow.ReadAllAsync<FACILITY>(source).ConfigureAwait(false)).ToList();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // The plan can exist before the PPDM schema is applied; the facilities are then all "planned".
            _failures.ReportHandled(
                ex,
                "reading the PPDM facilities for the facility states",
                "every planned facility is shown as planned, not operating",
                FailureSeverity.Degraded);

            return [];
        }
    }

    private static FIELD_DEVELOPMENT_PLAN? ResolvePlan(
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> plans,
        string? fieldId,
        string? fieldName,
        string? planId)
    {
        if (!string.IsNullOrWhiteSpace(planId))
        {
            return plans.FirstOrDefault(plan =>
                string.Equals(plan.FDP_ID, planId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return plans
            .Where(plan => FieldMatches(plan.FIELD_ID, fieldId, fieldName))
            .OrderByDescending(plan => plan.FDP_VERSION)
            .ThenByDescending(plan => plan.SUBMISSION_DATE)
            .ThenBy(plan => plan.FDP_ID, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static OgPoolsFacilitiesSnapshot Build(
        PoolFacilityModel model,
        FIELD_DEVELOPMENT_PLAN plan,
        DateTime asOf)
    {
        var pools = model.Pools
            .Where(row => string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.POOL_NAME, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgPoolPlan(
                row.POOL_PLAN_ID ?? string.Empty,
                row.FDP_ID ?? string.Empty,
                row.FIELD_ID ?? string.Empty,
                row.PLAN_VERSION,
                Blank(row.POOL_ID),
                Blank(row.POOL_NAME) ?? "(unnamed pool)",
                Blank(row.RESERVOIR),
                Blank(row.DRIVE_MECHANISM),
                row.OOIP > 0m ? row.OOIP : null,
                Blank(row.OOIP_OUOM),
                row.RECOVERY_FACTOR,
                row.PLANNED_WELLS,
                Blank(row.FACILITY_ID)))
            .ToList();

        var facilities = model.Facilities
            .Where(row => string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.FACILITY_NAME, StringComparer.OrdinalIgnoreCase)
            .Select(row => ToFacility(row, asOf, model.PpdmFacilities))
            .ToList();

        var schedules = model.Schedules
            .Where(row => string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new OgPoolsFacilitiesSnapshot(
            string.IsNullOrWhiteSpace(plan.FIELD_ID) ? "(no field)" : plan.FIELD_ID!,
            plan.FDP_ID,
            Blank(plan.FDP_NAME),
            plan.FDP_VERSION,
            pools,
            facilities,
            CheckCapacity(schedules, facilities),
            BuildAllocation(pools, facilities),
            asOf.Date);
    }

    private static OgFacilityPlan ToFacility(
        FACILITY_INVESTMENT row,
        DateTime asOf,
        IReadOnlyList<FACILITY> ppdm)
    {
        var existing = string.IsNullOrWhiteSpace(row.FACILITY_ID)
            ? null
            : ppdm.FirstOrDefault(facility =>
                string.Equals(facility.FACILITY_ID, row.FACILITY_ID, StringComparison.OrdinalIgnoreCase));

        var state = FacilityState(
            existing,
            row.PLANNED_START_DATE,
            asOf);

        return new OgFacilityPlan(
            row.FACILITY_INV_ID ?? string.Empty,
            row.FDP_ID ?? string.Empty,
            row.PLAN_VERSION,
            Blank(row.FACILITY_ID),
            Blank(row.FACILITY_NAME) ?? "(unnamed facility)",
            Blank(row.FACILITY_TYPE),
            Blank(row.INVESTMENT_PHASE),
            row.CAPACITY_PLANNED_BOPD is > 0m ? row.CAPACITY_PLANNED_BOPD : null,
            Blank(row.CAPACITY_OUOM),
            row.CAPEX_PLANNED_MM is > 0m ? row.CAPEX_PLANNED_MM : null,
            Blank(row.COST_CURRENCY),
            row.PLANNED_START_DATE,
            row.PLANNED_COMPLETION_DATE,
            state,
            ExistsInPpdm: existing is not null);
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record PoolFacilityModel(
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans,
        IReadOnlyList<DEVELOPMENT_POOL_PLAN> Pools,
        IReadOnlyList<FACILITY_INVESTMENT> Facilities,
        IReadOnlyList<DEVELOPMENT_WELL_SCHEDULE> Schedules,
        IReadOnlyList<FACILITY> PpdmFacilities);
}
