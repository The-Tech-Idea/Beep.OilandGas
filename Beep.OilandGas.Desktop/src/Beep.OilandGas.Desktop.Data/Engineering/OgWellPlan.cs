using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One section of an explicit plan: a hold, or an arc at a dogleg in a toolface direction (ENG-06).</summary>
public sealed record OgPlanSection(double Length, double Dogleg, double Toolface)
{
    public bool IsHold => Dogleg <= 0d;

    public string Describe(double courseLength) =>
        IsHold
            ? "hold " + Length.ToString("0.#", CultureInfo.InvariantCulture)
            : "curve " + Dogleg.ToString("0.##", CultureInfo.InvariantCulture) + " deg/" +
              courseLength.ToString("0.#", CultureInfo.InvariantCulture) + " at toolface " +
              Toolface.ToString("0.#", CultureInfo.InvariantCulture) + " for " +
              Length.ToString("0.#", CultureInfo.InvariantCulture);
}

/// <summary>The design request: the tie-on, the target or the sections, and the method (ENG-06).</summary>
public sealed record OgPlanRequest(
    string WellId,
    string? Field,
    string? PlanName,
    string Method,
    string Unit,
    double TieOnMd,
    double TieOnNorth,
    double TieOnEast,
    double TieOnTvd,
    double TieOnInclination,
    double TieOnAzimuth,
    double? TargetNorth,
    double? TargetEast,
    double? TargetTvd,
    double TargetTolerance,
    double Dogleg,
    IReadOnlyList<OgPlanSection> Sections);

/// <summary>One station of the plan (ENG-06's <c>OgPlanStation</c>), with the build and turn it implies.</summary>
public sealed record OgPlanStation(
    int Index,
    double Md,
    double Inclination,
    double Azimuth,
    double Tvd,
    double North,
    double East,
    double Dogleg,
    string Section,
    double Build,
    double Turn)
{
    public string Describe(double courseLength) =>
        Md.ToString("0.#", CultureInfo.InvariantCulture) + " · inc " +
        Inclination.ToString("0.##", CultureInfo.InvariantCulture) + "° · az " +
        Azimuth.ToString("0.##", CultureInfo.InvariantCulture) + "° · TVD " +
        Tvd.ToString("0.#", CultureInfo.InvariantCulture) + " · N " +
        North.ToString("0.#", CultureInfo.InvariantCulture) + " · E " +
        East.ToString("0.#", CultureInfo.InvariantCulture) + " · dogleg " +
        Dogleg.ToString("0.##", CultureInfo.InvariantCulture) + " · " + Section;
}

/// <summary>A view path point: X across, Y down or along (ENG-06's plan and section views).</summary>
public sealed record OgPlanPoint(double X, double Y);

