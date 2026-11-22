using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ChokeAnalysis.Constants;

namespace Beep.OilandGas.ChokeAnalysis.Calculations;

/// <summary>
/// What a correlation code means in this module: which path evaluates it, and whether the module has a model for it at all
/// (LFT-07 AC-2 and the state table's "correlation chosen but out of range").
/// </summary>
/// <param name="Code">The <see cref="ChokeAnalysisReferenceCodes"/> value.</param>
/// <param name="Label">The human name from the seed's long name.</param>
/// <param name="Path">Single-phase gas (rate from pressures) or multiphase empirical (pressure from rate).</param>
/// <param name="Implemented">
/// False when the module has no model and <see cref="ChokeCorrelationRegistry.Resolve"/> falls back to the Gilbert family.
/// </param>
/// <param name="FallbackFor">The code the fallback evaluates, when <paramref name="Implemented"/> is false.</param>
public sealed record ChokeCorrelationInfo(
    string Code,
    string Label,
    ChokeCorrelationPath Path,
    bool Implemented,
    string? FallbackFor);

/// <summary>Which of the module's two evaluation paths a correlation belongs to.</summary>
public enum ChokeCorrelationPath
{
    /// <summary><c>GasChokeCalculator</c>: a rate from the upstream and downstream pressures through the throat.</summary>
    SinglePhaseGas,

    /// <summary><c>MultiphaseChokeCalculator</c>: the upstream pressure a liquid rate needs at a choke size.</summary>
    MultiphaseEmpirical,
}

/// <summary>
/// The correlations the module can evaluate, resolved to a path and to the code it will actually run (LFT-07).
/// </summary>
/// <remarks>
/// The registry exists because two of the seeded codes have no model behind them: <c>MultiphaseChokeCalculator</c> documents
/// that <c>PILEHVARI</c> and <c>SACHDEVA</c> "fall back to Gilbert until dedicated mechanism models are wired", and
/// <c>EvaluatePilehvari</c> returns zero. A screen that labelled a fallback result with the chosen code would misname the
/// correlation, so the resolution lives here, once, and the caller records both codes.
/// </remarks>
public static class ChokeCorrelationRegistry
{
    /// <summary>Every correlation the module offers, in the order the reference seed lists them.</summary>
    public static IReadOnlyList<ChokeCorrelationInfo> All { get; } =
    [
        new(ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase,
            "Single-phase gas (isentropic orifice)", ChokeCorrelationPath.SinglePhaseGas, true, null),
        new(ChokeAnalysisReferenceCodes.CorrelationGilbert,
            "Gilbert (multiphase empirical)", ChokeCorrelationPath.MultiphaseEmpirical, true, null),
        new(ChokeAnalysisReferenceCodes.CorrelationRos,
            "Ros", ChokeCorrelationPath.MultiphaseEmpirical, true, null),
        new(ChokeAnalysisReferenceCodes.CorrelationAchong,
            "Achong", ChokeCorrelationPath.MultiphaseEmpirical, true, null),
        new(ChokeAnalysisReferenceCodes.CorrelationBaxendell,
            "Baxendell", ChokeCorrelationPath.MultiphaseEmpirical, true, null),
        new(ChokeAnalysisReferenceCodes.CorrelationMultiphase,
            "Multiphase empirical (alias for the Gilbert family)", ChokeCorrelationPath.MultiphaseEmpirical, true,
            null),
        new(ChokeAnalysisReferenceCodes.CorrelationPilehvari,
            "Pilehvari (no model in this module)", ChokeCorrelationPath.MultiphaseEmpirical, false,
            ChokeAnalysisReferenceCodes.CorrelationGilbert),
        new(ChokeAnalysisReferenceCodes.CorrelationSachdeva,
            "Sachdeva (no model in this module)", ChokeCorrelationPath.MultiphaseEmpirical, false,
            ChokeAnalysisReferenceCodes.CorrelationGilbert),
    ];

    /// <summary>Resolves a code, or null when it is not one of the module's correlations.</summary>
    public static ChokeCorrelationInfo? Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmed = code.Trim();

        return All.FirstOrDefault(info =>
            string.Equals(info.Code, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The code that will actually be evaluated for a chosen one - the chosen code, or its fallback when the module has no
    /// model for it.
    /// </summary>
    public static string Effective(string? chosen)
    {
        var info = Resolve(chosen);

        return info is null
            ? ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase
            : info.FallbackFor ?? info.Code;
    }

    /// <summary>
    /// The chokes this module states a diameter range for: the calculation bounds, not a catalogue of bean sizes.
    /// </summary>
    /// <remarks>
    /// The repository has no bean-size catalogue, so the screen accepts a diameter and shows its equivalent in 64ths rather
    /// than offering a list it cannot source.
    /// </remarks>
    public static (decimal Minimum, decimal Maximum) DiameterRange =>
        (ChokeConstants.MinimumChokeDiameter, ChokeConstants.MaximumChokeDiameter);

    /// <summary>A diameter in inches as the 64ths a field choke is specified in.</summary>
    public static decimal ToThroatSize64ths(decimal diameterInches) => diameterInches * 64m;
}
