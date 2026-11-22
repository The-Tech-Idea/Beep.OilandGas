using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One casing string of a design outline (DEV-03).</summary>
public sealed record OgCasingString(
    int Order,
    string Name,
    decimal Od,
    decimal Weight,
    string? Grade,
    decimal SetDepth,
    string? DepthUnit)
{
    public string Describe() =>
        $"{Order.ToString(CultureInfo.InvariantCulture)}. {Name} · {Od.ToString("0.###", CultureInfo.InvariantCulture)} in · " +
        $"{Weight.ToString("0.##", CultureInfo.InvariantCulture)} lb/ft · {Grade ?? "(no grade)"} · " +
        $"set {SetDepth.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit not recorded)"}";
}

/// <summary>The recorded casing-check result linked to a design version (AC-3).</summary>
public sealed record OgCasingCheck(
    string Id,
    string DesignId,
    string Outcome,
    IReadOnlyList<string> Findings,
    string? CheckedBy,
    DateTime? CheckedAt)
{
    public const string Passed = "PASS";
    public const string Failed = "FAIL";

    public bool IsPass => string.Equals(Outcome, Passed, StringComparison.OrdinalIgnoreCase);

    public string FindingsText => Findings.Count == 0 ? "no findings" : string.Join("; ", Findings);

    public string Describe() =>
        $"{Outcome} · checked by {CheckedBy ?? "(unknown)"} on " +
        $"{CheckedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · {FindingsText}";
}

/// <summary>
/// A development well's design (DEV-03). It mirrors the plan's <c>OgDevelopmentWell</c> contract plus the version, the
/// casing outline, the linked casing check and the derived completeness answer.
/// </summary>
public sealed record OgDevelopmentWell(
    string Id,
    string PlanId,
    string FieldId,
    string ScheduleId,
    string WellId,
    string? Name,
    int Version,
    bool IsCurrent,
    string DesignState,
    string? Objective,
    string? TargetFormation,
    decimal? TargetDepth,
    string? DepthUnit,
    string? TrajectoryIntent,
    string? CompletionConcept,
    string? Risk,
    decimal? EstimatedCost,
    string? Currency,
    DateTime? SpudWindowStart,
    DateTime? SpudWindowEnd,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? RevisionReason,
    IReadOnlyList<OgCasingString> Casing,
    OgCasingCheck? LastCheck)
{
    public const string Concept = "CONCEPT";
    public const string Defined = "DEFINED";
    public const string Approved = "APPROVED";
    public const string Drilled = "DRILLED";

    public static IReadOnlyList<string> States { get; } = [Concept, Defined, Approved, Drilled];

    public bool IsApproved =>
        string.Equals(DesignState, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(DesignState, Drilled, StringComparison.OrdinalIgnoreCase);

    public string StateText => DesignState.ToLowerInvariant();

    /// <summary>AC-2: a target depth without its unit is not a depth.</summary>
    public bool HasDepth => TargetDepth is > 0 && !string.IsNullOrWhiteSpace(DepthUnit);

    public string TargetText => TargetDepth is { } depth
        ? $"{depth.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit not recorded)"}"
        : "(target depth not recorded)";

    public string CostText => EstimatedCost is { } cost
        ? $"{cost.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "(cost not recorded)";

    public string SpudWindowText =>
        SpudWindowStart is null && SpudWindowEnd is null
            ? "(no spud window)"
            : $"{SpudWindowStart?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?"} to " +
              $"{SpudWindowEnd?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?"}";

    public string TrajectoryText => string.IsNullOrWhiteSpace(TrajectoryIntent)
        ? "(no trajectory intent)"
        : TrajectoryIntent!.Trim();

    /// <summary>State 1 of section 5: the preview needs a casing scheme.</summary>
    public bool CasingPreviewEnabled => Casing.Count > 0;

    public int CasingCount => Casing.Count;

    public string? CasingPreviewReason =>
        CasingPreviewEnabled
            ? null
            : "The schematic preview is disabled: no casing string is recorded yet.";

    /// <summary>AC-4: an incomplete design cannot be authored into a drilling program.</summary>
    public IReadOnlyList<string> MissingItems => OgWellDesignService.MissingItems(this);

    public bool CanAuthorProgram => MissingItems.Count == 0;

    public string MissingText => MissingItems.Count == 0
        ? "The design is complete."
        : "Missing: " + string.Join(", ", MissingItems);

    public string Describe() =>
        $"v{Version.ToString(CultureInfo.InvariantCulture)} · {StateText} · {TargetText} · {CostText} · " +
        $"{Casing.Count.ToString(CultureInfo.InvariantCulture)} casing string(s)";
}

