using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.FlashCalculations.Constants;
using Beep.OilandGas.FlashCalculations.Data;
using Beep.OilandGas.Models.Data.FlashCalculations;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The three outcome codes a flash run records, mirroring the rows seeded into the reference set.</summary>
/// <remarks>
/// The feature document names SUCCESS, CONVERGENCE_FAILED and FAILED as coming from
/// <c>R_FLASH_CALCULATION_REFERENCE_CODE</c>. The owner's seed did not contain them, so they were added under
/// <c>FLASH_STATUS_CODE</c>; these constants are the single place the screen reads them from, so no view carries the
/// strings.
/// </remarks>
public static class FlashStatusCodes
{
    public const string Success = "SUCCESS";
    public const string ConvergenceFailed = "CONVERGENCE_FAILED";
    public const string Failed = "FAILED";

    public static IReadOnlyList<string> All { get; } = [Success, ConvergenceFailed, Failed];
}

/// <summary>One component of the feed, with the critical properties the engine needs (RES-03).</summary>
public sealed record OgFlashComponent(
    string Name,
    decimal MoleFraction,
    decimal CriticalTemperature,
    decimal CriticalPressure,
    decimal AcentricFactor,
    decimal MolecularWeight)
{
    public string Describe() =>
        $"{Name} z={MoleFraction.ToString("0.####", CultureInfo.InvariantCulture)} " +
        $"Tc={CriticalTemperature.ToString("0.##", CultureInfo.InvariantCulture)} R " +
        $"Pc={CriticalPressure.ToString("0.##", CultureInfo.InvariantCulture)} psia " +
        $"w={AcentricFactor.ToString("0.####", CultureInfo.InvariantCulture)} " +
        $"MW={MolecularWeight.ToString("0.##", CultureInfo.InvariantCulture)}";
}

/// <summary>One component's result in a run (RES-03), matching the plan's <c>PhaseComposition[]</c>.</summary>
public sealed record OgFlashComponentResult(string Name, decimal Z, decimal K, decimal X, decimal Y)
{
    public string Describe() =>
        $"{Name} z={Z.ToString("0.####", CultureInfo.InvariantCulture)} " +
        $"K={K.ToString("0.######", CultureInfo.InvariantCulture)} " +
        $"x={X.ToString("0.######", CultureInfo.InvariantCulture)} " +
        $"y={Y.ToString("0.######", CultureInfo.InvariantCulture)}";
}

/// <summary>The inputs of one flash run (RES-03).</summary>
public sealed record OgFlashInputs(
    IReadOnlyList<OgFlashComponent> Components,
    decimal? Pressure,
    decimal? Temperature,
    string? EosCode)
{
    public decimal FeedSum => Components.Sum(component => component.MoleFraction);

    public string FeedText => string.Join(" · ", Components.Select(component => component.Describe()));
}

