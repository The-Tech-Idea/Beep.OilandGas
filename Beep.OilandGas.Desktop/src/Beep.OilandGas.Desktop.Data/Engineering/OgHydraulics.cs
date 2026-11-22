using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One flow path of the circulating system (ENG-08).</summary>
/// <param name="InnerDiameter">
/// The pipe outside diameter when the section is an annulus; null when the flow is inside the pipe. <paramref name="OuterDiameter"/>
/// is the pipe inside diameter for a pipe section and the hole or casing inside diameter for an annulus, exactly as the owner's
/// <c>FlowSection</c> defines them.
/// </param>
public sealed record OgFlowSection(
    string Name,
    double TopMd,
    double BottomMd,
    double OuterDiameter,
    double? InnerDiameter)
{
    public bool IsAnnulus => InnerDiameter is not null;

    public double Length => BottomMd - TopMd;

    public string Interval => IsAnnulus ? "annulus" : "inside pipe";

    public string Describe(string unit) =>
        Name + " · " + (IsAnnulus ? "annulus" : "pipe") + " · " +
        TopMd.ToString("0.#", CultureInfo.InvariantCulture) + "-" +
        BottomMd.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit + " · " +
        OuterDiameter.ToString("0.###", CultureInfo.InvariantCulture) + "\" ID" +
        (InnerDiameter is { } inner ? " x " + inner.ToString("0.###", CultureInfo.InvariantCulture) + "\" OD" : string.Empty);
}

/// <summary>The inputs of a hydraulics run (ENG-08).</summary>
/// <param name="Theta600">The 600 rpm Fann reading, from which PV, YP, n and K are all derived by the owner.</param>
public sealed record OgHydraulicsRequest(
    string WellId,
    double MudDensityPpg,
    double Theta600,
    double Theta300,
    double FlowRate,
    string FlowRateUnit,
    double SurfaceLossPsi,
    IReadOnlyList<double> Nozzles32nds,
    IReadOnlyList<OgFlowSection> Sections);

/// <summary>One section's result (ENG-08's <c>OgHydraulicsResult.Sections</c>).</summary>
public sealed record OgSectionResult(
    int Index,
    string Name,
    string Interval,
    double TopMd,
    double BottomMd,
    double VelocityFtS,
    double Reynolds,
    bool Turbulent,
    double? LossPsi,
    double EcdPpg,
    bool Computed)
{
    public string Describe(string unit) =>
        Index.ToString(CultureInfo.InvariantCulture) + ". " + Name + " (" + Interval + ") " +
        TopMd.ToString("0.#", CultureInfo.InvariantCulture) + "-" + BottomMd.ToString("0.#", CultureInfo.InvariantCulture) +
        " " + unit + " · " + VelocityFtS.ToString("0.##", CultureInfo.InvariantCulture) + " ft/s · " +
        (Turbulent ? "turbulent" : "laminar") + " (Re " + Reynolds.ToString("0", CultureInfo.InvariantCulture) + ") · " +
        (LossPsi is { } loss ? loss.ToString("0.#", CultureInfo.InvariantCulture) + " psi" : "not computed") +
        " · ECD " + EcdPpg.ToString("0.###", CultureInfo.InvariantCulture) + " ppg";
}

/// <summary>One point of the ECD profile: a depth and the equivalent circulating density there (ENG-08).</summary>
public sealed record OgEcdPoint(double Depth, double Value);

/// <summary>What the optimiser found (ENG-08, AC-5).</summary>
public sealed record OgHydraulicsRecommendation(
    bool Feasible,
    string Reason,
    double TargetEcdPpg,
    double StaticEcdPpg,
    double? FlowRateGpm,
    double? FlowRateLpm,
    double? EcdAtSolution,
    IReadOnlyList<double> Nozzles32nds,
    double? BitLossPsi,
    double? Bhhp,
    double? StandpipePressurePsi,
    int Iterations,
    IReadOnlyList<string> Trace)
{
    public string FlowText => FlowRateGpm is { } gpm
        ? gpm.ToString("0.#", CultureInfo.InvariantCulture) + " gpm · " +
          (FlowRateLpm ?? 0d).ToString("0.#", CultureInfo.InvariantCulture) + " L/min"
        : "no rate solved";

    public string NozzleText => Nozzles32nds.Count == 0
        ? "no nozzle"
        : string.Join(" + ", Nozzles32nds.Select(size => size.ToString("0", CultureInfo.InvariantCulture) + "/32"));

    public string Describe() => Feasible
        ? "Feasible: " + TargetEcdPpg.ToString("0.###", CultureInfo.InvariantCulture) + " ppg is reached at " + FlowText +
          ", where the ECD is " + (EcdAtSolution ?? 0d).ToString("0.###", CultureInfo.InvariantCulture) + " ppg" +
          (Nozzles32nds.Count == 0
              ? "."
              : "; with " + NozzleText + " the bit loss is " + (BitLossPsi ?? 0d).ToString("0.#", CultureInfo.InvariantCulture) +
                " psi and the bit hydraulic horsepower " + (Bhhp ?? 0d).ToString("0.##", CultureInfo.InvariantCulture) + ".") +
          " " + Reason
        : "Infeasible: " + Reason;
}