/// <summary>One set of design values, saved together.</summary>
public sealed record OgWellDesignValues(
    string? Objective,
    string? TargetFormation,
    decimal? TargetDepth,
    string? DepthUnit,
    string? TrajectoryIntent,
    string? CompletionConcept,
    string? Risk,
    decimal? EstimatedCost,
    string? CostCurrency,
    DateTime? SpudWindowStart,
    DateTime? SpudWindowEnd);

/// <summary>One casing string, as the page adds it.</summary>
public sealed record OgCasingStringRequest(
    string Name,
    decimal Od,
    decimal Weight,
    string? Grade,
    decimal SetDepth,
    string? DepthUnit);

/// <summary>One scheduled well of the plan, so the page can offer the wells that have no design yet (AC-1).</summary>
public sealed record OgDesignSchedule(
    string Id,
    string PlanId,
    string WellId,
    string? WellName,
    string? Status,
    bool HasDesign)
{
    public string Label => $"{WellName ?? WellId} · {Status ?? "(no status)"}" +
                           (HasDesign ? " · design recorded" : string.Empty);

    /// <summary>The picker offers a scheduled well that has no design.</summary>
    public bool CanDesign => !HasDesign;
}

/// <summary>The plans and their current designs for one field.</summary>
public sealed record OgWellDesignWorkspace(
    string FieldName,
    IReadOnlyList<OgDevelopmentWell> Wells,
    IReadOnlyList<OgDesignSchedule> Schedules)
{
    public bool IsEmpty => Wells.Count == 0;

    public IReadOnlyList<OgDevelopmentWell> ApprovedWells =>
        Wells.Where(well => well.IsApproved).ToList();

    public IReadOnlyList<OgDevelopmentWell> IncompleteWells =>
        Wells.Where(well => !well.CanAuthorProgram).ToList();

    /// <summary>The scheduled wells that still need a design.</summary>
    public IReadOnlyList<OgDesignSchedule> UndesignedSchedules =>
        Schedules.Where(schedule => schedule.CanDesign).ToList();
}

/// <summary>Everything the detail pane shows for one design, including its retained versions.</summary>
public sealed record OgWellDesignDetail(
    OgDevelopmentWell Current,
    IReadOnlyList<OgDevelopmentWell> Versions,
    OgCasingCheck? LastCheck)
{
    public IReadOnlyList<string> MissingItems => Current.MissingItems;

    public bool CanAuthorProgram => Current.CanAuthorProgram;
}