/// <summary>One isothermal flash run, run or saved (RES-03), matching the plan's <c>OgFlashResult</c>.</summary>
public sealed record OgFlashRun(
    string Id,
    string StatusCode,
    bool Converged,
    int Iterations,
    decimal VapourFraction,
    decimal LiquidFraction,
    string EosCode,
    string EosName,
    decimal FeedSum,
    string PhaseState,
    string? StabilityText,
    IReadOnlyList<OgFlashComponentResult> Components,
    IReadOnlyList<OgFlashComponent> Feed,
    decimal? Pressure,
    decimal? Temperature,
    decimal? ConvergenceError,
    string? FailureCode,
    string? FailureMessage,
    string? Actor,
    DateTime? RecordedAt)
{
    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-2: a run the solver did not converge is a failed run, and no composition is shown from it.</summary>
    public bool IsFailed => string.Equals(StatusCode, FlashStatusCodes.ConvergenceFailed, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(StatusCode, FlashStatusCodes.Failed, StringComparison.OrdinalIgnoreCase);

    public bool IsTwoPhase => string.Equals(PhaseState, FlashPhaseStates.TwoPhase, StringComparison.Ordinal);

    public string StatusText => StatusCode.ToLowerInvariant();

    public string PhaseText => IsFailed ? "no phase split: the run did not converge" : PhaseState;

    public string VapourFractionText => IsFailed
        ? "not computed"
        : VapourFraction.ToString("0.######", CultureInfo.InvariantCulture);

    public string LiquidFractionText => IsFailed
        ? "not computed"
        : LiquidFraction.ToString("0.######", CultureInfo.InvariantCulture);

    public string IterationsText => Iterations.ToString(CultureInfo.InvariantCulture) + " iteration(s)";

    public string CompositionText => IsFailed || Components.Count == 0
        ? "no composition is shown for a run that did not converge"
        : string.Join(Environment.NewLine, Components.Select(component => component.Describe()));

    public string FailureText => IsFailed
        ? $"{FailureCode ?? FlashStatusCodes.ConvergenceFailed}: " +
          $"{FailureMessage ?? "the solver did not converge"} after {IterationsText}"
        : "no failure";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"}"
        : "not saved";

    public string ConditionsText =>
        $"{Pressure?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "(no pressure)"} psia at " +
        $"{Temperature?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "(no temperature)"} R · " +
        $"{EosCode} ({EosName}) · feed sum {FeedSum.ToString("0.######", CultureInfo.InvariantCulture)}";

    public string Describe() => IsFailed
        ? $"FAILED · {FailureText} · {ConditionsText} · {SavedText}"
        : $"{PhaseText} · vapour {VapourFractionText} · liquid {LiquidFractionText} · {IterationsText} · " +
          $"{ConditionsText} · status {StatusCode} · {SavedText}";
}

/// <summary>One computed envelope point (RES-03 AC-5: only computed points are ever exposed).</summary>
public sealed record OgEnvelopePoint(string Type, decimal TemperatureR, decimal PressurePsia)
{
    public string Describe() =>
        $"{Type.ToLowerInvariant()} at {TemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} R / " +
        $"{PressurePsia.ToString("#,0.##", CultureInfo.InvariantCulture)} psia";
}

/// <summary>The computed part of a phase envelope (RES-03 AC-5).</summary>
public sealed record OgFlashEnvelope(
    IReadOnlyList<OgEnvelopePoint> Points,
    int Skipped,
    string EosUsed,
    decimal CricondenthermR,
    decimal CricondenbarPsia,
    decimal CriticalR,
    decimal CriticalPsia,
    string Text)
{
    /// <summary>True when the solver could not converge every point, so only part of the envelope exists.</summary>
    public bool Partial => Skipped > 0;

    public string LimitText => Partial
        ? $"{Skipped.ToString(CultureInfo.InvariantCulture)} point(s) of the envelope could not be computed and are " +
          "not shown: the envelope stops at the points the solver produced"
        : "every requested point of the envelope was computed";
}

/// <summary>The workspace: the saved history, the EOS codes on offer and their source (RES-03).</summary>
public sealed record OgFlashWorkspace(
    IReadOnlyList<OgFlashRun> History,
    IReadOnlyList<string> EosCodes,
    bool EosFromReference,
    IReadOnlyList<string> StatusCodes,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string HistoryText => History.Count == 0
        ? "No flash run is saved yet."
        : string.Join(Environment.NewLine, History.Select(run => run.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved history could not be read; new runs are still allowed"
        : $"{History.Count.ToString(CultureInfo.InvariantCulture)} saved run(s)";

    public string EosText => EosFromReference
        ? "EOS models from R_FLASH_CALCULATION_REFERENCE_CODE: " + string.Join(", ", EosCodes)
        : "R_FLASH_CALCULATION_REFERENCE_CODE has no FLASH_EOS_MODEL row, so the list is the mapping's own set: " +
          string.Join(", ", EosCodes);
}

/// <summary>The phase names the screen prints, matching the reference set's FLASH_PHASE_STATE labels.</summary>
public static class FlashPhaseStates
{
    public const string TwoPhase = "two-phase";
    public const string AllLiquid = "all liquid";
    public const string AllVapour = "all vapour";
}

/// <summary>The flash rules, stated once so the page, the service and the tests agree.</summary>
public static class FlashRules
{
    public const string CodeNoConnection = "flash-no-connection";
    public const string CodeReadFailed = "flash-read-failed";
    public const string CodeWriteFailed = "flash-write-failed";
    public const string CodeActorRequired = "flash-actor-required";
    public const string CodeComponentsRequired = "flash-components-required";
    public const string CodeComponentNameRequired = "flash-component-name-required";
    public const string CodeFractionInvalid = "flash-fraction-invalid";
    public const string CodePropertiesRequired = "flash-component-properties-required";
    public const string CodeFeedSum = "flash-feed-sum";
    public const string CodePressureRequired = "flash-pressure-required";
    public const string CodeTemperatureRequired = "flash-temperature-required";
    public const string CodeEosUnknown = "flash-eos-unknown";
    public const string CodeEosNotImplemented = "flash-eos-not-implemented";
    public const string CodeRunFailed = "flash-run-failed";
    public const string CodeNothingToSave = "flash-nothing-to-save";
    public const string CodeEnvelopeEmpty = "flash-envelope-empty";
    public const string CodeFeedLineInvalid = "flash-feed-line-invalid";

    /// <summary>
    /// AC-3: the mole fractions must sum to one within this tolerance. It is the engine's own convergence tolerance, so
    /// the screen does not invent a second number.
    /// </summary>
    public const decimal FeedSumTolerance = FlashConstants.ConvergenceTolerance;

    /// <summary>The codes the mapping knows, used when the reference set has no FLASH_EOS_MODEL rows.</summary>
    public static IReadOnlyList<string> MappingEosCodes { get; } = ["PR", "SRK", "SRK_MODIFIED", "IDEAL_K"];

    /// <summary>AC-4: an EOS name is always read through the owner's mapping, never typed into a view.</summary>
    public static string EosCode(string? eos) => FlashEquationOfStateMapping.ToReferenceCode(eos);

    /// <summary>The state table's "EOS not applicable": the engine's cubic path has PR and SRK only.</summary>
    public static (string Code, string Message)? Validate(OgFlashInputs inputs, IReadOnlyCollection<string> eosCodes)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(eosCodes);

        var components = inputs.Components ?? [];

        if (components.Count == 0)
        {
            return (CodeComponentsRequired, "A flash needs at least one component in the feed.");
        }

        foreach (var component in components)
        {
            if (string.IsNullOrWhiteSpace(component.Name))
            {
                return (CodeComponentNameRequired, "Every feed component needs a name.");
            }

            if (component.MoleFraction <= 0m || component.MoleFraction > 1m)
            {
                return (CodeFractionInvalid,
                    $"The mole fraction of {component.Name} is " +
                    $"{component.MoleFraction.ToString("0.####", CultureInfo.InvariantCulture)}; it must be above 0 " +
                    "and at most 1.");
            }

            if (component.CriticalTemperature <= 0m || component.CriticalPressure <= 0m
                || component.MolecularWeight <= 0m)
            {
                return (CodePropertiesRequired,
                    $"The critical properties of {component.Name} are incomplete: a critical temperature, a critical " +
                    "pressure and a molecular weight are required.");
            }
        }

        var sum = inputs.FeedSum;

        if (Math.Abs(sum - 1m) > FeedSumTolerance)
        {
            return (CodeFeedSum,
                $"The mole fractions sum to {sum.ToString("0.######", CultureInfo.InvariantCulture)}, not 1 within " +
                $"{FeedSumTolerance.ToString("0.######", CultureInfo.InvariantCulture)}.");
        }

        if (inputs.Pressure is null or <= 0m)
        {
            return (CodePressureRequired, "A flash needs a pressure above zero.");
        }

        if (inputs.Temperature is null or <= 0m)
        {
            return (CodeTemperatureRequired, "A flash needs a temperature above zero.");
        }

        var code = EosCode(inputs.EosCode);

        if (!eosCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
        {
            return (CodeEosUnknown,
                $"'{inputs.EosCode}' is not an equation of state on offer; use {string.Join(", ", eosCodes)}.");
        }

        // The owner's cubic path implements Peng-Robinson and Soave-Redlich-Kwong; the modified SRK alpha function does
        // not exist in AdvancedEOS.EosType, so the modifier is refused rather than silently treated as plain SRK.
        return string.Equals(code, "SRK_MODIFIED", StringComparison.OrdinalIgnoreCase)
            ? (CodeEosNotImplemented,
                "SRK_MODIFIED has no alpha function in the engine's cubic path (AdvancedEOS.EosType has " +
                "PengRobinson and SoaveRedlichKwong only), so it is refused rather than run as plain SRK.")
            : null;
    }

    /// <summary>How the phase split is named, from the vapour fraction the solver returned.</summary>
    public static string PhaseState(decimal vapourFraction, bool converged)
    {
        if (!converged)
        {
            return FlashStatusCodes.ConvergenceFailed;
        }

        if (vapourFraction <= 0m)
        {
            return FlashPhaseStates.AllLiquid;
        }

        return vapourFraction >= 1m ? FlashPhaseStates.AllVapour : FlashPhaseStates.TwoPhase;
    }
}

/// <summary>
/// The feed editor's text format: one component per line,
/// <c>name, mole fraction, critical temperature (R), critical pressure (psia), acentric factor, molecular weight</c>.
/// </summary>
/// <remarks>
/// The parser lives here rather than in the view so that what the operator's text means is testable and so the page
/// carries no format knowledge. Blank lines and lines starting with <c>#</c> are ignored.
/// </remarks>
public static class FlashFeedText
{
    public static IReadOnlyList<string> ReferenceMixture { get; } =
    [
        "# name, z, Tc (R), Pc (psia), omega, MW",
        "C1,  0.50, 343.0, 667.8, 0.0104, 16.04",
        "C2,  0.20, 549.8, 707.8, 0.0986, 30.07",
        "C3,  0.15, 665.7, 616.3, 0.1524, 44.10",
        "NC4, 0.15, 765.3, 550.7, 0.2002, 58.12",
    ];

    public static string DefaultText => string.Join(Environment.NewLine, ReferenceMixture);

    public static bool TryParse(
        string? text,
        out IReadOnlyList<OgFlashComponent> components,
        out string? error)
    {
        var parsed = new List<OgFlashComponent>();
        error = null;

        var lines = (text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToList();

        if (lines.Count == 0)
        {
            components = [];

            error = "The feed is empty: add one component per line.";

            return false;
        }

        for (var index = 0; index < lines.Count; index++)
        {
            var parts = lines[index]
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 6)
            {
                components = [];

                error =
                    $"Feed line {index + 1} has {parts.Length.ToString(CultureInfo.InvariantCulture)} field(s); " +
                    "each line needs name, z, Tc, Pc, omega and MW.";

                return false;
            }

            if (!decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var z)
                || !decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var tc)
                || !decimal.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var pc)
                || !decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var omega)
                || !decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var mw))
            {
                components = [];

                error = $"Feed line {index + 1} ('{parts[0]}') has a value that is not a number.";

                return false;
            }

            parsed.Add(new OgFlashComponent(parts[0], z, tc, pc, omega, mw));
        }

        components = parsed;

        return true;
    }
}

