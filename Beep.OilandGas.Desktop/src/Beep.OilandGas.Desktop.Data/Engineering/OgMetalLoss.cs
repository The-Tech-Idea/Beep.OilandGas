using System.Globalization;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Integrity;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One casing or tubing string of the well, as the tubular record holds it (ENG-03's nominal pipe).</summary>
public sealed record OgTubularString(
    string Id,
    string Name,
    double? Od,
    double? InnerDiameter,
    string? Grade,
    double Top,
    double Bottom,
    string Unit,
    double? Weight = null,
    string? WeightUnit = null)
{
    public double? NominalWall => Od is { } od && InnerDiameter is { } id && od > id ? (od - id) / 2 : null;

    public string Describe() =>
        Name + " · " + (Od?.ToString("0.###", CultureInfo.InvariantCulture) ?? "?") + "\" OD · " +
        (InnerDiameter?.ToString("0.###", CultureInfo.InvariantCulture) ?? "?") + "\" ID · " +
        (Grade ?? "no grade") +
        (Weight is { } weight ? " · " + weight.ToString("0.##", CultureInfo.InvariantCulture) + " " + (WeightUnit ?? "lb/ft") : string.Empty) +
        " · " + Top.ToString("0.#", CultureInfo.InvariantCulture) + "-" +
        Bottom.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit;
}

/// <summary>One joint's wall-loss statistics and re-rating (ENG-03's <c>OgJointResult</c> and <c>OgRerateResult</c>).</summary>
public sealed record OgJointResult(
    int Joint,
    double Top,
    double Bottom,
    string Unit,
    double? MaxPenetration,
    double? PenetrationDepth,
    double? MaxMetalLoss,
    double? MinRemainingWall,
    double? NominalWall,
    double? MaxOvality,
    double? MinId,
    string? DamageClass,
    string? StringName,
    string? Grade,
    double? WallUsed,
    double? Burst,
    double? Collapse,
    double? Tension,
    double? BurstLoad,
    double? CollapseLoad,
    double? TensionLoad,
    double? NominalBurst,
    double? NominalCollapse,
    double? NominalTension,
    bool? Pass,
    IReadOnlyList<string> Notes)
{
    /// <summary>AC-1's headline figure: the loss as a percentage of the nominal wall.</summary>
    public double? LossPercent => MaxPenetration is { } penetration ? penetration * 100d : null;

    public string LossText => LossPercent is { } loss
        ? loss.ToString("0.##", CultureInfo.InvariantCulture) + " %"
        : "not measured";

    public string VerdictText => Pass is null ? "not rated" : Pass.Value ? "passes" : "fails";

    public string Describe() =>
        "joint " + Joint.ToString(CultureInfo.InvariantCulture) + " · " +
        Top.ToString("0.#", CultureInfo.InvariantCulture) + "-" + Bottom.ToString("0.#", CultureInfo.InvariantCulture) +
        " " + Unit + " · nominal wall " +
        (NominalWall?.ToString("0.###", CultureInfo.InvariantCulture) ?? "?") + "\" · minimum " +
        (MinRemainingWall?.ToString("0.###", CultureInfo.InvariantCulture) ?? "?") + "\" · loss " + LossText +
        " · " + (string.IsNullOrWhiteSpace(DamageClass) ? "no damage class" : DamageClass) + " · " + VerdictText;
}

/// <summary>One load case's re-rating of a joint (ENG-03's <c>OgRerateResult</c>).</summary>
/// <param name="Resistance">What the pipe resists at the measured wall (psi or lbf).</param>
/// <param name="Rating">The allowed load: the resistance over the case's design factor.</param>
public sealed record OgRerateResult(
    string Case,
    double? Resistance,
    double? Rating,
    double? Utilisation,
    bool Passes)
{
    public string Unit => Case == "TENSION" ? "lbf" : "psi";

    public string Describe() =>
        Case.ToLowerInvariant() + " · resistance " + Figure(Resistance) + " " + Unit + " · allowed " + Figure(Rating) +
        " " + Unit + " · utilisation " + Figure(Utilisation) + " · " + (Passes ? "passes" : "fails");

    private static string Figure(double? value) =>
        value?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?";
}

/// <summary>
/// The remaining-life answer, and the basis it rests on (ENG-03, AC-4).
/// </summary>
/// <param name="HasBasis">False when no rate of loss can be formed, in which case nothing is projected.</param>
public sealed record OgRemainingLife(
    bool HasBasis,
    string Basis,
    double? RatePerYear,
    double? YearsRemaining)
{
    public string Describe() => HasBasis
        ? Basis + " · rate " + (RatePerYear ?? 0d).ToString("0.####", CultureInfo.InvariantCulture) + " of the wall per " +
          "year · " + (YearsRemaining is { } years
              ? "about " + years.ToString("0.0", CultureInfo.InvariantCulture) + " year(s) to the damage limit"
              : "no projection to a limit")
        : Basis;
}

/// <summary>The metal-loss workspace (ENG-03).</summary>
public sealed record OgMetalLossWorkspace(
    string WellId,
    string? WellName,
    IReadOnlyList<OgTubularString> Strings,
    string? LogName,
    string? LogReference,
    IReadOnlyList<string> Fingers,
    string JointSource,
    IReadOnlyList<OgJointResult> Joints,
    OgJointResult? Worst,
    IReadOnlyList<OgRerateResult> Rerate,
    OgRemainingLife Life,
    string DamageClassNote,
    IReadOnlyList<string> Warnings,
    string? ResultId,
    DateTime? CheckedAt,
    string? CheckedBy,
    DateTime ReadAt)
{
    /// <summary>AC-1's summary: how much wall has been lost, and where.</summary>
    public string WorstText => Worst is null
        ? "No joint has been measured, so no wall loss is stated."
        : "Worst joint: " + Worst.Describe();

    public int Failed => Joints.Count(joint => joint.Pass == false);

    public int Classified => Joints.Count(joint => !string.IsNullOrWhiteSpace(joint.DamageClass));

    public string CountsText =>
        Joints.Count.ToString(CultureInfo.InvariantCulture) + " joint(s) · " +
        Classified.ToString(CultureInfo.InvariantCulture) + " classified · " +
        Failed.ToString(CultureInfo.InvariantCulture) + " failing · " +
        (ResultId is null ? "not recorded" : "recorded");

    public string StringsText => Strings.Count == 0
        ? "No casing or tubing is recorded for this well, so there is no nominal pipe to measure against."
        : string.Join(System.Environment.NewLine, Strings.Select(item => item.Describe()));

    public string JointsText => Joints.Count == 0
        ? "No joint result is recorded."
        : string.Join(System.Environment.NewLine, Joints.Select(joint => joint.Describe()));

    public string RerateText => Rerate.Count == 0
        ? "No re-rating: a joint is only re-rated when its wall and grade allow it."
        : string.Join(System.Environment.NewLine, Rerate.Select(row => row.Describe()));

    public string WarningsText => Warnings.Count == 0
        ? "The analysis raised no warning."
        : string.Join(System.Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string LogText => LogName is null
        ? "No caliper log is loaded: choose a multi-finger caliper file and analyse it."
        : LogName + " · " + Fingers.Count.ToString(CultureInfo.InvariantCulture) + " finger curve(s) · joints by " +
          JointSource;

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The metal-loss rules ENG-03 states once.</summary>
public static class MetalLossRules
{
    public const string CodeNoConnection = "metal-loss-no-connection";
    public const string CodeReadFailed = "metal-loss-read-failed";
    public const string CodeWriteFailed = "metal-loss-write-failed";
    public const string CodeWellRequired = "metal-loss-well-required";
    public const string CodeNoLog = "metal-loss-no-log";
    public const string CodeNoFingers = "metal-loss-no-fingers";
    public const string CodeUncovered = "metal-loss-uncovered";
    public const string CodeNoLoadCase = "metal-loss-no-load-case";
    public const string CodeActorRequired = "metal-loss-actor-required";
    public const string CodeNoResult = "metal-loss-no-result";

    public const string Burst = "BURST";
    public const string Collapse = "COLLAPSE";
    public const string Tension = "TENSION";

    /// <summary>How far the deepest penetration may move between inspections and still be treated as one location.</summary>
    public const double SameDepthTolerance = 0.5;

    /// <summary>
    /// AC-2: the damage note. The classes are company practice and ship empty, so an unclassified joint is explained rather than
    /// given an invented band.
    /// </summary>
    public static string DamageClassNote(IReadOnlyList<DamageClass> classes) =>
        classes.Count == 0
            ? "No damage class is configured (integrity.damageClasses is empty), so no joint is classified: a class name here " +
              "would be a threshold this build invented."
            : classes.Count.ToString(CultureInfo.InvariantCulture) + " damage class(es) configured: " +
              string.Join(" · ", classes
                  .OrderBy(item => item.PenetrationFrom)
                  .Select(item => item.Name + " from " +
                                  (item.PenetrationFrom * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %"));

    /// <summary>
    /// AC-4: remaining life is projected only when a basis is recorded - an earlier inspection at the same depth, dated, and a
    /// configured damage limit to project to.
    /// </summary>
    public static OgRemainingLife RemainingLife(
        double? previousPenetration,
        double? previousDepth,
        DateTime? previousAt,
        double? currentPenetration,
        double? currentDepth,
        DateTime currentAt,
        double? limit)
    {
        if (previousPenetration is not { } before || previousAt is not { } then)
        {
            return new OgRemainingLife(false,
                "Basis not recorded: no earlier caliper result is stored for this well, so no rate of loss can be formed.",
                null, null);
        }

        if (currentPenetration is not { } now)
        {
            return new OgRemainingLife(false,
                "Basis not recorded: this result has no penetration figure to compare with " +
                then.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".", null, null);
        }

        if (previousDepth is { } was && currentDepth is { } isAt && Math.Abs(was - isAt) > SameDepthTolerance)
        {
            return new OgRemainingLife(false,
                "Basis not recorded: the deepest penetration moved from " + was.ToString("0.#", CultureInfo.InvariantCulture) +
                " to " + isAt.ToString("0.#", CultureInfo.InvariantCulture) + " between the two inspections, so a rate at one " +
                "depth cannot be stated.", null, null);
        }

        var days = (currentAt - then).TotalDays;

        if (days <= 0)
        {
            return new OgRemainingLife(false,
                "Basis not recorded: the two inspections are not separated in time, so no rate can be formed.", null, null);
        }

        var rate = (now - before) / (days / 365.25);

        if (limit is not { } cap)
        {
            return new OgRemainingLife(false,
                "Basis recorded - the worst joint moved from " +
                (before * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % to " +
                (now * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % between " +
                then.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " and " +
                currentAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                " - but no damage limit is configured, so no projection has an end point.",
                rate, null);
        }

        if (rate <= 0d)
        {
            return new OgRemainingLife(false,
                "Basis recorded - the worst joint did not lose wall between " +
                then.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " and " +
                currentAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                " - so no remaining life is projected from a rate of zero.", rate, null);
        }

        if (now >= cap)
        {
            return new OgRemainingLife(true,
                "Basis: the worst joint is at " + (now * 100d).ToString("0.#", CultureInfo.InvariantCulture) +
                " %, at or beyond the " + (cap * 100d).ToString("0.#", CultureInfo.InvariantCulture) +
                " % damage limit, so no further life remains", rate, 0d);
        }

        return new OgRemainingLife(true,
            "Basis: the worst joint moved from " + (before * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % to " +
            (now * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % between " +
            then.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " and " +
            currentAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ", projected to the " +
            (cap * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % damage limit",
            rate,
            (cap - now) / rate);
    }

    /// <summary>The damage limit to project to: the lowest configured class, or null when none is configured.</summary>
    public static double? LimitOf(IReadOnlyList<DamageClass> classes) =>
        classes.Count == 0 ? null : classes.Min(item => item.PenetrationFrom);

    /// <summary>AC-5: the export is built from the same rows the grid shows, column for column.</summary>
    public static string Csv(IReadOnlyList<OgJointResult> joints)
    {
        ArgumentNullException.ThrowIfNull(joints);

        var text = new StringBuilder();
        text.AppendLine("joint,top,bottom,unit,nominal_wall_in,min_wall_in,loss_percent,damage_class,grade,burst_psi,collapse_psi,tension_lbf,verdict");

        foreach (var joint in joints)
        {
            text.AppendLine(string.Join(",",
                joint.Joint.ToString(CultureInfo.InvariantCulture),
                Figure(joint.Top),
                Figure(joint.Bottom),
                Quote(joint.Unit),
                Figure(joint.NominalWall),
                Figure(joint.MinRemainingWall),
                Figure(joint.LossPercent),
                Quote(joint.DamageClass ?? string.Empty),
                Quote(joint.Grade ?? string.Empty),
                Figure(joint.Burst),
                Figure(joint.Collapse),
                Figure(joint.Tension),
                Quote(joint.VerdictText)));
        }

        return text.ToString();
    }

    public static string Figure(double? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>ENG-03: the caliper analysis, the re-rating, the remaining-life basis and the recorded result.</summary>
public interface IOgMetalLoss
{
    /// <summary>Reads the well's strings and the most recent recorded result.</summary>
    Task<OgResult<OgMetalLossWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a multi-finger caliper file, measures the wall loss joint by joint, re-rates the pipe at the wall used and records
    /// the result with the log, the damage classes and the wall basis that produced it.
    /// </summary>
    Task<OgResult<OgMetalLossWorkspace>> AnalyseAsync(
        string wellId,
        string caliperPath,
        decimal fluidGradientPpg,
        decimal testPressurePsi,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-03 over the owner's <c>WELL_TUBULAR</c> and <c>WELL_DIR_SRVY_STATION</c> plus the added <c>MFC_RESULT</c> and
/// <c>MFC_JOINT</c>, using the tools' <see cref="MfcAnalysis"/> and <see cref="Rerating"/>.
/// </summary>
/// <remarks>
/// <para>
/// The feature's open question - that the analysis needs a log set with the right curves - is answered by taking a LAS file and
/// reading it with the same reader INT-01 uses, so a caliper run needs no import path of its own. The nominal pipe comes from the
/// recorded tubular rows rather than being typed in.
/// </para>
/// <para>
/// Nothing is invented where the owner supplies no rule: a joint with no damage class says so (the classes ship empty), and a
/// remaining life is only projected when an earlier dated inspection at the same depth and a configured damage limit both exist.
/// </para>
/// </remarks>
public sealed class OgMetalLossService : IOgMetalLoss
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgMetalLossService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgMetalLossWorkspace>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "A metal-loss result belongs to one well; none is named.", MetalLossRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "No connection is open, so the metal-loss record cannot be read.", MetalLossRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, wellId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the metal-loss record for " + wellId, "the record is not shown", FailureSeverity.Error);

            return OgResult<OgMetalLossWorkspace>.Failed(
                "The metal-loss record could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? MetalLossRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgMetalLossWorkspace>> AnalyseAsync(
        string wellId,
        string caliperPath,
        decimal fluidGradientPpg,
        decimal testPressurePsi,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "A recorded analysis records who ran it.", MetalLossRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "A caliper log is analysed against one well; none is named.", MetalLossRules.CodeWellRequired);
        }

        if (string.IsNullOrWhiteSpace(caliperPath) || !File.Exists(caliperPath))
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "No multi-finger caliper file was found at '" + (caliperPath ?? string.Empty) +
                "', so the analysis cannot run.", MetalLossRules.CodeNoLog);
        }

        if (fluidGradientPpg <= 0m && testPressurePsi <= 0m)
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "No load case was given: re-rating a corroded joint needs the mud density for collapse, the surface test " +
                "pressure for burst, or both.", MetalLossRules.CodeNoLoadCase);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMetalLossWorkspace>.Failed(
                "No connection is open, so the analysis cannot be recorded.", MetalLossRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, wellId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var workspace = read.Value;

            var las = LasReader.LoadFile(caliperPath, into: null, new LasReadOptions());
            var log = las.Log;

            if (log is null || log.Index.Length == 0)
            {
                return OgResult<OgMetalLossWorkspace>.Failed(
                    "The file at '" + caliperPath + "' records no depth-indexed log set, so there is nothing to analyse.",
                    MetalLossRules.CodeNoLog);
            }

            var model = await BuildModelAsync(source, workspace, las.Model, cancellationToken).ConfigureAwait(false);
            var warnings = new List<string>(workspace.Warnings);

            var mfc = MfcAnalysis.Analyze(model, log, null, warnings);

            if (mfc.Joints.Count == 0 || mfc.Joints.All(joint => joint.MaxPenetration is null))
            {
                var top = log.Index.Min();
                var bottom = log.Index.Max();

                return OgResult<OgMetalLossWorkspace>.Failed(
                    "The caliper log covers " + top.ToString("0.#", CultureInfo.InvariantCulture) + "-" +
                    bottom.ToString("0.#", CultureInfo.InvariantCulture) + " " + log.IndexUnit +
                    " and the analysis produced no joint statistics there: " +
                    (warnings.Count == 0
                        ? "no recorded casing or tubing with an outside and inside diameter covers the logged interval."
                        : string.Join(" ", warnings)),
                    MetalLossRules.CodeUncovered);
            }

            var loads = new CasingLoadCase
            {
                FluidGradient = fluidGradientPpg > 0m ? new Measure((double)fluidGradientPpg, "ppg") : null,
                TestPressure = testPressurePsi > 0m ? new Measure((double)testPressurePsi, "psi") : null,
            };

            var ratings = Rerating.Rerate(model, mfc, loads, warnings);

            var joints = mfc.Joints.Select(joint => Map(joint, ratings)).ToList();
            var worst = joints.Where(item => item.MaxPenetration is not null)
                .OrderByDescending(item => item.MaxPenetration)
                .FirstOrDefault();

            var resultId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            var settings = IntegrityToolbox.Settings;

            var unit = OgUow.For<MFC_JOINT>(source);
            unit.PrimaryKey = nameof(MFC_JOINT.MFC_JOINT_ID);

            foreach (var joint in joints)
            {
                unit.Add(new MFC_JOINT
                {
                    MFC_JOINT_ID = Guid.NewGuid().ToString("N"),
                    MFC_RESULT_ID = resultId,
                    UWI = workspace.WellId,
                    JOINT_NUMBER = joint.Joint,
                    TOP_DEPTH = (decimal)joint.Top,
                    BOTTOM_DEPTH = (decimal)joint.Bottom,
                    DEPTH_OUOM = joint.Unit,
                    MAX_PENETRATION = joint.MaxPenetration is { } penetration ? (decimal)penetration : null,
                    PENETRATION_DEPTH = joint.PenetrationDepth is { } depth ? (decimal)depth : null,
                    MAX_METAL_LOSS = joint.MaxMetalLoss is { } loss ? (decimal)loss : null,
                    MIN_REMAINING_WALL = joint.MinRemainingWall is { } wall ? (decimal)wall : null,
                    NOMINAL_WALL = joint.NominalWall is { } nominal ? (decimal)nominal : null,
                    MAX_OVALITY = joint.MaxOvality is { } ovality ? (decimal)ovality : null,
                    MIN_ID = joint.MinId is { } id ? (decimal)id : null,
                    DAMAGE_CLASS = joint.DamageClass,
                    STRING_NAME = joint.StringName,
                    GRADE = joint.Grade,
                    WALL_USED = joint.WallUsed is { } used ? (decimal)used : null,
                    NOMINAL_BURST = joint.NominalBurst is { } nb ? (decimal)nb : null,
                    NOMINAL_COLLAPSE = joint.NominalCollapse is { } nc ? (decimal)nc : null,
                    NOMINAL_TENSION = joint.NominalTension is { } nt ? (decimal)nt : null,
                    BURST = joint.Burst is { } burst ? (decimal)burst : null,
                    COLLAPSE = joint.Collapse is { } collapse ? (decimal)collapse : null,
                    TENSION = joint.Tension is { } tension ? (decimal)tension : null,
                    BURST_LOAD = joint.BurstLoad is { } bl ? (decimal)bl : null,
                    COLLAPSE_LOAD = joint.CollapseLoad is { } cl ? (decimal)cl : null,
                    TENSION_LOAD = joint.TensionLoad is { } tl ? (decimal)tl : null,
                    PASSES = joint.Pass is null ? null : joint.Pass.Value ? "Y" : "N",
                    NOTES = joint.Notes.Count == 0 ? null : string.Join(System.Environment.NewLine, joint.Notes),
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgMetalLossWorkspace>.Failed(
                    OgUow.Message(committed, "The joint results were not saved."), MetalLossRules.CodeWriteFailed);
            }

            var header = OgUow.For<MFC_RESULT>(source);
            header.PrimaryKey = nameof(MFC_RESULT.MFC_RESULT_ID);

            header.Add(new MFC_RESULT
            {
                MFC_RESULT_ID = resultId,
                UWI = workspace.WellId,
                LOG_NAME = log.Name,
                LOG_REFERENCE = caliperPath,
                JOINT_SOURCE = mfc.JointSource,
                FINGERS = string.Join(",", mfc.Fingers),
                JOINT_COUNT = joints.Count,
                WALL_BASIS = settings.WallBasis,
                DAMAGE_CLASSES_JSON = JsonSerializer.Serialize(settings.DamageClasses),
                FLUID_GRADIENT_PPG = fluidGradientPpg,
                TEST_PRESSURE_PSI = testPressurePsi,
                WORST_JOINT = worst is null ? null : "joint " + worst.Joint.ToString(CultureInfo.InvariantCulture),
                WORST_DEPTH = worst?.PenetrationDepth is { } worstDepth ? (decimal)worstDepth : null,
                WORST_PENETRATION = worst?.MaxPenetration is { } worstPenetration ? (decimal)worstPenetration : 0m,
                WORST_DAMAGE_CLASS = worst?.DamageClass,
                FAILED_JOINTS = joints.Count(joint => joint.Pass == false),
                DEPTH_OUOM = log.IndexUnit,
                CHECKED_BY = actor.Trim(),
                CHECKED_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var saved = await header.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(saved))
            {
                return OgResult<OgMetalLossWorkspace>.Failed(
                    OgUow.Message(saved, "The result header was not saved, so the joints are not published."),
                    MetalLossRules.CodeWriteFailed);
            }

            return await ReadAsync(source, workspace.WellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "analysing the caliper log " + caliperPath + " for " + wellId,
                "the analysis is not recorded", FailureSeverity.Error);

            return OgResult<OgMetalLossWorkspace>.Failed(
                "The caliper log could not be analysed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? MetalLossRules.CodeReadFailed);
        }
    }

    // ------------------------------------------------------------------ the mapping

    private static OgJointResult Map(MfcJoint joint, IReadOnlyList<JointRating> ratings)
    {
        var rating = ratings.FirstOrDefault(item => item.Joint.Number == joint.Number);

        return new OgJointResult(
            joint.Number,
            joint.Top,
            joint.Bottom,
            joint.Unit,
            joint.MaxPenetration,
            joint.MaxPenetrationDepth,
            joint.MaxMetalLoss,
            joint.MinRemainingWall,
            rating?.NominalWall ?? ((joint.Nominal.od - joint.Nominal.id) / 2d),
            joint.MaxOvality,
            joint.MinId,
            joint.DamageClass,
            rating?.StringName,
            rating?.Grade,
            rating?.Wall,
            rating?.Burst,
            rating?.Collapse,
            rating?.Tension,
            rating?.BurstLoad,
            rating?.CollapseLoad,
            rating?.TensionLoad,
            rating?.NominalBurst,
            rating?.NominalCollapse,
            rating?.NominalTension,
            rating?.Pass,
            rating?.Notes ?? []);
    }

    private static IReadOnlyList<OgRerateResult> RerateOf(OgJointResult? joint, OgCasingDesignFactors factors)
    {
        if (joint is null)
        {
            return [];
        }

        var rows = new List<OgRerateResult>();

        if (joint.Burst is { } burst && joint.BurstLoad is { } burstLoad)
        {
            rows.Add(Row(MetalLossRules.Burst, burst, burstLoad, (double)factors.Burst));
        }

        if (joint.Collapse is { } collapse && joint.CollapseLoad is { } collapseLoad)
        {
            rows.Add(Row(MetalLossRules.Collapse, collapse, collapseLoad, (double)factors.Collapse));
        }

        if (joint.Tension is { } tension && joint.TensionLoad is { } tensionLoad)
        {
            rows.Add(Row(MetalLossRules.Tension, tension, tensionLoad, (double)factors.Tension));
        }

        return rows;

        static OgRerateResult Row(string loadCase, double resistance, double load, double factor)
        {
            var rating = resistance / factor;
            var utilisation = rating <= 0d ? double.MaxValue : load / rating;

            return new OgRerateResult(loadCase, resistance, rating, utilisation, utilisation <= 1d);
        }
    }

    // ------------------------------------------------------------------ the model and the read model

    private static async Task<WellModel> BuildModelAsync(
        IDataSource source,
        OgMetalLossWorkspace workspace,
        WellModel fileModel,
        CancellationToken cancellationToken)
    {
        var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var model = new WellModel { Header = fileModel.Header };

        foreach (var item in workspace.Strings)
        {
            var str = new TubularString
            {
                Name = item.Name,
                Kind = StringKind.Production,
                Top = new Quantity(item.Top, item.Unit),
                Bottom = new Quantity(item.Bottom, item.Unit),
            };

            if (item.Od is { } od)
            {
                str.Attributes.OuterDiameter = new Quantity(od, "in");
            }

            if (item.InnerDiameter is { } id)
            {
                str.Attributes.InnerDiameter = new Quantity(id, "in");
            }

            if (!string.IsNullOrWhiteSpace(item.Grade))
            {
                str.Attributes.Grade = item.Grade;
            }

            if (item.Weight is { } weight)
            {
                str.Attributes.Weight = new Quantity(weight, string.IsNullOrWhiteSpace(item.WeightUnit) ? "lbm/ft" : item.WeightUnit.Trim());
            }

            model.Strings.Add(str);
        }

        foreach (var row in stations
                     .Where(row => string.Equals(row.UWI?.Trim(), workspace.WellId, StringComparison.OrdinalIgnoreCase))
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(row => row.STATION_MD))
        {
            model.Survey.Add(new SurveyStation
            {
                Md = new Quantity((double)row.STATION_MD, row.STATION_MD_OUOM?.Trim() ?? "m"),
                Inclination = (double)row.INCLINATION,
                Azimuth = (double)row.AZIMUTH,
            });
        }

        return model;
    }

    private async Task<OgResult<OgMetalLossWorkspace>> ReadAsync(
        IDataSource source,
        string wellId,
        CancellationToken cancellationToken)
    {
        var tubulars = await OgUow.ReadAllAsync<WELL_TUBULAR>(source).ConfigureAwait(false);
        var results = await OgUow.ReadAllAsync<MFC_RESULT>(source).ConfigureAwait(false);
        var joints = await OgUow.ReadAllAsync<MFC_JOINT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var mine = tubulars
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.OUTSIDE_DIAMETER)
            .ToList();

        var strings = mine.Select(row =>
        {
            var unit = row.BASE_DEPTH_OUOM?.Trim() ?? "m";
            var top = row.HUNG_TOP_DEPTH > 0m ? (double)row.HUNG_TOP_DEPTH : 0d;
            var bottom = row.SHOE_DEPTH > 0m ? (double)row.SHOE_DEPTH : (double)row.BASE_DEPTH;

            return new OgTubularString(
                row.UWI + "/" + row.SOURCE + "/" + row.TUBING_OBS_NO.ToString(CultureInfo.InvariantCulture),
                string.IsNullOrWhiteSpace(row.TUBING_TYPE) ? "TUBULAR" : row.TUBING_TYPE!.Trim(),
                (double)row.OUTSIDE_DIAMETER > 0d ? (double)row.OUTSIDE_DIAMETER : null,
                (double)row.INSIDE_DIAMETER > 0d ? (double)row.INSIDE_DIAMETER : null,
                row.TUBING_GRADE,
                top,
                bottom,
                unit,
                row.TUBING_WEIGHT > 0m ? (double)row.TUBING_WEIGHT : null,
                row.TUBING_WEIGHT_OUOM);
        }).ToList();

        var ordered = results
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.CHECKED_AT_UTC)
            .ToList();

        var latest = ordered.FirstOrDefault();
        var previous = ordered.Skip(1).FirstOrDefault();

        var mineJoints = latest is null
            ? []
            : joints
                .Where(row => string.Equals(row.MFC_RESULT_ID, latest.MFC_RESULT_ID, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.JOINT_NUMBER)
                .Select(row => new OgJointResult(
                    row.JOINT_NUMBER,
                    (double)row.TOP_DEPTH,
                    (double)row.BOTTOM_DEPTH,
                    row.DEPTH_OUOM ?? latest.DEPTH_OUOM ?? "m",
                    row.MAX_PENETRATION is { } penetration ? (double)penetration : null,
                    row.PENETRATION_DEPTH is { } depth ? (double)depth : null,
                    row.MAX_METAL_LOSS is { } loss ? (double)loss : null,
                    row.MIN_REMAINING_WALL is { } wall ? (double)wall : null,
                    row.NOMINAL_WALL is { } nominal ? (double)nominal : null,
                    row.MAX_OVALITY is { } ovality ? (double)ovality : null,
                    row.MIN_ID is { } id ? (double)id : null,
                    row.DAMAGE_CLASS,
                    row.STRING_NAME,
                    row.GRADE,
                    row.WALL_USED is { } used ? (double)used : null,
                    row.BURST is { } burst ? (double)burst : null,
                    row.COLLAPSE is { } collapse ? (double)collapse : null,
                    row.TENSION is { } tension ? (double)tension : null,
                    row.BURST_LOAD is { } burstLoad ? (double)burstLoad : null,
                    row.COLLAPSE_LOAD is { } collapseLoad ? (double)collapseLoad : null,
                    row.TENSION_LOAD is { } tensionLoad ? (double)tensionLoad : null,
                    row.NOMINAL_BURST is { } nb ? (double)nb : null,
                    row.NOMINAL_COLLAPSE is { } nc ? (double)nc : null,
                    row.NOMINAL_TENSION is { } nt ? (double)nt : null,
                    string.IsNullOrWhiteSpace(row.PASSES) ? null : string.Equals(row.PASSES.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                    string.IsNullOrWhiteSpace(row.NOTES) ? [] : row.NOTES!.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)))
                .ToList();

        var worst = mineJoints.Where(joint => joint.MaxPenetration is not null)
            .OrderByDescending(joint => joint.MaxPenetration)
            .FirstOrDefault();

        var classes = IntegrityToolbox.Settings.DamageClasses;
        var factors = CasingDesignRules.FactorsOf(WellboreEnvironment.Current.CasingDesign);

        var life = MetalLossRules.RemainingLife(
            previous?.WORST_PENETRATION is { } before ? (double)before : null,
            previous?.WORST_DEPTH is { } wasAt ? (double)wasAt : null,
            previous?.CHECKED_AT_UTC,
            latest?.WORST_PENETRATION is { } now ? (double)now : null,
            latest?.WORST_DEPTH is { } isAt ? (double)isAt : null,
            latest?.CHECKED_AT_UTC ?? DateTime.UtcNow,
            MetalLossRules.LimitOf(classes));

        var warnings = new List<string>();

        if (strings.Count == 0)
        {
            warnings.Add("No casing or tubing with an outside and inside diameter is recorded for " + wellId +
                         ", so a caliper log could not be measured against nominal pipe.");
        }

        return OgResult<OgMetalLossWorkspace>.Ok(new OgMetalLossWorkspace(
            wellId,
            null,
            strings,
            latest?.LOG_NAME,
            latest?.LOG_REFERENCE,
            string.IsNullOrWhiteSpace(latest?.FINGERS)
                ? []
                : latest!.FINGERS!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
            latest?.JOINT_SOURCE ?? "intervals",
            mineJoints,
            worst,
            RerateOf(worst, factors),
            life,
            MetalLossRules.DamageClassNote(classes),
            warnings,
            latest?.MFC_RESULT_ID,
            latest?.CHECKED_AT_UTC,
            latest?.CHECKED_BY,
            DateTime.UtcNow));
    }
}
