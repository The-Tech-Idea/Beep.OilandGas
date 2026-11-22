using System.Globalization;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;
using Beep.OilandGas.PipelineAnalysis.Constants;
using Beep.OilandGas.PipelineAnalysis.Exceptions;
using Beep.OilandGas.PipelineAnalysis.Validation;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;

/// <summary>The two pipeline analysis modes (WTA-03).</summary>
public static class PipelineModes
{
    public const string Gas = "GAS";
    public const string Liquid = "LIQUID";

    public static IReadOnlyList<string> All { get; } = [Gas, Liquid];

    public static bool IsGas(string? mode) =>
        string.Equals(mode?.Trim(), Gas, StringComparison.OrdinalIgnoreCase);

    public static bool IsSupported(string? mode) =>
        !string.IsNullOrWhiteSpace(mode) && All.Contains(mode.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string NameOf(string mode) => mode.Trim().ToUpperInvariant() switch
    {
        Gas => "Gas (Weymouth)",
        Liquid => "Liquid (Darcy-Weisbach)",
        _ => mode,
    };
}

/// <summary>The inputs of one pipeline analysis (WTA-03).</summary>
public sealed record OgPipelineInputs(
    string? WellId,
    string? PipelineId,
    string? Mode,
    decimal? DiameterIn,
    decimal? LengthMiles,
    decimal? RoughnessFt,
    decimal? InletPressure,
    decimal? OutletPressure,
    decimal? AverageTemperatureR,
    decimal? GasSpecificGravity,
    decimal? BasePressure,
    decimal? BaseTemperatureR,
    decimal? LiquidSpecificGravity,
    decimal? LiquidViscosityCp,
    decimal? LiquidRateBblPerDay,
    decimal? SandRateBblPerDay,
    decimal? ParticleDiameterMicron,
    string? ErosionService);

/// <summary>The erosion check as the page shows it (WTA-03 AC-2, SG-08-6).</summary>
public sealed record OgPipelineErosion(
    decimal ActualVelocity,
    decimal Threshold,
    decimal Ratio,
    string Source,
    bool AboveThreshold,
    decimal? SandRateMpy,
    string? SandSource)
{
    public bool HasSource => !string.IsNullOrWhiteSpace(Source);

    public string Describe() =>
        $"velocity {Number(ActualVelocity)} ft/s against the {Number(Threshold)} ft/s limit " +
        $"(ratio {Ratio.ToString("0.###", CultureInfo.InvariantCulture)}" +
        (AboveThreshold ? ", ABOVE it" : ", inside it") + ") · threshold from " + Source +
        (SandRateMpy is { } mpy
            ? " · sand erosion " + Number(mpy) + " mpy from " + (SandSource ?? "an unnamed model")
            : string.Empty);

    private static string Number(decimal value) =>
        value.ToString("#,0.##", CultureInfo.InvariantCulture);
}

/// <summary>
/// One pipeline analysis (WTA-03), matching the plan's <c>OgPipelineResult</c> plus the pressure drop and the inputs.
/// </summary>
public sealed record OgPipelineResult(
    string Id,
    string? WellId,
    string? PipelineId,
    string Mode,
    decimal Capacity,
    string CapacityUnit,
    string Regime,
    decimal? Reynolds,
    decimal? FrictionFactor,
    decimal? PressureDrop,
    decimal? Velocity,
    OgPipelineErosion? Erosion,
    bool Feasible,
    IReadOnlyList<string> Notes,
    OgPipelineInputs Inputs,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string GasCapacityUnit = "Mscf/d";
    public const string LiquidCapacityUnit = "bbl/d";
    public const string PressureUnit = "psia";
    public const string VelocityUnit = "ft/s";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public string ModeText => PipelineModes.NameOf(Mode);

    /// <summary>AC-3: the regime is displayed with the result, and an undecided one says so.</summary>
    public bool RegimeDetermined => !string.Equals(Regime, PipelineStandards.RegimeUndetermined,
        StringComparison.OrdinalIgnoreCase);

    public bool ErosionAboveThreshold => Erosion?.AboveThreshold == true;

    public string CapacityText =>
        Capacity.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + CapacityUnit;

    public string RegimeText => RegimeDetermined
        ? Regime + " (" + PipelineStandards.ThresholdsText + ")"
        : "UNDETERMINED - the Reynolds number could not be computed from these inputs";

    public string ReynoldsText => Reynolds is { } re
        ? re.ToString("#,0.##", CultureInfo.InvariantCulture)
        : "not computed";

    public string FrictionText => FrictionFactor is { } factor
        ? factor.ToString("0.#####", CultureInfo.InvariantCulture)
        : "not computed";

    public string PressureDropText => PressureDrop is { } drop
        ? Number(drop) + " " + PressureUnit
        : "not computed";

    /// <summary>AC-2: a threshold is either sourced or explicitly unconfigured.</summary>
    public string ThresholdText => Erosion is null
        ? "no erosion check was run: threshold source not configured"
        : Erosion.HasSource
            ? Erosion.Describe()
            : "threshold source not configured";

    public string NotesText => Notes.Count == 0
        ? "no note"
        : string.Join(Environment.NewLine, Notes);

    public string VerdictText => Feasible ? "feasible" : "NOT feasible";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {ModeText} · capacity {CapacityText} · {Regime} · {VerdictText}";

    /// <summary>
    /// AC-5: the exported analysis, built from the same properties the screen prints - so an export cannot carry a
    /// different number from the panel it was taken from.
    /// </summary>
    /// <remarks>
    /// The module's own <c>PipelineAnalysisService.Reporting</c> is not used: it returns canned findings
    /// ("Pipeline operating within normal parameters", "All safety factors within acceptable limits") regardless of the
    /// analysis, so exporting through it would print invented conclusions beside real numbers.
    /// </remarks>
    public string ExportText => string.Join(Environment.NewLine,
    [
        "Pipeline analysis",
        "Line: " + (PipelineId ?? "(none)"),
        "Mode: " + ModeText,
        "Capacity: " + CapacityText,
        "Regime: " + RegimeText,
        "Reynolds number: " + ReynoldsText,
        "Friction factor: " + FrictionText,
        "Pressure drop: " + PressureDropText,
        "Erosion: " + ThresholdText,
        "Verdict: " + VerdictText,
        "Notes: " + NotesText,
    ]);

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    private static string Number(decimal value) =>
        value.ToString("#,0.##", CultureInfo.InvariantCulture);
}

/// <summary>A result whose save was not acknowledged: retained and retried (WTA-03's state table).</summary>
public sealed record OgPipelinePending(OgPipelineResult Result, string Actor);

/// <summary>The pipeline workspace (WTA-03).</summary>
public sealed record OgPipelineWorkspace(
    IReadOnlyList<OgPipelineResult> Results,
    OgPipelineResult? Selected,
    IReadOnlyList<string> Modes,
    IReadOnlyList<string> ErosionServices,
    IReadOnlyList<string> DisabledTabs,
    string ErosionSource,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ResultsText => Results.Count == 0
        ? "No pipeline analysis is saved yet."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved analyses could not be read; new analyses are still allowed"
        : $"{Results.Count.ToString(CultureInfo.InvariantCulture)} saved analysis(es)";

    /// <summary>AC-4: the experimental tabs are named, and why they are off.</summary>
    public string DisabledText =>
        "disabled by default: " + string.Join(", ", DisabledTabs) +
        " - both are labelled experimental in the module and are off unless explicitly enabled";

    public string ReferenceText =>
        $"modes {string.Join(", ", Modes)} · erosion C-factors from " +
        $"{string.Join(", ", ErosionServices)} · threshold from {ErosionSource}";
}

/// <summary>The pipeline rules, stated once so the page, the service and the tests agree.</summary>
public static class PipelineRules
{
    public const string CodeNoConnection = "pipeline-no-connection";
    public const string CodeReadFailed = "pipeline-read-failed";
    public const string CodeWriteFailed = "pipeline-write-failed";
    public const string CodeActorRequired = "pipeline-actor-required";
    public const string CodeWellRequired = "pipeline-well-required";
    public const string CodeInputRequired = "pipeline-input-required";
    public const string CodeInputInvalid = "pipeline-input-invalid";
    public const string CodeModeUnknown = "pipeline-mode-unknown";
    public const string CodeServiceUnknown = "pipeline-erosion-service-unknown";
    public const string CodeRegimeUndetermined = "pipeline-regime-undetermined";
    public const string CodeAnalysisRefused = "pipeline-analysis-refused";
    public const string CodeNothingToSave = "pipeline-nothing-to-save";
    public const string CodeNothingPending = "pipeline-nothing-pending";

