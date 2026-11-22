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

/// <summary>One component of the drill string, listed from the bit up (ENG-07).</summary>
/// <param name="Od">Outside diameter in inches - the torque radius the owner's model uses.</param>
/// <param name="Id">Inside diameter in inches, which gives the wall the pipe-body yield is computed at.</param>
/// <param name="Grade">Steel grade, so the component's limit is the owner's API 5C3 pipe-body yield.</param>
public sealed record OgDrillStringComponent(
    string Name,
    double Od,
    double Id,
    double WeightPerLength,
    double Length,
    string? Grade)
{
    public double Wall => Od > Id && Id > 0d ? (Od - Id) / 2d : 0d;

    public string Describe(string unit, string weightUnit) =>
        Name + " · " + Od.ToString("0.###", CultureInfo.InvariantCulture) + "\" x " +
        Id.ToString("0.###", CultureInfo.InvariantCulture) + "\" · " +
        WeightPerLength.ToString("0.##", CultureInfo.InvariantCulture) + " " + weightUnit + " · " +
        Length.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + " · " + (Grade ?? "no grade");
}

/// <summary>The inputs of a torque-and-drag run (ENG-07).</summary>
public sealed record OgTorqueDragRequest(
    string WellId,
    string Scenario,
    double CasedFriction,
    double OpenHoleFriction,
    double? ShoeDepth,
    double? MudDensity,
    double ElementLength,
    double? WeightOnBitKn,
    double BitTensionKn,
    double BitTorqueKnm,
    IReadOnlyList<OgDrillStringComponent> String);

/// <summary>One station of the run (ENG-07's <c>OgTorqueDragResult.Rows</c>).</summary>
public sealed record OgTorqueDragRow(
    int Index,
    double Md,
    double Inclination,
    double Azimuth,
    double LoadKn,
    double TorqueKnm,
    double DragKn,
    double SideForceKn,
    double LimitKn,
    string Interval,
    string Component,
    bool OverLimit)
{
    public string Describe(string unit) =>
        Md.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + " · inc " +
        Inclination.ToString("0.##", CultureInfo.InvariantCulture) + "° · load " +
        LoadKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN · torque " +
        TorqueKnm.ToString("0.##", CultureInfo.InvariantCulture) + " kN·m · drag " +
        DragKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN · limit " +
        LimitKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN · " + Interval +
        (string.IsNullOrWhiteSpace(Component) ? string.Empty : " · " + Component) +
        (OverLimit ? " · ABOVE ITS LIMIT" : string.Empty);
}

/// <summary>One plotted point of the load chart (ENG-07).</summary>
public sealed record OgTorqueDragChartPoint(double Md, double LoadKn, double LimitKn);

