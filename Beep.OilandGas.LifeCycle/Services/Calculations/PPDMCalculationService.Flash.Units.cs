using System;

namespace Beep.OilandGas.LifeCycle.Services.Calculations;

/// <summary>
/// PPDM temperature fields for US oilfield data are often stored in °F; <see cref="Beep.OilandGas.FlashCalculations.Calculations.FlashCalculator"/> expects Rankine with critical properties in °R.
/// </summary>
public partial class PPDMCalculationService
{
    // OILGAS-CATCH-01: the screening defaults (1000 psia, ~150 °F) that stood in for a pressure and temperature PPDM did not
    // supply are gone — a flash with neither given nor recorded is refused, not run on made-up conditions.

    private static decimal FahrenheitToRankine(decimal tempF) => tempF + 459.67m;

    /// <summary>
    /// Converts a PPDM-reported temperature to Rankine using <paramref name="ouom"/> when present; otherwise assumes °F (common for WELL_TEST / WELL_PRESSURE_BH in US datasets).
    /// </summary>
    private static decimal ToRankineFromPpdm(decimal value, string? ouom)
    {
        if (string.IsNullOrWhiteSpace(ouom))
            return FahrenheitToRankine(value);

        var u = ouom.Trim().ToUpperInvariant();
        if (u.Contains("FAH", StringComparison.Ordinal) || u is "F" or "DEGF")
            return FahrenheitToRankine(value);
        if (u.Contains("RANK", StringComparison.Ordinal) || u is "R")
            return value;
        if (u.Contains("KEL", StringComparison.Ordinal) || u is "K")
            return value * 1.8m;
        if (u.Contains("CEL", StringComparison.Ordinal) || u is "C")
            return value * 9m / 5m + 491.67m;

        return FahrenheitToRankine(value);
    }
}
