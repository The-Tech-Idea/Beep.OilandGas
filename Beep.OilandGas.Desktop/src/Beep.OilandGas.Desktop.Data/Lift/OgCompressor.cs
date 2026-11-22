using System.Globalization;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.Models.Data.Calculations;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The compressor kinds and analysis tokens, taken from <see cref="CompressorAnalysisWellKnown"/> (LFT-08 AC-4).</summary>
public static class CompressorKinds
{
    public const string Centrifugal = CompressorAnalysisWellKnown.CompressorType.Centrifugal;
    public const string Reciprocating = CompressorAnalysisWellKnown.CompressorType.Reciprocating;

    public static IReadOnlyList<string> All { get; } = [Centrifugal, Reciprocating];

    public static IReadOnlyList<string> AnalysisTypes { get; } =
    [
        CompressorAnalysisWellKnown.AnalysisType.Power,
        CompressorAnalysisWellKnown.AnalysisType.Pressure,
        CompressorAnalysisWellKnown.AnalysisType.Efficiency,
    ];

    public static bool IsSupported(string? kind) =>
        !string.IsNullOrWhiteSpace(kind)
        && All.Contains(kind.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static bool IsCentrifugal(string? kind) =>
        string.Equals(kind?.Trim(), Centrifugal, StringComparison.OrdinalIgnoreCase);

    public static string NameOf(string kind) => kind.Trim().ToUpperInvariant() switch
    {
        Centrifugal => "Centrifugal",
        Reciprocating => "Reciprocating",
        _ => kind,
    };
}

/// <summary>The inputs of one compressor analysis (LFT-08).</summary>
public sealed record OgCompressorInputs(
    string? WellId,
    string? Kind,
    decimal? SuctionPressure,
    decimal? DischargePressure,
    decimal? SuctionTemperatureF,
    decimal? DischargeTemperatureF,
    decimal? GasFlowMscfPerDay,
    decimal? GasSpecificGravity,
    decimal? PolytropicEfficiency,
    decimal? SpecificHeatRatio,
    decimal? MechanicalEfficiency,
    int? Stages,
    decimal? Speed,
    decimal? CylinderDiameter,
    decimal? StrokeLength,
    decimal? RotationalSpeed,
    int? Cylinders,
    decimal? VolumetricEfficiency,
    decimal? ClearanceFactor);

/// <summary>One staging candidate as the page lists it (LFT-08 AC-2).</summary>
public sealed record OgCompressorStage(int Stages, decimal Ratio, decimal HottestR, bool Allowed)
{
    public string RatioText => Ratio.ToString("0.####", CultureInfo.InvariantCulture);

    public string TemperatureText => HottestR.ToString("#,0.##", CultureInfo.InvariantCulture) + " R";

    public string OutcomeText => Allowed ? "allowed" : "rejected";

