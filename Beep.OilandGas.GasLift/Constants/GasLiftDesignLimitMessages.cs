using System;

namespace Beep.OilandGas.GasLift.Constants;

/// <summary>
/// Maps <see cref="GasLiftReferenceSets.DesignLimit"/> <c>REFERENCE_CODE</c> values (seeded in <c>R_GAS_LIFT_REFERENCE_CODE</c>)
/// to user-facing validation text that embeds numeric limits from <see cref="GasLiftConstants"/>.
/// </summary>
public static class GasLiftDesignLimitMessages
{
    /// <summary>Returns a validation or help message for <paramref name="referenceCode"/>; <c>null</c> if unknown.</summary>
    public static string? GetMessage(string? referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            return null;

        return referenceCode.Trim().ToUpperInvariant() switch
        {
            "MIN_INJECTION_MSCFD" =>
                $"Minimum gas injection rate is {GasLiftConstants.MinimumGasInjectionRate} Mscf/day.",
            "MAX_INJECTION_MSCFD" =>
                $"Maximum gas injection rate is {GasLiftConstants.MaximumGasInjectionRate} Mscf/day.",
            "MIN_VALVES" =>
                $"Minimum number of gas lift valves is {GasLiftConstants.MinimumNumberOfValves}.",
            "MAX_VALVES" =>
                $"Maximum number of gas lift valves is {GasLiftConstants.MaximumNumberOfValves}.",
            "MIN_VALVE_DEPTH_FT" =>
                $"Minimum valve depth (screening) is {GasLiftConstants.MinimumValveDepth} ft.",
            "MIN_VALVE_SPACING_FT" =>
                $"Minimum valve spacing (screening) is {GasLiftConstants.MinimumValveSpacing} ft.",
            _ => null
        };
    }
}
