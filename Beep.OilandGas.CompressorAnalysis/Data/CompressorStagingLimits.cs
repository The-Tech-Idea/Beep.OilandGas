using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Data;

namespace Beep.OilandGas.CompressorAnalysis.Data;

/// <summary>One candidate stage count: how many stages, the per-stage ratio, the hottest stage and whether it is allowed.</summary>
/// <param name="Stages">The number of stages evaluated.</param>
/// <param name="TotalBrakeHorsepower">Total brake horsepower across the stages.</param>
/// <param name="StageRatio">The per-stage compression ratio.</param>
/// <param name="MaximumStageTemperatureR">The hottest stage discharge temperature, in Rankine.</param>
/// <param name="Allowed">True when both the per-stage ratio and the temperature are inside the limits.</param>
public sealed record CompressorStagingOption(
    int Stages,
    decimal TotalBrakeHorsepower,
    decimal StageRatio,
    decimal MaximumStageTemperatureR,
    bool Allowed);

/// <summary>The staging a compressor needs, with the limits it was judged against (LFT-08 AC-2, AC-3).</summary>
/// <param name="RecommendedStages">The fewest stages that satisfy both limits; null when none up to the maximum does.</param>
/// <param name="Options">Every candidate, from the fewest stages to the most.</param>
/// <param name="MaximumRatioPerStage">The per-stage ratio limit.</param>
/// <param name="MaximumDischargeTemperatureR">The interstage temperature limit.</param>
/// <param name="MaximumStages">How many stages were tried.</param>
public sealed record CompressorStaging(
    int? RecommendedStages,
    IReadOnlyList<CompressorStagingOption> Options,
    decimal MaximumRatioPerStage,
    decimal MaximumDischargeTemperatureR,
    int MaximumStages)
{
    /// <summary>Every option that breaks a limit, so the reasons can name them.</summary>
    public IReadOnlyList<CompressorStagingOption> Rejected =>
        Options.Where(option => !option.Allowed).ToList();

    /// <summary>The hottest stage temperature any option reached - what the temperature limit is compared with.</summary>
    public decimal HottestStageR => Options.Count == 0 ? 0m : Options.Max(option => option.MaximumStageTemperatureR);

    /// <summary>True when the recommended option is the one with the fewest stages that still cools to the limit.</summary>
    public bool TemperatureConstrained =>
        Options.Any(option => option.MaximumStageTemperatureR > MaximumDischargeTemperatureR);
}

/// <summary>
/// The staging limits and the recommendation, stated once (LFT-08).
/// </summary>
/// <remarks>
/// <c>MultistageCompressor.OptimiseStageCount</c> carries these as default parameters (maxStages 6, ratio 4.0, discharge
/// temperature 660 degR = 200 degF) and documents each. They are named here because a screen has to print the limit it
/// refused against, and a caller that repeated the literals could print a different one than the engine applied. The
/// numbers are the engine's own documented defaults, not new thresholds.
/// </remarks>
public static class CompressorStagingLimits
{
    /// <summary>The most stages the engine evaluates.</summary>
    public const int MaximumStages = 6;

    /// <summary>The largest per-stage compression ratio the engine accepts for a centrifugal machine.</summary>
    public const decimal MaximumRatioPerStage = 4.0m;

    /// <summary>The interstage (per-stage discharge) temperature limit, in Rankine - 660 degR is 200 degF.</summary>
    public const decimal MaximumDischargeTemperatureR = 660m;

    /// <summary>The interstage temperature limit in Fahrenheit, for the sentence a person reads.</summary>
    public static decimal MaximumDischargeTemperatureF => MaximumDischargeTemperatureR - 459.67m;

    /// <summary>
    /// Evaluates every stage count the engine supports and recommends the fewest that satisfies both limits.
    /// </summary>
    public static CompressorStaging Plan(CENTRIFUGAL_COMPRESSOR_PROPERTIES compressor)
    {
        ArgumentNullException.ThrowIfNull(compressor);

        var options = MultistageCompressor
            .OptimiseStageCount(compressor, MaximumStages, MaximumRatioPerStage, MaximumDischargeTemperatureR)
            .Select(candidate => new CompressorStagingOption(
                candidate.Stages,
                candidate.TotalBHP,
                candidate.StageRatio,
                candidate.MaxTemp,
                candidate.ConstraintsSatisfied))
            .ToList();

        var recommended = options
            .Where(option => option.Allowed)
            .OrderBy(option => option.Stages)
            .Select(option => (int?)option.Stages)
            .FirstOrDefault();

        return new CompressorStaging(
            recommended,
            options,
            MaximumRatioPerStage,
            MaximumDischargeTemperatureR,
            MaximumStages);
    }

    /// <summary>
    /// The stages for a machine that must use a fixed count: the count itself, with the limits it broke.
    /// </summary>
    /// <remarks>
    /// A caller that fixes the stage count (because the machine exists) still needs to know whether that count is
    /// acceptable, so the limits are re-applied rather than assumed.
    /// </remarks>
    public static CompressorStagingOption Judge(
        CENTRIFUGAL_COMPRESSOR_PROPERTIES compressor,
        int stages)
    {
        ArgumentNullException.ThrowIfNull(compressor);

        if (stages < 1 || stages > MaximumStages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stages),
                stages,
                $"A stage count must be between 1 and {MaximumStages}.");
        }

        var result = MultistageCompressor.AnalyzeMultistage(compressor, stages);
        var hottest = result.Stages.Count == 0
            ? 0m
            : result.Stages.Max(stage => stage.DischargeTemperature);

        return new CompressorStagingOption(
            stages,
            result.TotalBrakeHorsepower,
            result.StageCompressionRatio,
            hottest,
            result.StageCompressionRatio <= MaximumRatioPerStage
            && hottest <= MaximumDischargeTemperatureR);
    }
}