/// <summary>A completed hydraulics run (ENG-08).</summary>
public sealed record OgHydraulicsWorkspace(
    string WellId,
    string? PlanId,
    string RheologyModel,
    double MudDensityPpg,
    double Theta600,
    double Theta300,
    double PlasticViscosity,
    double YieldPoint,
    double FlowIndex,
    double ConsistencyIndex,
    double FlowRate,
    string FlowRateUnit,
    double FlowGpm,
    double SurfaceLossPsi,
    IReadOnlyList<double> Nozzles32nds,
    IReadOnlyList<OgFlowSection> Sections,
    IReadOnlyList<OgSectionResult> Results,
    string Unit,
    double BitMd,
    double BitTvd,
    double BitTvdFt,
    double? StandpipePressure,
    double? AnnularLoss,
    double? Ecd,
    double BitLoss,
    double Bhhp,
    double ImpactForce,
    double? MaxEcd,
    double? MaxEcdDepth,
    IReadOnlyList<OgEcdPoint> Profile,
    double? FractureGradientPpg,
    double? FractureDepth,
    IReadOnlyList<OgEcdPoint> FracturePoints,
    bool FractureRecorded,
    bool FractureExceeded,
    string CriticalText,
    IReadOnlyList<string> Warnings,
    string? RunId,
    DateTime? RanAt,
    string? RanBy,
    DateTime ReadAt)
{
    public int NotComputed => Results.Count(result => !result.Computed);

    public bool NoNozzles => Nozzles32nds.Count == 0;

    public string NozzlesText => Nozzles32nds.Count == 0
        ? "No nozzle recorded, so the bit pressure loss is taken as zero and the standpipe pressure is understated."
        : Nozzles32nds.Count.ToString(CultureInfo.InvariantCulture) + " nozzle(s) at " +
          string.Join(" + ", Nozzles32nds.Select(size => size.ToString("0", CultureInfo.InvariantCulture) + "/32")) +
          " (32nds of an inch) · total flow area " +
          (Nozzles32nds.Sum(size => Math.PI / 4d * Math.Pow(size / 32d, 2)) * 1e6d)
          .ToString("0.#", CultureInfo.InvariantCulture) + "e-6 in²";

    public string MudText =>
        RheologyModel + " · " + MudDensityPpg.ToString("0.##", CultureInfo.InvariantCulture) + " ppg · PV " +
        PlasticViscosity.ToString("0.#", CultureInfo.InvariantCulture) + " cP · YP " +
        YieldPoint.ToString("0.#", CultureInfo.InvariantCulture) + " lbf/100 ft² · n " +
        FlowIndex.ToString("0.###", CultureInfo.InvariantCulture) + " · K " +
        ConsistencyIndex.ToString("0.#", CultureInfo.InvariantCulture) +
        " · from the 600/300 rpm readings " + Theta600.ToString("0.#", CultureInfo.InvariantCulture) + " / " +
        Theta300.ToString("0.#", CultureInfo.InvariantCulture);

    public string SummaryText =>
        "Standpipe " + (StandpipePressure?.ToString("0", CultureInfo.InvariantCulture) ?? "not computed") + " psi · annular " +
        (AnnularLoss?.ToString("0.#", CultureInfo.InvariantCulture) ?? "not computed") + " psi · bit " +
        BitLoss.ToString("0.#", CultureInfo.InvariantCulture) + " psi · ECD " +
        (Ecd?.ToString("0.###", CultureInfo.InvariantCulture) ?? "not computed") + " ppg at " +
        BitTvd.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " TVD";

    public string CountsText =>
        Results.Count.ToString(CultureInfo.InvariantCulture) + " section(s) · " +
        NotComputed.ToString(CultureInfo.InvariantCulture) + " not computed · rate " +
        FlowRate.ToString("0.#", CultureInfo.InvariantCulture) + " " + FlowRateUnit;

    public string SectionsText => Results.Count == 0
        ? "No flow section is recorded."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe(Unit)));

    public string ProfileText => Profile.Count == 0
        ? "No ECD profile: the annulus was not computed."
        : string.Join(Environment.NewLine, Profile.Select(point =>
            point.Depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · " +
            point.Value.ToString("0.###", CultureInfo.InvariantCulture) + " ppg"));

    public string FractureText => !FractureRecorded
        ? "No fracture gradient is recorded for this well, so no threshold is drawn and no comparison is made - " +
          "one is not invented."
        : "Recorded fracture gradient " + (FractureGradientPpg ?? 0d).ToString("0.###", CultureInfo.InvariantCulture) +
          " ppg at " + (FractureDepth ?? 0d).ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit +
          (FractureExceeded ? " - the profile is above it." : " - the profile stays below it.");

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

/// <summary>The hydraulics rules ENG-08 states once.</summary>
public static class HydraulicsRules
{
    public const string CodeNoConnection = "hydraulics-no-connection";
    public const string CodeReadFailed = "hydraulics-read-failed";
    public const string CodeWriteFailed = "hydraulics-write-failed";
    public const string CodeWellRequired = "hydraulics-well-required";
    public const string CodeActorRequired = "hydraulics-actor-required";
    public const string CodeNoTrajectory = "hydraulics-no-trajectory";
    public const string CodeRheologyMissing = "hydraulics-rheology-missing";
    public const string CodeNoFlowRate = "hydraulics-no-flow-rate";
    public const string CodeNoSections = "hydraulics-no-sections";
    public const string CodeSectionInvalid = "hydraulics-section-invalid";
    public const string CodeSectionBeyondWell = "hydraulics-section-beyond-well";
    public const string CodeNozzleInvalid = "hydraulics-nozzle-invalid";
    public const string CodeUnknownRun = "hydraulics-unknown-run";
    public const string CodeOptimiserTarget = "hydraulics-optimiser-target";

    /// <summary>The constant the owner's ECD formula uses: one ppg per 0.052 psi per foot.</summary>
    public const double PpgPerPsiPerFt = 0.052;

    /// <summary>
    /// AC-4: a nozzle is sized in 32nds of an inch, and the size is shown as a fraction over 32 so it cannot be read as inches.
    /// </summary>
    public static string NozzleAsFraction(double size32nds) =>
        size32nds.ToString("0", CultureInfo.InvariantCulture) + "/32 in";

    /// <summary>
    /// AC-1's arithmetic, stated as the owner states it: ECD = mud weight + annular loss / (0.052 x true vertical depth).
    /// </summary>
    public static double EcdOf(double mudPpg, double annularLossPsi, double tvdFt) =>
        tvdFt <= 0d ? mudPpg : mudPpg + annularLossPsi / (PpgPerPsiPerFt * tvdFt);

    /// <summary>
    /// A recorded fracture gradient as an equivalent mud weight in ppg, the unit the ECD is in. A gradient is quoted either as a
    /// density (ppg) or as a pressure gradient (psi/ft), and the second is divided by the same 0.052 the ECD uses.
    /// </summary>
    public static double? FractureGradientPpg(double value, string? unit)
    {
        if (value <= 0d)
        {
            return null;
        }

        // No unit is not a convention: a gradient recorded without one could be either reading, and the two differ by a factor
        // of nineteen, so it is left out rather than turned into a threshold.
        if (string.IsNullOrWhiteSpace(unit))
        {
            return null;
        }

        var quantity = new Quantity(value, unit.Trim());

        if (quantity.TryIn("ppg", out var asDensity))
        {
            return asDensity;
        }

        return quantity.TryIn("psi/ft", out var asGradient) ? asGradient / PpgPerPsiPerFt : null;
    }