/// <summary>RES-03: flash calculations and the phase envelope.</summary>
public interface IOgFlash
{
    Task<OgResult<OgFlashRun>> RunAsync(OgFlashInputs inputs, CancellationToken cancellationToken = default);

    /// <summary>The envelope, exposing only the points the solver computed (AC-5).</summary>
    Task<OgResult<OgFlashEnvelope>> EnvelopeAsync(
        OgFlashInputs inputs,
        decimal minTemperatureR = 400m,
        decimal maxTemperatureR = 1200m,
        int steps = 20,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFlashWorkspace>> SaveAsync(
        OgFlashRun run,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFlashWorkspace>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-03 over the FlashCalculations engine and the owner's <c>FLASH_CALCULATION_RESULT</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The engine is a concrete static class, which the feature's open question says must be wrapped: this service is that
/// wrapper, and no view sees <c>FlashCalculator</c> or <c>MultiComponentFlash</c>. IDEAL_K runs the Wilson/Rachford-Rice
/// path, PR and SRK run the cubic-EOS path, and the convergence flag the engine reports is carried into the result
/// instead of being turned into a number.
/// </remarks>
public sealed class OgFlashService : IOgFlash
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgFlashService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public Task<OgResult<OgFlashRun>> RunAsync(
        OgFlashInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        cancellationToken.ThrowIfCancellationRequested();

        var code = FlashRules.EosCode(inputs.EosCode);

        if (FlashRules.Validate(inputs, FlashRules.MappingEosCodes) is { } refusal)
        {
            return Task.FromResult(OgResult<OgFlashRun>.Failed(refusal.Message, refusal.Code));
        }

        var feed = inputs.Components
            .Select(component => new FLASH_COMPONENT
            {
                FLASH_COMPONENT_ID = Guid.NewGuid().ToString("N"),
                COMPONENT_NAME = component.Name.Trim(),
                NAME = component.Name.Trim(),
                MOLE_FRACTION = component.MoleFraction,
                CRITICAL_TEMPERATURE = component.CriticalTemperature,
                CRITICAL_PRESSURE = component.CriticalPressure,
                ACENTRIC_FACTOR = component.AcentricFactor,
                MOLECULAR_WEIGHT = component.MolecularWeight,
            })
            .ToList();

        try
        {
            if (string.Equals(code, "IDEAL_K", StringComparison.OrdinalIgnoreCase))
            {
                var engine = FlashCalculator.PerformIsothermalFlash(new FLASH_CONDITIONS
                {
                    PRESSURE = inputs.Pressure!.Value,
                    TEMPERATURE = inputs.Temperature!.Value,
                    FEED_COMPOSITION = feed,
                });

                return Task.FromResult(OgResult<OgFlashRun>.Ok(FromWilson(engine, inputs, code, feed)));
            }

            var cubic = MultiComponentFlash.PerformEnhancedFlash(
                feed,
                (double)inputs.Pressure!.Value,
                (double)inputs.Temperature!.Value,
                string.Equals(code, "SRK", StringComparison.OrdinalIgnoreCase)
                    ? AdvancedEOS.EosType.SoaveRedlichKwong
                    : AdvancedEOS.EosType.PengRobinson);

            return Task.FromResult(OgResult<OgFlashRun>.Ok(FromCubic(cubic, inputs, code, feed)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"running a {code} flash at " +
                $"{inputs.Pressure?.ToString("#,0.##", CultureInfo.InvariantCulture)} psia",
                "the run is recorded as failed and no composition is reported",
                FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgFlashRun>.Ok(new OgFlashRun(
                string.Empty,
                FlashStatusCodes.Failed,
                false,
                0,
                0m,
                0m,
                code,
                EosName(code),
                inputs.FeedSum,
                FlashStatusCodes.ConvergenceFailed,
                null,
                [],
                inputs.Components,
                inputs.Pressure,
                inputs.Temperature,
                null,
                FlashRules.CodeRunFailed,
                $"the engine did not complete the flash (reference {reference})",
                null,
                null)));
        }
    }

    /// <summary>
    /// AC-2 and AC-1: the engine's own convergence flag becomes the run's state. A run that did not converge keeps its
    /// iteration count and exposes no phase composition.
    /// </summary>
    public static OgFlashRun FromWilson(
        FlashResult engine,
        OgFlashInputs inputs,
        string eosCode,
        IReadOnlyList<FLASH_COMPONENT> feed)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(inputs);

        var converged = engine.Converged;

        var components = converged
            ? engine.KValues
                .Select(k =>
                {
                    var vapour = engine.VaporComposition.FirstOrDefault(c => c.ComponentName == k.ComponentName);
                    var liquid = engine.LiquidComposition.FirstOrDefault(c => c.ComponentName == k.ComponentName);
                    var overall = inputs.Components.FirstOrDefault(c =>
                        string.Equals(c.Name, k.ComponentName, StringComparison.OrdinalIgnoreCase));

                    return new OgFlashComponentResult(
                        k.ComponentName ?? string.Empty,
                        overall?.MoleFraction ?? 0m,
                        k.KValue,
                        liquid?.MoleFraction ?? 0m,
                        vapour?.MoleFraction ?? 0m);
                })
                .ToList()
            : [];

        return new OgFlashRun(
            string.Empty,
            converged ? FlashStatusCodes.Success : FlashStatusCodes.ConvergenceFailed,
            converged,
            engine.Iterations,
            engine.VaporFraction,
            engine.LiquidFraction,
            eosCode,
            EosName(eosCode),
            inputs.FeedSum,
            FlashRules.PhaseState(engine.VaporFraction, converged),
            null,
            components,
            inputs.Components,
            inputs.Pressure,
            inputs.Temperature,
            engine.ConvergenceError,
            converged ? null : FlashStatusCodes.ConvergenceFailed,
            converged
                ? null
                : $"the solver did not converge within {engine.Iterations.ToString(CultureInfo.InvariantCulture)} " +
                  "iteration(s), so its phase split is not reported as a result",
            null,
            null);
    }

    /// <summary>The cubic path's mapping; its stability test can legitimately answer "stable single-phase".</summary>
    public static OgFlashRun FromCubic(
        MultiComponentFlash.EnhancedFlashResult engine,
        OgFlashInputs inputs,
        string eosCode,
        IReadOnlyList<FLASH_COMPONENT> feed)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(inputs);

        var singlePhase = engine.VaporFraction >= 1.0 || engine.VaporFraction <= 0.0;

        var components = engine.KValues
            .Select((k, index) => new OgFlashComponentResult(
                index < feed.Count ? feed[index].COMPONENT_NAME ?? string.Empty : string.Empty,
                index < feed.Count ? feed[index].MOLE_FRACTION : 0m,
                (decimal)k,
                index < engine.LiquidComposition.Length ? (decimal)engine.LiquidComposition[index] : 0m,
                index < engine.VaporComposition.Length ? (decimal)engine.VaporComposition[index] : 0m))
            .ToList();

        return new OgFlashRun(
            string.Empty,
            engine.Converged ? FlashStatusCodes.Success : FlashStatusCodes.ConvergenceFailed,
            engine.Converged,
            engine.Iterations,
            (decimal)engine.VaporFraction,
            (decimal)engine.LiquidFraction,
            eosCode,
            EosName(eosCode),
            inputs.FeedSum,
            FlashRules.PhaseState((decimal)engine.VaporFraction, engine.Converged),
            string.IsNullOrWhiteSpace(engine.StabilityResult) || !singlePhase
                ? null
                : engine.StabilityResult,
            components,
            inputs.Components,
            inputs.Pressure,
            inputs.Temperature,
            (decimal)engine.ConvergenceResidual,
            engine.Converged ? null : FlashStatusCodes.ConvergenceFailed,
            engine.Converged
                ? null
                : $"the fugacity iteration did not converge within " +
                  $"{engine.Iterations.ToString(CultureInfo.InvariantCulture)} iteration(s), residual " +
                  $"{engine.ConvergenceResidual.ToString("0.######", CultureInfo.InvariantCulture)}",
            null,
            null);
    }

    public Task<OgResult<OgFlashEnvelope>> EnvelopeAsync(
        OgFlashInputs inputs,
        decimal minTemperatureR = 400m,
        decimal maxTemperatureR = 1200m,
        int steps = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        cancellationToken.ThrowIfCancellationRequested();

        var code = FlashRules.EosCode(inputs.EosCode);

        if (FlashRules.Validate(inputs, FlashRules.MappingEosCodes) is { } refusal)
        {
            return Task.FromResult(OgResult<OgFlashEnvelope>.Failed(refusal.Message, refusal.Code));
        }

        if (minTemperatureR <= 0m || maxTemperatureR <= minTemperatureR || steps < 2)
        {
            var rangeMessage =
                "The envelope needs a positive temperature range with a maximum above the minimum, and at least two " +
                "steps.";

            return Task.FromResult(OgResult<OgFlashEnvelope>.Failed(
                rangeMessage, FlashRules.CodeEnvelopeEmpty));
        }

        var feed = inputs.Components
            .Select(component => new FLASH_COMPONENT
            {
                FLASH_COMPONENT_ID = Guid.NewGuid().ToString("N"),
                COMPONENT_NAME = component.Name.Trim(),
                NAME = component.Name.Trim(),
                MOLE_FRACTION = component.MoleFraction,
                CRITICAL_TEMPERATURE = component.CriticalTemperature,
                CRITICAL_PRESSURE = component.CriticalPressure,
                ACENTRIC_FACTOR = component.AcentricFactor,
                MOLECULAR_WEIGHT = component.MolecularWeight,
            })
            .ToList();

        var useEos = !string.Equals(code, "IDEAL_K", StringComparison.OrdinalIgnoreCase);

        try
        {
            var engine = PhaseEnvelope.CalculateEnvelope(
                feed, (double)minTemperatureR, (double)maxTemperatureR, steps, useEos);

            var all = engine.BubbleCurve.Concat(engine.DewCurve).ToList();

            // AC-5: only the points the solver converged are exposed; the rest are counted, not drawn.
            var points = all
                .Where(point => point.Converged && point.Pressure > 0.0)
                .Select(point => new OgEnvelopePoint(
                    point.PointType,
                    (decimal)point.Temperature,
                    (decimal)point.Pressure))
                .ToList();

            var skipped = all.Count - points.Count;

            if (points.Count == 0)
            {
                return Task.FromResult(OgResult<OgFlashEnvelope>.Failed(
                    "The envelope solver did not converge a single point over that temperature range, so there is " +
                    "nothing to show.", FlashRules.CodeEnvelopeEmpty));
            }

            return Task.FromResult(OgResult<OgFlashEnvelope>.Ok(new OgFlashEnvelope(
                points,
                skipped,
                string.IsNullOrWhiteSpace(engine.EosUsed) ? EosName(code) : engine.EosUsed,
                (decimal)engine.CricondenthermTemperature,
                (decimal)engine.CricondenbarPressure,
                (decimal)engine.CriticalTemperature,
                (decimal)engine.CriticalPressure,
                string.Join(Environment.NewLine, points.Select(point => point.Describe())))));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"computing the phase envelope between {minTemperatureR} and {maxTemperatureR} R",
                "the envelope is not shown",
                FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgFlashEnvelope>.Failed(
                $"The envelope could not be computed (reference {reference}).", FlashRules.CodeEnvelopeEmpty));
        }
    }