/// <summary>A computed run and its result (ENG-07).</summary>
public sealed record OgTorqueDragWorkspace(
    string WellId,
    string? PlanId,
    string Scenario,
    string Unit,
    double CasedFriction,
    double OpenHoleFriction,
    double? ShoeDepth,
    string ShoeText,
    double? MudDensity,
    double ElementLength,
    IReadOnlyList<OgDrillStringComponent> String,
    IReadOnlyList<OgTorqueDragRow> Rows,
    double HookLoadKn,
    double SurfaceTorqueKnm,
    string LimitText,
    string? LimitComponent,
    double? MaxUtilisation,
    double? MarginKn,
    bool Passed,
    IReadOnlyList<OgTorqueDragChartPoint> Chart,
    IReadOnlyList<string> Warnings,
    string? RunId,
    DateTime? RanAt,
    string? RanBy,
    DateTime ReadAt)
{
    public int OverLimit => Rows.Count(row => row.OverLimit);

    public string CountsText =>
        Rows.Count.ToString(CultureInfo.InvariantCulture) + " station(s) · " +
        OverLimit.ToString(CultureInfo.InvariantCulture) + " above a limit · " +
        (Passed ? "within the string" : "outside the string");

    public string SummaryText =>
        "Hook load " + HookLoadKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN · surface torque " +
        SurfaceTorqueKnm.ToString("0.##", CultureInfo.InvariantCulture) + " kN·m · " +
        (MarginKn is { } margin
            ? "smallest remaining capacity " + margin.ToString("0.#", CultureInfo.InvariantCulture) + " kN"
            : "no limit to compare with");

    public string FrictionText =>
        "cased " + CasedFriction.ToString("0.###", CultureInfo.InvariantCulture) + " · open hole " +
        OpenHoleFriction.ToString("0.###", CultureInfo.InvariantCulture) + " · " + ShoeText;

    public string StringText => String.Count == 0
        ? "No string component is recorded."
        : string.Join(Environment.NewLine, String.Select(component => component.Describe(Unit, "as recorded")));

    public string RowsText => Rows.Count == 0
        ? "No station was computed."
        : string.Join(Environment.NewLine, Rows.Select(row => row.Describe(Unit)));

    public string WarningsText => Warnings.Count == 0
        ? "The run raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string SavedText => RunId is null
        ? "The run is not saved."
        : "run saved " + (RanAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "at an unrecorded time") +
          " by " + (RanBy ?? "an unrecorded person");

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The torque-and-drag rules ENG-07 states once.</summary>
public static class TorqueDragRules
{
    public const string CodeNoConnection = "torque-drag-no-connection";
    public const string CodeReadFailed = "torque-drag-read-failed";
    public const string CodeWriteFailed = "torque-drag-write-failed";
    public const string CodeWellRequired = "torque-drag-well-required";
    public const string CodeActorRequired = "torque-drag-actor-required";
    public const string CodeScenarioUnknown = "torque-drag-scenario-unknown";
    public const string CodeNoTrajectory = "torque-drag-no-trajectory";
    public const string CodeNoFriction = "torque-drag-no-friction";
    public const string CodeStringMissing = "torque-drag-string-missing";
    public const string CodeStringRequired = "torque-drag-string-required";
    public const string CodeMixedUnits = "torque-drag-mixed-units";
    public const string CodeComponentUnknown = "torque-drag-component-unknown";
    public const string CodeUnknownRun = "torque-drag-unknown-run";
    public const string CodeRunRefused = "torque-drag-run-refused";

    public const string PickUp = "PICK_UP";
    public const string SlackOff = "SLACK_OFF";
    public const string RotateOff = "ROTATE_OFF";
    public const string Drilling = "DRILLING";

    public static IReadOnlyList<string> Scenarios { get; } = [PickUp, SlackOff, RotateOff, Drilling];

    /// <summary>The owner's operation for a scenario, or null when the name is not one of the four.</summary>
    public static StringOperation? OperationOf(string? scenario) =>
        (scenario ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            PickUp => StringOperation.TripOut,
            SlackOff => StringOperation.TripIn,
            RotateOff => StringOperation.RotatingOffBottom,
            Drilling => StringOperation.Drilling,
            _ => null,
        };

    /// <summary>AC-3: the interval a station sits in, from the shoe and its factor.</summary>
    public static (string Interval, double Factor) IntervalOf(double md, double cased, double openHole, double? shoe) =>
        shoe is { } depth && md > depth
            ? ("open hole", openHole)
            : ("cased", cased);

    /// <summary>
    /// The pipe-body yield of a component in newtons, from the owner's API 5C3 relation at its own wall and grade, or null when the
    /// grade has no minimum yield in the configuration.
    /// </summary>
    public static double? LimitOf(OgDrillStringComponent component, CasingDesignSettings settings)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(settings);

        if (component.Wall <= 0d)
        {
            return null;
        }

        return CasingDesign.YieldOf(component.Grade, settings) is { } yield
            ? CasingDesign.Tension(component.Od, component.Wall, yield) * 4.4482216152605
            : null;
    }

    /// <summary>
    /// AC-2: the station with the largest load over its limit, named with the component it belongs to.
    /// </summary>
    public static (string Text, string? Component, double? Utilisation) Limiting(IReadOnlyList<OgTorqueDragRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var rated = rows.Where(row => row.LimitKn > 0d).ToList();

        if (rated.Count == 0)
        {
            return ("No string limit is available for any station, so no limiting factor is named.", null, null);
        }

        var worst = rated
            .OrderByDescending(row => row.LoadKn / row.LimitKn)
            .ThenBy(row => row.Md)
            .First();

        var utilisation = worst.LoadKn / worst.LimitKn;

        return ("Limiting factor: " + worst.Component + " at " +
                worst.Md.ToString("0.#", CultureInfo.InvariantCulture) +
                " carries " + worst.LoadKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN against its " +
                worst.LimitKn.ToString("0.#", CultureInfo.InvariantCulture) + " kN pipe-body yield (" +
                (utilisation * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %)",
            worst.Component,
            utilisation);
    }

    /// <summary>
    /// AC-5: a component written with a unit suffix that is not the plan's unit is refused rather than converted.
    /// </summary>
    public static (string? Suffix, string? Problem) CheckUnit(string suffix, string unit) =>
        string.Equals(suffix, unit, StringComparison.OrdinalIgnoreCase)
            ? (null, null)
            : (suffix,
                "a component is written in '" + suffix + "' while the trajectory is in '" + unit +
                "', so the units are mixed and the run is refused");
}

