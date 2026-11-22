using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One casing section of a design (ENG-02's <c>OgCasingSection</c>).</summary>
/// <param name="Wall">
/// The wall thickness in inches, as recorded, or null when only the nominal weight is known - in which case the check resolves it
/// from the owner's own pipe catalogue, or refuses the section.
/// </param>
public sealed record OgCasingSection(
    string Id,
    int Order,
    string Name,
    decimal Od,
    decimal? Wall,
    decimal Weight,
    string? Grade,
    decimal Top,
    decimal Bottom,
    string Unit)
{
    /// <summary>The recorded length in the section's own unit.</summary>
    public decimal Length => Bottom - Top;

    public string Describe() =>
        Order.ToString(CultureInfo.InvariantCulture) + ". " + Name + " · " + Od.ToString("0.###", CultureInfo.InvariantCulture) +
        "\" · " + (Wall is { } wall ? wall.ToString("0.###", CultureInfo.InvariantCulture) + "\" wall" : "wall unknown") +
        " · " + Weight.ToString("0.##", CultureInfo.InvariantCulture) + " lb/ft · " + (Grade ?? "no grade") +
        " · " + Top.ToString("0.#", CultureInfo.InvariantCulture) + "-" + Bottom.ToString("0.#", CultureInfo.InvariantCulture) +
        " " + Unit;
}

/// <summary>The design factors the check applied (ENG-02, AC-4).</summary>
public sealed record OgCasingDesignFactors(decimal Burst, decimal Collapse, decimal Tension)
{
    public string Describe() =>
        "burst " + Burst.ToString("0.###", CultureInfo.InvariantCulture) +
        " · collapse " + Collapse.ToString("0.###", CultureInfo.InvariantCulture) +
        " · tension " + Tension.ToString("0.###", CultureInfo.InvariantCulture) + " (from configuration)";

    public decimal Of(string loadCase) => loadCase switch
    {
        CasingDesignRules.Burst => Burst,
        CasingDesignRules.Collapse => Collapse,
        _ => Tension,
    };
}

/// <summary>One section's verdict for one load case (ENG-02's <c>OgCasingVerdict</c>).</summary>
public sealed record OgCasingVerdict(
    string SectionId,
    string Section,
    string LoadCase,
    decimal Load,
    decimal Resistance,
    decimal Utilisation,
    decimal RequiredFactor,
    bool Passes)
{
    /// <summary>The achieved design factor: resistance over load.</summary>
    public decimal Factor => Load <= 0m ? 0m : Resistance / Load;

    public string Unit => LoadCase == CasingDesignRules.Tension ? "lbf" : "psi";

    public string VerdictText => Passes ? "passes" : "fails";

    public string Describe() =>
        Section + " · " + LoadCase.ToLowerInvariant() + " · load " +
        Load.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · resistance " +
        Resistance.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · utilisation " +
        Utilisation.ToString("0.###", CultureInfo.InvariantCulture) + " against " +
        RequiredFactor.ToString("0.###", CultureInfo.InvariantCulture) + " · " + VerdictText;
}

/// <summary>The casing design workspace (ENG-02).</summary>
public sealed record OgCasingDesignWorkspace(
    string DesignId,
    string? WellName,
    string? DepthUnit,
    IReadOnlyList<OgCasingSection> Sections,
    IReadOnlyList<OgCasingVerdict> Verdicts,
    OgCasingDesignFactors Factors,
    string? GoverningCase,
    decimal? GoverningUtilisation,
    string? Outcome,
    string? RunId,
    DateTime? RunAt,
    string? RunBy,
    IReadOnlyList<string> Warnings,
    DateTime ReadAt)
{
    /// <summary>AC-3: the section and case that decide the design.</summary>
    public bool Passed => string.Equals(Outcome, "PASS", StringComparison.OrdinalIgnoreCase);

    public int Failed => Verdicts.Count(verdict => !verdict.Passes);

    public string CountsText =>
        Sections.Count.ToString(CultureInfo.InvariantCulture) + " section(s) · " +
        Verdicts.Count.ToString(CultureInfo.InvariantCulture) + " verdict(s) · " +
        Failed.ToString(CultureInfo.InvariantCulture) + " failing · " +
        (Outcome ?? "not checked");

    public string SectionsText => Sections.Count == 0
        ? "No casing section is recorded for this design."
        : string.Join(System.Environment.NewLine, Sections.Select(section => section.Describe()));

    public string VerdictsText => Verdicts.Count == 0
        ? "No verdict is recorded: run the check to rate the sections."
        : string.Join(System.Environment.NewLine, Verdicts.Select(verdict => verdict.Describe()));

    public string GoverningText => Verdicts.Count == 0
        ? "No governing case: the design has not been checked."
        : "Governing case: " + (GoverningCase ?? "(none)") + " at utilisation " +
          (GoverningUtilisation ?? 0m).ToString("0.###", CultureInfo.InvariantCulture) +
          (Passed ? " - within its design factor." : " - outside its design factor.");

    public string WarningsText => Warnings.Count == 0
        ? "The records supplied every input the check needed."
        : string.Join(System.Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string RunText => RunId is null
        ? "No run is recorded."
        : "run " + RunId + " · " + (RunAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "") +
          " · " + (RunBy ?? "no actor") + " · " + Factors.Describe();

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The casing-design rules ENG-02 states once.</summary>
public static class CasingDesignRules
{
    public const string CodeNoConnection = "casing-no-connection";
    public const string CodeReadFailed = "casing-read-failed";
    public const string CodeWriteFailed = "casing-write-failed";
    public const string CodeDesignRequired = "casing-design-required";
    public const string CodeDesignUnknown = "casing-design-unknown";
    public const string CodeNoSections = "casing-no-sections";
    public const string CodeNoLoadCase = "casing-no-load-case";
    public const string CodeMixedUnits = "casing-mixed-units";
    public const string CodeUnknownGrade = "casing-grade-unknown";
    public const string CodeNoWall = "casing-no-wall";
    public const string CodeNotRated = "casing-not-rated";
    public const string CodeSectionInvalid = "casing-section-invalid";
    public const string CodeActorRequired = "casing-actor-required";

    public const string Burst = "BURST";
    public const string Collapse = "COLLAPSE";
    public const string Tension = "TENSION";

    public static IReadOnlyList<string> Cases { get; } = [Burst, Collapse, Tension];

    /// <summary>
    /// AC-2: the design utilisation of one case - the load times the required factor over the resistance, so above 1 is a
    /// failure. The achieved factor (resistance / load) is its reciprocal.
    /// </summary>
    public static decimal Utilisation(decimal load, decimal resistance, decimal requiredFactor) =>
        resistance <= 0m ? decimal.MaxValue : load * requiredFactor / resistance;

    /// <summary>
    /// AC-4: the factors come from the owner's configuration, never from the screen.
    /// </summary>
    public static OgCasingDesignFactors FactorsOf(CasingDesignSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new OgCasingDesignFactors(
            (decimal)settings.DesignFactors.Burst,
            (decimal)settings.DesignFactors.Collapse,
            (decimal)settings.DesignFactors.Tension);
    }

    /// <summary>AC-5: the minimum yield of a grade, or null when the configuration does not carry it.</summary>
    public static double? YieldOf(string? grade, CasingDesignSettings settings) =>
        CasingDesign.YieldOf(grade, settings);

    /// <summary>The governing case of a set of verdicts: the highest utilisation, with its section and case named (AC-3).</summary>
    public static (string? Case, decimal? Utilisation) Governing(IReadOnlyList<OgCasingVerdict> verdicts)
    {
        ArgumentNullException.ThrowIfNull(verdicts);

        if (verdicts.Count == 0)
        {
            return (null, null);
        }

        var worst = verdicts
            .OrderByDescending(verdict => verdict.Utilisation)
            .ThenBy(verdict => verdict.Section, StringComparer.Ordinal)
            .ThenBy(verdict => verdict.LoadCase, StringComparer.Ordinal)
            .First();

        return (worst.Section + " · " + worst.LoadCase, worst.Utilisation);
    }

    /// <summary>
    /// The depth in feet, because the API ratings and the owner's load arithmetic are in oilfield units. A unit the catalogue
    /// cannot convert is refused rather than assumed.
    /// </summary>
    public static bool TryFeet(decimal value, string? unit, out double feet)
    {
        feet = 0;

        return Units.TryConvert((double)value, string.IsNullOrWhiteSpace(unit) ? "m" : unit.Trim(), "ft", out feet);
    }

    /// <summary>The plain-end weight of a pipe per API 5CT, used when the string records no nominal weight.</summary>
    public static double PlainEndWeight(double odIn, double wallIn) => 10.69 * (odIn - wallIn) * wallIn;

    /// <summary>AC-2's counterpart: whether a verdict meets its factor.</summary>
    public static bool Passes(decimal utilisation) => utilisation <= 1m;
}

/// <summary>ENG-02: the casing design, its check and the recorded runs.</summary>
public interface IOgCasingDesign
{
    /// <summary>Reads the design's sections and its most recent run.</summary>
    Task<OgResult<OgCasingDesignWorkspace>> LoadAsync(
        string designId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rates every section against the simple load cases and records the run: burst from the surface test pressure, collapse from
    /// full evacuation at the shoe, tension from the buoyed string weight plus the overpull.
    /// </summary>
    Task<OgResult<OgCasingDesignWorkspace>> CheckAsync(
        string designId,
        decimal fluidGradientPpg,
        decimal testPressurePsi,
        decimal overpullLbf,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a section, with the wall and top the rating needs (AC-6).</summary>
    Task<OgResult<OgCasingDesignWorkspace>> AddSectionAsync(
        string designId,
        string name,
        decimal od,
        decimal wall,
        decimal weight,
        string grade,
        decimal top,
        decimal bottom,
        string depthUnit,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-02 over the DevelopmentPlanning-owned <c>DEVELOPMENT_WELL_DESIGN</c>, <c>DEVELOPMENT_CASING_STRING</c>,
/// <c>CASING_DESIGN_RUN</c> and <c>CASING_DESIGN_VERDICT</c>, using the tools' <see cref="CasingDesign"/> for the API 5C3
/// ratings.
/// </summary>
/// <remarks>
/// <para>
/// The owner says of its own casing string table that the burst, collapse and tension arithmetic belongs to ENG-02 - so the
/// arithmetic here is the tools' <see cref="CasingDesign.Burst"/>, <see cref="CasingDesign.Collapse"/>,
/// <see cref="CasingDesign.Tension"/> and <see cref="CasingDesign.Rate"/>, and the design factors are the owner's configured
/// ones. What this service adds is the load side (evacuation, test pressure, buoyed weight), the section-level verdicts and the
/// record.
/// </para>
/// <para>
/// A section that cannot be rated is refused with its reason - an unknown grade, an unresolved wall - rather than being given a
/// verdict computed from a guessed wall.
/// </para>
/// </remarks>
public sealed class OgCasingDesignService : IOgCasingDesign
{
    private const double PpgToPsiPerFt = 0.051948;

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgCasingDesignService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgCasingDesignWorkspace>> LoadAsync(
        string designId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(designId))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A casing design is read by the design it belongs to; none is named.",
                CasingDesignRules.CodeDesignRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "No connection is open, so the casing design cannot be read.",
                CasingDesignRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, designId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the casing design " + designId, "the design is not shown", FailureSeverity.Error);

            return OgResult<OgCasingDesignWorkspace>.Failed(
                "The casing design could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? CasingDesignRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgCasingDesignWorkspace>> CheckAsync(
        string designId,
        decimal fluidGradientPpg,
        decimal testPressurePsi,
        decimal overpullLbf,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A casing check records who ran it.", CasingDesignRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(designId))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A casing check runs against one design; none is named.", CasingDesignRules.CodeDesignRequired);
        }

        // The feature's own state: a check with no load case has nothing to check against.
        if (fluidGradientPpg <= 0m && testPressurePsi <= 0m)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "No load case was given: a check needs the mud density outside the casing for collapse, the surface test " +
                "pressure for burst, or both.",
                CasingDesignRules.CodeNoLoadCase);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "No connection is open, so the casing check cannot run.",
                CasingDesignRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, designId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var workspace = read.Value;
            var settings = WellboreEnvironment.Current.CasingDesign;
            var factors = CasingDesignRules.FactorsOf(settings);

            if (workspace.Sections.Count == 0)
            {
                return OgResult<OgCasingDesignWorkspace>.Failed(
                    workspace.WellName + " records no casing section, so there is nothing to rate.",
                    CasingDesignRules.CodeNoSections);
            }

            // Mixed units are refused before anything is rated, because a load computed across two units is wrong silently.
            foreach (var section in workspace.Sections)
            {
                if (string.IsNullOrWhiteSpace(section.Unit) || string.IsNullOrWhiteSpace(workspace.DepthUnit))
                {
                    continue;
                }

                if (!string.Equals(section.Unit.Trim(), workspace.DepthUnit.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        workspace.WellName + "'s design is recorded in '" + workspace.DepthUnit + "' but " + section.Name +
                        " is recorded in '" + section.Unit + "', so the sections are in mixed units and are not checked.",
                        CasingDesignRules.CodeMixedUnits);
                }
            }

            var gradientPerFoot = fluidGradientPpg * (decimal)PpgToPsiPerFt;
            var steelPpg = (decimal)settings.SteelDensity.In("ppg");

            var trajectory = await TrajectoryAsync(source, workspace, cancellationToken).ConfigureAwait(false);

            var verdicts = new List<OgCasingVerdict>();
            var warnings = new List<string>(workspace.Warnings);

            if (trajectory is null)
            {
                warnings.Add(
                    "No survey is recorded for the planned well, so each section's shoe depth is used as its true vertical " +
                    "depth - the collapse load of a deviated well would be over-stated.");
            }

            foreach (var section in workspace.Sections)
            {
                if (section.Bottom <= section.Top)
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        section.Name + " records its shoe at " + section.Bottom.ToString("0.#", CultureInfo.InvariantCulture) +
                        " " + section.Unit + " and its top at " + section.Top.ToString("0.#", CultureInfo.InvariantCulture) +
                        " " + section.Unit + ", so it has no length to rate.",
                        CasingDesignRules.CodeSectionInvalid);
                }

                var rating = Rate(section, settings);

                // AC-5: a grade the configuration does not carry is refused, with the grade named.
                if (CasingDesignRules.YieldOf(section.Grade, settings) is null)
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        section.Name + " records the grade '" + (string.IsNullOrWhiteSpace(section.Grade) ? "(none)" : section.Grade) +
                        "', which has no minimum yield in casingDesign.gradeYield, so it cannot be rated.",
                        CasingDesignRules.CodeUnknownGrade);
                }

                // The feature's own state: a missing wall is refused rather than rated on a guessed one.
                if (rating.Wall is null)
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        section.Name + " records neither a wall thickness nor a nominal weight the pipe catalogue knows, so its " +
                        "burst, collapse and tension cannot be rated. Record the wall.",
                        CasingDesignRules.CodeNoWall);
                }

                if (rating.Burst is null || rating.Collapse is null || rating.Tension is null)
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        section.Name + " could not be rated: " + string.Join(" ", rating.Notes),
                        CasingDesignRules.CodeNotRated);
                }

                var lengthFeet = (double)section.Length;
                var unit = string.IsNullOrWhiteSpace(section.Unit) ? workspace.DepthUnit ?? "ft" : section.Unit;

                if (CasingDesignRules.TryFeet(section.Length, unit, out var length))
                {
                    lengthFeet = length;
                }

                if (!CasingDesignRules.TryFeet(section.Bottom, unit, out var shoeFeet))
                {
                    return OgResult<OgCasingDesignWorkspace>.Failed(
                        section.Name + " records its shoe in '" + unit + "', which is not a depth the catalogue converts.",
                        CasingDesignRules.CodeSectionInvalid);
                }

                // The collapse load is the mud gradient at this section's own shoe, so each section asks the trajectory itself.
                var tvd = trajectory?.TvdAt(shoeFeet) ?? shoeFeet;

                // Burst: the surface test pressure applied over the string.
                if (testPressurePsi > 0m)
                {
                    verdicts.Add(Verdict(section, CasingDesignRules.Burst, testPressurePsi,
                        (decimal)rating.Burst.Value, factors.Of(CasingDesignRules.Burst)));
                }

                // Collapse: full evacuation at the shoe - the mud gradient times the shoe's true vertical depth.
                if (fluidGradientPpg > 0m)
                {
                    verdicts.Add(Verdict(section, CasingDesignRules.Collapse, gradientPerFoot * (decimal)tvd,
                        (decimal)rating.Collapse.Value, factors.Of(CasingDesignRules.Collapse)));
                }

                // Tension: the buoyed weight of the section, plus the overpull being held.
                var weightPerFoot = section.Weight > 0m
                    ? (double)section.Weight
                    : CasingDesignRules.PlainEndWeight((double)section.Od, rating.Wall.Value);

                var buoyancy = fluidGradientPpg > 0m && steelPpg > 0m
                    ? 1d - (double)fluidGradientPpg / (double)steelPpg
                    : 1d;

                var tension = (decimal)(weightPerFoot * lengthFeet * buoyancy) + overpullLbf;

                verdicts.Add(Verdict(section, CasingDesignRules.Tension, tension,
                    (decimal)rating.Tension.Value, factors.Of(CasingDesignRules.Tension)));
            }

            var (governing, governingUtilisation) = CasingDesignRules.Governing(verdicts);
            var passed = governingUtilisation is <= 1m;

            var runId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            // The verdicts are written first: a run whose detail failed to save must not claim a result it cannot show.
            var verdictUnit = OgUow.For<CASING_DESIGN_VERDICT>(source);
            verdictUnit.PrimaryKey = nameof(CASING_DESIGN_VERDICT.VERDICT_ID);

            foreach (var verdict in verdicts)
            {
                verdictUnit.Add(new CASING_DESIGN_VERDICT
                {
                    VERDICT_ID = Guid.NewGuid().ToString("N"),
                    RUN_ID = runId,
                    DESIGN_ID = workspace.DesignId,
                    CASING_STRING_ID = verdict.SectionId,
                    SECTION_NAME = verdict.Section,
                    LOAD_CASE = verdict.LoadCase,
                    LOAD = decimal.Round(verdict.Load, 2, MidpointRounding.AwayFromZero),
                    RESISTANCE = verdict.Resistance,
                    UTILISATION = decimal.Round(verdict.Utilisation, 6, MidpointRounding.AwayFromZero),
                    REQUIRED_FACTOR = verdict.RequiredFactor,
                    PASSES = verdict.Passes ? "Y" : "N",
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var verdictCommit = await verdictUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(verdictCommit))
            {
                return OgResult<OgCasingDesignWorkspace>.Failed(
                    OgUow.Message(verdictCommit, "The verdicts were not saved."), CasingDesignRules.CodeWriteFailed);
            }

            var runUnit = OgUow.For<CASING_DESIGN_RUN>(source);
            runUnit.PrimaryKey = nameof(CASING_DESIGN_RUN.RUN_ID);

            runUnit.Add(new CASING_DESIGN_RUN
            {
                RUN_ID = runId,
                DESIGN_ID = workspace.DesignId,
                FLUID_GRADIENT_PPG = fluidGradientPpg,
                TEST_PRESSURE_PSI = testPressurePsi,
                OVERPULL_LBF = overpullLbf,
                DESIGN_FACTORS_JSON = JsonSerializer.Serialize(factors),
                GOVERNING_CASE = governing,
                GOVERNING_UTILISATION = governingUtilisation ?? 0m,
                OUTCOME = passed ? "PASS" : "FAIL",
                SECTIONS_CHECKED = workspace.Sections.Count,
                CHECKED_BY = actor.Trim(),
                CHECKED_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var runCommit = await runUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(runCommit))
            {
                return OgResult<OgCasingDesignWorkspace>.Failed(
                    OgUow.Message(runCommit, "The run was not saved, so the verdicts are not published."),
                    CasingDesignRules.CodeWriteFailed);
            }

            return await ReadAsync(source, workspace.DesignId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "checking the casing design " + designId, "the check is not recorded", FailureSeverity.Error);

            return OgResult<OgCasingDesignWorkspace>.Failed(
                "The casing check could not be completed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? CasingDesignRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCasingDesignWorkspace>> AddSectionAsync(
        string designId,
        string name,
        decimal od,
        decimal wall,
        decimal weight,
        string grade,
        decimal top,
        decimal bottom,
        string depthUnit,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "Recording a section records who did it.", CasingDesignRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(designId))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A section belongs to one design; none is named.", CasingDesignRules.CodeDesignRequired);
        }

        if (string.IsNullOrWhiteSpace(grade))
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A casing section records its grade; none was given.", CasingDesignRules.CodeUnknownGrade);
        }

        if (CasingDesignRules.YieldOf(grade, WellboreEnvironment.Current.CasingDesign) is null)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "'" + grade + "' has no minimum yield in casingDesign.gradeYield, so it cannot be rated and is not recorded.",
                CasingDesignRules.CodeUnknownGrade);
        }

        if (od <= 0m || wall <= 0m || bottom <= top)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "A section needs a positive outside diameter and wall, and a shoe below its top.",
                CasingDesignRules.CodeSectionInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "No connection is open, so the section cannot be recorded.", CasingDesignRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, designId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            if (!string.IsNullOrWhiteSpace(read.Value.DepthUnit) && !string.IsNullOrWhiteSpace(depthUnit) &&
                !string.Equals(read.Value.DepthUnit.Trim(), depthUnit.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCasingDesignWorkspace>.Failed(
                    read.Value.WellName + "'s design is recorded in '" + read.Value.DepthUnit + "', so a section in '" +
                    depthUnit + "' would mix units and is refused.",
                    CasingDesignRules.CodeMixedUnits);
            }

            var unit = CasingDesignRunUnit(source);
            var now = DateTime.UtcNow;

            unit.Add(new DEVELOPMENT_CASING_STRING
            {
                CASING_STRING_ID = Guid.NewGuid().ToString("N"),
                DESIGN_ID = read.Value.DesignId,
                STRING_ORDER = read.Value.Sections.Count + 1,
                STRING_NAME = string.IsNullOrWhiteSpace(name) ? null : name.Trim().ToUpperInvariant(),
                OD = od,
                WEIGHT = weight,
                GRADE = grade.Trim(),
                SET_DEPTH = bottom,
                DEPTH_OUOM = string.IsNullOrWhiteSpace(depthUnit) ? read.Value.DepthUnit : depthUnit.Trim(),
                WALL_THICKNESS = wall,
                TOP_DEPTH = top,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCasingDesignWorkspace>.Failed(
                    OgUow.Message(committed, "The section was not recorded."), CasingDesignRules.CodeWriteFailed);
            }

            return await ReadAsync(source, read.Value.DesignId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a casing section on " + designId, "the section is not recorded", FailureSeverity.Error);

            return OgResult<OgCasingDesignWorkspace>.Failed(
                "The section could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? CasingDesignRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the rating and the loads

    private static CasingRating Rate(OgCasingSection section, CasingDesignSettings settings)
    {
        var str = new TubularString
        {
            Name = section.Name,
            Kind = StringKind.Production,
            Top = new Quantity((double)section.Top, string.IsNullOrWhiteSpace(section.Unit) ? "m" : section.Unit),
            Bottom = new Quantity((double)section.Bottom, string.IsNullOrWhiteSpace(section.Unit) ? "m" : section.Unit),
        };

        str.Attributes.OuterDiameter = new Quantity((double)section.Od, "in");
        str.Attributes.Grade = section.Grade;

        // A recorded wall is carried as the inside diameter, which is how the owner's own Rate takes an exact wall.
        if (section.Wall is { } wall && wall > 0m && section.Od - 2m * wall > 0m)
        {
            str.Attributes.InnerDiameter = new Quantity((double)(section.Od - 2m * wall), "in");
        }

        if (section.Weight > 0m)
        {
            str.Attributes.Weight = new Quantity((double)section.Weight, "lbm/ft");
        }

        return CasingDesign.Rate(str, settings);
    }

    private static OgCasingVerdict Verdict(
        OgCasingSection section,
        string loadCase,
        decimal load,
        decimal resistance,
        decimal requiredFactor)
    {
        var utilisation = CasingDesignRules.Utilisation(load, resistance, requiredFactor);

        return new OgCasingVerdict(
            section.Id,
            section.Name,
            loadCase,
            load,
            resistance,
            utilisation,
            requiredFactor,
            CasingDesignRules.Passes(utilisation));
    }

    /// <summary>
    /// The planned well's trajectory from its recorded survey, in feet - the same construction the owner's own check makes, so a
    /// deviated well's collapse load uses its true vertical depth rather than its measured depth. Null when no survey is
    /// recorded, which the caller states.
    /// </summary>
    private static async Task<Trajectory?> TrajectoryAsync(
        IDataSource source,
        OgCasingDesignWorkspace workspace,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspace.DesignId) || workspace.Sections.Count == 0)
        {
            return null;
        }

        // The design's own planned well is the UWI the survey is recorded against.
        var designs = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_DESIGN>(source).ConfigureAwait(false);

        var design = designs.FirstOrDefault(row =>
            string.Equals(row.DESIGN_ID, workspace.DesignId, StringComparison.OrdinalIgnoreCase));

        if (design is null || string.IsNullOrWhiteSpace(design.PLANNED_WELL_ID))
        {
            return null;
        }

        var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);

        var mine = stations
            .Where(row => string.Equals(row.UWI?.Trim(), design.PLANNED_WELL_ID.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.STATION_MD)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        if (mine.Count < 2)
        {
            return null;
        }

        var rows = new List<(double Md, double Inc, double Az)>();

        foreach (var row in mine)
        {
            if (!Units.TryConvert((double)row.STATION_MD, row.STATION_MD_OUOM?.Trim() ?? "m", "ft", out var md))
            {
                return null;
            }

            rows.Add((md, (double)row.INCLINATION, (double)row.AZIMUTH));
        }

        return Trajectory.Compute(rows, "ft");
    }

    private static UnitofWork<DEVELOPMENT_CASING_STRING> CasingDesignRunUnit(IDataSource source)
    {
        var unit = OgUow.For<DEVELOPMENT_CASING_STRING>(source);
        unit.PrimaryKey = nameof(DEVELOPMENT_CASING_STRING.CASING_STRING_ID);

        return unit;
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgCasingDesignWorkspace>> ReadAsync(
        IDataSource source,
        string designId,
        CancellationToken cancellationToken)
    {
        var designs = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_DESIGN>(source).ConfigureAwait(false);
        var strings = await OgUow.ReadAllAsync<DEVELOPMENT_CASING_STRING>(source).ConfigureAwait(false);
        var runs = await OgUow.ReadAllAsync<CASING_DESIGN_RUN>(source).ConfigureAwait(false);
        var verdicts = await OgUow.ReadAllAsync<CASING_DESIGN_VERDICT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var design = designs.FirstOrDefault(row =>
            string.Equals(row.DESIGN_ID, designId, StringComparison.OrdinalIgnoreCase));

        if (design is null)
        {
            return OgResult<OgCasingDesignWorkspace>.Failed(
                "Casing design '" + designId + "' is not recorded.", CasingDesignRules.CodeDesignUnknown);
        }

        var sections = strings
            .Where(row => string.Equals(row.DESIGN_ID, design.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.STRING_ORDER)
            .Select(row => new OgCasingSection(
                row.CASING_STRING_ID,
                row.STRING_ORDER,
                string.IsNullOrWhiteSpace(row.STRING_NAME) ? "STRING " + row.STRING_ORDER : row.STRING_NAME!.Trim(),
                row.OD,
                row.WALL_THICKNESS,
                row.WEIGHT,
                row.GRADE,
                row.TOP_DEPTH ?? 0m,
                row.SET_DEPTH,
                string.IsNullOrWhiteSpace(row.DEPTH_OUOM) ? design.DEPTH_OUOM ?? "m" : row.DEPTH_OUOM!.Trim()))
            .ToList();

        var warnings = new List<string>();

        foreach (var section in sections.Where(section => strings
                     .Any(row => string.Equals(row.CASING_STRING_ID, section.Id, StringComparison.OrdinalIgnoreCase) &&
                                 row.TOP_DEPTH is null)))
        {
            warnings.Add(section.Name + " records no top, so its length is measured from surface (zero).");
        }

        var latest = runs
            .Where(row => string.Equals(row.DESIGN_ID, design.DESIGN_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.CHECKED_AT_UTC)
            .FirstOrDefault();

        var mine = latest is null
            ? []
            : verdicts
                .Where(row => string.Equals(row.RUN_ID, latest.RUN_ID, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.SECTION_NAME, StringComparer.Ordinal)
                .ThenBy(row => CasingDesignRules.Cases.ToList().IndexOf(row.LOAD_CASE))
                .Select(row => new OgCasingVerdict(
                    row.CASING_STRING_ID,
                    row.SECTION_NAME ?? row.CASING_STRING_ID,
                    row.LOAD_CASE,
                    row.LOAD,
                    row.RESISTANCE,
                    row.UTILISATION,
                    row.REQUIRED_FACTOR,
                    string.Equals(row.PASSES?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)))
                .ToList();

        var settings = WellboreEnvironment.Current.CasingDesign;

        return OgResult<OgCasingDesignWorkspace>.Ok(new OgCasingDesignWorkspace(
            design.DESIGN_ID,
            design.WELL_NAME,
            design.DEPTH_OUOM,
            sections,
            mine,
            CasingDesignRules.FactorsOf(settings),
            latest?.GOVERNING_CASE,
            latest?.GOVERNING_UTILISATION,
            latest?.OUTCOME,
            latest?.RUN_ID,
            latest?.CHECKED_AT_UTC,
            latest?.CHECKED_BY,
            warnings,
            DateTime.UtcNow));
    }
}