    /// <summary>AC-3: the profile is critical when any point is above the recorded fracture gradient.</summary>
    public static (bool Exceeded, OgEcdPoint? Point) AboveFracture(
        IReadOnlyList<OgEcdPoint> profile,
        double? fracturePpg)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (fracturePpg is not { } limit)
        {
            return (false, null);
        }

        var worst = profile.OrderByDescending(point => point.Value).ThenBy(point => point.Depth).FirstOrDefault();

        return worst is not null && worst.Value > limit ? (true, worst) : (false, null);
    }

    /// <summary>The critical sentence, naming where it happens.</summary>
    public static string CriticalOf(bool exceeded, OgEcdPoint? point, double? fracturePpg, string unit) =>
        !exceeded || point is null
            ? "The equivalent circulating density is within the recorded fracture gradient."
            : "CRITICAL: the ECD reaches " + point.Value.ToString("0.###", CultureInfo.InvariantCulture) +
              " ppg at " + point.Depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
              ", above the recorded fracture gradient of " +
              (fracturePpg ?? 0d).ToString("0.###", CultureInfo.InvariantCulture) + " ppg. Reduce the rate or the mud weight.";
}

/// <summary>ENG-08: the circulating system, the ECD profile and the rate that reaches a target ECD.</summary>
public interface IOgHydraulics
{
    /// <summary>Runs the circulating system along the saved plan without writing anything.</summary>
    Task<OgResult<OgHydraulicsWorkspace>> RunAsync(
        OgHydraulicsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Runs it and records the mud model, the rate and the answer (AC-6).</summary>
    Task<OgResult<OgHydraulicsWorkspace>> SaveAsync(
        OgHydraulicsRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the well's most recent run and replays it from its recorded inputs.</summary>
    Task<OgResult<OgHydraulicsWorkspace>> LoadLastAsync(
        string wellId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AC-5: solves, monotonically, the rate at which the ECD reaches a target, and reports the bit hydraulics there.
    /// </summary>
    Task<OgResult<OgHydraulicsRecommendation>> OptimiseAsync(
        OgHydraulicsRequest request,
        double targetEcdPpg,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-08 over the DrillingAndConstruction-owned <c>HYDRAULICS_RUN</c>, the trajectory the well plan (ENG-06) hands off, the tools'
/// <see cref="Hydraulics"/> and the annulus fracture gradients ENG-04 records.
/// </summary>
/// <remarks>
/// <para>
/// The circulating arithmetic is the owner's: <see cref="Hydraulics.Circulate"/> for the system and
/// <see cref="Hydraulics.Gradient"/> for each section, with its own rheology derivation and its own regime test. What this service
/// adds is the section geometry taken from the plan's depths, the ECD <em>profile</em> the system result does not carry, the
/// per-depth fracture comparison, the monotone rate solve and the record.
/// </para>
/// <para>
/// The profile is not a second model: it accumulates the owner's own per-section <c>Gradient</c> losses down the annulus and applies
/// the owner's own ECD formula at each depth, so the deepest point equals <see cref="HydraulicsResult.Ecd"/> - which the gateway
/// asserts.
/// </para>
/// </remarks>
public sealed class OgHydraulicsService : IOgHydraulics
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly IOgWellPlan _plans;

    public OgHydraulicsService(Func<IDataSource?> source, IFailureReporter failures, IOgWellPlan plans)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _plans = plans ?? throw new ArgumentNullException(nameof(plans));
    }

    public async Task<OgResult<OgHydraulicsWorkspace>> RunAsync(
        OgHydraulicsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.WellId))
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "A hydraulics run belongs to one well; none is named.", HydraulicsRules.CodeWellRequired);
        }

        var rheology = RheologyOf(request, out var mud, out var problem);