/// <summary>ENG-07: predict the hook load, the torque and the limiting component of a string in a planned well.</summary>
public interface IOgTorqueDrag
{
    /// <summary>Runs the model along the well's saved trajectory without writing anything.</summary>
    Task<OgResult<OgTorqueDragWorkspace>> RunAsync(
        OgTorqueDragRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Runs the model and records the scenario, the friction factors and the answer (AC-6).</summary>
    Task<OgResult<OgTorqueDragWorkspace>> SaveAsync(
        OgTorqueDragRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the well's most recent run and runs it again from its recorded inputs.</summary>
    Task<OgResult<OgTorqueDragWorkspace>> LoadLastAsync(
        string wellId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-07 over the DrillingAndConstruction-owned <c>TORQUE_DRAG_RUN</c>, the trajectory the well plan (ENG-06) hands off and the
/// tools' <see cref="TorqueDrag"/>.
/// </summary>
/// <remarks>
/// <para>
/// The soft-string model and its arithmetic are the owner's; what this service adds is the string it is given, the per-interval
/// friction bookkeeping, the component limits from the API 5C3 pipe-body yield, the stations ordered by measured depth and the
/// record.
/// </para>
/// <para>
/// The trajectory is not stored or copied: it comes from the saved plan through <see cref="IOgWellPlan"/>, which is what ENG-06's
/// handoff passes, so a run is always against the plan it names.
/// </para>
/// </remarks>
public sealed class OgTorqueDragService : IOgTorqueDrag
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly IOgWellPlan _plans;

    public OgTorqueDragService(Func<IDataSource?> source, IFailureReporter failures, IOgWellPlan plans)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _plans = plans ?? throw new ArgumentNullException(nameof(plans));
    }

    public async Task<OgResult<OgTorqueDragWorkspace>> RunAsync(
        OgTorqueDragRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.WellId))
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "A torque-and-drag run belongs to one well; none is named.", TorqueDragRules.CodeWellRequired);
        }

        var operation = TorqueDragRules.OperationOf(request.Scenario);

        if (operation is not { } chosen)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "'" + (request.Scenario ?? "(none)") + "' is not an operation; use " +
                string.Join(", ", TorqueDragRules.Scenarios) + ".", TorqueDragRules.CodeScenarioUnknown);
        }

        // The trajectory comes from the saved plan: this is the handoff ENG-06 passes.
        var plan = await _plans.LoadAsync(request.WellId.Trim(), cancellationToken).ConfigureAwait(false);

        if (!plan.Success || plan.Value is not { } workspace || workspace.Stations.Count < 2)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "No trajectory is available for " + request.WellId.Trim() +
                ": design and save a well plan first - a torque-and-drag run is computed along one." +
                (plan.Success ? string.Empty : " The plan read said: " + plan.Message),
                TorqueDragRules.CodeNoTrajectory);
        }

        var unit = workspace.Unit;

        var trajectory = TheTechIdea.OilGas.Wellbore.Data.Trajectory.Compute(
            workspace.Stations.Select(station => (station.Md, station.Inclination, station.Azimuth)),
            unit);

        if (request.String.Count == 0)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The run needs a drill string, listed from the bit up; none is recorded.",
                TorqueDragRules.CodeStringRequired);
        }

        if (request.ElementLength <= 0d)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The element length must be greater than zero for the soft-string model to step.",
                TorqueDragRules.CodeStringRequired);
        }

        // AC-3: both friction factors are calibration inputs, so a missing one is refused with its interval.
        if (request.CasedFriction <= 0d)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The cased-hole interval has no friction factor: it is a field-calibrated input and the run is refused " +
                "rather than assumed.", TorqueDragRules.CodeNoFriction);
        }

        var shoe = request.ShoeDepth;
        var openHole = shoe is { } shoeDepth && shoeDepth < WorkspaceBitMd(trajectory, unit);

        if (openHole && request.OpenHoleFriction <= 0d)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The open-hole interval from " + shoe!.Value.ToString("0.#", CultureInfo.InvariantCulture) +
                " to the bit has no friction factor: it is a field-calibrated input and the run is refused rather than " +
                "assumed.", TorqueDragRules.CodeNoFriction);
        }

        // The string has to reach TD: its length must cover the trajectory it is run in.
        var span = WorkspaceBitMd(trajectory, unit) - TopMd(trajectory, unit);
        var length = request.String.Sum(component => component.Length);

        if (length < span - 1e-6)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The string is " + length.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
                " long and the trajectory spans " + span.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
                " from the top to the last station, so the string does not reach TD.",
                TorqueDragRules.CodeStringMissing);
        }

        var settings = WellboreEnvironment.Current.CasingDesign;

        foreach (var component in request.String)
        {
            if (TorqueDragRules.LimitOf(component, settings) is null)
            {
                return OgResult<OgTorqueDragWorkspace>.Failed(
                    component.Name + " records the grade '" + (string.IsNullOrWhiteSpace(component.Grade) ? "(none)" : component.Grade) +
                    "' or a wall that gives no pipe-body yield, so the run has no limit to compare with.",
                    TorqueDragRules.CodeComponentUnknown);
            }
        }

        try
        {
            var sections = request.String
                .Select(component => new StringSection
                {
                    Name = component.Name,
                    Length = new Quantity(component.Length, unit),
                    WeightPerLength = new Quantity(component.WeightPerLength, "lbm/ft"),
                    OuterDiameter = new Quantity(component.Od, "in"),
                })
                .ToList();

            var result = TorqueDrag.Run(trajectory, sections, new TorqueDragCase
            {
                Operation = chosen,
                CasedFriction = request.CasedFriction,
                OpenHoleFriction = request.OpenHoleFriction,
                ShoeDepth = shoe is { } shoeAt ? new Quantity(shoeAt, unit) : null,
                MudDensity = request.MudDensity is { } mud ? new Measure(mud, "ppg") : null,
                WeightOnBit = request.WeightOnBitKn is { } wob ? new Quantity(wob * 1000d, "N") : null,
                BitTension = request.BitTensionKn * 1000d,
                BitTorque = request.BitTorqueKnm * 1000d,
                ElementLength = new Quantity(request.ElementLength, "m"),
            });

            var rows = Build(result, trajectory, request, unit, settings);
            var (limitText, limitComponent, utilisation) = TorqueDragRules.Limiting(rows);
            var passed = utilisation is null or <= 1d;

            var warnings = new List<string>();

            if (!passed)
            {
                warnings.Add(limitText);
            }

            if (result.CompressionDepths.Count > 0)
            {
                warnings.Add(result.CompressionDepths.Count.ToString(CultureInfo.InvariantCulture) +
                             " station(s) are in compression, the deepest at " +
                             result.CompressionDepths.Max().ToString("0.#", CultureInfo.InvariantCulture) + " m.");
            }

            var margin = rows.Where(row => row.LimitKn > 0d)
                .Select(row => row.LimitKn - row.LoadKn)
                .DefaultIfEmpty(double.NaN)
                .Min();

            return OgResult<OgTorqueDragWorkspace>.Ok(new OgTorqueDragWorkspace(
                request.WellId.Trim(),
                workspace.PlanId,
                ScenarioOf(chosen),
                unit,
                request.CasedFriction,
                request.OpenHoleFriction,
                shoe,
                shoe is { } at
                    ? "shoe at " + at.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + ", open hole below"
                    : "no shoe recorded, so the whole string is taken as cased",
                request.MudDensity,
                request.ElementLength,
                request.String,
                rows,
                result.HookLoad / 1000d,
                result.SurfaceTorque / 1000d,
                limitText,
                limitComponent,
                utilisation,
                double.IsNaN(margin) ? null : margin,
                passed,
                rows.Select(row => new OgTorqueDragChartPoint(row.Md, row.LoadKn, row.LimitKn)).ToList(),
                warnings,
                null,
                null,
                null,
                DateTime.UtcNow));
        }
        catch (ArgumentException ex)
        {
            // The owner refuses a run whose friction factors or inputs are not usable; the sentence it raises is an ordinary
            // answer, reported so it is stored, and the person is given a sentence of ours with the reference.
            var reference = _failures.ReportHandled(
                ex, "running torque and drag for " + request.WellId,
                "the run is refused before it is computed", FailureSeverity.Degraded);

            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The run was refused before it was computed: check the string, the friction factors and the element length." +
                (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")"),
                TorqueDragRules.CodeRunRefused);
        }
    }

    public async Task<OgResult<OgTorqueDragWorkspace>> SaveAsync(
        OgTorqueDragRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "A saved run records who made it.", TorqueDragRules.CodeActorRequired);
        }

        var run = await RunAsync(request, cancellationToken).ConfigureAwait(false);

        if (!run.Success || run.Value is null)
        {
            return run;
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "No connection is open, so the run cannot be saved.", TorqueDragRules.CodeNoConnection);
        }

        try
        {
            var result = run.Value;
            var runId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var unit = OgUow.For<TORQUE_DRAG_RUN>(source);
            unit.PrimaryKey = nameof(TORQUE_DRAG_RUN.RUN_ID);

            unit.Add(new TORQUE_DRAG_RUN
            {
                RUN_ID = runId,
                UWI = result.WellId,
                PLAN_ID = result.PlanId,
                SCENARIO = result.Scenario,
                CASED_FRICTION = (decimal)result.CasedFriction,
                OPEN_HOLE_FRICTION = (decimal)result.OpenHoleFriction,
                SHOE_DEPTH = result.ShoeDepth is { } shoe ? (decimal)shoe : null,
                SHOE_DEPTH_OUOM = result.Unit,
                MUD_DENSITY = result.MudDensity is { } mud ? (decimal)mud : null,
                MUD_DENSITY_OUOM = result.MudDensity is null ? null : "ppg",
                ELEMENT_LENGTH_M = (decimal)result.ElementLength,
                WOB = request.WeightOnBitKn is { } wob ? (decimal)wob : null,
                WOB_OUOM = request.WeightOnBitKn is null ? null : "kN",
                BIT_TENSION_N = request.BitTensionKn == 0d ? null : (decimal)(request.BitTensionKn * 1000d),
                BIT_TORQUE_NM = request.BitTorqueKnm == 0d ? null : (decimal)(request.BitTorqueKnm * 1000d),
                STRING_TEXT = JsonSerializer.Serialize(request.String),
                HOOK_LOAD_KN = (decimal)result.HookLoadKn,
                SURFACE_TORQUE_KNM = (decimal)result.SurfaceTorqueKnm,
                LIMIT_TEXT = result.LimitText,
                LIMIT_COMPONENT = result.LimitComponent,
                MAX_UTILISATION = result.MaxUtilisation is { } utilisation ? (decimal)utilisation : null,
                MARGIN_KN = result.MarginKn is { } margin ? (decimal)margin : null,
                STATION_COUNT = result.Rows.Count,
                PASSED = result.Passed ? "Y" : "N",
                RAN_BY = actor.Trim(),
                RAN_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgTorqueDragWorkspace>.Failed(
                    OgUow.Message(committed, "The run was not saved."), TorqueDragRules.CodeWriteFailed);
            }

            return OgResult<OgTorqueDragWorkspace>.Ok(result with
            {
                RunId = runId,
                RanAt = now,
                RanBy = actor.Trim(),
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the torque-and-drag run for " + request.WellId, "the run is not saved",
                FailureSeverity.Error);

            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The run could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? TorqueDragRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgTorqueDragWorkspace>> LoadLastAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "A torque-and-drag run belongs to one well; none is named.", TorqueDragRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgTorqueDragWorkspace>.Failed(
                "No connection is open, so the run cannot be read.", TorqueDragRules.CodeNoConnection);
        }

        try
        {
            var runs = await OgUow.ReadAllAsync<TORQUE_DRAG_RUN>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var saved = runs
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.UWI?.Trim(), wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.RAN_AT_UTC)
                .FirstOrDefault();

            if (saved is null)
            {
                return OgResult<OgTorqueDragWorkspace>.Failed(
                    "No torque-and-drag run is saved for " + wellId.Trim() + ".", TorqueDragRules.CodeUnknownRun);
            }

            var rerun = await RunAsync(new OgTorqueDragRequest(
                saved.UWI,
                saved.SCENARIO,
                (double)saved.CASED_FRICTION,
                (double)saved.OPEN_HOLE_FRICTION,
                saved.SHOE_DEPTH is { } shoe ? (double)shoe : null,
                saved.MUD_DENSITY is { } mud ? (double)mud : null,
                (double)saved.ELEMENT_LENGTH_M,
                saved.WOB is { } wob ? (double)wob : null,
                saved.BIT_TENSION_N is { } tension ? (double)tension / 1000d : 0d,
                saved.BIT_TORQUE_NM is { } torque ? (double)torque / 1000d : 0d,
                ComponentsOf(saved.STRING_TEXT)), cancellationToken).ConfigureAwait(false);

            if (!rerun.Success || rerun.Value is null)
            {
                return rerun;
            }

            return OgResult<OgTorqueDragWorkspace>.Ok(rerun.Value with
            {
                RunId = saved.RUN_ID,
                RanAt = saved.RAN_AT_UTC,
                RanBy = saved.RAN_BY,
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the torque-and-drag run for " + wellId, "the run is not shown", FailureSeverity.Error);

            return OgResult<OgTorqueDragWorkspace>.Failed(
                "The run could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? TorqueDragRules.CodeReadFailed);
        }
    }

    // ------------------------------------------------------------------ the stations

    private static string ScenarioOf(StringOperation operation) => operation switch
    {
        StringOperation.TripOut => TorqueDragRules.PickUp,
        StringOperation.TripIn => TorqueDragRules.SlackOff,
        StringOperation.RotatingOffBottom => TorqueDragRules.RotateOff,
        StringOperation.Drilling => TorqueDragRules.Drilling,
    };

    private static IReadOnlyList<OgTorqueDragRow> Build(
        TorqueDragResult result,
        Trajectory trajectory,
        OgTorqueDragRequest request,
        string unit,
        CasingDesignSettings settings)
    {
        var bitMd = WorkspaceBitMd(trajectory, unit);

        // The component covering each station, from the bit up, exactly as the owner's model slices the string.
        var ranges = new List<(double From, double To, OgDrillStringComponent Component)>();
        var at = bitMd;

        foreach (var component in request.String)
        {
            ranges.Add((at - component.Length, at, component));
            at -= component.Length;
        }

        var rows = new List<OgTorqueDragRow>();
        var index = 0;

        foreach (var point in result.Points.OrderBy(point => point.Md))
        {
            var component = ranges.FirstOrDefault(range => point.Md > range.From - 1e-9 && point.Md <= range.To + 1e-9).Component;

            var limit = component is null ? 0d : TorqueDragRules.LimitOf(component, settings) ?? 0d;
            var (interval, factor) = TorqueDragRules.IntervalOf(point.Md, request.CasedFriction, request.OpenHoleFriction,
                request.ShoeDepth);

            rows.Add(new OgTorqueDragRow(
                ++index,
                point.Md,
                InclinationAt(trajectory, point.Md),
                AzimuthAt(trajectory, point.Md),
                point.Tension / 1000d,
                point.Torque / 1000d,
                factor * point.NormalForce / 1000d,
                point.NormalForce / 1000d,
                limit / 1000d,
                interval,
                component?.Name ?? "(above the string)",
                limit > 0d && point.Tension > limit));
        }

        return rows;
    }

    /// <summary>The inclination of the trajectory at a measured depth, from the station either side of it.</summary>
    private static double InclinationAt(Trajectory trajectory, double md)
    {
        var (a, b) = Bracketing(trajectory, md);

        return a.Inclination + (b.Inclination - a.Inclination) * Fraction(a.Md, b.Md, md);
    }

    /// <summary>The azimuth of the trajectory at a measured depth, from the station either side of it.</summary>
    private static double AzimuthAt(Trajectory trajectory, double md)
    {
        var (a, b) = Bracketing(trajectory, md);

        return a.Azimuth + Wrap(b.Azimuth - a.Azimuth) * Fraction(a.Md, b.Md, md);
    }

    private static (TrajectoryStation A, TrajectoryStation B) Bracketing(Trajectory trajectory, double md)
    {
        var stations = trajectory.Stations;

        for (var index = 1; index < stations.Count; index++)
        {
            if (md <= stations[index].Md + 1e-9)
            {
                return (stations[index - 1], stations[index]);
            }
        }

        return (stations[^2], stations[^1]);
    }

    private static double Fraction(double a, double b, double md) =>
        b - a <= 1e-9 ? 0d : Math.Clamp((md - a) / (b - a), 0d, 1d);

    private static double Wrap(double degrees)
    {
        var wrapped = degrees % 360d;

        return wrapped > 180d ? wrapped - 360d : wrapped < -180d ? wrapped + 360d : wrapped;
    }

    private static double WorkspaceBitMd(Trajectory trajectory, string unit)
    {
        var last = trajectory.Stations[^1].Md;

        return Units.TryConvert(last, trajectory.Unit, unit, out var converted) ? converted : last;
    }

    private static double TopMd(Trajectory trajectory, string unit)
    {
        var first = trajectory.Stations[0].Md;

        return Units.TryConvert(first, trajectory.Unit, unit, out var converted) ? converted : first;
    }

    private IReadOnlyList<OgDrillStringComponent> ComponentsOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<OgDrillStringComponent>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            // A stored string that no longer parses leaves the run with nothing to run; the caller refuses it with "needs a
            // drill string", and the malformed record is reported rather than shown as a half-read run.
            _failures.ReportHandled(
                ex, "reading a recorded drill string", "the run's string is not read", FailureSeverity.Degraded);

            return [];
        }
    }
}