    public static IReadOnlyList<string> Missing(OgPipelineInputs inputs)
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

        if (string.IsNullOrWhiteSpace(inputs.Mode))
        {
            missing.Add("mode");
        }

        // The state table's "missing fluid properties - refused, links to RES-01".
        Need(inputs.DiameterIn, "diameter");
        Need(inputs.LengthMiles, "length");
        Need(inputs.InletPressure, "inlet pressure");
        Need(inputs.OutletPressure, "outlet pressure");
        Need(inputs.AverageTemperatureR, "average temperature");

        if (PipelineModes.IsGas(inputs.Mode))
        {
            Need(inputs.GasSpecificGravity, "gas specific gravity");
            Need(inputs.BasePressure, "base pressure");
            Need(inputs.BaseTemperatureR, "base temperature");
        }
        else
        {
            Need(inputs.LiquidSpecificGravity, "liquid specific gravity");
            Need(inputs.LiquidViscosityCp, "liquid viscosity");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgPipelineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well or line the analysis is for.")
                : (CodeInputRequired, $"The analysis needs {string.Join(", ", missing)}.");
        }

        if (!PipelineModes.IsSupported(inputs.Mode))
        {
            return (CodeModeUnknown,
                $"'{inputs.Mode}' is not a pipeline mode; use {string.Join(", ", PipelineModes.All)}.");
        }

