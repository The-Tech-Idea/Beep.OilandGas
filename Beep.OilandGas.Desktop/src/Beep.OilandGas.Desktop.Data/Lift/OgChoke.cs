using System.Globalization;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.ChokeAnalysis.Validation;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>One warning or note carried with a choke result (LFT-07).</summary>
public sealed record OgChokeNote(string Code, string Message, bool Warning)
{
    public string Describe() => (Warning ? "warning" : "note") + " · " + Code.ToLowerInvariant() + ": " + Message;
}

/// <summary>The inputs of one choke analysis (LFT-07).</summary>
public sealed record OgChokeInputs(
    string? WellId,
    string? Correlation,
    string? ChokeType,
    decimal? ChokeDiameterIn,
    decimal? DischargeCoefficient,
    decimal? UpstreamPressure,
    decimal? DownstreamPressure,
    decimal? TemperatureF,
    decimal? GasSpecificGravity,
    decimal? ZFactor,
    decimal? LiquidRate,
    decimal? GasRateMscfPerDay,
    decimal? Glr,
    decimal? SandRate);

/// <summary>The erosion assessment as the page shows it (LFT-07 AC-3).</summary>
public sealed record OgChokeErosion(
    decimal Velocity,
    decimal Limit,
    decimal Ratio,
    string Risk,
    string ThresholdSource,
    bool AboveThreshold)
{
    public const string Source = "API RP 14E erosional velocity, Ve = C / sqrt(mixture density)";

    public string Describe() =>
        $"velocity {Velocity.ToString("#,0.##", CultureInfo.InvariantCulture)} ft/s against the " +
        $"{Limit.ToString("#,0.##", CultureInfo.InvariantCulture)} ft/s limit " +
        $"(ratio {Ratio.ToString("0.###", CultureInfo.InvariantCulture)}, {Risk.ToLowerInvariant()}) · " +
        "threshold from " + ThresholdSource;
}