/// <summary>A computed plan and its checks (ENG-06).</summary>
public sealed record OgWellPlanWorkspace(
    string WellId,
    string? Field,
    string? PlanName,
    string Method,
    string Unit,
    double CourseLength,
    double TieOnMd,
    double TieOnNorth,
    double TieOnEast,
    double TieOnTvd,
    double TieOnInclination,
    double TieOnAzimuth,
    double? TargetNorth,
    double? TargetEast,
    double? TargetTvd,
    double TargetTolerance,
    double Dogleg,
    IReadOnlyList<string> Sections,
    IReadOnlyList<OgPlanStation> Stations,
    double? FinalMd,
    double? FinalNorth,
    double? FinalEast,
    double? FinalTvd,
    double? FinalInclination,
    double? FinalAzimuth,
    double? Miss,
    bool TargetHit,
    double? MaxStationDogleg,
    double? DoglegLimit,
    bool DoglegWithinLimit,
    double? Tortuosity,
    string TortuosityBasis,
    IReadOnlyList<string> Warnings,
    string? PlanId,
    DateTime? SavedAt,
    string? SavedBy,
    DateTime? HandedOffAt,
    string? HandedOffBy,
    WellModel? Trajectory,
    DateTime ReadAt)
{
    public bool HasTarget => TargetNorth is not null && TargetEast is not null && TargetTvd is not null;

    public string SummaryText =>
        Stations.Count.ToString(CultureInfo.InvariantCulture) + " station(s) · final MD " +
        (FinalMd?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?") + " " + Unit +
        " · TVD " + (FinalTvd?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?") + " " + Unit +
        " · N " + (FinalNorth?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?") +
        " · E " + (FinalEast?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?");

    public string TargetText => !HasTarget
        ? "No target: the plan was built from its sections, so there is nothing to hit."
        : TargetHit
            ? "On target: the plan ends " + (Miss ?? 0d).ToString("0.###", CultureInfo.InvariantCulture) + " " + Unit +
              " from (" + TargetNorth!.Value.ToString("0.#", CultureInfo.InvariantCulture) + ", " +
              TargetEast!.Value.ToString("0.#", CultureInfo.InvariantCulture) + ", " +
              TargetTvd!.Value.ToString("0.#", CultureInfo.InvariantCulture) + ") against a tolerance of " +
              TargetTolerance.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + "."
            : "Off target by " + (Miss ?? 0d).ToString("0.##", CultureInfo.InvariantCulture) + " " + Unit +
              ", beyond the " + TargetTolerance.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit +
              " tolerance.";

    public string DoglegText => DoglegLimit is not { } limit
        ? "No dogleg limit is configured (drilling.maxDogleg is unset), and the largest station dogleg is " +
          (MaxStationDogleg?.ToString("0.##", CultureInfo.InvariantCulture) ?? "?") + " " +
          TheTechIdea.OilGas.Wellbore.Data.Trajectory.DoglegCourseLabel(Unit) + "."
        : "Largest station dogleg " + (MaxStationDogleg?.ToString("0.##", CultureInfo.InvariantCulture) ?? "?") + " " +
          TheTechIdea.OilGas.Wellbore.Data.Trajectory.DoglegCourseLabel(Unit) + " against a limit of " +
          limit.ToString("0.##", CultureInfo.InvariantCulture) + " - " +
          (DoglegWithinLimit ? "within it." : "above it.");

    public string TortuosityText => Tortuosity is { } value
        ? value.ToString("0.###", CultureInfo.InvariantCulture) + " · " + TortuosityBasis
        : TortuosityBasis;

    public string WarningsText => Warnings.Count == 0
        ? "The plan raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string SavedText => PlanId is null
        ? "The plan is not saved, so there is nothing to hand off."
        : "saved " + (SavedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "at an unrecorded time") +
          " by " + (SavedBy ?? "an unrecorded person") +
          (HandedOffAt is { } handed
              ? " · handed off " + handed.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) +
                " by " + (HandedOffBy ?? "an unrecorded person")
              : " · not handed off");

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The well-plan rules ENG-06 states once.</summary>
public static class WellPlanRules
{
    public const string CodeNoConnection = "well-plan-no-connection";
    public const string CodeReadFailed = "well-plan-read-failed";
    public const string CodeWriteFailed = "well-plan-write-failed";
    public const string CodeWellRequired = "well-plan-well-required";
    public const string CodeActorRequired = "well-plan-actor-required";
    public const string CodeMethodUnknown = "well-plan-method-unknown";
    public const string CodeUnitRequired = "well-plan-unit-required";
    public const string CodeDatumMissing = "well-plan-datum-missing";
    public const string CodeTargetMissing = "well-plan-target-missing";
    public const string CodeNoDesign = "well-plan-no-design";
    public const string CodeMixedUnits = "well-plan-mixed-units";
    public const string CodeUnreachable = "well-plan-unreachable";
    public const string CodeUnknownPlan = "well-plan-unknown";
    public const string CodeHandoffBeforeSave = "well-plan-handoff-before-save";

    public const string BuildAndHold = "BUILD_AND_HOLD";
    public const string Explicit = "EXPLICIT";

    /// <summary>The methods this build offers, and why the other two the feature names are not among them.</summary>
    public static IReadOnlyList<string> Methods { get; } = [BuildAndHold, Explicit];

    public const string NotOffered =
        "Only BUILD_AND_HOLD and EXPLICIT are offered: the owner's plan is a sequence of straight holds and circular arcs, and " +
        "its one target solver curves then holds. A J-shape is that same solver with the target at TD, a horizontal well is the " +
        "same solver with a target at the landing TVD, and an S-shape needs a drop-back solve the package does not provide - so " +
        "the method list would promise a design the maths cannot produce.";

    /// <summary>AC-2: the dogleg a station shows, compared with the configured limit.</summary>
    public static bool WithinLimit(double? largest, double? limit) => limit is not { } max || largest is not { } dls || dls <= max + 1e-9;

    /// <summary>
    /// The tortuosity figure, with its definition stated: the plan's measured length over the straight-line distance from the tie-on
    /// to its last station, less one.
    /// </summary>
    public static (double? Value, string Basis) TortuosityOf(
        double tieOnMd,
        double tieOnNorth,
        double tieOnEast,
        double tieOnTvd,
        double finalMd,
        double finalNorth,
        double finalEast,
        double finalTvd,
        string unit)
    {
        var dn = finalNorth - tieOnNorth;
        var de = finalEast - tieOnEast;
        var dt = finalTvd - tieOnTvd;
        var straight = Math.Sqrt(dn * dn + de * de + dt * dt);
        var length = finalMd - tieOnMd;

        const string basis =
            "tortuosity = the plan's measured length over the straight-line distance from the tie-on to the last station, less " +
            "one; zero is a straight hole.";

        return straight <= 1e-9 || length <= 1e-9 ? (null, basis) : (length / straight - 1d, basis);
    }

    /// <summary>
    /// AC-4: a section may carry a unit suffix, and a suffix that is not the plan's unit is refused rather than converted.
    /// </summary>
    public static (OgPlanSection? Section, string? Problem) ParseSection(string text, string unit, double courseLength)
    {
        var parts = (text ?? string.Empty)
            .Split([',', ';', ' '], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (parts.Count == 0)
        {
            return (null, "an empty section");
        }

        var kind = parts[0].ToLowerInvariant();
        var values = parts.Skip(1).ToList();
        var suffix = values.LastOrDefault(value => value.Any(char.IsLetter) && !value.All(char.IsDigit));

        if (suffix is not null &&
            !string.Equals(suffix, unit, StringComparison.OrdinalIgnoreCase))
        {
            return (null, "the section is written in '" + suffix + "' while the plan is in '" + unit +
                          "', so the units are mixed and it is refused");
        }

        var numbers = values
            .Where(value => !ReferenceEquals(value, suffix))
            .Select(TryNumber)
            .ToList();

        if (numbers.Any(number => number is null))
        {
            return (null, "a section value is not a number");
        }

        var figures = numbers.Select(number => number!.Value).ToList();

        if (kind.StartsWith("hold", StringComparison.Ordinal))
        {
            return figures.Count != 1 || figures[0] <= 0d
                ? (null, "a hold is 'hold <length>' with a positive length")
                : (new OgPlanSection(figures[0], 0d, 0d), null);
        }

        if (kind.StartsWith("curve", StringComparison.Ordinal))
        {
            return figures.Count != 3 || figures[0] <= 0d || figures[2] <= 0d
                ? (null, "a curve is 'curve <dogleg>, <toolface>, <length>' with a positive dogleg and length")
                : (new OgPlanSection(figures[2], figures[0], figures[1]), null);
        }

        return (null, "'" + kind + "' is not a section; use 'hold <length>' or 'curve <dogleg>, <toolface>, <length>'");
    }

    private static double? TryNumber(string text) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;
}

/// <summary>ENG-06: design a directional well plan, save it and hand its trajectory on.</summary>
public interface IOgWellPlan
{
    /// <summary>Designs a plan without writing anything.</summary>
    Task<OgResult<OgWellPlanWorkspace>> DesignAsync(
        OgPlanRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Designs the plan and records it (AC-6).</summary>
    Task<OgResult<OgWellPlanWorkspace>> SaveAsync(
        OgPlanRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the well's most recent saved plan and recomputes its stations.</summary>
    Task<OgResult<OgWellPlanWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default);

    /// <summary>Records that the computed trajectory was handed to torque and drag (AC-5).</summary>
    Task<OgResult<OgWellPlanWorkspace>> HandoffAsync(
        string planId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-06 over the DrillingAndConstruction-owned <c>WELL_PLAN</c>, using the tools' <see cref="WellPlan"/> for the geometry and
/// <see cref="Trajectory"/> for the independent check and the handoff model.
/// </summary>
/// <remarks>
/// <para>
/// The stations are the owner's: <see cref="WellPlan.Stations"/> interpolates every section exactly, and <see cref="Trajectory"/>
/// recomputes the same path by minimum curvature, so the plan view, the section view and the dogleg column all come from one
/// station list and cannot disagree with the table.
/// </para>
/// <para>
/// Nothing is stored but the design inputs and the outcome: the stations are reproducible from them, and the handoff passes the
/// computed <see cref="WellModel"/> rather than a copy of the record.
/// </para>
/// </remarks>
public sealed class OgWellPlanService : IOgWellPlan
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellPlanService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public Task<OgResult<OgWellPlanWorkspace>> DesignAsync(
        OgPlanRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Design(request, null, null, null));

    public async Task<OgResult<OgWellPlanWorkspace>> SaveAsync(
        OgPlanRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "A saved plan records who saved it.", WellPlanRules.CodeActorRequired);
        }

        var designed = Design(request, null, null, null);

        if (!designed.Success || designed.Value is null)
        {
            return designed;
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "No connection is open, so the plan cannot be saved.", WellPlanRules.CodeNoConnection);
        }

        try
        {
            var plan = designed.Value;
            var planId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var unit = OgUow.For<WELL_PLAN>(source);
            unit.PrimaryKey = nameof(WELL_PLAN.PLAN_ID);

            unit.Add(new WELL_PLAN
            {
                PLAN_ID = planId,
                UWI = plan.WellId,
                FIELD_NAME = plan.Field,
                PLAN_NAME = plan.PlanName,
                DESIGN_METHOD = plan.Method,
                DEPTH_OUOM = plan.Unit,
                TIE_ON_MD = (decimal)plan.TieOnMd,
                TIE_ON_NORTH = (decimal)plan.TieOnNorth,
                TIE_ON_EAST = (decimal)plan.TieOnEast,
                TIE_ON_TVD = (decimal)plan.TieOnTvd,
                TIE_ON_INCLINATION = (decimal)request.TieOnInclination,
                TIE_ON_AZIMUTH = (decimal)request.TieOnAzimuth,
                TARGET_NORTH = plan.TargetNorth is { } north ? (decimal)north : null,
                TARGET_EAST = plan.TargetEast is { } east ? (decimal)east : null,
                TARGET_TVD = plan.TargetTvd is { } tvd ? (decimal)tvd : null,
                TARGET_TOLERANCE = (decimal)plan.TargetTolerance,
                DESIGN_DOGLEG = (decimal)plan.Dogleg,
                MAX_DOGLEG = plan.DoglegLimit is { } limit ? (decimal)limit : null,
                SECTIONS_TEXT = request.Sections.Count == 0
                    ? null
                    : JsonSerializer.Serialize(request.Sections),
                STATION_COUNT = plan.Stations.Count,
                FINAL_MD = plan.FinalMd is { } finalMd ? (decimal)finalMd : null,
                FINAL_NORTH = plan.FinalNorth is { } finalNorth ? (decimal)finalNorth : null,
                FINAL_EAST = plan.FinalEast is { } finalEast ? (decimal)finalEast : null,
                FINAL_TVD = plan.FinalTvd is { } finalTvd ? (decimal)finalTvd : null,
                FINAL_INCLINATION = plan.FinalInclination is { } inclination ? (decimal)inclination : null,
                FINAL_AZIMUTH = plan.FinalAzimuth is { } azimuth ? (decimal)azimuth : null,
                MISS_DISTANCE = plan.Miss is { } miss ? (decimal)miss : null,
                TARGET_HIT = plan.HasTarget ? (plan.TargetHit ? "Y" : "N") : null,
                MAX_STATION_DOGLEG = plan.MaxStationDogleg is { } max ? (decimal)max : null,
                TORTUOSITY = plan.Tortuosity is { } tortuosity ? (decimal)tortuosity : null,
                TORTUOSITY_BASIS = plan.TortuosityBasis,
                SAVED_BY = actor.Trim(),
                SAVED_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellPlanWorkspace>.Failed(
                    OgUow.Message(committed, "The plan was not saved."), WellPlanRules.CodeWriteFailed);
            }

            return OgResult<OgWellPlanWorkspace>.Ok(plan with
            {
                PlanId = planId,
                SavedAt = now,
                SavedBy = actor.Trim(),
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the well plan for " + request.WellId, "the plan is not saved", FailureSeverity.Error);

            return OgResult<OgWellPlanWorkspace>.Failed(
                "The plan could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPlanRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellPlanWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "A well plan belongs to one well; none is named.", WellPlanRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "No connection is open, so the plan cannot be read.", WellPlanRules.CodeNoConnection);
        }

        try
        {
            var plans = await OgUow.ReadAllAsync<WELL_PLAN>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var saved = plans
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.UWI?.Trim(), wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.SAVED_AT_UTC)
                .FirstOrDefault();

            if (saved is null)
            {
                return OgResult<OgWellPlanWorkspace>.Failed(
                    "No well plan is saved for " + wellId.Trim() + ".", WellPlanRules.CodeUnknownPlan);
            }

            var request = new OgPlanRequest(
                saved.UWI,
                saved.FIELD_NAME,
                saved.PLAN_NAME,
                saved.DESIGN_METHOD,
                string.IsNullOrWhiteSpace(saved.DEPTH_OUOM) ? "m" : saved.DEPTH_OUOM!,
                (double)saved.TIE_ON_MD,
                (double)saved.TIE_ON_NORTH,
                (double)saved.TIE_ON_EAST,
                (double)saved.TIE_ON_TVD,
                (double)saved.TIE_ON_INCLINATION,
                (double)saved.TIE_ON_AZIMUTH,
                saved.TARGET_NORTH is { } north ? (double)north : null,
                saved.TARGET_EAST is { } east ? (double)east : null,
                saved.TARGET_TVD is { } tvd ? (double)tvd : null,
                (double)saved.TARGET_TOLERANCE,
                (double)saved.DESIGN_DOGLEG,
                SectionsOf(saved.SECTIONS_TEXT));

            var redesigned = Design(request, saved.PLAN_ID, saved.SAVED_AT_UTC, saved.SAVED_BY);

            if (!redesigned.Success || redesigned.Value is null)
            {
                return redesigned;
            }

            return OgResult<OgWellPlanWorkspace>.Ok(redesigned.Value with
            {
                HandedOffAt = saved.HANDED_OFF_AT_UTC,
                HandedOffBy = saved.HANDED_OFF_BY,
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the well plan for " + wellId, "the plan is not shown", FailureSeverity.Error);

            return OgResult<OgWellPlanWorkspace>.Failed(
                "The plan could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPlanRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellPlanWorkspace>> HandoffAsync(
        string planId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "A handoff records who made it.", WellPlanRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(planId))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "A handoff names the saved plan it passes on.", WellPlanRules.CodeHandoffBeforeSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "No connection is open, so the handoff cannot be recorded.", WellPlanRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELL_PLAN>(source);
            unit.PrimaryKey = nameof(WELL_PLAN.PLAN_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(WELL_PLAN.PLAN_ID), planId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWellPlanWorkspace>.Failed(
                    "Plan '" + planId + "' is not saved, so there is no trajectory to hand off.",
                    WellPlanRules.CodeHandoffBeforeSave);
            }

            var now = DateTime.UtcNow;

            row.HANDED_OFF_AT_UTC = now;
            row.HANDED_OFF_BY = actor.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWellPlanWorkspace>.Failed(
                    OgUow.Message(updated, "The handoff was not recorded."), WellPlanRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellPlanWorkspace>.Failed(
                    OgUow.Message(committed, "The handoff was not recorded."), WellPlanRules.CodeWriteFailed);
            }

            return await LoadAsync(row.UWI, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording the handoff of plan " + planId, "the handoff is not recorded", FailureSeverity.Error);

            return OgResult<OgWellPlanWorkspace>.Failed(
                "The handoff could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPlanRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the design

    private OgResult<OgWellPlanWorkspace> Design(
        OgPlanRequest request,
        string? planId,
        DateTime? savedAt,
        string? savedBy)
    {
        if (string.IsNullOrWhiteSpace(request.WellId))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "A plan is designed for one well; none is named.", WellPlanRules.CodeWellRequired);
        }

        // The feature's state: a missing start datum is refused, and the datum is the unit plus the tie-on.
        if (string.IsNullOrWhiteSpace(request.Unit))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "The plan records no start datum: its unit is blank, so no length can be read.",
                WellPlanRules.CodeUnitRequired);
        }

        var unit = request.Unit.Trim();

        if (!double.IsFinite(request.TieOnMd) || !double.IsFinite(request.TieOnNorth) ||
            !double.IsFinite(request.TieOnEast) || !double.IsFinite(request.TieOnTvd))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "The tie-on is not a finite position, so the plan has no start datum.",
                WellPlanRules.CodeDatumMissing);
        }

        var method = (request.Method ?? string.Empty).Trim().ToUpperInvariant();

        if (!WellPlanRules.Methods.Contains(method, StringComparer.Ordinal))
        {
            return OgResult<OgWellPlanWorkspace>.Failed(
                "'" + (request.Method ?? "(none)") + "' is not a design method this build offers. " + WellPlanRules.NotOffered,
                WellPlanRules.CodeMethodUnknown);
        }

        var courseLength = Trajectory.DoglegCourse(unit);

        WellPlan plan;

        try
        {
            plan = WellPlan.Start(unit, request.TieOnMd, request.TieOnNorth, request.TieOnEast, request.TieOnTvd,
                request.TieOnInclination, request.TieOnAzimuth);

            if (string.Equals(method, WellPlanRules.BuildAndHold, StringComparison.Ordinal))
            {
                if (request.TargetNorth is not { } north || request.TargetEast is not { } east ||
                    request.TargetTvd is not { } tvd)
                {
                    return OgResult<OgWellPlanWorkspace>.Failed(
                        "A build-and-hold plan is designed to a target, and none is recorded.",
                        WellPlanRules.CodeTargetMissing);
                }

                if (request.Dogleg <= 0d)
                {
                    return OgResult<OgWellPlanWorkspace>.Failed(
                        "A build-and-hold plan needs a positive dogleg severity; " +
                        request.Dogleg.ToString("0.##", CultureInfo.InvariantCulture) + " cannot turn the hole.",
                        WellPlanRules.CodeNoDesign);
                }

                plan.CurveHoldToTarget(request.Dogleg, north, east, tvd);
            }
            else
            {
                if (request.Sections.Count == 0)
                {
                    return OgResult<OgWellPlanWorkspace>.Failed(
                        "An explicit plan needs at least one section; none was given.", WellPlanRules.CodeNoDesign);
                }

                foreach (var section in request.Sections)
                {
                    if (section.IsHold)
                    {
                        plan.Hold(section.Length);
                    }
                    else
                    {
                        plan.Curve(section.Dogleg, section.Toolface, section.Length);
                    }
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            // The owner's solver refuses an unreachable target as an ordinary planning answer; it is reported so the sentence
            // reaches the person, and returned as a refusal rather than an exception on the page.
            var reference = _failures.ReportHandled(
                ex, "designing the plan for " + request.WellId,
                "the target cannot be reached from this tie-on and dogleg", FailureSeverity.Degraded);

            return OgResult<OgWellPlanWorkspace>.Failed(
                "The target cannot be reached from this tie-on at " +
                request.Dogleg.ToString("0.##", CultureInfo.InvariantCulture) +
                " deg per course length: an arc and a hold cannot land on it, which means the target is behind the bit or " +
                "inside the turning circle. Use a higher dogleg or a different approach." +
                (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")."),
                WellPlanRules.CodeUnreachable);
        }

        var stations = plan.Stations();
        var trajectory = Trajectory.Compute(
            stations.Select(station => (station.Md, station.Inclination, station.Azimuth)),
            unit);

        // AC-2: the dogleg each station actually shows, from the minimum-curvature walk.
        var rows = stations
            .Select((station, index) =>
            {
                var previous = index == 0 ? null : stations[index - 1];
                var dmd = previous is null ? 0d : station.Md - previous.Md;
                var dInc = previous is null ? 0d : station.Inclination - previous.Inclination;
                var dAz = previous is null ? 0d : WrapAzimuth(station.Azimuth - previous.Azimuth);

                return new OgPlanStation(
                    index + 1,
                    station.Md,
                    station.Inclination,
                    station.Azimuth,
                    station.Tvd,
                    station.North,
                    station.East,
                    index < trajectory.Stations.Count ? trajectory.Stations[index].Dls : station.Dls,
                    station.Section,
                    dmd > 1e-9 ? dInc / dmd * courseLength : 0d,
                    dmd > 1e-9 ? dAz / dmd * courseLength : 0d);
            })
            .ToList();

        var last = rows[^1];
        var limit = DrillingToolbox.Settings.MaxDogleg;
        var largest = rows.Max(row => row.Dogleg);

        double? miss = null;
        var hit = false;

        if (request.TargetNorth is { } targetNorth && request.TargetEast is { } targetEast && request.TargetTvd is { } targetTvd)
        {
            miss = Distance(last.North - targetNorth, last.East - targetEast, last.Tvd - targetTvd);
            hit = miss <= Math.Max(0d, request.TargetTolerance);
        }

        var (tortuosity, basis) = WellPlanRules.TortuosityOf(
            request.TieOnMd, request.TieOnNorth, request.TieOnEast, request.TieOnTvd,
            last.Md, last.North, last.East, last.Tvd, unit);

        var warnings = plan.Warnings.ToList();

        if (!WellPlanRules.WithinLimit(largest, limit))
        {
            warnings.Add("the largest station dogleg " + largest.ToString("0.##", CultureInfo.InvariantCulture) +
                         " is above drilling.maxDogleg " + (limit?.ToString("0.##", CultureInfo.InvariantCulture) ?? "?"));
        }

        if (request.TargetNorth is not null && !hit && miss is { } offTarget)
        {
            warnings.Add("the plan ends " + offTarget.ToString("0.##", CultureInfo.InvariantCulture) + " " + unit +
                         " from the target, beyond its tolerance");
        }

        return OgResult<OgWellPlanWorkspace>.Ok(new OgWellPlanWorkspace(
            request.WellId.Trim(),
            request.Field,
            request.PlanName,
            method,
            unit,
            courseLength,
            request.TieOnMd,
            request.TieOnNorth,
            request.TieOnEast,
            request.TieOnTvd,
            request.TieOnInclination,
            request.TieOnAzimuth,
            request.TargetNorth,
            request.TargetEast,
            request.TargetTvd,
            request.TargetTolerance,
            request.Dogleg,
            request.Sections.Select(section => section.Describe(courseLength)).ToList(),
            rows,
            last.Md,
            last.North,
            last.East,
            last.Tvd,
            last.Inclination,
            last.Azimuth,
            miss,
            hit,
            largest,
            limit,
            WellPlanRules.WithinLimit(largest, limit),
            tortuosity,
            basis,
            warnings,
            planId,
            savedAt,
            savedBy,
            null,
            null,
            plan.ToWellModel(new WellHeader { Name = request.WellId.Trim() }),
            DateTime.UtcNow));
    }

    private static double WrapAzimuth(double degrees)
    {
        var wrapped = degrees % 360d;

        return wrapped > 180d ? wrapped - 360d : wrapped < -180d ? wrapped + 360d : wrapped;
    }

    private static double Distance(double north, double east, double tvd) =>
        Math.Sqrt(north * north + east * east + tvd * tvd);

    private IReadOnlyList<OgPlanSection> SectionsOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<OgPlanSection>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            // A stored section list that no longer parses leaves an explicit plan with nothing to build; the caller then refuses
            // it with "needs at least one section", and the malformed record is reported rather than shown as a half-read plan.
            _failures.ReportHandled(
                ex, "reading the recorded plan sections", "the plan's sections are not read",
                FailureSeverity.Degraded);

            return [];
        }
    }
}
