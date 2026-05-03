using System;

namespace Beep.OilandGas.FlashCalculations.Constants;

/// <summary>
/// Maps wire / UI <see cref="Beep.OilandGas.Models.Data.Calculations.FlashCalculationOptions.EquationOfState"/> strings
/// to canonical <c>FLASH_EOS_MODEL</c> <c>REFERENCE_CODE</c> values seeded in <c>R_FLASH_CALCULATION_REFERENCE_CODE</c>
/// (<see cref="FlashReferenceCodeSeed"/>).
/// </summary>
public static class FlashEquationOfStateMapping
{
    /// <summary>
    /// Default when unspecified — screening Wilson/Rachford–Rice path without cubic fugacity iteration.
    /// </summary>
    public const string DefaultReferenceCode = "IDEAL_K";

    /// <summary>
    /// Normalizes free text to PR, SRK, SRK_MODIFIED, or IDEAL_K. Unknown tokens fall back to <see cref="DefaultReferenceCode"/>.
    /// </summary>
    public static string ToReferenceCode(string? equationOfState)
    {
        if (string.IsNullOrWhiteSpace(equationOfState))
            return DefaultReferenceCode;

        var s = equationOfState.Trim();
        if (s.Length == 0)
            return DefaultReferenceCode;

        // Exact LOV codes (case-insensitive)
        if (string.Equals(s, "PR", StringComparison.OrdinalIgnoreCase))
            return "PR";
        if (string.Equals(s, "SRK", StringComparison.OrdinalIgnoreCase))
            return "SRK";
        if (string.Equals(s, "SRK_MODIFIED", StringComparison.OrdinalIgnoreCase))
            return "SRK_MODIFIED";
        if (string.Equals(s, "IDEAL_K", StringComparison.OrdinalIgnoreCase))
            return "IDEAL_K";

        var u = s.ToUpperInvariant();

        if (u.Contains("PENG") && u.Contains("ROBINSON"))
            return "PR";
        if (u.Contains("SOAVE") || (u.Contains("REDLICH") && u.Contains("KWONG")))
            return "SRK";
        if (u.Contains("SRK") && (u.Contains("MOD") || u.Contains("ALPHA")))
            return "SRK_MODIFIED";
        if (u.Contains("WILSON") || u.Contains("IDEAL") || u.Contains("K-VALUE") || u.Contains("KVALUE"))
            return "IDEAL_K";

        return DefaultReferenceCode;
    }
}