    public string Describe() =>
        $"{Stages.ToString(CultureInfo.InvariantCulture)} stage(s) · ratio {RatioText} · hottest " +
        $"{TemperatureText} · {OutcomeText}";
}

/// <summary>
/// One compressor analysis (LFT-08), matching the plan's <c>OgCompressorResult</c> plus the staging detail and the reasons.
/// </summary>
public sealed record OgCompressorResult(
    string Id,
    string? WellId,
    string Kind,
    int? Stages,
    decimal PressureRatio,
    decimal? Head,
    string HeadUnit,
    decimal? Efficiency,
    decimal? Power,
    string PowerUnit,
    decimal? PowerKilowatts,
    decimal? DischargeTemperature,
    bool Feasible,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<OgCompressorStage> Staging,
    decimal? StageTemperatureLimitR,
    decimal? StageRatioLimit,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string HeadUnitText = "ft-lbf/lbm";
    public const string PowerUnitText = "hp";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public string KindText => CompressorKinds.NameOf(Kind);

    /// <summary>AC-5: a power figure never appears without its unit.</summary>
    public string PowerText => Power is { } power
        ? power.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + PowerUnit +
          (PowerKilowatts is { } kw
              ? " (" + kw.ToString("#,0.##", CultureInfo.InvariantCulture) + " kW)"
              : string.Empty)
        : "no power figure: the machine is not feasible";

    public string HeadText => Head is { } head
        ? head.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + HeadUnit
        : "no head figure";

    public string EfficiencyText => Efficiency is { } efficiency
        ? (efficiency * 100m).ToString("0.##", CultureInfo.InvariantCulture) + " %"
        : "no efficiency";

    public string RatioText => PressureRatio.ToString("0.####", CultureInfo.InvariantCulture);

    public string StageText => Stages is { } stages
        ? $"{stages.ToString(CultureInfo.InvariantCulture)} stage(s)" +
          (StageRatioLimit is { } ratioLimit
              ? " · per-stage ratio limit " + ratioLimit.ToString("0.##", CultureInfo.InvariantCulture)
              : string.Empty) +
          (StageTemperatureLimitR is { } temperatureLimit
              ? " · interstage limit " + temperatureLimit.ToString("#,0.##", CultureInfo.InvariantCulture) +
                " R (" + (temperatureLimit - 459.67m).ToString("#,0.##", CultureInfo.InvariantCulture) + " F)"
              : string.Empty)
        : "no staging";

    public string DischargeTemperatureText => DischargeTemperature is { } temperature
        ? temperature.ToString("#,0.##", CultureInfo.InvariantCulture) + " R (" +
          (temperature - 459.67m).ToString("#,0.##", CultureInfo.InvariantCulture) + " F)"
        : "no discharge temperature";

    public string ReasonsText => Reasons.Count == 0
        ? "no reason against it: the machine meets the duty"
        : string.Join(Environment.NewLine, Reasons);

    public string StagingText => Staging.Count == 0
        ? "no staging was evaluated"
        : string.Join(Environment.NewLine, Staging.Select(stage => stage.Describe()));

    public string VerdictText => Feasible ? "feasible" : "INFEASIBLE";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {KindText} · ratio {RatioText} · {StageText} · " +
        $"{PowerText} · {VerdictText} · {SavedText}";
}

/// <summary>A result whose save was not acknowledged: retained and retried (LFT-08's state table).</summary>
public sealed record OgCompressorPending(OgCompressorResult Result, string Actor);

/// <summary>The compressor workspace (LFT-08).</summary>
public sealed record OgCompressorWorkspace(
    IReadOnlyList<OgCompressorResult> Results,
    OgCompressorResult? Selected,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<string> AnalysisTypes,
    decimal InterstageLimitR,
    decimal InterstageLimitF,
    decimal MaximumRatioPerStage,
    int MaximumStages,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ResultsText => Results.Count == 0
        ? "No compressor analysis is saved yet."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved results could not be read; new analyses are still allowed"
        : $"{Results.Count.ToString(CultureInfo.InvariantCulture)} saved result(s)";

    /// <summary>The state table's "type not selected - analysis disabled with the reason".</summary>
    public string KindNote => "A compressor analysis needs its kind: centrifugal and reciprocating machines are " +
                              $"evaluated by different calculators, so the analysis is disabled until one is named. " +
                              $"Kinds: {string.Join(", ", Kinds)}.";

    public string ReferenceText =>
        $"kinds {string.Join(", ", Kinds)} · interstage limit " +
        $"{InterstageLimitF.ToString("#,0.##", CultureInfo.InvariantCulture)} F " +
        $"({InterstageLimitR.ToString("#,0.##", CultureInfo.InvariantCulture)} R) · per-stage ratio limit " +
        $"{MaximumRatioPerStage.ToString("0.##", CultureInfo.InvariantCulture)} · up to " +
        $"{MaximumStages.ToString(CultureInfo.InvariantCulture)} stages";
}

/// <summary>The compressor rules, stated once so the page, the service and the tests agree.</summary>
public static class CompressorRules
{
    public const string CodeNoConnection = "compressor-no-connection";
    public const string CodeReadFailed = "compressor-read-failed";
    public const string CodeWriteFailed = "compressor-write-failed";
    public const string CodeActorRequired = "compressor-actor-required";
    public const string CodeWellRequired = "compressor-well-required";
    public const string CodeInputRequired = "compressor-input-required";
    public const string CodeKindRequired = "compressor-kind-required";
    public const string CodeKindUnknown = "compressor-kind-unknown";
    public const string CodeInputInvalid = "compressor-input-invalid";
    public const string CodeInterstageTooHot = "compressor-interstage-too-hot";
    public const string CodeNothingToSave = "compressor-nothing-to-save";
    public const string CodeNothingPending = "compressor-nothing-pending";

