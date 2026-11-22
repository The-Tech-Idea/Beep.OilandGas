using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PipelineAnalysis.Calculations;

namespace Beep.OilandGas.PipelineAnalysis.Constants;

/// <summary>Which service an API RP 14E C-factor is for, and the standard that states it.</summary>
/// <param name="Code">The service code the screen stores, e.g. <c>SOLID</c>.</param>
/// <param name="Service">The service as the standard names it.</param>
/// <param name="CFactor">The C-factor, from <see cref="ErosionPrediction"/>'s own constants.</param>
public sealed record PipelineErosionService(string Code, string Service, decimal CFactor);

/// <summary>
/// The standards behind each pipeline threshold, stated once (WTA-03 AC-2, SG-08-6).
/// </summary>
/// <remarks>
/// The engine already documents its references in <see cref="ErosionPrediction"/>'s header - API RP 14E (2007) for the
/// erosional velocity, Salama-Venkatesh (1983, OTC-4485) for sand, DNV GL RP-O501 (2015) for the erosion rate, NORSOK
/// P-002 for the corrosion allowance - but a result has to print the source beside the number, and a caller that wrote its
/// own sentence could name a different edition. The C-factors come from <see cref="ErosionPrediction"/>'s public constants
/// rather than being repeated, so the catalog cannot drift from the calculator.
/// </remarks>
public static class PipelineStandards
{
    /// <summary>The erosional velocity threshold's citation, printed beside every limit.</summary>
    public const string ErosionalVelocitySource =
        "API RP 14E (2007) 2.5 - erosional velocity Ve = C / sqrt(mixture density)";

    /// <summary>The sand erosion model's citation, printed beside every sand rate.</summary>
    public const string SandErosionSource =
        "Salama-Venkatesh (1983) OTC-4485 - sand erosion rate";

    /// <summary>The DNV erosion model's citation.</summary>
    public const string DnvErosionSource =
        "DNV GL RP-O501 (2015) - managing sand production and erosion";

    /// <summary>The corrosion allowance's citation.</summary>
    public const string CorrosionSource =
        "NORSOK P-002 (2020) - corrosion allowance guidance";

    /// <summary>
    /// The C-factor services the engine states, in its own order: solid or sand-laden, inhibited or intermittent sand, and
    /// clean continuous service.
    /// </summary>
    public static IReadOnlyList<PipelineErosionService> ErosionServices { get; } =
    [
        new("SOLID", "Solid or sand-laden service", (decimal)ErosionPrediction.C_SolidService),
        new("INHIBITED", "Inhibited or intermittent sand service", (decimal)ErosionPrediction.C_InhibitedService),
        new("CLEAN", "Clean, non-corrosive, sand-free continuous service",
            (decimal)ErosionPrediction.C_CleanService),
    ];

    /// <summary>The default service - the engine's own default, <c>C_SolidService</c>.</summary>
    public const string DefaultServiceCode = "SOLID";

    /// <summary>Resolves a service code, or null when it is not one of the standards' services.</summary>
    public static PipelineErosionService? Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmed = code.Trim();

        return ErosionServices.FirstOrDefault(service =>
            string.Equals(service.Code, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The source sentence for a service, so a limit is never stored without one.</summary>
    public static string ThresholdSourceFor(PipelineErosionService service) =>
        $"{ErosionalVelocitySource}; C = {service.CFactor.ToString("0", System.Globalization.CultureInfo.InvariantCulture)} " +
        $"for {service.Service.ToLowerInvariant()}";

    /// <summary>
    /// The flow regime the module's own Reynolds thresholds assign (AC-3).
    /// </summary>
    /// <remarks>
    /// The bands are <see cref="PipelineConstants.LaminarFlowThreshold"/> (2,000) and
    /// <see cref="PipelineConstants.TurbulentFlowThreshold"/> (4,000), which the module states and its liquid friction
    /// factor already uses. Naming them here means the screen cannot print a different boundary than the one applied.
    /// </remarks>
    public static string RegimeOf(decimal reynolds) =>
        reynolds <= 0m
            ? RegimeUndetermined
            : reynolds < PipelineConstants.LaminarFlowThreshold
                ? RegimeLaminar
                : reynolds < PipelineConstants.TurbulentFlowThreshold
                    ? RegimeTransitional
                    : RegimeTurbulent;

    public const string RegimeLaminar = "LAMINAR";
    public const string RegimeTransitional = "TRANSITIONAL";
    public const string RegimeTurbulent = "TURBULENT";

    /// <summary>A Reynolds number of zero or less decides nothing, and says so rather than guessing.</summary>
    public const string RegimeUndetermined = "UNDETERMINED";

    /// <summary>The Reynolds thresholds, printed with the regime.</summary>
    public static string ThresholdsText =>
        $"laminar below {PipelineConstants.LaminarFlowThreshold.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)}, " +
        $"turbulent from {PipelineConstants.TurbulentFlowThreshold.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)}";
}