/// <summary>
/// One choke analysis (LFT-07), matching the plan's <c>OgChokeResult</c> plus the correlation actually used and the
/// erosion risk.
/// </summary>
public sealed record OgChokeResult(
    string Id,
    string? WellId,
    string CorrelationChosen,
    string CorrelationUsed,
    string ChokeType,
    decimal ChokeDiameter,
    decimal? ChokeSize64ths,
    decimal Rate,
    string RateUnit,
    decimal UpstreamPressure,
    decimal DownstreamPressure,
    decimal? PressureRatio,
    decimal? CriticalPressureRatio,
    string Regime,
    OgChokeErosion? Erosion,
    IReadOnlyList<OgChokeNote> Notes,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string RateUnitText = "MMscf/d";
    public const string PressureUnit = "psia";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public bool IsSonic => string.Equals(Regime, ChokeAnalysisReferenceCodes.RegimeSonic, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-2: the result says the chosen correlation was not the one evaluated.</summary>
    public bool CorrelationFellBack =>
        !string.Equals(CorrelationChosen, CorrelationUsed, StringComparison.OrdinalIgnoreCase);

    public bool HasWarning => Notes.Any(note => note.Warning) || Erosion?.AboveThreshold == true;

    public bool ErosionAboveThreshold => Erosion?.AboveThreshold == true;

    public string RegimeText => IsSonic ? "sonic (critical)" : "subsonic";

    public string CorrelationText => CorrelationFellBack
        ? $"{Chosen(CorrelationChosen)} - the module has no model for it and ran {Chosen(CorrelationUsed)}"
        : Chosen(CorrelationChosen);

    public string RateText =>
        Rate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + (RateUnit ?? RateUnitText);

    public string PressuresText =>
        $"{UpstreamPressure.ToString("#,0.##", CultureInfo.InvariantCulture)} to " +
        $"{DownstreamPressure.ToString("#,0.##", CultureInfo.InvariantCulture)} {PressureUnit}";

    public string RatiosText => PressureRatio is { } ratio
        ? $"pressure ratio {ratio.ToString("0.####", CultureInfo.InvariantCulture)} against a critical " +
          $"{CriticalPressureRatio?.ToString("0.####", CultureInfo.InvariantCulture) ?? "(unknown)"}, so the flow is " +
          RegimeText
        : "no pressure ratio was computed";

    public string ChokeText =>
        $"{ChokeDiameter.ToString("0.####", CultureInfo.InvariantCulture)} in" +
        (ChokeSize64ths is { } size
            ? $" ({size.ToString("#,0.##", CultureInfo.InvariantCulture)}/64 in)"
            : string.Empty) +
        " · " + ChokeType.ToLowerInvariant();

    public string ErosionText => Erosion is null
        ? "no erosion assessment"
        : Erosion.Describe();

    public string NotesText => Notes.Count == 0
        ? "no note"
        : string.Join(Environment.NewLine, Notes.Select(note => note.Describe()));

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {ChokeText} · {RateText} · {RegimeText} · {CorrelationText} · {SavedText}";

    private static string Chosen(string code) =>
        ChokeCorrelationRegistry.Resolve(code)?.Label ?? code;
}

/// <summary>A result whose save was not acknowledged: retained and retried (LFT-07's state table).</summary>
public sealed record OgChokePending(OgChokeResult Result, string Actor);

/// <summary>The choke workspace (LFT-07).</summary>
public sealed record OgChokeWorkspace(
    IReadOnlyList<OgChokeResult> Results,
    OgChokeResult? Selected,
    IReadOnlyList<string> Correlations,
    IReadOnlyList<string> ChokeTypes,
    IReadOnlyList<string> Regimes,
    decimal MinimumDiameter,
    decimal MaximumDiameter,
    string ThresholdSource,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ResultsText => Results.Count == 0
        ? "No choke analysis is saved yet."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved results could not be read; new analyses are still allowed"
        : $"{Results.Count.ToString(CultureInfo.InvariantCulture)} saved result(s)";

    public string ReferenceText =>
        $"correlations {string.Join(", ", Correlations)} · choke types from ChokeType · regimes from " +
        $"ChokeFlowRegime · diameters {MinimumDiameter.ToString("0.##", CultureInfo.InvariantCulture)} to " +
        $"{MaximumDiameter.ToString("0.##", CultureInfo.InvariantCulture)} in · erosion threshold from " +
        ThresholdSource;
}

/// <summary>The choke rules, stated once so the page, the service and the tests agree.</summary>
public static class ChokeRules
{
    public const string CodeNoConnection = "choke-no-connection";
    public const string CodeReadFailed = "choke-read-failed";
    public const string CodeWriteFailed = "choke-write-failed";
    public const string CodeActorRequired = "choke-actor-required";
    public const string CodeWellRequired = "choke-well-required";
    public const string CodeInputRequired = "choke-input-required";
    public const string CodeInputInvalid = "choke-input-invalid";
    public const string CodeCorrelationUnknown = "choke-correlation-unknown";
    public const string CodeTypeUnknown = "choke-type-unknown";
    public const string CodeRegimeUndetermined = "choke-regime-undetermined";
    public const string CodeTemperatureUnaffordable = "choke-temperature-out-of-range";
    public const string CodeZFactorUndefined = "choke-zfactor-undefined";

    /// <summary>
    /// The temperature the gas choke calculator accepts, in Rankine.
    /// </summary>
    /// <remarks>
    /// <c>GasChokeCalculator</c> refuses anything outside 450-800 degR ("correlations may be unreliable") and the numbers
    /// are literals inside its own validation, so they are stated here to pre-check rather than to swallow its throw. The
    /// multiphase path does not use the temperature at all.
    /// </remarks>
    public const decimal MinimumTemperatureR = 450m;

    public const decimal MaximumTemperatureR = 800m;

    /// <summary>The Rankine value of a Fahrenheit input (the erosion assessment takes Fahrenheit, the gas calculator
    /// Rankine).</summary>
    public static decimal ToRankine(decimal fahrenheit) => fahrenheit + 459.67m;
    public const string CodeNothingToSave = "choke-nothing-to-save";
    public const string CodeNothingPending = "choke-nothing-pending";

    public static IReadOnlyList<string> Missing(OgChokeInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        void Need(decimal? value, string name)
        {
            if (value is null)
            {
                missing.Add(name);
            }
        }

        if (string.IsNullOrWhiteSpace(inputs.WellId))
        {
            missing.Add("well");
        }

        if (string.IsNullOrWhiteSpace(inputs.Correlation))
        {
            missing.Add("correlation");
        }

        if (string.IsNullOrWhiteSpace(inputs.ChokeType))
        {
            missing.Add("choke type");
        }

        Need(inputs.ChokeDiameterIn, "choke diameter");
        Need(inputs.UpstreamPressure, "upstream pressure");
        Need(inputs.DownstreamPressure, "downstream pressure");
        Need(inputs.TemperatureF, "temperature");

        var path = ChokeCorrelationRegistry.Resolve(inputs.Correlation)?.Path;

        if (path == ChokeCorrelationPath.SinglePhaseGas)
        {
            Need(inputs.GasSpecificGravity, "gas specific gravity");
            Need(inputs.ZFactor, "z factor");
        }
        else
        {
            // The state table: "missing GOR - refused".
            if (inputs.LiquidRate is null)
            {
                missing.Add("liquid rate");
            }

            if (inputs.Glr is null && inputs.GasRateMscfPerDay is null)
            {
                missing.Add("gas-liquid ratio or gas rate");
            }
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgChokeInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the analysis is for.")
                : missing.Contains("gas-liquid ratio or gas rate")
                    ? (CodeInputRequired,
                        "The multiphase correlations need the gas-liquid ratio (or the gas rate it is computed from): " +
                        "it is what the correlation is a function of.")
                    : (CodeInputRequired, $"The analysis needs {string.Join(", ", missing)}.");
        }

        if (ChokeCorrelationRegistry.Resolve(inputs.Correlation) is null)
        {
            return (CodeCorrelationUnknown,
                $"'{inputs.Correlation}' is not a choke correlation; the module offers " +
                $"{string.Join(", ", ChokeCorrelationRegistry.All.Select(info => info.Code))}.");
        }

        if (!Enum.TryParse<ChokeType>(inputs.ChokeType, ignoreCase: true, out var type)
            || !Enum.IsDefined(type))
        {
            return (CodeTypeUnknown,
                $"'{inputs.ChokeType}' is not a choke type; the module's types are " +
                $"{string.Join(", ", Enum.GetNames<ChokeType>())}.");
        }

        var (minimum, maximum) = ChokeCorrelationRegistry.DiameterRange;

        if (inputs.ChokeDiameterIn < minimum || inputs.ChokeDiameterIn > maximum)
        {
            return (CodeInputInvalid,
                $"The choke diameter {inputs.ChokeDiameterIn!.Value.ToString("0.####", CultureInfo.InvariantCulture)} in " +
                $"is outside the module's {minimum.ToString("0.##", CultureInfo.InvariantCulture)} to " +
                $"{maximum.ToString("0.##", CultureInfo.InvariantCulture)} in range.");
        }

        if (inputs.DownstreamPressure >= inputs.UpstreamPressure)
        {
            return (CodeInputInvalid,
                "The downstream pressure must be below the upstream pressure, or there is no flow to analyse.");
        }

        if (inputs.DischargeCoefficient is <= 0m or > 1m)
        {
            return (CodeInputInvalid, "The discharge coefficient is a fraction above zero and at most 1.");
        }

        if (inputs.TemperatureF <= -459.67m)
        {
            return (CodeInputInvalid, "The temperature is at or below absolute zero.");
        }

        // The gas path divides by the temperature and asks the Brill-Beggs Z-factor correlation, which refuses outside
        // its own range - checked here so the caller is told which input, rather than meeting an exception.
        if (ChokeCorrelationRegistry.Resolve(inputs.Correlation)?.Path != ChokeCorrelationPath.MultiphaseEmpirical)
        {
            var rankine = ToRankine(inputs.TemperatureF!.Value);

            if (rankine < MinimumTemperatureR || rankine > MaximumTemperatureR)
            {
                return (CodeTemperatureUnaffordable,
                    $"The temperature {inputs.TemperatureF.Value.ToString("#,0.##", CultureInfo.InvariantCulture)} F is " +
                    $"outside the {MinimumTemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} to " +
                    $"{MaximumTemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} R range the gas choke " +
                    "correlations are defined for, so no rate is computed.");
            }

            if (!ZFactorCalculator.IsBrillBeggsDefined(rankine, inputs.GasSpecificGravity!.Value))
            {
                return (CodeZFactorUndefined,
                    $"The Brill-Beggs Z-factor correlation is not defined at {rankine.ToString("#,0.##", CultureInfo.InvariantCulture)} R " +
                    $"for a gas of specific gravity {inputs.GasSpecificGravity.Value.ToString("0.###", CultureInfo.InvariantCulture)}.");
            }
        }

        return null;
    }
}

/// <summary>LFT-07: choke analysis.</summary>
public interface IOgChoke
{
    /// <summary>Analyses the choke. Nothing is written.</summary>
    Task<OgResult<OgChokeWorkspace>> AnalyseAsync(
        OgChokeInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the result with its correlation and regime (AC-6).</summary>
    Task<OgResult<OgChokeWorkspace>> SaveAsync(
        OgChokeResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved results; an unavailable history is a state.</summary>
    Task<OgResult<OgChokeWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgChokePending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgChokeWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-07 over the ChokeAnalysis module's gas choke, multiphase correlation and erosion calculators, its correlation
/// registry and the <c>CHOKE_FLOW_RESULT</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// A single-phase gas correlation gives a <b>rate</b> from the two pressures (<c>GasChokeCalculator</c>); a multiphase
/// empirical one gives the <b>upstream pressure</b> a liquid rate needs at a choke size
/// (<c>MultiphaseChokeCalculator</c>), which is what those correlations are functions of - the plan's "predict the
/// resulting rate <i>or</i> pressure". The regime is the module's own: its gas calculator derives it from the critical
/// pressure ratio, and the multiphase path uses the module's documented default critical ratio. Erosion is
/// <c>ChokeErosionAndSizing.Assess</c>, whose limit the module documents as API RP 14E.
/// </remarks>
public sealed class OgChokeService : IOgChoke
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgChokeService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgChokePending? Pending { get; private set; }

    public async Task<OgResult<OgChokeWorkspace>> AnalyseAsync(
        OgChokeInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (ChokeRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgChokeWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = Build(inputs);

            // The state table: a regime that cannot be decided is refused, naming the input.
            if (result is null)
            {
                return OgResult<OgChokeWorkspace>.Failed(
                    "The flow regime could not be decided: the analysis needs an upstream pressure above the " +
                    "downstream one and a positive rate, and one of them was not usable.",
                    ChokeRules.CodeRegimeUndetermined);
            }

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgChokeWorkspace>.Ok(Workspace(history, result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a choke for well {inputs.WellId}",
                "the analysis is not shown",
                FailureSeverity.Error);

            return OgResult<OgChokeWorkspace>.Failed(
                "The choke analysis could not be run.", reference ?? ChokeRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgChokeWorkspace>> SaveAsync(
        OgChokeResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgChokeWorkspace>.Failed(
                "A saved result must record who saved it.", ChokeRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(result.Regime))
        {
            return OgResult<OgChokeWorkspace>.Failed(
                "There is no result to save; run the analysis first.", ChokeRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgChokeWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", ChokeRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<CHOKE_FLOW_RESULT>(source);
            unit.PrimaryKey = "CHOKE_FLOW_RESULT_ID";

            unit.Add(new CHOKE_FLOW_RESULT
            {
                CHOKE_FLOW_RESULT_ID = Guid.NewGuid().ToString("N"),
                CHOKE_PROPERTIES_ID = null,
                GAS_CHOKE_PROPERTIES_ID = null,
                FLOW_RATE = result.Rate,
                DOWNSTREAM_PRESSURE = result.DownstreamPressure,
                UPSTREAM_PRESSURE = result.UpstreamPressure,
                PRESSURE_RATIO = result.PressureRatio ?? 0m,
                FLOW_REGIME = result.Regime,
                CRITICAL_PRESSURE_RATIO = result.CriticalPressureRatio ?? 0m,
                CORRELATION_CHOSEN = result.CorrelationChosen,
                CORRELATION_USED = result.CorrelationUsed,
                CHOKE_DIAMETER_IN = result.ChokeDiameter,
                CHOKE_SIZE_64THS = result.ChokeSize64ths,
                CHOKE_TYPE = result.ChokeType,
                DISCHARGE_COEFFICIENT = null,
                RATE_OUOM = result.RateUnit,
                PRESSURE_OUOM = OgChokeResult.PressureUnit,
                EROSION_VELOCITY_FPS = result.Erosion?.Velocity,
                EROSION_LIMIT_FPS = result.Erosion?.Limit,
                EROSION_RATIO = result.Erosion?.Ratio,
                EROSION_RISK = result.Erosion?.Risk,
                THRESHOLD_SOURCE = result.Erosion?.ThresholdSource,
                WARNING_IND = result.HasWarning ? "Y" : "N",
                NOTES = result.Notes.Count == 0
                    ? null
                    : string.Join("\n", result.Notes.Select(note => note.Describe())),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgChokePending(result, actor.Trim());

                return OgResult<OgChokeWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved; the original is retained and can be retried."),
                    ChokeRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgChokeWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgChokePending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a choke analysis for well {result.WellId}",
                "the result was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgChokeWorkspace>.Failed(
                "The result was not acknowledged; the original is retained and the save can be retried.",
                reference ?? ChokeRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgChokeWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgChokeWorkspace>.Failed(
                "There is no result waiting to be retried.", ChokeRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgChokeWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgChokeWorkspace>.Failed(
                "No connection is open, so the saved results cannot be read.", ChokeRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(resultId)
                ? history.FirstOrDefault(result => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(result.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(result =>
                      string.Equals(result.Id, resultId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgChokeWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved choke results",
                "the saved results are unavailable; new analyses are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgChokeWorkspace>.Ok(new OgChokeWorkspace(
                [],
                null,
                ChokeCorrelationRegistry.All.Select(info => info.Code).ToList(),
                Enum.GetNames<ChokeType>().ToList(),
                Enum.GetNames<ChokeFlowRegime>().ToList(),
                ChokeCorrelationRegistry.DiameterRange.Minimum,
                ChokeCorrelationRegistry.DiameterRange.Maximum,
                OgChokeErosion.Source,
                true,
                $"the saved results could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the analysis

    /// <summary>
    /// Runs the module's calculator for the resolved path. Returns null when no regime can be decided, which the caller
    /// refuses rather than reporting a guess.
    /// </summary>
    public static OgChokeResult? Build(OgChokeInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var chosen = inputs.Correlation!.Trim().ToUpperInvariant();
        var info = ChokeCorrelationRegistry.Resolve(chosen);
        var used = ChokeCorrelationRegistry.Effective(chosen);
        var type = Enum.Parse<ChokeType>(inputs.ChokeType!.Trim(), ignoreCase: true);
        var diameter = inputs.ChokeDiameterIn!.Value;
        var upstream = inputs.UpstreamPressure!.Value;
        var downstream = inputs.DownstreamPressure!.Value;

        var notes = new List<OgChokeNote>();

        if (info is { Implemented: false })
        {
            notes.Add(new OgChokeNote(
                "CORRELATION_FALLBACK",
                $"{info.Label}: the module has no model for it and evaluated {used} instead, so the result is marked " +
                "with the correlation it actually used.",
                Warning: true));
        }

        // The liquid rate and the gas-liquid ratio, from whichever the caller supplied.
        var liquidRate = inputs.LiquidRate ?? 0m;
        var glr = inputs.Glr ?? (liquidRate > 0m && inputs.GasRateMscfPerDay is { } gas
            ? gas * 1000m / liquidRate
            : 0m);
        var gasRate = inputs.GasRateMscfPerDay ?? (glr * liquidRate / 1000m);

        decimal rate;
        string rateUnit;
        decimal? pressureRatio = null;
        decimal? criticalRatio = null;
        ChokeFlowRegime regime;

        if (info?.Path == ChokeCorrelationPath.SinglePhaseGas || info is null)
        {
            var choke = ChokeProperties(diameter, type, inputs.DischargeCoefficient);
            var gasProperties = new GAS_CHOKE_PROPERTIES
            {
                GAS_CHOKE_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                UPSTREAM_PRESSURE = upstream,
                DOWNSTREAM_PRESSURE = downstream,
                // Rankine: the gas calculator's own unit (it validates 450-800 degR and uses it in ideal-gas relations).
                TEMPERATURE = ChokeRules.ToRankine(inputs.TemperatureF!.Value),
                Z_FACTOR = inputs.ZFactor!.Value,
                GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
                FLOW_RATE = 0m,
            };

            // The module's own non-throwing validator: the first problem names the input that could not decide the run.
            if (ChokeValidator.FindChokeProblem(choke) is { } chokeProblem)
            {
                notes.Add(new OgChokeNote("CHOKE_INVALID", chokeProblem.Sentence, Warning: false));

                return null;
            }

            if (ChokeValidator.FindGasChokeProblem(gasProperties) is { } gasProblem)
            {
                notes.Add(new OgChokeNote("GAS_INVALID", gasProblem.Sentence, Warning: false));

                return null;
            }

            var flow = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);

            rate = flow.FLOW_RATE;
            rateUnit = OgChokeResult.RateUnitText;
            pressureRatio = flow.PRESSURE_RATIO;
            criticalRatio = flow.CRITICAL_PRESSURE_RATIO;
            regime = string.Equals(flow.FLOW_REGIME, ChokeAnalysisReferenceCodes.RegimeSonic,
                StringComparison.OrdinalIgnoreCase)
                ? ChokeFlowRegime.Sonic
                : ChokeFlowRegime.Subsonic;
        }
        else
        {
            if (liquidRate <= 0m || glr <= 0m)
            {
                return null;
            }

            var pressures = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, diameter);
            var requiredUpstream = MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(pressures, used);

            if (requiredUpstream <= 0m)
            {
                return null;
            }

            // The correlation's required upstream pressure can come out at or below the downstream pressure the caller
            // gave. That is a real answer - the choke is larger than this rate needs at this drawdown - so it is reported
            // with a warning rather than refused, and the regime falls out of the module's own ratio rule.
            if (requiredUpstream <= downstream)
            {
                notes.Add(new OgChokeNote(
                    "REQUIRED_PRESSURE_BELOW_DOWNSTREAM",
                    $"the correlation needs only {requiredUpstream.ToString("#,0.##", CultureInfo.InvariantCulture)} psia " +
                    $"upstream for this rate, which is at or below the " +
                    $"{downstream.ToString("#,0.##", CultureInfo.InvariantCulture)} psia downstream pressure given, so " +
                    "this choke does not limit the rate at these conditions",
                    Warning: true));
            }

            // The plan's other direction: this correlation gives the pressure a rate needs, so the result's "rate" is
            // the upstream pressure it answers with, and it is labelled as such.
            rate = requiredUpstream;
            rateUnit = OgChokeResult.PressureUnit;

            pressureRatio = downstream / requiredUpstream;
            criticalRatio = MultiphaseChokeCalculator.DefaultMultiphaseCriticalPressureRatio;
            regime = pressureRatio < criticalRatio ? ChokeFlowRegime.Sonic : ChokeFlowRegime.Subsonic;

            notes.Add(new OgChokeNote(
                "PRESSURE_RESULT",
                $"the correlation predicts the upstream pressure this rate needs at this choke " +
                $"({requiredUpstream.ToString("#,0.##", CultureInfo.InvariantCulture)} psia), not a rate from two " +
                "pressures",
                Warning: false));
        }

        // AC-3: the erosion assessment, whose limit the module documents as API RP 14E.
        var erosion = AssessErosion(diameter, type, inputs, liquidRate, gasRate, upstream);

        if (erosion is { AboveThreshold: true })
        {
            notes.Add(new OgChokeNote(
                "EROSION_ABOVE_THRESHOLD",
                $"the throat velocity is {erosion.Ratio.ToString("0.###", CultureInfo.InvariantCulture)} times the " +
                $"erosional limit ({erosion.Risk.ToLowerInvariant()}: {erosion.ThresholdSource})",
                Warning: true));
        }

        return new OgChokeResult(
            string.Empty,
            inputs.WellId!.Trim(),
            chosen,
            used,
            type.ToString().ToUpperInvariant(),
            diameter,
            ChokeCorrelationRegistry.ToThroatSize64ths(diameter),
            rate,
            rateUnit,
            upstream,
            downstream,
            pressureRatio,
            criticalRatio,
            regime.ToString().ToUpperInvariant(),
            erosion,
            notes,
            null,
            null);
    }

    /// <summary>
    /// The module's erosion assessment. A refusal inside it is reported as a note and no assessment, rather than as a
    /// fabricated zero: it needs a positive rate and a positive upstream pressure to compute a mixture density.
    /// </summary>
    private static OgChokeErosion? AssessErosion(
        decimal diameter,
        ChokeType type,
        OgChokeInputs inputs,
        decimal liquidRate,
        decimal gasRate,
        decimal upstream)
    {
        if (liquidRate <= 0m || gasRate <= 0m || upstream <= 0m)
        {
            return null;
        }

        var choke = ChokeProperties(diameter, type, inputs.DischargeCoefficient);

        var assessment = ChokeErosionAndSizing.Assess(
            choke,
            (double)liquidRate,
            (double)gasRate,
            (double)upstream,
            (double)inputs.TemperatureF!.Value,
            waterCutFraction: 0.0,
            sandProductionLbPerDay: inputs.SandRate is { } sand ? (double)sand : null);

        return new OgChokeErosion(
            (decimal)assessment.ChokeVelocityFtPerSec,
            (decimal)assessment.ErosionalVelocityLimitFtPerSec,
            (decimal)assessment.VelocityRatio,
            assessment.Risk.ToString().ToUpperInvariant(),
            OgChokeErosion.Source,
            (decimal)assessment.VelocityRatio > 1m);
    }

    private static CHOKE_PROPERTIES ChokeProperties(decimal diameter, ChokeType type, decimal? dischargeCoefficient) =>
        new()
        {
            CHOKE_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            CHOKE_DIAMETER = diameter,
            CHOKE_TYPE = type.ToString().ToUpperInvariant(),
            DISCHARGE_COEFFICIENT = dischargeCoefficient ?? ChokeConstants.StandardDischargeCoefficient,
        };

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgChokeResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<CHOKE_FLOW_RESULT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.CHOKE_FLOW_RESULT_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .ThenBy(row => row.CHOKE_FLOW_RESULT_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgChokeResult ToResult(CHOKE_FLOW_RESULT row)
    {
        var notes = string.IsNullOrWhiteSpace(row.NOTES)
            ? []
            : row.NOTES!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line =>
                {
                    var separator = line.IndexOf('·');
                    var head = separator > 0 ? line[..separator].Trim() : line;
                    var rest = separator > 0 ? line[(separator + 1)..].Trim() : line;
                    var colon = rest.IndexOf(':');
                    var code = colon > 0 ? rest[..colon].Trim().ToUpperInvariant() : string.Empty;
                    var message = colon > 0 ? rest[(colon + 1)..].Trim() : rest;

                    return new OgChokeNote(code, message, head.StartsWith("warning", StringComparison.OrdinalIgnoreCase));
                })
                .ToList();

        OgChokeErosion? erosion = row.EROSION_LIMIT_FPS is { } limit
            ? new OgChokeErosion(
                row.EROSION_VELOCITY_FPS ?? 0m,
                limit,
                row.EROSION_RATIO ?? 0m,
                row.EROSION_RISK ?? "UNKNOWN",
                row.THRESHOLD_SOURCE ?? "(no source recorded)",
                (row.EROSION_RATIO ?? 0m) > 1m)
            : null;

        return new OgChokeResult(
            row.CHOKE_FLOW_RESULT_ID ?? string.Empty,
            null,
            row.CORRELATION_CHOSEN ?? ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase,
            row.CORRELATION_USED ?? ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase,
            row.CHOKE_TYPE ?? ChokeType.Unknown.ToString().ToUpperInvariant(),
            row.CHOKE_DIAMETER_IN,
            row.CHOKE_SIZE_64THS,
            row.FLOW_RATE,
            row.RATE_OUOM ?? OgChokeResult.RateUnitText,
            row.UPSTREAM_PRESSURE,
            row.DOWNSTREAM_PRESSURE,
            row.PRESSURE_RATIO,
            row.CRITICAL_PRESSURE_RATIO,
            row.FLOW_REGIME,
            erosion,
            notes,
            row.ACTOR,
            row.RECORDED_AT);
    }

    private static OgChokeWorkspace Workspace(IReadOnlyList<OgChokeResult> history, OgChokeResult? selected) =>
        new(
            history,
            selected,
            ChokeCorrelationRegistry.All.Select(info => info.Code).ToList(),
            Enum.GetNames<ChokeType>().ToList(),
            Enum.GetNames<ChokeFlowRegime>().ToList(),
            ChokeCorrelationRegistry.DiameterRange.Minimum,
            ChokeCorrelationRegistry.DiameterRange.Maximum,
            OgChokeErosion.Source,
            false,
            null);
}
