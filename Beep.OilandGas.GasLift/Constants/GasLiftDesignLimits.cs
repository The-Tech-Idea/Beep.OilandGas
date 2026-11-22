using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.GasLift.Constants;

/// <summary>One violated gas-lift design limit: the code, the catalog message and whether it blocks a save.</summary>
/// <param name="Code">The <see cref="GasLiftReferenceSets.DesignLimit"/> code that was violated.</param>
/// <param name="Message">The message from <see cref="GasLiftDesignLimitMessages"/>, or the code when the catalog has none.</param>
/// <param name="Blocking">True when the violation makes the design unsaveable.</param>
public sealed record GasLiftLimitViolation(string Code, string Message, bool Blocking);

/// <summary>
/// Evaluates the design guardrails in <see cref="GasLiftConstants"/> and reports each violation with the catalog's own
/// message (LFT-02 AC-1, AC-2).
/// </summary>
/// <remarks>
/// The limits live here, with the constants they enforce, so a screen shows the owning project's message rather than a
/// sentence of its own. Rate and valve-count violations block a save, because a design outside them is not a design the
/// module supports; depth and spacing violations are screening warnings, because a short spacing or a shallow valve is
/// legal and only needs a look.
/// </remarks>
public static class GasLiftDesignLimits
{
    /// <summary>
    /// Every limit this evaluator checks, in order, whether or not it was violated - what a saved design records as
    /// "all limits evaluated" (LFT-02 AC-6).
    /// </summary>
    public static IReadOnlyList<string> EvaluatedCodes { get; } =
    [
        "MIN_INJECTION_MSCFD",
        "MAX_INJECTION_MSCFD",
        "MIN_VALVES",
        "MAX_VALVES",
        "MIN_VALVE_DEPTH_FT",
        "MIN_VALVE_SPACING_FT",
    ];

    /// <summary>Evaluates every limit for one design.</summary>
    /// <param name="gasInjectionRate">The injection rate the design uses, in Mscf/day.</param>
    /// <param name="numberOfValves">How many valves the design places.</param>
    /// <param name="valveDepths">The valve depths in feet, in order.</param>
    /// <returns>One entry per violated limit, in a stable order.</returns>
    public static IReadOnlyList<GasLiftLimitViolation> Evaluate(
        decimal gasInjectionRate,
        int numberOfValves,
        IReadOnlyList<decimal>? valveDepths)
    {
        var violations = new List<GasLiftLimitViolation>();

        void Add(string code, bool violated, bool blocking)
        {
            if (!violated)
            {
                return;
            }

            // The catalog is the only source of the sentence; a code it does not know is shown as the code itself.
            var message = GasLiftDesignLimitMessages.GetMessage(code) ?? code;

            violations.Add(new GasLiftLimitViolation(code, message, blocking));
        }

        Add("MIN_INJECTION_MSCFD", gasInjectionRate < GasLiftConstants.MinimumGasInjectionRate, blocking: true);
        Add("MAX_INJECTION_MSCFD", gasInjectionRate > GasLiftConstants.MaximumGasInjectionRate, blocking: true);
        Add("MIN_VALVES", numberOfValves < GasLiftConstants.MinimumNumberOfValves, blocking: true);
        Add("MAX_VALVES", numberOfValves > GasLiftConstants.MaximumNumberOfValves, blocking: true);

        var depths = valveDepths ?? [];

        Add("MIN_VALVE_DEPTH_FT", depths.Any(depth => depth < GasLiftConstants.MinimumValveDepth),
            blocking: false);

        var spacing = depths
            .OrderBy(depth => depth)
            .Zip(depths.OrderBy(depth => depth).Skip(1), (upper, lower) => lower - upper)
            .ToList();

        Add("MIN_VALVE_SPACING_FT",
            spacing.Any(gap => gap < GasLiftConstants.MinimumValveSpacing),
            blocking: false);

        return violations;
    }
}