    public static IReadOnlyList<string> Missing(OgCompressorInputs inputs)
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

        // The state table: "type not selected - analysis disabled with the reason".
        if (string.IsNullOrWhiteSpace(inputs.Kind))
        {
            missing.Add("compressor kind");
        }

        Need(inputs.SuctionPressure, "suction pressure");
        Need(inputs.DischargePressure, "discharge pressure");
        Need(inputs.SuctionTemperatureF, "suction temperature");
        Need(inputs.DischargeTemperatureF, "discharge temperature");
        Need(inputs.GasFlowMscfPerDay, "gas flow");
        Need(inputs.GasSpecificGravity, "gas specific gravity");

        if (CompressorKinds.IsCentrifugal(inputs.Kind))
        {
            Need(inputs.PolytropicEfficiency, "polytropic efficiency");
            Need(inputs.SpecificHeatRatio, "specific heat ratio");
        }

        if (CompressorKinds.IsSupported(inputs.Kind) && !CompressorKinds.IsCentrifugal(inputs.Kind))
        {
            Need(inputs.CylinderDiameter, "cylinder diameter");
            Need(inputs.StrokeLength, "stroke length");
            Need(inputs.RotationalSpeed, "rotational speed");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgCompressorInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the analysis is for.")
                : missing.Contains("compressor kind")
                    ? (CodeKindRequired,
                        "Choose the machine: a centrifugal and a reciprocating compressor are different calculations, " +
                        "and the analysis is disabled until one is named.")
                    : (CodeInputRequired, $"The analysis needs {string.Join(", ", missing)}.");
        }

        if (!CompressorKinds.IsSupported(inputs.Kind))
        {
            return (CodeKindUnknown,
                $"'{inputs.Kind}' is not a compressor kind; use {string.Join(", ", CompressorKinds.All)}.");
        }

        if (inputs.DischargePressure <= inputs.SuctionPressure)
        {
            return (CodeInputInvalid,
                "The discharge pressure must be above the suction pressure, or there is nothing to compress.");
        }

        if (inputs.PolytropicEfficiency is <= 0m or > 1m)
        {
            return (CodeInputInvalid, "The polytropic efficiency is a fraction above zero and at most 1.");
        }

        if (inputs.SpecificHeatRatio is <= 1m)
        {
            return (CodeInputInvalid, "The specific heat ratio must be above 1.");
        }

        if (inputs.MechanicalEfficiency is <= 0m or > 1m)
        {
            return (CodeInputInvalid, "The mechanical efficiency is a fraction above zero and at most 1.");
        }

        return inputs.Stages is < 1 or > CompressorStagingLimits.MaximumStages
            ? (CodeInputInvalid,
                $"A stage count must be between 1 and " +
                $"{CompressorStagingLimits.MaximumStages.ToString(CultureInfo.InvariantCulture)}.")
            : null;
    }
}