    public async Task<OgResult<OgFlashWorkspace>> SaveAsync(
        OgFlashRun run,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgFlashWorkspace>.Failed(
                "A saved run must record who saved it.", FlashRules.CodeActorRequired);
        }

        if (run.IsFailed || run.Feed.Count == 0)
        {
            return OgResult<OgFlashWorkspace>.Failed(
                "There is no converged run to save; run the flash first.", FlashRules.CodeNothingToSave);
        }

        if (run.IsSaved)
        {
            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFlashWorkspace>.Failed(
                "No connection is open, so the run cannot be saved.", FlashRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<FLASH_CALCULATION_RESULT>(source);
            unit.PrimaryKey = "FLASH_CALCULATION_RESULT_ID";

            unit.Add(new FLASH_CALCULATION_RESULT
            {
                FLASH_CALCULATION_RESULT_ID = Guid.NewGuid().ToString("N"),
                CALCULATION_ID = null,
                FLASH_CALCULATION_ID = null,
                COMPONENT_ID = null,
                WELL_ID = null,
                FACILITY_ID = null,
                PRESSURE = run.Pressure,
                TEMPERATURE = run.Temperature,
                VAPOR_FRACTION = run.VapourFraction,
                LIQUID_FRACTION = run.LiquidFraction,
                CONVERGED = run.Converged ? "Y" : "N",
                CONVERGENCE_ERROR = run.ConvergenceError,
                FEED_COMPOSITION_JSON = ToJson(run.Feed.Select(component => new StoredComponent(
                    component.Name, component.MoleFraction, 0m, 0m, 0m,
                    component.CriticalTemperature, component.CriticalPressure,
                    component.AcentricFactor, component.MolecularWeight))),
                VAPOR_COMPOSITION_JSON = ToJson(run.Components.Select(component => new StoredComponent(
                    component.Name, component.Z, component.K, 0m, component.Y, 0m, 0m, 0m, 0m))),
                LIQUID_COMPOSITION_JSON = ToJson(run.Components.Select(component => new StoredComponent(
                    component.Name, component.Z, component.K, component.X, 0m, 0m, 0m, 0m, 0m))),
                K_VALUES_JSON = ToJson(run.Components.Select(component => new StoredComponent(
                    component.Name, component.Z, component.K, component.X, component.Y, 0m, 0m, 0m, 0m))),
                CALCULATION_DATE = now,
                STATUS = run.StatusCode,
                ERROR_MESSAGE = run.FailureMessage,
                EOS_CODE = run.EosCode,
                ITERATIONS = run.Iterations,
                STABILITY_TEXT = run.StabilityText,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgFlashWorkspace>.Failed(
                    OgUow.Message(committed, "The run was not saved."), FlashRules.CodeWriteFailed);
            }

            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "saving a flash run",
                "the run was not saved",
                FailureSeverity.Error);

            return OgResult<OgFlashWorkspace>.Failed(
                "The run could not be saved.", reference ?? FlashRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFlashWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgFlashWorkspace>.Failed(
                "No connection is open, so the flash history cannot be read.", FlashRules.CodeNoConnection);
        }

        try
        {
            var results = await OgUow.ReadAllAsync<FLASH_CALCULATION_RESULT>(source).ConfigureAwait(false);
            var reference = await OgUow.ReadAllAsync<R_FLASH_CALCULATION_REFERENCE_CODE>(source)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var eosCodes = reference
                .Where(row => string.Equals(row.REFERENCE_SET, FlashReferenceSets.EosModel, StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
                .Select(row => row.REFERENCE_CODE.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var history = results
                .Where(row => !string.IsNullOrWhiteSpace(row.FLASH_CALCULATION_RESULT_ID))
                .OrderByDescending(row => row.CALCULATION_DATE ?? DateTime.MinValue)
                .ThenBy(row => row.FLASH_CALCULATION_RESULT_ID, StringComparer.Ordinal)
                .Select(ToRun)
                .Take(50)
                .ToList();

            return OgResult<OgFlashWorkspace>.Ok(new OgFlashWorkspace(
                history,
                eosCodes.Count == 0 ? FlashRules.MappingEosCodes : eosCodes,
                eosCodes.Count > 0,
                FlashStatusCodes.All,
                false,
                null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var errorReference = _failures.ReportHandled(
                ex,
                "reading the flash history",
                "the saved history is unavailable; new runs are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgFlashWorkspace>.Ok(new OgFlashWorkspace(
                [],
                FlashRules.MappingEosCodes,
                false,
                FlashStatusCodes.All,
                true,
                $"the saved history could not be read (reference {errorReference})"));
        }
    }

    private static string EosName(string code) => code switch
    {
        "PR" => "Peng-Robinson equation of state",
        "SRK" => "Soave-Redlich-Kwong equation of state",
        "SRK_MODIFIED" => "Modified SRK (custom alpha)",
        _ => "Ideal K-value / simplified vapour-liquid split",
    };

    private static string ToJson<T>(IEnumerable<T> rows) => JsonSerializer.Serialize(rows.ToList());

    private static OgFlashRun ToRun(FLASH_CALCULATION_RESULT row)
    {
        var feed = string.IsNullOrWhiteSpace(row.FEED_COMPOSITION_JSON)
            ? []
            : JsonSerializer.Deserialize<List<StoredComponent>>(row.FEED_COMPOSITION_JSON!) ?? [];

        var components = string.IsNullOrWhiteSpace(row.K_VALUES_JSON)
            ? []
            : JsonSerializer.Deserialize<List<StoredComponent>>(row.K_VALUES_JSON!) ?? [];

        return new OgFlashRun(
            row.FLASH_CALCULATION_RESULT_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(row.STATUS) ? FlashStatusCodes.Success : row.STATUS!,
            string.Equals(row.CONVERGED, "Y", StringComparison.OrdinalIgnoreCase),
            row.ITERATIONS ?? 0,
            row.VAPOR_FRACTION ?? 0m,
            row.LIQUID_FRACTION ?? 0m,
            row.EOS_CODE ?? string.Empty,
            EosName(row.EOS_CODE ?? string.Empty),
            feed.Sum(component => component.Z),
            FlashRules.PhaseState(row.VAPOR_FRACTION ?? 0m, string.Equals(row.CONVERGED, "Y",
                StringComparison.OrdinalIgnoreCase)),
            row.STABILITY_TEXT,
            components
                .Select(component => new OgFlashComponentResult(
                    component.Name, component.Z, component.K, component.X, component.Y))
                .ToList(),
            feed
                .Select(component => new OgFlashComponent(
                    component.Name, component.Z, component.Tc, component.Pc, component.Omega, component.Mw))
                .ToList(),
            row.PRESSURE,
            row.TEMPERATURE,
            row.CONVERGENCE_ERROR,
            string.IsNullOrWhiteSpace(row.ERROR_MESSAGE) ? null : FlashStatusCodes.ConvergenceFailed,
            row.ERROR_MESSAGE,
            row.ROW_CREATED_BY,
            row.CALCULATION_DATE);
    }

    /// <summary>The shape of a component in the owner's JSON columns; the unused numbers stay zero.</summary>
    private sealed record StoredComponent(
        string Name,
        decimal Z,
        decimal K,
        decimal X,
        decimal Y,
        decimal Tc,
        decimal Pc,
        decimal Omega,
        decimal Mw);
}