/// <summary>DEV-03: development well design - objective, trajectory intent, casing outline, completion, cost.</summary>
public interface IOgWellDesign
{
    Task<OgResult<OgWellDesignWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? planId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates the first design version for a scheduled well of a plan (AC-1).</summary>
    Task<OgResult<OgWellDesignDetail>> CreateAsync(
        string scheduleId,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgWellDesignDetail>> SaveAsync(
        string designId,
        OgWellDesignValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves the design; an incomplete design is refused with the missing items (AC-4).</summary>
    Task<OgResult<OgWellDesignDetail>> ApproveAsync(
        string designId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Appends a new version and retains the previous one (AC-5).</summary>
    Task<OgResult<OgWellDesignDetail>> ReviseAsync(
        string designId,
        OgWellDesignValues values,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgWellDesignDetail>> AddCasingStringAsync(
        string designId,
        OgCasingStringRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Checks the design's inputs and records a result linked to the version (AC-3).</summary>
    Task<OgResult<OgWellDesignDetail>> RunCasingCheckAsync(
        string designId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The design over the DevelopmentPlanning-owned <c>DEVELOPMENT_WELL_DESIGN</c>,
/// <c>DEVELOPMENT_CASING_STRING</c> and <c>DEVELOPMENT_CASING_CHECK</c>, with the schedule and plan rows read for the
/// scope (AC-1), all through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The casing check is a check of the design's <em>inputs</em>: it requires at least one string, a name, an OD, a
/// grade, a set depth with the design's depth unit, strictly increasing set depths and no string set below the
/// design's target depth. The burst, collapse and tension arithmetic is ENG-02's and is not performed or implied
/// here.
/// </remarks>
public sealed class OgWellDesignService : IOgWellDesign
{
    public const string CodeNoConnection = "well-design-no-connection";
    public const string CodeReadFailed = "well-design-read-failed";
    public const string CodeWriteFailed = "well-design-write-failed";
    public const string CodeUnknown = "well-design-unknown";
    public const string CodeActorRequired = "well-design-actor-required";
    public const string CodeScheduleRequired = "well-design-schedule-required";
    public const string CodeScheduleUnknown = "well-design-schedule-unknown";
    public const string CodeAlreadyDesigned = "well-design-exists";
    public const string CodeFieldMismatch = "well-design-field-mismatch";
    public const string CodeDepthUnitRequired = "well-design-depth-unit-required";
    public const string CodeDepthInvalid = "well-design-depth-invalid";
    public const string CodeCostInvalid = "well-design-cost-invalid";
    public const string CodeCurrencyRequired = "well-design-currency-required";
    public const string CodeWindowOrder = "well-design-window-order";
    public const string CodeIncomplete = "well-design-incomplete";
    public const string CodeLocked = "well-design-locked";
    public const string CodeCasingInvalid = "well-design-casing-invalid";
    public const string CodeCasingRequired = "well-design-casing-required";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellDesignService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    /// <summary>AC-4: the design fields a drilling program needs, in the order the plan lists them.</summary>
    public static IReadOnlyList<string> MissingItems(OgDevelopmentWell well)
    {
        ArgumentNullException.ThrowIfNull(well);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(well.Objective))
        {
            missing.Add("Objective");
        }

        if (string.IsNullOrWhiteSpace(well.TargetFormation))
        {
            missing.Add("Target formation");
        }

        if (!well.HasDepth)
        {
            missing.Add("Target depth with its unit");
        }

        if (string.IsNullOrWhiteSpace(well.TrajectoryIntent))
        {
            missing.Add("Trajectory intent");
        }

        if (well.Casing.Count == 0)
        {
            missing.Add("Casing scheme");
        }

        if (string.IsNullOrWhiteSpace(well.CompletionConcept))
        {
            missing.Add("Completion concept");
        }

        if (well.EstimatedCost is not > 0 || string.IsNullOrWhiteSpace(well.Currency))
        {
            missing.Add("Estimated cost with its currency");
        }

        if (well.SpudWindowStart is null || well.SpudWindowEnd is null)
        {
            missing.Add("Spud window");
        }

        return missing;
    }

    /// <summary>
    /// AC-3: the input check. A finding names the string and what is wrong; an empty list means the inputs are
    /// consistent. Nothing here calculates a load or a rating.
    /// </summary>
    public static IReadOnlyList<string> CheckCasing(OgDevelopmentWell well, IReadOnlyList<OgCasingString> strings)
    {
        ArgumentNullException.ThrowIfNull(well);
        ArgumentNullException.ThrowIfNull(strings);

        var findings = new List<string>();

        if (strings.Count == 0)
        {
            findings.Add("No casing string is recorded, so there is nothing to check.");
            return findings;
        }

        if (!well.HasDepth)
        {
            findings.Add("The design's target depth has no unit, so the casing depths cannot be compared with it.");
        }

        foreach (var casing in strings.OrderBy(item => item.Order))
        {
            var label = $"String {casing.Order.ToString(CultureInfo.InvariantCulture)} ({casing.Name})";

            if (string.IsNullOrWhiteSpace(casing.Name))
            {
                findings.Add($"{label} has no name.");
            }

            if (casing.Od <= 0m)
            {
                findings.Add($"{label} has no outside diameter.");
            }

            if (string.IsNullOrWhiteSpace(casing.Grade))
            {
                findings.Add($"{label} has no steel grade.");
            }

            if (casing.SetDepth <= 0m)
            {
                findings.Add($"{label} has no set depth.");
            }

            if (string.IsNullOrWhiteSpace(casing.DepthUnit))
            {
                findings.Add($"{label} has a set depth with no unit.");
            }
            else if (!string.IsNullOrWhiteSpace(well.DepthUnit)
                     && !string.Equals(casing.DepthUnit.Trim(), well.DepthUnit.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(
                    $"{label} is measured in {casing.DepthUnit!.Trim()} but the design's depth unit is {well.DepthUnit!.Trim()}.");
            }

            if (well.TargetDepth is { } target && casing.SetDepth > target)
            {
                findings.Add(
                    $"{label} is set at {casing.SetDepth.ToString("0.##", CultureInfo.InvariantCulture)}, below the " +
                    $"design's target depth of {target.ToString("0.##", CultureInfo.InvariantCulture)}.");
            }
        }

        var ordered = strings.OrderBy(item => item.Order).ToList();

        for (var index = 1; index < ordered.Count; index++)
        {
            if (ordered[index].SetDepth <= ordered[index - 1].SetDepth)
            {
                findings.Add(
                    $"String {ordered[index].Order.ToString(CultureInfo.InvariantCulture)} ({ordered[index].Name}) is " +
                    $"set at {ordered[index].SetDepth.ToString("0.##", CultureInfo.InvariantCulture)}, which is not " +
                    $"deeper than string {ordered[index - 1].Order.ToString(CultureInfo.InvariantCulture)} " +
                    $"({ordered[index - 1].Name}) at " +
                    $"{ordered[index - 1].SetDepth.ToString("0.##", CultureInfo.InvariantCulture)}.");
            }
        }

        return findings;
    }

    public async Task<OgResult<OgWellDesignWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? planId,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignWorkspace>.Failed(
                "No connection is open, so well designs cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var wells = model.Designs
                .Where(design => string.Equals(design.CURRENT_IND, "Y", StringComparison.OrdinalIgnoreCase)
                                 || string.IsNullOrWhiteSpace(design.CURRENT_IND))
                .Where(design => FieldMatches(design.FIELD_ID, fieldId, fieldName))
                .Where(design => string.IsNullOrWhiteSpace(planId)
                                 || string.Equals(design.FDP_ID, planId.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(design => design.WELL_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(design => design.DESIGN_ID, StringComparer.Ordinal)
                .Select(design => ToWell(design, model))
                .ToList();

            var designed = model.Designs
                .Select(design => design.SCHEDULE_ID ?? string.Empty)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var schedules = model.Schedules
                .Where(schedule => FieldMatches(schedule.FIELD_ID, fieldId, fieldName))
                .Where(schedule => string.IsNullOrWhiteSpace(planId)
                                   || string.Equals(schedule.FDP_ID, planId.Trim(), StringComparison.OrdinalIgnoreCase))
                // A schedule whose plan belongs to another field cannot become a design (AC-1), so it is not
                // offered. The service still refuses it if a caller asks directly.
                .Where(schedule => model.Plans.Any(plan =>
                    string.Equals(plan.FDP_ID, schedule.FDP_ID, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(plan.FIELD_ID, schedule.FIELD_ID, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(schedule => schedule.WELL_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(schedule => schedule.SCHEDULE_ID, StringComparer.Ordinal)
                .Select(schedule => new OgDesignSchedule(
                    schedule.SCHEDULE_ID ?? string.Empty,
                    schedule.FDP_ID ?? string.Empty,
                    schedule.PLANNED_WELL_ID ?? string.Empty,
                    Blank(schedule.WELL_NAME),
                    Blank(schedule.SCHEDULE_STATUS),
                    designed.Contains(schedule.SCHEDULE_ID ?? string.Empty)))
                .ToList();

            return OgResult<OgWellDesignWorkspace>.Ok(new OgWellDesignWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                wells,
                schedules));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the development well designs",
                "the designs are not shown",
                FailureSeverity.Error);

            return OgResult<OgWellDesignWorkspace>.Failed(
                "The well designs could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> CreateAsync(
        string scheduleId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A design must record who created it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(scheduleId))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "Choose the scheduled well the design belongs to.", CodeScheduleRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the design cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var schedule = model.Schedules.FirstOrDefault(row =>
                string.Equals(row.SCHEDULE_ID, scheduleId.Trim(), StringComparison.OrdinalIgnoreCase));

            // AC-1: the design takes its plan and field from the scheduled well, so the two cannot disagree.
            if (schedule is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Scheduled well '{scheduleId}' is not in this database.", CodeScheduleUnknown);
            }

            var plan = model.Plans.FirstOrDefault(row =>
                string.Equals(row.FDP_ID, schedule.FDP_ID, StringComparison.OrdinalIgnoreCase));

            if (plan is null
                || !string.Equals(plan.FIELD_ID, schedule.FIELD_ID, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Scheduled well '{scheduleId}' and its plan do not belong to the same field, so the design is " +
                    "not created.",
                    CodeFieldMismatch);
            }

            if (model.Designs.Any(design =>
                    string.Equals(design.SCHEDULE_ID, schedule.SCHEDULE_ID, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"'{schedule.WELL_NAME ?? schedule.PLANNED_WELL_ID}' already has a design; edit it or revise its cost.",
                    CodeAlreadyDesigned);
            }

            var now = DateTime.UtcNow;
            var designId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_WELL_DESIGN>(source);

            unit.Add(new DEVELOPMENT_WELL_DESIGN
            {
                DESIGN_ID = designId,
                SCHEDULE_ID = schedule.SCHEDULE_ID ?? string.Empty,
                FDP_ID = schedule.FDP_ID ?? string.Empty,
                FIELD_ID = schedule.FIELD_ID ?? string.Empty,
                PLANNED_WELL_ID = schedule.PLANNED_WELL_ID ?? string.Empty,
                WELL_NAME = schedule.WELL_NAME,
                VERSION = 1,
                CURRENT_IND = "Y",
                DESIGN_STATE = OgDevelopmentWell.Concept,
                TARGET_FORMATION = schedule.TARGET_RESERVOIR,
                TARGET_DEPTH = 0m,
                DEPTH_OUOM = null,
                ESTIMATED_COST = schedule.AFE_COST_MM ?? 0m,
                COST_CURRENCY = schedule.AFE_CURRENCY,
                SPUD_WINDOW_START = schedule.PLANNED_SPUD_DATE,
                SPUD_WINDOW_END = schedule.PLANNED_COMPLETION_DATE,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadDetailAsync(source, designId, cancellationToken).ConfigureAwait(false)
                : OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The design was not created."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating the design of scheduled well {scheduleId}",
                "the design was not created",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The design could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> SaveAsync(
        string designId,
        OgWellDesignValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A design change must record who made it.", CodeActorRequired);
        }

        if (Validate(values) is { } refusal)
        {
            return OgResult<OgWellDesignDetail>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the design cannot be saved.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<DEVELOPMENT_WELL_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("DESIGN_ID", designId.Trim())], cancellationToken)
                .ConfigureAwait(false);
            var design = rows.FirstOrDefault();

            if (design is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            if (IsLocked(design.DESIGN_STATE))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"This design is {design.DESIGN_STATE}; revise it to a new version instead of editing it.",
                    CodeLocked);
            }

            var now = DateTime.UtcNow;

            design.OBJECTIVE = Blank(values.Objective);
            design.TARGET_FORMATION = Blank(values.TargetFormation);
            design.TARGET_DEPTH = values.TargetDepth ?? 0m;
            design.DEPTH_OUOM = Blank(values.DepthUnit)
                ?.Trim();
            design.TRAJECTORY_INTENT = Blank(values.TrajectoryIntent);
            design.COMPLETION_CONCEPT = Blank(values.CompletionConcept);
            design.RISK_TEXT = Blank(values.Risk);
            design.ESTIMATED_COST = values.EstimatedCost ?? 0m;
            design.COST_CURRENCY = Blank(values.CostCurrency)?.ToUpperInvariant();
            design.SPUD_WINDOW_START = values.SpudWindowStart;
            design.SPUD_WINDOW_END = values.SpudWindowEnd;
            design.DESIGN_STATE = HasAnyValue(design) ? OgDevelopmentWell.Defined : OgDevelopmentWell.Concept;
            design.ACTOR = actor.Trim();
            design.RECORDED_AT = now;
            design.ROW_CHANGED_BY = actor.Trim();
            design.ROW_CHANGED_DATE = now;

            var updated = unit.Update(design);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(updated, "The design was not saved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadDetailAsync(source, design.DESIGN_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The design was not saved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving design {designId}",
                "the design was not saved",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The design could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> ApproveAsync(
        string designId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "An approval must record who approved it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the design cannot be approved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var design = model.Designs.FirstOrDefault(row =>
                string.Equals(row.DESIGN_ID, designId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (design is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            var well = ToWell(design, model);
            var missing = MissingItems(well);

            // AC-4: an incomplete design is refused, and the refusal names every missing item.
            if (missing.Count > 0)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"The design of {well.Name ?? well.Id} cannot be approved; it is missing: " +
                    $"{string.Join(", ", missing)}.",
                    CodeIncomplete);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_WELL_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("DESIGN_ID", design.DESIGN_ID)], cancellationToken)
                .ConfigureAwait(false);
            var current = rows.FirstOrDefault();

            if (current is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            current.DESIGN_STATE = OgDevelopmentWell.Approved;
            current.APPROVED_BY = actor.Trim();
            current.APPROVED_AT = now;
            current.ROW_CHANGED_BY = actor.Trim();
            current.ROW_CHANGED_DATE = now;

            var updated = unit.Update(current);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(updated, "The design was not approved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadDetailAsync(source, current.DESIGN_ID, cancellationToken).ConfigureAwait(false)
                : OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The design was not approved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"approving design {designId}",
                "the design was not approved",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The design could not be approved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> ReviseAsync(
        string designId,
        OgWellDesignValues values,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A revision must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A revision must record why it was made.", "well-design-reason-required");
        }

        if (Validate(values) is { } refusal)
        {
            return OgResult<OgWellDesignDetail>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the design cannot be revised.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var previous = model.Designs.FirstOrDefault(row =>
                string.Equals(row.DESIGN_ID, designId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (previous is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            var version = model.Designs
                .Where(row => string.Equals(row.SCHEDULE_ID, previous.SCHEDULE_ID, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.VERSION)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var now = DateTime.UtcNow;
            var newId = Guid.NewGuid().ToString("N");

            // The new version is written first; the previous one is then marked not current, never deleted (AC-5).
            var unit = OgUow.For<DEVELOPMENT_WELL_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new DEVELOPMENT_WELL_DESIGN
            {
                DESIGN_ID = newId,
                SCHEDULE_ID = previous.SCHEDULE_ID,
                FDP_ID = previous.FDP_ID,
                FIELD_ID = previous.FIELD_ID,
                PLANNED_WELL_ID = previous.PLANNED_WELL_ID,
                WELL_NAME = previous.WELL_NAME,
                VERSION = version,
                CURRENT_IND = "Y",
                DESIGN_STATE = OgDevelopmentWell.Defined,
                OBJECTIVE = Blank(values.Objective),
                TARGET_FORMATION = Blank(values.TargetFormation),
                TARGET_DEPTH = values.TargetDepth ?? 0m,
                DEPTH_OUOM = Blank(values.DepthUnit),
                TRAJECTORY_INTENT = Blank(values.TrajectoryIntent),
                COMPLETION_CONCEPT = Blank(values.CompletionConcept),
                RISK_TEXT = Blank(values.Risk),
                ESTIMATED_COST = values.EstimatedCost ?? 0m,
                COST_CURRENCY = Blank(values.CostCurrency)?.ToUpperInvariant(),
                SPUD_WINDOW_START = values.SpudWindowStart,
                SPUD_WINDOW_END = values.SpudWindowEnd,
                REVISION_REASON = reason.Trim(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            // The casing outline is part of the design, so the new version carries it forward and the previous
            // version's strings stay attached to the previous design.
            var casing = model.Casing
                .Where(item => string.Equals(item.DESIGN_ID, previous.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (casing.Count > 0)
            {
                var casingUnit = OgUow.For<DEVELOPMENT_CASING_STRING>(source);
                casingUnit.PrimaryKey = "CASING_STRING_ID";

                foreach (var item in casing)
                {
                    casingUnit.Add(new DEVELOPMENT_CASING_STRING
                    {
                        CASING_STRING_ID = Guid.NewGuid().ToString("N"),
                        DESIGN_ID = newId,
                        STRING_ORDER = item.STRING_ORDER,
                        STRING_NAME = item.STRING_NAME,
                        OD = item.OD,
                        WEIGHT = item.WEIGHT,
                        GRADE = item.GRADE,
                        SET_DEPTH = item.SET_DEPTH,
                        DEPTH_OUOM = item.DEPTH_OUOM,
                        ROW_CREATED_BY = actor.Trim(),
                        ROW_CREATED_DATE = now,
                        ROW_CHANGED_BY = actor.Trim(),
                        ROW_CHANGED_DATE = now,
                    });
                }

                var copiedCasing = await casingUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(copiedCasing))
                {
                    _failures.ReportHandled(
                        new InvalidOperationException(
                            $"The casing outline was not copied to design version {newId}: " +
                            OgUow.Message(copiedCasing, "the rows were not inserted")),
                        $"carrying the casing outline into version {version.ToString(CultureInfo.InvariantCulture)}",
                        "the new version is recorded but its casing outline is empty",
                        FailureSeverity.Degraded);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The revision was not created."), CodeWriteFailed);
            }

            var previousUnit = OgUow.For<DEVELOPMENT_WELL_DESIGN>(source);
            previousUnit.PrimaryKey = "DESIGN_ID";

            var rows = await OgUow.ReadAsync(
                previousUnit,
                [OgUow.Equal("DESIGN_ID", previous.DESIGN_ID)],
                cancellationToken).ConfigureAwait(false);
            var old = rows.FirstOrDefault();

            if (old is not null)
            {
                old.CURRENT_IND = "N";
                old.ROW_CHANGED_BY = actor.Trim();
                old.ROW_CHANGED_DATE = now;
                previousUnit.Update(old);

                var closed = await previousUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(closed))
                {
                    _failures.ReportHandled(
                        new InvalidOperationException(
                            $"The previous design version {previous.DESIGN_ID} was not marked not current: " +
                            OgUow.Message(closed, "the row was not updated")),
                        $"retiring the previous design version of {previous.PLANNED_WELL_ID}",
                        "the new version is recorded but the previous one still shows as current",
                        FailureSeverity.Degraded);
                }
            }

            return await ReloadDetailAsync(source, newId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"revising design {designId}",
                "the revision was not created",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The design could not be revised.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> AddCasingStringAsync(
        string designId,
        OgCasingStringRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A casing change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A casing string needs its name (conductor, surface, intermediate, production or liner).",
                CodeCasingInvalid);
        }

        if (request.SetDepth <= 0m)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A casing string needs a set depth greater than zero.", CodeCasingInvalid);
        }

        if (string.IsNullOrWhiteSpace(request.DepthUnit))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A casing set depth needs its unit.", CodeDepthUnitRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the casing string cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var design = model.Designs.FirstOrDefault(row =>
                string.Equals(row.DESIGN_ID, designId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (design is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            if (IsLocked(design.DESIGN_STATE))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"This design is {design.DESIGN_STATE}; revise it to a new version instead of editing it.",
                    CodeLocked);
            }

            var existing = model.Casing
                .Where(item => string.Equals(item.DESIGN_ID, design.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_CASING_STRING>(source);
            unit.PrimaryKey = "CASING_STRING_ID";

            unit.Add(new DEVELOPMENT_CASING_STRING
            {
                CASING_STRING_ID = Guid.NewGuid().ToString("N"),
                DESIGN_ID = design.DESIGN_ID!,
                STRING_ORDER = existing.Count == 0 ? 1 : existing.Max(item => item.STRING_ORDER) + 1,
                STRING_NAME = request.Name.Trim().ToUpperInvariant(),
                OD = request.Od,
                WEIGHT = request.Weight,
                GRADE = Blank(request.Grade)?.ToUpperInvariant(),
                SET_DEPTH = request.SetDepth,
                DEPTH_OUOM = request.DepthUnit.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadDetailAsync(source, design.DESIGN_ID!, cancellationToken).ConfigureAwait(false)
                : OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The casing string was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"adding a casing string to design {designId}",
                "the casing string was not recorded",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The casing string could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellDesignDetail>> RunCasingCheckAsync(
        string designId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "A casing check must record who ran it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                "No connection is open, so the casing check cannot run.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var design = model.Designs.FirstOrDefault(row =>
                string.Equals(row.DESIGN_ID, designId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (design is null)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    $"Design '{designId}' is not in this database.", CodeUnknown);
            }

            var well = ToWell(design, model);
            var findings = CheckCasing(well, well.Casing);

            if (well.Casing.Count == 0)
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    "No casing string is recorded, so there is nothing to check.", CodeCasingRequired);
            }

            var now = DateTime.UtcNow;
            var outcome = findings.Count == 0 ? OgCasingCheck.Passed : OgCasingCheck.Failed;

            var unit = OgUow.For<DEVELOPMENT_CASING_CHECK>(source);
            unit.PrimaryKey = "CHECK_ID";

            unit.Add(new DEVELOPMENT_CASING_CHECK
            {
                CHECK_ID = Guid.NewGuid().ToString("N"),
                DESIGN_ID = design.DESIGN_ID!,
                OUTCOME = outcome,
                FINDINGS_TEXT = findings.Count == 0
                    ? $"Inputs verified: {well.Casing.Count.ToString(CultureInfo.InvariantCulture)} string(s), " +
                      "each with a name, an OD, a grade and a set depth in " + (well.DepthUnit ?? "the design's unit") +
                      "; set depths increase with the string order and none is below the target depth."
                    : string.Join("\n", findings),
                CHECKED_BY = actor.Trim(),
                CHECKED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellDesignDetail>.Failed(
                    OgUow.Message(committed, "The casing check was not recorded."), CodeWriteFailed);
            }

            var reloaded = await ReloadDetailAsync(source, design.DESIGN_ID!, cancellationToken)
                .ConfigureAwait(false);

            if (!reloaded.Success || reloaded.Value is null)
            {
                return reloaded;
            }

            // A failing check is a recorded result, not an unrecorded operation: the detail comes back with it.
            return OgResult<OgWellDesignDetail>.Ok(reloaded.Value);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"running the casing check on design {designId}",
                "the check was not recorded",
                FailureSeverity.Error);

            return OgResult<OgWellDesignDetail>.Failed(
                "The casing check could not run.", reference ?? CodeWriteFailed);
        }
    }

    /// <summary>The shared value rules; stated once so the page, the service and the tests agree.</summary>
    public static (string Code, string Message)? Validate(OgWellDesignValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        // AC-2: a target depth without its unit is refused, not stored as a bare number.
        if (values.TargetDepth is > 0m && string.IsNullOrWhiteSpace(values.DepthUnit))
        {
            return (CodeDepthUnitRequired,
                "A target depth needs the unit it is measured in (for example m or ft).");
        }

        if (values.TargetDepth is < 0m)
        {
            return (CodeDepthInvalid, "A target depth cannot be negative.");
        }

        if (values.EstimatedCost is < 0m)
        {
            return (CodeCostInvalid, "An estimated cost cannot be negative.");
        }

        if (values.EstimatedCost is > 0m && string.IsNullOrWhiteSpace(values.CostCurrency))
        {
            return (CodeCurrencyRequired, "An estimated cost needs the currency it is in.");
        }

        if (values.SpudWindowStart is { } start && values.SpudWindowEnd is { } end && end < start)
        {
            return (CodeWindowOrder,
                $"The spud window ends {end:yyyy-MM-dd}, before it starts on {start:yyyy-MM-dd}.");
        }

        return null;
    }

    private static bool IsLocked(string? state) =>
        string.Equals(state, OgDevelopmentWell.Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(state, OgDevelopmentWell.Drilled, StringComparison.OrdinalIgnoreCase);

    private static bool HasAnyValue(DEVELOPMENT_WELL_DESIGN design) =>
        !string.IsNullOrWhiteSpace(design.OBJECTIVE)
        || !string.IsNullOrWhiteSpace(design.TARGET_FORMATION)
        || design.TARGET_DEPTH > 0
        || !string.IsNullOrWhiteSpace(design.TRAJECTORY_INTENT)
        || !string.IsNullOrWhiteSpace(design.COMPLETION_CONCEPT)
        || !string.IsNullOrWhiteSpace(design.RISK_TEXT)
        || design.ESTIMATED_COST > 0;

    // ------------------------------------------------------------------ reads

    private static async Task<DesignModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var designs = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_DESIGN>(source).ConfigureAwait(false);
        var casing = await OgUow.ReadAllAsync<DEVELOPMENT_CASING_STRING>(source).ConfigureAwait(false);
        var checks = await OgUow.ReadAllAsync<DEVELOPMENT_CASING_CHECK>(source).ConfigureAwait(false);
        var schedules = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_SCHEDULE>(source).ConfigureAwait(false);
        var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new DesignModel(designs, casing, checks, schedules, plans);
    }

    private static OgDevelopmentWell ToWell(DEVELOPMENT_WELL_DESIGN design, DesignModel model) =>
        ToWell(design, model.Casing, model.Checks);

    /// <summary>
    /// The DEV-03 view of a design built from its own rows, so another screen (DEV-05) applies the same completeness
    /// rule rather than a second copy of it.
    /// </summary>
    public static OgDevelopmentWell ToWell(
        DEVELOPMENT_WELL_DESIGN design,
        IReadOnlyList<DEVELOPMENT_CASING_STRING> allCasing,
        IReadOnlyList<DEVELOPMENT_CASING_CHECK> allChecks)
    {
        ArgumentNullException.ThrowIfNull(design);
        ArgumentNullException.ThrowIfNull(allCasing);
        ArgumentNullException.ThrowIfNull(allChecks);

        var casing = allCasing
            .Where(item => string.Equals(item.DESIGN_ID, design.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.STRING_ORDER)
            .Select(item => new OgCasingString(
                item.STRING_ORDER,
                Blank(item.STRING_NAME) ?? "(unnamed)",
                item.OD,
                item.WEIGHT,
                Blank(item.GRADE),
                item.SET_DEPTH,
                Blank(item.DEPTH_OUOM)))
            .ToList();

        var check = allChecks
            .Where(item => string.Equals(item.DESIGN_ID, design.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.CHECKED_AT)
            .Select(item => new OgCasingCheck(
                item.CHECK_ID ?? string.Empty,
                item.DESIGN_ID ?? string.Empty,
                Blank(item.OUTCOME) ?? "UNKNOWN",
                Split(item.FINDINGS_TEXT),
                Blank(item.CHECKED_BY),
                item.CHECKED_AT))
            .FirstOrDefault();

        return new OgDevelopmentWell(
            design.DESIGN_ID ?? string.Empty,
            design.FDP_ID ?? string.Empty,
            design.FIELD_ID ?? string.Empty,
            design.SCHEDULE_ID ?? string.Empty,
            design.PLANNED_WELL_ID ?? string.Empty,
            Blank(design.WELL_NAME) ?? Blank(design.PLANNED_WELL_ID),
            design.VERSION,
            !string.Equals(design.CURRENT_IND, "N", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrWhiteSpace(design.DESIGN_STATE)
                ? OgDevelopmentWell.Concept
                : design.DESIGN_STATE.Trim().ToUpperInvariant(),
            Blank(design.OBJECTIVE),
            Blank(design.TARGET_FORMATION),
            design.TARGET_DEPTH > 0 ? design.TARGET_DEPTH : null,
            Blank(design.DEPTH_OUOM),
            Blank(design.TRAJECTORY_INTENT),
            Blank(design.COMPLETION_CONCEPT),
            Blank(design.RISK_TEXT),
            design.ESTIMATED_COST > 0 ? design.ESTIMATED_COST : null,
            Blank(design.COST_CURRENCY),
            design.SPUD_WINDOW_START,
            design.SPUD_WINDOW_END,
            Blank(design.APPROVED_BY),
            design.APPROVED_AT,
            Blank(design.REVISION_REASON),
            casing,
            check);
    }

    private async Task<OgResult<OgWellDesignDetail>> ReloadDetailAsync(
        IDataSource source,
        string designId,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
        var design = model.Designs.FirstOrDefault(row =>
            string.Equals(row.DESIGN_ID, designId, StringComparison.OrdinalIgnoreCase));

        if (design is null)
        {
            return OgResult<OgWellDesignDetail>.Failed(
                $"Design '{designId}' is not in this database.", CodeUnknown);
        }

        var current = ToWell(design, model);

        var versions = model.Designs
            .Where(row => string.Equals(row.SCHEDULE_ID, design.SCHEDULE_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.VERSION)
            .Select(row => ToWell(row, model))
            .ToList();

        return OgResult<OgWellDesignDetail>.Ok(
            new OgWellDesignDetail(current, versions, current.LastCheck));
    }

    private static IReadOnlyList<string> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record DesignModel(
        IReadOnlyList<DEVELOPMENT_WELL_DESIGN> Designs,
        IReadOnlyList<DEVELOPMENT_CASING_STRING> Casing,
        IReadOnlyList<DEVELOPMENT_CASING_CHECK> Checks,
        IReadOnlyList<DEVELOPMENT_WELL_SCHEDULE> Schedules,
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans);
}