/// <summary>LFT-08: compressor analysis.</summary>
public interface IOgCompressor
{
    /// <summary>Analyses the machine. Nothing is written.</summary>
    Task<OgResult<OgCompressorWorkspace>> AnalyseAsync(
        OgCompressorInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the result with its kind, staging and efficiency (AC-6).</summary>
    Task<OgResult<OgCompressorWorkspace>> SaveAsync(
        OgCompressorResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved results; an unavailable history is a state.</summary>
    Task<OgResult<OgCompressorWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgCompressorPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgCompressorWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-08 over the CompressorAnalysis module's centrifugal, reciprocating and multistage calculators, its staging limits
/// and the <c>COMPRESSOR_POWER_RESULT</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The numbers are the module's: <c>CentrifugalCompressorCalculator.CalculatePower</c> or
/// <c>ReciprocatingCompressorCalculator.CalculatePower</c> for the head, power and efficiency, and
/// <c>CompressorStagingLimits</c> (which delegates to <c>MultistageCompressor</c>) for the staging and the interstage
/// temperature limit. The screen adds the feasibility reasons and the record.
/// </remarks>
public sealed class OgCompressorService : IOgCompressor
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgCompressorService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgCompressorPending? Pending { get; private set; }

    public async Task<OgResult<OgCompressorWorkspace>> AnalyseAsync(
        OgCompressorInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (CompressorRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgCompressorWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = Build(inputs);

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgCompressorWorkspace>.Ok(Workspace(history, result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a compressor for well {inputs.WellId}",
                "the analysis is not shown",
                FailureSeverity.Error);

            return OgResult<OgCompressorWorkspace>.Failed(
                "The compressor analysis could not be run.", reference ?? CompressorRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgCompressorWorkspace>> SaveAsync(
        OgCompressorResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCompressorWorkspace>.Failed(
                "A saved result must record who saved it.", CompressorRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(result.Kind))
        {
            return OgResult<OgCompressorWorkspace>.Failed(
                "There is no result to save; run the analysis first.", CompressorRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompressorWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", CompressorRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<COMPRESSOR_POWER_RESULT>(source);
            unit.PrimaryKey = "COMPRESSOR_POWER_RESULT_ID";

            unit.Add(new COMPRESSOR_POWER_RESULT
            {
                COMPRESSOR_POWER_RESULT_ID = Guid.NewGuid().ToString("N"),
                COMPRESSOR_OPERATING_CONDITIONS_ID = null,
                COMPRESSION_RATIO = result.PressureRatio,

                // A refused machine has no achieved power: the figure is left out rather than stored as its answer.
                THEORETICAL_POWER = result.Feasible ? result.Power ?? 0m : 0m,
                BRAKE_HORSEPOWER = result.Feasible ? result.Power ?? 0m : 0m,
                MOTOR_HORSEPOWER = result.Feasible ? result.Power ?? 0m : 0m,
                POWER_CONSUMPTION_KW = result.Feasible ? result.PowerKilowatts ?? 0m : 0m,
                POLYTROPIC_HEAD = result.Head ?? 0m,
                DISCHARGE_TEMPERATURE = result.DischargeTemperature ?? 0m,
                OVERALL_EFFICIENCY = result.Efficiency ?? 0m,
                POLYTROPIC_EFFICIENCY = result.Efficiency ?? 0m,
                KIND = result.Kind,
                STAGES = result.Stages,
                STAGE_RATIO = result.Staging
                    .Where(stage => result.Stages is null || stage.Stages == result.Stages)
                    .Select(stage => (decimal?)stage.Ratio)
                    .FirstOrDefault(),
                MAX_STAGE_TEMPERATURE_R = result.Staging
                    .Where(stage => result.Stages is null || stage.Stages == result.Stages)
                    .Select(stage => (decimal?)stage.HottestR)
                    .FirstOrDefault(),
                STAGE_TEMPERATURE_LIMIT_R = result.StageTemperatureLimitR,
                STAGE_RATIO_LIMIT = result.StageRatioLimit,
                FEASIBLE_IND = result.Feasible ? "Y" : "N",
                REASONS = result.Reasons.Count == 0 ? null : string.Join("\n", result.Reasons),
                POWER_ACHIEVED_IND = result.Feasible ? "Y" : "N",
                INLET_PRESSURE_PSIA = null,
                OUTLET_PRESSURE_PSIA = null,
                GAS_FLOW_MSCFD = null,
                GAS_SPECIFIC_GRAVITY = null,
                HEAD_OUOM = result.HeadUnit,
                POWER_OUOM = result.PowerUnit,
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
                Pending = new OgCompressorPending(result, actor.Trim());

                return OgResult<OgCompressorWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved; the original is retained and can be retried."),
                    CompressorRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgCompressorWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgCompressorPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a compressor analysis for well {result.WellId}",
                "the result was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgCompressorWorkspace>.Failed(
                "The result was not acknowledged; the original is retained and the save can be retried.",
                reference ?? CompressorRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompressorWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgCompressorWorkspace>.Failed(
                "There is no result waiting to be retried.", CompressorRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgCompressorWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompressorWorkspace>.Failed(
                "No connection is open, so the saved results cannot be read.", CompressorRules.CodeNoConnection);
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

            return OgResult<OgCompressorWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved compressor results",
                "the saved results are unavailable; new analyses are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgCompressorWorkspace>.Ok(new OgCompressorWorkspace(
                [],
                null,
                CompressorKinds.All,
                CompressorKinds.AnalysisTypes,
                CompressorStagingLimits.MaximumDischargeTemperatureR,
                CompressorStagingLimits.MaximumDischargeTemperatureF,
                CompressorStagingLimits.MaximumRatioPerStage,
                CompressorStagingLimits.MaximumStages,
                true,
                $"the saved results could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the analysis

    /// <summary>
    /// Runs the module's calculator for the kind and the module's staging plan, then states every reason the machine
    /// cannot meet the duty. An infeasible machine carries no power figure (AC-3).
    /// </summary>
    public static OgCompressorResult Build(OgCompressorInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var kind = inputs.Kind!.Trim().ToUpperInvariant();

        var conditions = new COMPRESSOR_OPERATING_CONDITIONS
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = Guid.NewGuid().ToString("N"),
            SUCTION_PRESSURE = inputs.SuctionPressure!.Value,
            DISCHARGE_PRESSURE = inputs.DischargePressure!.Value,
            SUCTION_TEMPERATURE = ToRankine(inputs.SuctionTemperatureF!.Value),
            DISCHARGE_TEMPERATURE = ToRankine(inputs.DischargeTemperatureF!.Value),
            GAS_FLOW_RATE = inputs.GasFlowMscfPerDay!.Value,
            GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
            COMPRESSOR_EFFICIENCY = inputs.PolytropicEfficiency ?? 0.75m,
            MECHANICAL_EFFICIENCY = inputs.MechanicalEfficiency ?? 0.95m,
        };

        decimal ratio = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;
        var reasons = new List<string>();
        var staging = new List<OgCompressorStage>();
        int? stages = inputs.Stages;
        decimal? stageLimitR = null;
        decimal? stageRatioLimit = null;

        decimal? head;
        decimal? power;
        decimal? kilowatts;
        decimal? efficiency;
        decimal? dischargeTemperature;

        if (CompressorKinds.IsCentrifugal(kind))
        {
            var machine = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                OPERATING_CONDITIONS = conditions,
                POLYTROPIC_EFFICIENCY = inputs.PolytropicEfficiency!.Value,
                SPECIFIC_HEAT_RATIO = inputs.SpecificHeatRatio!.Value,
                NUMBER_OF_STAGES = inputs.Stages,
                SPEED = inputs.Speed ?? 0m,
            };

            // The module's own staging plan: the fewest stages that satisfy its ratio and interstage temperature limits.
            var plan = CompressorStagingLimits.Plan(machine);

            stageLimitR = plan.MaximumDischargeTemperatureR;
            stageRatioLimit = plan.MaximumRatioPerStage;
            staging.AddRange(plan.Options.Select(option => new OgCompressorStage(
                option.Stages, option.StageRatio, option.MaximumStageTemperatureR, option.Allowed)));

            if (plan.RecommendedStages is null)
            {
                reasons.Add(
                    $"no stage count up to {plan.MaximumStages.ToString(CultureInfo.InvariantCulture)} keeps the " +
                    $"per-stage ratio at or below {plan.MaximumRatioPerStage.ToString("0.##", CultureInfo.InvariantCulture)} " +
                    $"and every stage at or below " +
                    $"{plan.MaximumDischargeTemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} R");

                stages ??= plan.MaximumStages;
            }
            else
            {
                stages ??= plan.RecommendedStages;
            }

            // A staged machine is what the module's staging calculator describes, so its stage-by-stage result is the
            // answer: the summed stage head, the total motor power and the real final discharge temperature. A fixed
            // stage count is judged against the same limits rather than assumed to be acceptable.
            var chosen = CompressorStagingLimits.Judge(machine, stages.Value);

            if (!chosen.Allowed)
            {
                if (chosen.MaximumStageTemperatureR > plan.MaximumDischargeTemperatureR)
                {
                    reasons.Add(
                        $"the stage count of {chosen.Stages.ToString(CultureInfo.InvariantCulture)} leaves a stage at " +
                        $"{chosen.MaximumStageTemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} R, above the " +
                        $"interstage limit of " +
                        $"{plan.MaximumDischargeTemperatureR.ToString("#,0.##", CultureInfo.InvariantCulture)} R " +
                        $"({(plan.MaximumDischargeTemperatureR - 459.67m).ToString("#,0.##", CultureInfo.InvariantCulture)} F)");
                }

                if (chosen.StageRatio > plan.MaximumRatioPerStage)
                {
                    reasons.Add(
                        $"the stage count of {chosen.Stages.ToString(CultureInfo.InvariantCulture)} leaves a per-stage " +
                        $"ratio of {chosen.StageRatio.ToString("0.####", CultureInfo.InvariantCulture)}, above the limit " +
                        $"of {plan.MaximumRatioPerStage.ToString("0.##", CultureInfo.InvariantCulture)}");
                }
            }

            var staged = MultistageCompressor.AnalyzeMultistage(machine, stages.Value);

            head = staged.Stages.Sum(stage => stage.PolytropicHead);
            power = staged.TotalMotorHorsepower;
            kilowatts = staged.TotalPowerKw;
            efficiency = staged.OverallPolytropicEfficiency * conditions.MECHANICAL_EFFICIENCY;
            dischargeTemperature = staged.FinalDischargeTemperature;
        }
        else
        {
            var machine = new RECIPROCATING_COMPRESSOR_PROPERTIES
            {
                RECIPROCATING_COMPRESSOR_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                OPERATING_CONDITIONS = conditions,
                CYLINDER_DIAMETER = inputs.CylinderDiameter!.Value,
                STROKE_LENGTH = inputs.StrokeLength!.Value,
                ROTATIONAL_SPEED = inputs.RotationalSpeed!.Value,
                NUMBER_OF_CYLINDERS = inputs.Cylinders ?? 1,
                VOLUMETRIC_EFFICIENCY = inputs.VolumetricEfficiency ?? 0.85m,
                CLEARANCE_FACTOR = inputs.ClearanceFactor ?? 0.05m,
            };

            var result = ReciprocatingCompressorCalculator.CalculatePower(machine);

            // No interstage temperature limit is applied here: the module states 660 R only for its centrifugal staging
            // plan, and a reciprocating machine's discharge is normally well above 200 F. Inventing a limit for this kind
            // would refuse ordinary duties, so the discharge temperature is reported and not judged.
            stageLimitR = null;
            stageRatioLimit = null;

            head = result.ADIABATIC_HEAD;
            power = result.MOTOR_HORSEPOWER;
            kilowatts = result.POWER_CONSUMPTION_KW;
            efficiency = result.OVERALL_EFFICIENCY;
            dischargeTemperature = result.DISCHARGE_TEMPERATURE;
        }

        // The reasons a machine cannot meet the duty, each from the module's own ratios and limits.
        if (ratio > CompressorConstants.MaximumCompressionRatio)
        {
            reasons.Add(
                $"the overall ratio of {ratio.ToString("0.####", CultureInfo.InvariantCulture)} is above the module's " +
                $"maximum of {CompressorConstants.MaximumCompressionRatio.ToString("0.##", CultureInfo.InvariantCulture)}");
        }

        if (efficiency is { } overall && overall <= 0m)
        {
            reasons.Add("the machine's overall efficiency came out at or below zero, so no power figure is meaningful");
        }

        var feasible = reasons.Count == 0;

        return new OgCompressorResult(
            string.Empty,
            inputs.WellId!.Trim(),
            kind,
            stages,
            ratio,
            head,
            OgCompressorResult.HeadUnitText,
            efficiency,
            feasible ? power : null,
            OgCompressorResult.PowerUnitText,
            feasible ? kilowatts : null,
            dischargeTemperature,
            feasible,
            reasons,
            staging,
            stageLimitR,
            stageRatioLimit,
            null,
            null);
    }

    private static decimal ToRankine(decimal fahrenheit) => fahrenheit + 459.67m;

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgCompressorResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<COMPRESSOR_POWER_RESULT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.COMPRESSOR_POWER_RESULT_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .ThenBy(row => row.COMPRESSOR_POWER_RESULT_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgCompressorResult ToResult(COMPRESSOR_POWER_RESULT row)
    {
        var reasons = string.IsNullOrWhiteSpace(row.REASONS)
            ? []
            : row.REASONS!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        var feasible = string.Equals(row.FEASIBLE_IND, "Y", StringComparison.OrdinalIgnoreCase);

        var staging = row.STAGES is { } stages && row.STAGE_RATIO is { } ratio
            ? new List<OgCompressorStage>
            {
                new(stages, ratio, row.MAX_STAGE_TEMPERATURE_R ?? 0m, feasible),
            }
            : [];

        return new OgCompressorResult(
            row.COMPRESSOR_POWER_RESULT_ID ?? string.Empty,
            null,
            row.KIND ?? CompressorKinds.Centrifugal,
            row.STAGES,
            row.COMPRESSION_RATIO,
            row.POLYTROPIC_HEAD == 0m ? null : row.POLYTROPIC_HEAD,
            row.HEAD_OUOM ?? OgCompressorResult.HeadUnitText,
            row.OVERALL_EFFICIENCY == 0m ? null : row.OVERALL_EFFICIENCY,
            string.Equals(row.POWER_ACHIEVED_IND, "Y", StringComparison.OrdinalIgnoreCase)
                ? row.MOTOR_HORSEPOWER
                : null,
            row.POWER_OUOM ?? OgCompressorResult.PowerUnitText,
            string.Equals(row.POWER_ACHIEVED_IND, "Y", StringComparison.OrdinalIgnoreCase)
                ? row.POWER_CONSUMPTION_KW
                : null,
            row.DISCHARGE_TEMPERATURE == 0m ? null : row.DISCHARGE_TEMPERATURE,
            feasible,
            reasons,
            staging,
            row.STAGE_TEMPERATURE_LIMIT_R,
            row.STAGE_RATIO_LIMIT,
            row.ACTOR,
            row.RECORDED_AT);
    }

    private static OgCompressorWorkspace Workspace(
        IReadOnlyList<OgCompressorResult> history,
        OgCompressorResult? selected) =>
        new(
            history,
            selected,
            CompressorKinds.All,
            CompressorKinds.AnalysisTypes,
            CompressorStagingLimits.MaximumDischargeTemperatureR,
            CompressorStagingLimits.MaximumDischargeTemperatureF,
            CompressorStagingLimits.MaximumRatioPerStage,
            CompressorStagingLimits.MaximumStages,
            false,
            null);
}