        if (inputs.DiameterIn <= 0m || inputs.LengthMiles <= 0m)
        {
            return (CodeInputInvalid, "The diameter and the length must both be above zero.");
        }

        if (inputs.OutletPressure >= inputs.InletPressure)
        {
            return (CodeInputInvalid,
                "The outlet pressure must be below the inlet pressure, or there is no flow to analyse.");
        }

        if (inputs.AverageTemperatureR <= 0m)
        {
            return (CodeInputInvalid, "The average temperature must be above zero (Rankine).");
        }

        if (inputs.RoughnessFt is < 0m)
        {
            return (CodeInputInvalid, "The roughness cannot be negative.");
        }

        return PipelineStandards.Resolve(inputs.ErosionService) is null
            ? (CodeServiceUnknown,
                $"'{inputs.ErosionService}' is not an erosion service; the standards state " +
                $"{string.Join(", ", PipelineStandards.ErosionServices.Select(service => service.Code))}.")
            : null;
    }
}

/// <summary>WTA-03: pipeline analysis and integrity.</summary>
public interface IOgPipeline
{
    /// <summary>Analyses the line. Nothing is written.</summary>
    Task<OgResult<OgPipelineWorkspace>> AnalyseAsync(
        OgPipelineInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the result with its mode and inputs (AC-6).</summary>
    Task<OgResult<OgPipelineWorkspace>> SaveAsync(
        OgPipelineResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved analyses; an unavailable history is a state.</summary>
    Task<OgResult<OgPipelineWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgPipelinePending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgPipelineWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WTA-03 over the PipelineAnalysis module's capacity, flow, erosion and validation code, through a unit of work.
/// </summary>
/// <remarks>
/// Capacity is the module's own: <c>PipelineCapacityCalculator.CalculateGasPipelineCapacity</c> (Weymouth) or
/// <c>CalculateLiquidPipelineCapacity</c> (Darcy-Weisbach). The flow regime is the module's own Reynolds thresholds, named
/// in <see cref="PipelineStandards"/> so the printed boundary is the applied one. Erosion is
/// <c>ErosionPrediction.CalculateErosionalVelocity</c> with a C-factor from
/// <see cref="PipelineStandards.ErosionServices"/> and its citation beside it, so a limit is never stored without a source
/// (AC-2, SG-08-6).
/// </remarks>
public sealed class OgPipelineService : IOgPipeline
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPipelineService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgPipelinePending? Pending { get; private set; }

    public async Task<OgResult<OgPipelineWorkspace>> AnalyseAsync(
        OgPipelineInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (PipelineRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgPipelineWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var built = Build(inputs);

            if (built.Refusal is { } refused)
            {
                return OgResult<OgPipelineWorkspace>.Failed(refused.Message, refused.Code);
            }

            // The state table's "flow regime not determined - refused with the input responsible".
            if (built.Result is { RegimeDetermined: false } undetermined)
            {
                return OgResult<OgPipelineWorkspace>.Failed(
                    $"The flow regime could not be decided: the Reynolds number came out as " +
                    $"{undetermined.ReynoldsText}, which points at the velocity the capacity relation produced " +
                    $"({undetermined.Velocity?.ToString("#,0.####", CultureInfo.InvariantCulture) ?? "none"} ft/s). " +
                    "Check the diameter, the pressures and the fluid properties.",
                    PipelineRules.CodeRegimeUndetermined);
            }

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgPipelineWorkspace>.Ok(Workspace(history, built.Result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a pipeline for {inputs.WellId}",
                "the analysis is not shown",
                FailureSeverity.Error);

            return OgResult<OgPipelineWorkspace>.Failed(
                "The pipeline analysis could not be run.", reference ?? PipelineRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPipelineWorkspace>> SaveAsync(
        OgPipelineResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPipelineWorkspace>.Failed(
                "A saved result must record who saved it.", PipelineRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(result.Mode) || !result.RegimeDetermined)
        {
            return OgResult<OgPipelineWorkspace>.Failed(
                "There is no decided result to save; run an analysis that determines the regime first.",
                PipelineRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", PipelineRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<PIPELINE_ANALYSIS_REGISTER>(source);
            unit.PrimaryKey = "REGISTER_ID";

            var input = result.Inputs;

            unit.Add(new PIPELINE_ANALYSIS_REGISTER
            {
                REGISTER_ID = Guid.NewGuid().ToString("N"),
                MODE = result.Mode,
                WELL_UWI = result.WellId,
                PIPELINE_ID = result.PipelineId,
                ANALYSIS_DATE = now,
                DIAMETER_IN = input.DiameterIn,
                LENGTH_MILES = input.LengthMiles,
                ROUGHNESS_FT = input.RoughnessFt,
                INLET_PRESSURE = input.InletPressure,
                OUTLET_PRESSURE = input.OutletPressure,
                AVERAGE_TEMPERATURE_R = input.AverageTemperatureR,
                GAS_SPECIFIC_GRAVITY = input.GasSpecificGravity,
                BASE_PRESSURE = input.BasePressure,
                BASE_TEMPERATURE_R = input.BaseTemperatureR,
                LIQUID_SPECIFIC_GRAVITY = input.LiquidSpecificGravity,
                LIQUID_VISCOSITY_CP = input.LiquidViscosityCp,
                CAPACITY = result.Capacity,
                CAPACITY_OUOM = result.CapacityUnit,
                FLOW_REGIME = result.Regime,
                REYNOLDS_NUMBER = result.Reynolds,
                FRICTION_FACTOR = result.FrictionFactor,
                PRESSURE_DROP_PSI = result.PressureDrop,
                VELOCITY_FPS = result.Velocity,
                EROSION_RATE_MPY = result.Erosion?.SandRateMpy,
                EROSION_THRESHOLD_FPS = result.Erosion?.Threshold,
                VELOCITY_RATIO = result.Erosion?.Ratio,
                THRESHOLD_SOURCE = result.Erosion?.Source ?? "threshold source not configured",
                EROSION_SERVICE = input.ErosionService,
                FEASIBLE_IND = result.Feasible ? "Y" : "N",
                NOTES = result.Notes.Count == 0 ? null : string.Join("\n", result.Notes),
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
                Pending = new OgPipelinePending(result, actor.Trim());

                return OgResult<OgPipelineWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved; the original is retained and can be retried."),
                    PipelineRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgPipelineWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgPipelinePending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a pipeline analysis for {result.WellId}",
                "the result was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgPipelineWorkspace>.Failed(
                "The result was not acknowledged; the original is retained and the save can be retried.",
                reference ?? PipelineRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPipelineWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgPipelineWorkspace>.Failed(
                "There is no result waiting to be retried.", PipelineRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgPipelineWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineWorkspace>.Failed(
                "No connection is open, so the saved analyses cannot be read.", PipelineRules.CodeNoConnection);
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

            return OgResult<OgPipelineWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved pipeline analyses",
                "the saved analyses are unavailable; new analyses are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgPipelineWorkspace>.Ok(Workspace(
                [], null, true, $"the saved analyses could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the analysis

    /// <summary>What an analysis produced: a result, or the refusal the engine stated.</summary>
    public sealed record BuiltResult(OgPipelineResult? Result, (string Code, string Message)? Refusal);

    /// <summary>Runs the module's capacity relation, its Reynolds thresholds and its erosion check.</summary>
    public BuiltResult Build(OgPipelineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var mode = inputs.Mode!.Trim().ToUpperInvariant();
        var service = PipelineStandards.Resolve(inputs.ErosionService)!;
        var roughness = inputs.RoughnessFt ?? PipelineConstants.Roughness.CommercialSteel;

        // The module's capacity relations do not share a length unit: Weymouth takes miles and the liquid Darcy-Weisbach
        // path takes feet. One input, converted once, and the conversion is stated with the result.
        var pipeline = new PIPELINE_PROPERTIES
        {
            PIPELINE_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            DIAMETER = inputs.DiameterIn!.Value,
            LENGTH = PipelineModes.IsGas(mode) ? inputs.LengthMiles!.Value : inputs.LengthMiles.Value * 5280m,
            ROUGHNESS = roughness,
            INLET_PRESSURE = inputs.InletPressure!.Value,
            OUTLET_PRESSURE = inputs.OutletPressure!.Value,
            AVERAGE_TEMPERATURE = inputs.AverageTemperatureR!.Value,
        };

        var notes = new List<string>();
        PIPELINE_CAPACITY_RESULT capacity;

        try
        {
            if (PipelineModes.IsGas(mode))
            {
                var gas = new GAS_PIPELINE_FLOW_PROPERTIES
                {
                    GAS_PIPELINE_FLOW_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                    PIPELINE = pipeline,
                    GAS_FLOW_RATE = 0m,
                    GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
                    BASE_PRESSURE = inputs.BasePressure!.Value,
                    BASE_TEMPERATURE = inputs.BaseTemperatureR!.Value,
                };

                PipelineValidator.ValidateGasFlowProperties(gas);
                capacity = PipelineCapacityCalculator.CalculateGasPipelineCapacity(gas);
            }
            else
            {
                var liquid = new LIQUID_PIPELINE_FLOW_PROPERTIES
                {
                    LIQUID_PIPELINE_FLOW_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                    PIPELINE = pipeline,
                    LIQUID_FLOW_RATE = inputs.LiquidRateBblPerDay ?? 0m,
                    LIQUID_SPECIFIC_GRAVITY = inputs.LiquidSpecificGravity!.Value,
                    LIQUID_VISCOSITY = inputs.LiquidViscosityCp!.Value,
                };

                PipelineValidator.ValidateLiquidFlowProperties(liquid);
                capacity = PipelineCapacityCalculator.CalculateLiquidPipelineCapacity(liquid);

                // The liquid path evaluates its friction factor at a typical velocity rather than the answer it produces.
                notes.Add(
                    "the liquid friction factor is evaluated at the module's typical 5 ft/s rather than at the velocity " +
                    "this answer implies, so the liquid capacity is a screening figure; the arithmetic is checked against " +
                    "the friction factor the module reports");
            }
        }
        catch (PipelineException ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a pipeline for {inputs.WellId}",
                "the analysis is refused and no result is shown",
                FailureSeverity.Degraded);

            return new BuiltResult(null, (PipelineRules.CodeAnalysisRefused,
                $"The pipeline calculation refused these inputs, so no result is shown (reference {reference}). " +
                "Check the diameter, the length, the pressures and the fluid properties."));
        }

        var capacityUnit = PipelineModes.IsGas(mode)
            ? OgPipelineResult.GasCapacityUnit
            : OgPipelineResult.LiquidCapacityUnit;

        var regime = PipelineStandards.RegimeOf(capacity.REYNOLDS_NUMBER);

        if (!string.Equals(regime, PipelineStandards.RegimeUndetermined, StringComparison.OrdinalIgnoreCase)
            && capacity.MAXIMUM_FLOW_RATE > 0m)
        {
            notes.Add(
                $"capacity from {(PipelineModes.IsGas(mode) ? "the Weymouth relation" : "the Darcy-Weisbach relation")} " +
                $"over {inputs.LengthMiles!.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} mi");
        }

        var erosion = AssessErosion(inputs, service, capacity, pipeline, mode, notes);

        var feasible = capacity.MAXIMUM_FLOW_RATE > 0m
                       && !string.Equals(regime, PipelineStandards.RegimeUndetermined, StringComparison.OrdinalIgnoreCase)
                       && erosion is not { AboveThreshold: true };

        if (erosion is { AboveThreshold: true })
        {
            notes.Add(
                "the velocity is above the erosional limit, so the line is not feasible at this capacity: " +
                erosion.Source);
        }

        return new BuiltResult(
            new OgPipelineResult(
                string.Empty,
                inputs.WellId!.Trim(),
                inputs.PipelineId,
                mode,
                capacity.MAXIMUM_FLOW_RATE,
                capacityUnit,
                regime,
                capacity.REYNOLDS_NUMBER,
                capacity.FRICTION_FACTOR,
                capacity.PRESSURE_DROP,
                capacity.FLOW_VELOCITY,
                erosion,
                feasible,
                notes,
                inputs,
                null,
                null),
            null);
    }

    /// <summary>
    /// AC-2 and SG-08-6: the erosional limit with the standard that states it, and the sand rate when one was given.
    /// </summary>
    private static OgPipelineErosion? AssessErosion(
        OgPipelineInputs inputs,
        PipelineErosionService service,
        PIPELINE_CAPACITY_RESULT capacity,
        PIPELINE_PROPERTIES pipeline,
        string mode,
        List<string> notes)
    {
        if (capacity.FLOW_VELOCITY <= 0m || pipeline.INLET_PRESSURE <= 0m)
        {
            notes.Add("no erosion check was run: the capacity relation produced no velocity to check");

            return null;
        }

        // The liquid path carries the line's liquid; the gas path carries what the caller states, defaulting to dry gas.
        var liquidFraction = PipelineModes.IsGas(mode) ? 0.0 : 1.0;
        var liquidDensity = PipelineModes.IsGas(mode)
            ? 62.4
            : (double)(inputs.LiquidSpecificGravity!.Value * 62.4m);

        var check = ErosionPrediction.CalculateErosionalVelocity(
            (double)(inputs.GasSpecificGravity ?? 0.65m),
            liquidFraction,
            liquidDensity,
            (double)pipeline.INLET_PRESSURE,
            (double)(pipeline.AVERAGE_TEMPERATURE - 459.67m),
            (double)service.CFactor,
            (double)capacity.FLOW_VELOCITY);

        decimal? sandMpy = null;

        if (inputs.SandRateBblPerDay is { } sand && sand > 0m)
        {
            var sandResult = ErosionPrediction.CalculateSalamaErosionRate(
                (double)sand,
                (double)(inputs.ParticleDiameterMicron ?? 250m),
                check.MixtureDensityLbFt3,
                (double)pipeline.DIAMETER);

            sandMpy = (decimal)sandResult.ErosionRateMpy;
        }

        return new OgPipelineErosion(
            (decimal)check.ActualVelocityFtS,
            (decimal)check.ErosionalVelocityFtS,
            (decimal)check.VelocityRatio,
            PipelineStandards.ThresholdSourceFor(service),
            (decimal)check.VelocityRatio > 1m,
            sandMpy,
            sandMpy is null ? null : PipelineStandards.SandErosionSource);
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgPipelineResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<PIPELINE_ANALYSIS_REGISTER>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.REGISTER_ID))
            .Where(row => !string.IsNullOrWhiteSpace(row.MODE))
            .OrderByDescending(row => row.RECORDED_AT ?? row.ANALYSIS_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.REGISTER_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgPipelineResult ToResult(PIPELINE_ANALYSIS_REGISTER row)
    {
        var notes = string.IsNullOrWhiteSpace(row.NOTES)
            ? []
            : row.NOTES!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        var mode = row.MODE;

        OgPipelineErosion? erosion = row.EROSION_THRESHOLD_FPS is { } threshold
            ? new OgPipelineErosion(
                row.VELOCITY_FPS ?? 0m,
                threshold,
                row.VELOCITY_RATIO ?? 0m,
                row.THRESHOLD_SOURCE ?? "threshold source not configured",
                (row.VELOCITY_RATIO ?? 0m) > 1m,
                row.EROSION_RATE_MPY,
                row.EROSION_RATE_MPY is null ? null : PipelineStandards.SandErosionSource)
            : null;

        return new OgPipelineResult(
            row.REGISTER_ID,
            row.WELL_UWI,
            row.PIPELINE_ID,
            mode,
            row.CAPACITY,
            row.CAPACITY_OUOM,
            row.FLOW_REGIME,
            row.REYNOLDS_NUMBER,
            row.FRICTION_FACTOR,
            row.PRESSURE_DROP_PSI,
            row.VELOCITY_FPS,
            erosion,
            string.Equals(row.FEASIBLE_IND, "Y", StringComparison.OrdinalIgnoreCase),
            notes,
            new OgPipelineInputs(
                row.WELL_UWI,
                row.PIPELINE_ID,
                mode,
                row.DIAMETER_IN,
                row.LENGTH_MILES,
                row.ROUGHNESS_FT,
                row.INLET_PRESSURE,
                row.OUTLET_PRESSURE,
                row.AVERAGE_TEMPERATURE_R,
                row.GAS_SPECIFIC_GRAVITY,
                row.BASE_PRESSURE,
                row.BASE_TEMPERATURE_R,
                row.LIQUID_SPECIFIC_GRAVITY,
                row.LIQUID_VISCOSITY_CP,
                null,
                null,
                null,
                row.EROSION_SERVICE),
            row.ACTOR,
            row.RECORDED_AT ?? row.ANALYSIS_DATE);
    }

    private static OgPipelineWorkspace Workspace(
        IReadOnlyList<OgPipelineResult> history,
        OgPipelineResult? selected,
        bool historyFailed = false,
        string? historyReason = null) =>
        new(
            history,
            selected,
            PipelineModes.All,
            PipelineStandards.ErosionServices.Select(service => service.Code).ToList(),
            ["slug flow", "simulation"],
            PipelineStandards.ErosionalVelocitySource,
            historyFailed,
            historyReason);
}