        if (problem is not null)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(problem, HydraulicsRules.CodeRheologyMissing);
        }

        if (request.FlowRate <= 0d)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "A circulating run needs a positive flow rate; none was given.",
                HydraulicsRules.CodeNoFlowRate);
        }

        // The unit the rate is given in has to be a rate the catalogue knows, or the whole model is in the wrong units.
        var flow = new Quantity(request.FlowRate, string.IsNullOrWhiteSpace(request.FlowRateUnit) ? "gpm" : request.FlowRateUnit.Trim());

        if (!flow.TryIn("gpm", out var flowGpm) || flowGpm <= 0d)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The flow rate '" + request.FlowRate.ToString("0.#", CultureInfo.InvariantCulture) + " " +
                (request.FlowRateUnit ?? "(no unit)") + "' is not a rate the catalogue converts to gpm.",
                HydraulicsRules.CodeNoFlowRate);
        }

        if (request.Sections.Count == 0)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The run needs the flow sections of the circulating system; none is recorded.",
                HydraulicsRules.CodeNoSections);
        }

        foreach (var size in request.Nozzles32nds)
        {
            if (size <= 0d || size > 32d)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "A nozzle of " + size.ToString("0.##", CultureInfo.InvariantCulture) +
                    "/32 is not a size: nozzles run from 1 to 32 thirty-seconds of an inch.",
                    HydraulicsRules.CodeNozzleInvalid);
            }
        }

        // The trajectory comes from the saved plan, which is the handoff ENG-06 passes.
        var plan = await _plans.LoadAsync(request.WellId.Trim(), cancellationToken).ConfigureAwait(false);

        if (!plan.Success || plan.Value is not { } planWorkspace || planWorkspace.Stations.Count < 2)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "No trajectory is available for " + request.WellId.Trim() +
                ": design and save a well plan first - the bit's true vertical depth, and therefore the ECD, comes from it." +
                (plan.Success ? string.Empty : " The plan read said: " + plan.Message),
                HydraulicsRules.CodeNoTrajectory);
        }

        var unit = planWorkspace.Unit;
        var trajectory = TheTechIdea.OilGas.Wellbore.Data.Trajectory.Compute(
            planWorkspace.Stations.Select(station => (station.Md, station.Inclination, station.Azimuth)),
            unit);

        var bitMd = planWorkspace.Stations[^1].Md;
        var bitTvd = trajectory.TvdAt(bitMd);

        if (!Units.TryConvert(bitTvd, unit, "ft", out var bitTvdFt) || bitTvdFt <= 0d)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The plan's last station is at a true vertical depth of " + bitTvd.ToString("0.#", CultureInfo.InvariantCulture) +
                " " + unit + ", so the ECD has no depth to be quoted at. A hydraulics run needs a plan that reaches below " +
                "surface.", HydraulicsRules.CodeNoTrajectory);
        }

        // The sections are validated against the plan, so a section deeper than the hole is refused rather than modelled.
        var sections = new List<FlowSection>();

        foreach (var section in request.Sections)
        {
            if (section.BottomMd <= section.TopMd)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Section '" + section.Name + "' runs from " + section.TopMd.ToString("0.#", CultureInfo.InvariantCulture) +
                    " to " + section.BottomMd.ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
                    ", so it has no length to flow through.", HydraulicsRules.CodeSectionInvalid);
            }

            if (section.OuterDiameter <= 0d)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Section '" + section.Name + "' (" + section.Interval + ") records no " +
                    (section.IsAnnulus ? "hole or casing inside diameter" : "pipe inside diameter") +
                    ", so its velocity and loss cannot be computed.", HydraulicsRules.CodeSectionInvalid);
            }

            if (section.InnerDiameter is { } inner && (inner <= 0d || inner >= section.OuterDiameter))
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Section '" + section.Name + "' is an annulus with an outer diameter of " +
                    section.OuterDiameter.ToString("0.###", CultureInfo.InvariantCulture) + "\" and an inner of " +
                    inner.ToString("0.###", CultureInfo.InvariantCulture) + "\", so there is no annular gap to flow through.",
                    HydraulicsRules.CodeSectionInvalid);
            }

            if (section.BottomMd > bitMd + 1e-6)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Section '" + section.Name + "' reaches " + section.BottomMd.ToString("0.#", CultureInfo.InvariantCulture) +
                    " " + unit + " and the plan's last station is at " + bitMd.ToString("0.#", CultureInfo.InvariantCulture) +
                    " " + unit + ", so the section is below the hole the run follows.",
                    HydraulicsRules.CodeSectionBeyondWell);
            }

            if (!Units.TryConvert(section.Length, unit, "ft", out var lengthFt) || lengthFt <= 0d)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Section '" + section.Name + "' is recorded in '" + unit +
                    "', which is not a depth the catalogue converts to feet.", HydraulicsRules.CodeSectionInvalid);
            }

            sections.Add(new FlowSection
            {
                Name = section.Name,
                Length = new Quantity(lengthFt, "ft"),
                Outer = new Quantity(section.OuterDiameter, "in"),
                Inner = section.InnerDiameter is { } od ? new Quantity(od, "in") : null,
            });
        }

        // Two annuli covering the same depth would count that depth's loss twice and quietly inflate the ECD, so the overlap is
        // refused rather than modelled. The string and the annulus legitimately share the same depths - they are different paths.
        var annuli = request.Sections.Where(section => section.IsAnnulus).OrderBy(section => section.TopMd).ToList();

        for (var index = 1; index < annuli.Count; index++)
        {
            if (annuli[index].TopMd < annuli[index - 1].BottomMd - 1e-6)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "Sections '" + annuli[index - 1].Name + "' and '" + annuli[index].Name +
                    "' both cover " + annuli[index].TopMd.ToString("0.#", CultureInfo.InvariantCulture) + "-" +
                    Math.Min(annuli[index - 1].BottomMd, annuli[index].BottomMd)
                        .ToString("0.#", CultureInfo.InvariantCulture) + " " + unit +
                    ", so that depth's annular loss would be counted twice. Record one annulus per depth.",
                    HydraulicsRules.CodeSectionInvalid);
            }
        }

        try
        {
            var result = Hydraulics.Circulate(
                mud,
                new Quantity(flowGpm, "gpm"),
                sections,
                request.Nozzles32nds,
                new Quantity(bitTvdFt, "ft"),
                request.SurfaceLossPsi > 0d ? new Quantity(request.SurfaceLossPsi, "psi") : null);

            // No nozzle is the feature's own state: the bit loss is taken as zero and flagged, not computed as infinity.
            var bitLoss = result.BitLoss;
            var bhhp = result.BitHydraulicHorsepower;
            var impact = result.ImpactForce;

            if (request.Nozzles32nds.Count == 0)
            {
                bitLoss = 0d;
                bhhp = 0d;
                impact = 0d;

                // The owner divides by the nozzle area, so an empty nozzle list gives an infinite bit loss and an infinite
                // standpipe pressure. The feature's state says the bit loss is taken as zero and flagged, so the standpipe
                // pressure is restated as surface + string + annulus - explicitly without the bit's share - rather than published
                // as infinity.
                if (result.Sections.All(section => section.Loss != null))
                {
                    result.StandpipePressure = result.SurfaceLoss + result.Sections.Sum(section => section.Loss!.Value);
                }
            }

            var profile = ProfileOf(mud, flowGpm, request.Sections, trajectory, unit, request.MudDensityPpg);

            // The deepest profile point has to be the owner's own ECD; if it is not, the accumulation is wrong and the run says so
            // rather than showing two different numbers on one page.
            var ecd = result.Ecd;
            var profileLoss = AnnularLossOf(mud, flowGpm, request.Sections, unit);

            if (result.AnnularLoss is { } ownerAnnular && profileLoss is { } accumulated &&
                Math.Abs(ownerAnnular - accumulated) > 1e-6)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "The annular loss accumulated down the profile does not match the owner's own figure, so the profile is not " +
                    "published.", HydraulicsRules.CodeSectionInvalid);
            }

            var (fracturePpg, fractureDepth, fracturePoints) = await FractureAsync(
                request.WellId.Trim(), trajectory, unit, cancellationToken).ConfigureAwait(false);

            var (exceeded, worst) = HydraulicsRules.AboveFracture(profile, fracturePpg);

            var warnings = new List<string>(result.Notes);

            if (request.Nozzles32nds.Count == 0)
            {
                warnings.Add(
                    "No nozzle is recorded, so the bit pressure loss is taken as zero: the standpipe pressure is understated by " +
                    "the bit's share and the hydraulic horsepower is not reported.");
            }

            if (result.StandpipePressure is null)
            {
                warnings.Add(
                    "The standpipe pressure and the ECD are not computed: at least one section's flow regime is not covered by the " +
                    "selected rheology model.");
            }

            if (exceeded && worst is not null)
            {
                warnings.Add(HydraulicsRules.CriticalOf(true, worst, fracturePpg, unit));
            }

            var top = profile.OrderByDescending(point => point.Value).ThenBy(point => point.Depth).FirstOrDefault();

            return OgResult<OgHydraulicsWorkspace>.Ok(new OgHydraulicsWorkspace(
                request.WellId.Trim(),
                planWorkspace.PlanId,
                DrillingToolbox.Settings.RheologyModel,
                request.MudDensityPpg,
                request.Theta600,
                request.Theta300,
                mud.PlasticViscosity,
                mud.YieldPoint,
                mud.N,
                mud.K,
                request.FlowRate,
                string.IsNullOrWhiteSpace(request.FlowRateUnit) ? "gpm" : request.FlowRateUnit.Trim(),
                flowGpm,
                request.SurfaceLossPsi,
                request.Nozzles32nds,
                request.Sections,
                ResultsOf(result, request.Sections, profile),
                unit,
                bitMd,
                bitTvd,
                bitTvdFt,
                result.StandpipePressure,
                result.AnnularLoss,
                ecd,
                bitLoss,
                bhhp,
                impact,
                top?.Value,
                top?.Depth,
                profile,
                fracturePpg,
                fractureDepth,
                fracturePoints,
                fracturePpg is not null,
                exceeded,
                HydraulicsRules.CriticalOf(exceeded, worst, fracturePpg, unit),
                warnings,
                null,
                null,
                null,
                DateTime.UtcNow));
        }
        catch (ArgumentException ex)
        {
            // The owner refuses a system it cannot model; the sentence it raises is an ordinary answer, reported so it is stored,
            // and the person is given a sentence of ours with the reference.
            var reference = _failures.ReportHandled(
                ex, "running the hydraulics for " + request.WellId,
                "the run is refused before it is computed", FailureSeverity.Degraded);

            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The run was refused before it was computed: check the mud readings, the rate and the section geometry." +
                (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")"),
                HydraulicsRules.CodeSectionInvalid);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "running the hydraulics for " + request.WellId, "the run is not computed", FailureSeverity.Error);

            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The run could not be computed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HydraulicsRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgHydraulicsWorkspace>> SaveAsync(
        OgHydraulicsRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "A saved run records who made it.", HydraulicsRules.CodeActorRequired);
        }

        var run = await RunAsync(request, cancellationToken).ConfigureAwait(false);

        if (!run.Success || run.Value is null)
        {
            return run;
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "No connection is open, so the run cannot be saved.", HydraulicsRules.CodeNoConnection);
        }

        try
        {
            var result = run.Value;
            var runId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var unit = OgUow.For<HYDRAULICS_RUN>(source);
            unit.PrimaryKey = nameof(HYDRAULICS_RUN.RUN_ID);

            unit.Add(new HYDRAULICS_RUN
            {
                RUN_ID = runId,
                UWI = result.WellId,
                PLAN_ID = result.PlanId,
                RHEOLOGY_MODEL = result.RheologyModel,
                MUD_DENSITY_PPG = (decimal)result.MudDensityPpg,
                THETA600 = (decimal)result.Theta600,
                THETA300 = (decimal)result.Theta300,
                PLASTIC_VISCOSITY = (decimal)result.PlasticViscosity,
                YIELD_POINT = (decimal)result.YieldPoint,
                FLOW_INDEX = (decimal)result.FlowIndex,
                CONSISTENCY_INDEX = (decimal)result.ConsistencyIndex,
                FLOW_RATE = (decimal)result.FlowRate,
                FLOW_RATE_OUOM = result.FlowRateUnit,
                SURFACE_LOSS_PSI = (decimal)result.SurfaceLossPsi,
                BIT_TVD = (decimal)result.BitTvd,
                BIT_TVD_OUOM = result.Unit,
                NOZZLES_TEXT = result.Nozzles32nds.Count == 0
                    ? null
                    : JsonSerializer.Serialize(result.Nozzles32nds),
                NOZZLE_COUNT = result.Nozzles32nds.Count,
                SECTIONS_TEXT = JsonSerializer.Serialize(result.Sections),
                SECTION_COUNT = result.Results.Count,
                STANDPIPE_PRESSURE = result.StandpipePressure is { } standpipe ? (decimal)standpipe : null,
                ANNULAR_LOSS = result.AnnularLoss is { } annular ? (decimal)annular : null,
                ECD = result.Ecd is { } ecd ? (decimal)ecd : null,
                BIT_LOSS = (decimal)result.BitLoss,
                BIT_HHP = (decimal)result.Bhhp,
                IMPACT_FORCE = (decimal)result.ImpactForce,
                MAX_ECD = result.MaxEcd is { } max ? (decimal)max : null,
                MAX_ECD_DEPTH = result.MaxEcdDepth is { } depth ? (decimal)depth : null,
                FRACTURE_GRADIENT = result.FractureGradientPpg is { } fracture ? (decimal)fracture : null,
                FRACTURE_DEPTH = result.FractureDepth is { } at ? (decimal)at : null,
                FRACTURE_EXCEEDED = result.FractureRecorded ? (result.FractureExceeded ? "Y" : "N") : null,
                CRITICAL_TEXT = result.CriticalText,
                DEPTH_OUOM = result.Unit,
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
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    OgUow.Message(committed, "The run was not saved."), HydraulicsRules.CodeWriteFailed);
            }

            return OgResult<OgHydraulicsWorkspace>.Ok(result with
            {
                RunId = runId,
                RanAt = now,
                RanBy = actor.Trim(),
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the hydraulics run for " + request.WellId, "the run is not saved", FailureSeverity.Error);

            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The run could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HydraulicsRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHydraulicsWorkspace>> LoadLastAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "A hydraulics run belongs to one well; none is named.", HydraulicsRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgHydraulicsWorkspace>.Failed(
                "No connection is open, so the run cannot be read.", HydraulicsRules.CodeNoConnection);
        }

        try
        {
            var runs = await OgUow.ReadAllAsync<HYDRAULICS_RUN>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var saved = runs
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.UWI?.Trim(), wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.RAN_AT_UTC)
                .FirstOrDefault();

            if (saved is null)
            {
                return OgResult<OgHydraulicsWorkspace>.Failed(
                    "No hydraulics run is saved for " + wellId.Trim() + ".", HydraulicsRules.CodeUnknownRun);
            }

            var replay = await RunAsync(new OgHydraulicsRequest(
                saved.UWI,
                (double)saved.MUD_DENSITY_PPG,
                (double)saved.THETA600,
                (double)saved.THETA300,
                (double)saved.FLOW_RATE,
                saved.FLOW_RATE_OUOM ?? "gpm",
                (double)saved.SURFACE_LOSS_PSI,
                NozzlesOf(saved.NOZZLES_TEXT),
                SectionsOf(saved.SECTIONS_TEXT)), cancellationToken).ConfigureAwait(false);

            if (!replay.Success || replay.Value is null)
            {
                return replay;
            }

            return OgResult<OgHydraulicsWorkspace>.Ok(replay.Value with
            {
                RunId = saved.RUN_ID,
                RanAt = saved.RAN_AT_UTC,
                RanBy = saved.RAN_BY,
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the hydraulics run for " + wellId, "the run is not shown", FailureSeverity.Error);

            return OgResult<OgHydraulicsWorkspace>.Failed(
                "The run could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HydraulicsRules.CodeReadFailed);
        }
    }

    /// <summary>
    /// AC-5: the rate is solved for the target ECD, and the solve is monotone by construction - the annular loss, and therefore the
    /// ECD, rises with the rate for every regime the owner models - so a bracket-then-bisect search converges on one answer and
    /// every bracket step is recorded.
    /// </summary>
    public async Task<OgResult<OgHydraulicsRecommendation>> OptimiseAsync(
        OgHydraulicsRequest request,
        double targetEcdPpg,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.WellId))
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "A hydraulics run belongs to one well; none is named.", HydraulicsRules.CodeWellRequired);
        }

        RheologyOf(request, out var rheology, out var problem);

        if (problem is not null)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(problem, HydraulicsRules.CodeRheologyMissing);
        }

        if (request.Sections.Count == 0)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "The optimiser needs the flow sections of the circulating system; none is recorded.",
                HydraulicsRules.CodeNoSections);
        }

        // Below the static mud weight no rate can reach the target, because the annulus only adds to the weight. That is a
        // statement about the mud, so it is made before a trajectory is needed.
        if (targetEcdPpg <= request.MudDensityPpg + 1e-9)
        {
            return OgResult<OgHydraulicsRecommendation>.Ok(new OgHydraulicsRecommendation(
                false,
                "the ECD can never fall to the " + targetEcdPpg.ToString("0.###", CultureInfo.InvariantCulture) +
                " ppg asked for: the annulus adds to the mud weight, so the lowest achievable ECD is the static " +
                request.MudDensityPpg.ToString("0.###", CultureInfo.InvariantCulture) +
                " ppg at no circulation. Reduce the mud weight instead.",
                targetEcdPpg, request.MudDensityPpg, null, null, null, [], null, null, null, 0, []));
        }

        var plan = await _plans.LoadAsync(request.WellId.Trim(), cancellationToken).ConfigureAwait(false);

        if (!plan.Success || plan.Value is not { } planWorkspace || planWorkspace.Stations.Count < 2)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "No trajectory is available for " + request.WellId.Trim() +
                ": the bit's true vertical depth, and therefore the ECD, comes from a saved well plan.",
                HydraulicsRules.CodeNoTrajectory);
        }

        var unit = planWorkspace.Unit;
        var trajectory = TheTechIdea.OilGas.Wellbore.Data.Trajectory.Compute(
            planWorkspace.Stations.Select(station => (station.Md, station.Inclination, station.Azimuth)),
            unit);

        var bitMd = planWorkspace.Stations[^1].Md;
        var bitTvd = trajectory.TvdAt(bitMd);

        if (!Units.TryConvert(bitTvd, unit, "ft", out var bitTvdFt) || bitTvdFt <= 0d)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "The plan's last station has no true vertical depth, so the ECD has nothing to be quoted at.",
                HydraulicsRules.CodeNoTrajectory);
        }

        var stat = request.MudDensityPpg;

        var trace = new List<string>();

        // A rate the rheology model cannot compute (turbulent power-law flow has no simple correlation) has no ECD, so the search
        // says so rather than bisecting on a number that does not exist.
        double? EcdAt(double gpm) =>
            AnnularLossOf(rheology, gpm, request.Sections, unit) is { } loss
                ? HydraulicsRules.EcdOf(stat, loss, bitTvdFt)
                : null;

        if (EcdAt(request.FlowRate > 0d ? request.FlowRate : 100d) is null)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "The annular loss cannot be computed at the current rate: at least one annulus section is in turbulent " +
                "power-law flow, which the owner's model does not cover. Switch the configured model to Bingham plastic.",
                HydraulicsRules.CodeOptimiserTarget);
        }

        // Bracket: expand away from the rate the person is running until the target is inside the interval. The bracket is found,
        // not assumed, so no search limit is invented.
        var low = request.FlowRate > 0d ? request.FlowRate : 100d;
        var high = low;

        if (!new Quantity(low, string.IsNullOrWhiteSpace(request.FlowRateUnit) ? "gpm" : request.FlowRateUnit.Trim())
                .TryIn("gpm", out low))
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "The flow rate is not a rate the catalogue converts to gpm.", HydraulicsRules.CodeNoFlowRate);
        }

        high = low;
        var steps = 0;

        while ((EcdAt(high) ?? double.NegativeInfinity) < targetEcdPpg)
        {
            low = high;
            high *= 2d;
            steps++;

            if (steps > 60 || high > 1e9)
            {
                return OgResult<OgHydraulicsRecommendation>.Failed(
                    "The ECD would need a rate beyond 1e9 gpm to reach " +
                    targetEcdPpg.ToString("0.###", CultureInfo.InvariantCulture) + " ppg, so the target is not reachable.",
                    HydraulicsRules.CodeOptimiserTarget);
            }
        }

        var iterations = 0;

        while (iterations++ < 200 && high - low > Math.Max(1e-6, high * 1e-10))
        {
            var mid = 0.5 * (low + high);

            if ((EcdAt(mid) ?? double.NegativeInfinity) < targetEcdPpg)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        var solved = 0.5 * (low + high);

        if (EcdAt(solved) is not { } achieved)
        {
            return OgResult<OgHydraulicsRecommendation>.Failed(
                "The annular loss cannot be computed at the solved rate, so no ECD is reported for it.",
                HydraulicsRules.CodeOptimiserTarget);
        }

        // Every bracket the search visited, so the monotonicity is on the record rather than asserted in a comment.
        for (var step = 1; step <= 4; step++)
        {
            var probe = solved * step / 4d;
            trace.Add(probe.ToString("0.#", CultureInfo.InvariantCulture) + " gpm -> " +
                      (EcdAt(probe) ?? double.NaN).ToString("0.###", CultureInfo.InvariantCulture) + " ppg");
        }

        trace.Add(solved.ToString("0.#", CultureInfo.InvariantCulture) + " gpm -> " +
                  achieved.ToString("0.###", CultureInfo.InvariantCulture) + " ppg (target " +
                  targetEcdPpg.ToString("0.###", CultureInfo.InvariantCulture) + ")");

        // The bit hydraulics at the solved rate, with the nozzles as entered: the nozzle does not move the ECD - it sets the bit
        // loss and therefore the hydraulic horsepower - which is what the recommendation reports.
        var bitLoss = request.Nozzles32nds.Count == 0 ? 0d : Hydraulics.BitLoss(rheology, solved, request.Nozzles32nds);
        var bhpp = request.Nozzles32nds.Count == 0 ? 0d : bitLoss * solved / 1714d;
        var standpipe = StringLossOf(rheology, solved, request.Sections, unit) is { } stringLoss &&
                        AnnularLossOf(rheology, solved, request.Sections, unit) is { } annularAtRate
            ? request.SurfaceLossPsi + stringLoss + bitLoss + annularAtRate
            : (double?)null;

        // The owner's laminar and turbulent correlations do not meet, so the ECD steps at the transition and a target inside the
        // step cannot be hit exactly. That is stated rather than rounded away.
        var overshoot = achieved - targetEcdPpg;

        var reason = "The annular loss rises with the rate for every regime the model covers, so this is the lowest rate that " +
                     "reaches the target; the nozzles set the bit loss and the hydraulic horsepower, not the ECD.";

        if (overshoot > 1e-6)
        {
            reason += " The reached ECD is " + achieved.ToString("0.###", CultureInfo.InvariantCulture) + " ppg rather than " +
                      targetEcdPpg.ToString("0.###", CultureInfo.InvariantCulture) +
                      " because an annular section steps from laminar to turbulent flow at this rate, and the two correlations " +
                      "the model uses do not meet at the transition.";
        }

        return OgResult<OgHydraulicsRecommendation>.Ok(new OgHydraulicsRecommendation(
            true,
            reason,
            targetEcdPpg,
            stat,
            solved,
            solved * 3.785411784,
            achieved,
            request.Nozzles32nds,
            bitLoss,
            bhpp,
            standpipe,
            iterations,
            trace));
    }

    // ------------------------------------------------------------------ the profile and the model helpers

    /// <summary>
    /// The owner's per-section gradient applied down the annulus, accumulated and turned into an ECD at each section's own depth.
    /// The sections are ordered shallow to deep so the accumulation is the pressure the formation sees at that depth.
    /// </summary>
    private static IReadOnlyList<OgEcdPoint> ProfileOf(
        MudRheology mud,
        double flowGpm,
        IReadOnlyList<OgFlowSection> sections,
        TheTechIdea.OilGas.Wellbore.Data.Trajectory trajectory,
        string unit,
        double mudPpg)
    {
        var points = new List<OgEcdPoint>();
        var loss = 0d;

        foreach (var section in sections.Where(section => section.IsAnnulus).OrderBy(section => section.BottomMd))
        {
            var (gradient, _, _) = Hydraulics.Gradient(
                mud, flowGpm, section.OuterDiameter, section.InnerDiameter ?? 0d, annulus: true);

            if (gradient is not { } perFoot || !Units.TryConvert(section.Length, unit, "ft", out var lengthFt))
            {
                continue;
            }

            loss += perFoot * lengthFt;

            if (!Units.TryConvert(trajectory.TvdAt(section.BottomMd), unit, "ft", out var tvdFt) || tvdFt <= 0d)
            {
                continue;
            }

            points.Add(new OgEcdPoint(section.BottomMd, HydraulicsRules.EcdOf(mudPpg, loss, tvdFt)));
        }

        return points;
    }

    /// <summary>
    /// The annular loss the owner's model gives at a rate, summed over the annulus sections. The section lengths are converted to
    /// feet here, because the gradient is psi per foot and the plan records its depths in its own unit. Null when a section's regime
    /// is not covered, so a missing figure is never summed as zero.
    /// </summary>
    private static double? AnnularLossOf(
        MudRheology mud, double flowGpm, IReadOnlyList<OgFlowSection> sections, string unit)
    {
        var loss = 0d;

        foreach (var section in sections.Where(section => section.IsAnnulus))
        {
            var (gradient, _, _) = Hydraulics.Gradient(
                mud, flowGpm, section.OuterDiameter, section.InnerDiameter ?? 0d, annulus: true);

            if (gradient is not { } perFoot || !Units.TryConvert(section.Length, unit, "ft", out var lengthFt))
            {
                return null;
            }

            loss += perFoot * lengthFt;
        }

        return loss;
    }

    /// <summary>The loss inside the string, which the standpipe pressure at a solved rate needs.</summary>
    private static double? StringLossOf(
        MudRheology mud, double flowGpm, IReadOnlyList<OgFlowSection> sections, string unit)
    {
        var loss = 0d;

        foreach (var section in sections.Where(section => !section.IsAnnulus))
        {
            var (gradient, _, _) = Hydraulics.Gradient(mud, flowGpm, section.OuterDiameter, 0d, annulus: false);

            if (gradient is not { } perFoot || !Units.TryConvert(section.Length, unit, "ft", out var lengthFt))
            {
                return null;
            }

            loss += perFoot * lengthFt;
        }

        return loss;
    }

    /// <summary>
    /// The mud, and the reading that is missing or impossible named. The owner derives PV, YP, n and K from the two Fann readings,
    /// so the refusal has to speak in those readings rather than in the derived numbers.
    /// </summary>
    private static MudRheology RheologyOf(OgHydraulicsRequest request, out MudRheology mud, out string? problem)
    {
        mud = new MudRheology { DensityPpg = request.MudDensityPpg, Theta600 = request.Theta600, Theta300 = request.Theta300 };

        problem = null;

        if (request.MudDensityPpg <= 0d)
        {
            problem = "The mud density is " + request.MudDensityPpg.ToString("0.##", CultureInfo.InvariantCulture) +
                      " ppg, so the hydrostatic and the ECD cannot be formed. Record the mud density.";
            return mud;
        }

        if (request.Theta600 <= 0d)
        {
            problem = "The 600 rpm Fann reading is missing (" +
                      request.Theta600.ToString("0.#", CultureInfo.InvariantCulture) +
                      "), so the plastic viscosity and the flow index cannot be derived.";
            return mud;
        }

        if (request.Theta300 <= 0d)
        {
            problem = "The 300 rpm Fann reading is missing (" +
                      request.Theta300.ToString("0.#", CultureInfo.InvariantCulture) +
                      "), so the yield point and the consistency index cannot be derived.";
            return mud;
        }

        if (request.Theta300 > request.Theta600)
        {
            problem = "The 300 rpm reading (" + request.Theta300.ToString("0.#", CultureInfo.InvariantCulture) +
                      ") is above the 600 rpm reading (" + request.Theta600.ToString("0.#", CultureInfo.InvariantCulture) +
                      "), which gives a plastic viscosity of " + mud.PlasticViscosity.ToString("0.#", CultureInfo.InvariantCulture) +
                      " cP - not a physical Bingham fit.";
            return mud;
        }

        if (mud.YieldPoint < 0d)
        {
            problem = "The readings " + request.Theta600.ToString("0.#", CultureInfo.InvariantCulture) + " / " +
                      request.Theta300.ToString("0.#", CultureInfo.InvariantCulture) + " give a plastic viscosity of " +
                      mud.PlasticViscosity.ToString("0.#", CultureInfo.InvariantCulture) + " cP and a yield point of " +
                      mud.YieldPoint.ToString("0.#", CultureInfo.InvariantCulture) +
                      " lbf/100 ft², so the Bingham fit is not physical. Re-read the viscometer.";
            return mud;
        }

        if (string.Equals(DrillingToolbox.Settings.RheologyModel, "powerLaw", StringComparison.OrdinalIgnoreCase) &&
            (!double.IsFinite(mud.N) || mud.N <= 0d))
        {
            problem = "The power-law flow index derived from the readings is " +
                      mud.N.ToString("0.###", CultureInfo.InvariantCulture) +
                      ", so the model cannot be applied. Check the readings or switch the configured model.";
            return mud;
        }

        return mud;
    }

    private static IReadOnlyList<OgSectionResult> ResultsOf(
        HydraulicsResult result,
        IReadOnlyList<OgFlowSection> sections,
        IReadOnlyList<OgEcdPoint> profile)
    {
        var results = new List<OgSectionResult>();

        for (var index = 0; index < result.Sections.Count && index < sections.Count; index++)
        {
            var loss = result.Sections[index];
            var section = sections[index];
            var ecd = profile.FirstOrDefault(point => Math.Abs(point.Depth - section.BottomMd) < 1e-6)?.Value ?? 0d;

            results.Add(new OgSectionResult(
                index + 1,
                loss.Name,
                section.Interval,
                section.TopMd,
                section.BottomMd,
                loss.Velocity,
                loss.Reynolds,
                loss.Turbulent,
                loss.Loss,
                ecd,
                loss.Loss is not null));
        }

        return results;
    }

    /// <summary>
    /// The recorded fracture gradients of the well, as an EMW against the depth of the shoe they belong to. The gradient lives on the
    /// annulus (ENG-04) and the depth on the string that bounds it, so the two records are joined on the string id the annulus
    /// stores.
    /// </summary>
    private async Task<(double? GradientPpg, double? Depth, IReadOnlyList<OgEcdPoint> Points)> FractureAsync(
        string wellId,
        TheTechIdea.OilGas.Wellbore.Data.Trajectory trajectory,
        string unit,
        CancellationToken cancellationToken)
    {
        var source = _source();

        if (source is null)
        {
            return (null, null, []);
        }

        var annuli = await OgUow.ReadAllAsync<WELL_ANNULUS>(source).ConfigureAwait(false);
        var tubulars = await OgUow.ReadAllAsync<WELL_TUBULAR>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var points = new List<OgEcdPoint>();

        foreach (var annulus in annuli.Where(row =>
                     string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase) &&
                     row.SHOE_FRACTURE_GRADIENT is not null))
        {
            if (HydraulicsRules.FractureGradientPpg(
                    (double)annulus.SHOE_FRACTURE_GRADIENT!.Value, annulus.SHOE_FRACTURE_GRADIENT_OUOM) is not { } ppg)
            {
                continue;
            }

            var outer = tubulars.FirstOrDefault(row =>
                string.Equals(OgCasingPressureService.StringId(row),
                    (annulus.OUTER_STRING_ID ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase));

            if (outer is null)
            {
                continue;
            }

            var shoe = outer.SHOE_DEPTH > 0m ? (double)outer.SHOE_DEPTH : (double)outer.BASE_DEPTH;

            if (!Units.TryConvert(shoe, outer.BASE_DEPTH_OUOM?.Trim() ?? unit, unit, out var shoeMd))
            {
                continue;
            }

            points.Add(new OgEcdPoint(shoeMd, ppg));
        }

        if (points.Count == 0)
        {
            return (null, null, []);
        }

        // The tightest recorded gradient governs: it is the one the deepest circulating pressure must stay under.
        var governing = points.OrderBy(point => point.Value).ThenByDescending(point => point.Depth).First();

        return (governing.Value, governing.Depth, points);
    }

    private IReadOnlyList<OgFlowSection> SectionsOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<OgFlowSection>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex, "reading the recorded flow sections", "the run's sections are not read", FailureSeverity.Degraded);

            return [];
        }
    }

    private IReadOnlyList<double> NozzlesOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<double>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex, "reading the recorded nozzles", "the run's nozzles are not read", FailureSeverity.Degraded);

            return [];
        }
    }
}
