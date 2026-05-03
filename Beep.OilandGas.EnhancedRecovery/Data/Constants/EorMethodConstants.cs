namespace Beep.OilandGas.EnhancedRecovery.Constants;

/// <summary>
/// EOR method type identifiers aligned with <c>R_ENHANCED_RECOVERY_REFERENCE_CODE</c> seed data
/// and <c>PDEN.ENHANCED_RECOVERY_TYPE</c> / <c>PDEN.PDEN_SUBTYPE</c> values.
/// </summary>
/// <remarks>
/// Reference: <see cref="EnhancedRecoveryReferenceCodeSeed"/> seeds these codes into
/// <c>R_ENHANCED_RECOVERY_REFERENCE_CODE</c> under <c>EOR_METHOD_CATEGORY</c>.
/// </remarks>
public static class EorMethodConstants
{
    /// <summary>Water flooding (secondary recovery). Aligns with seed code <c>WATER_FLOOD</c>.</summary>
    public const string WaterFlood = "WATER_FLOOD";

    /// <summary>Gas injection (immiscible / vapor). Aligns with seed code <c>GAS_INJECTION</c>.</summary>
    public const string GasInjection = "GAS_INJECTION";

    /// <summary>CO₂ miscible / dense gas EOR. Aligns with seed code <c>CO2_MISCIBLE</c>.</summary>
    public const string CO2Miscible = "CO2_MISCIBLE";

    /// <summary>Hydrocarbon miscible gas. Aligns with seed code <c>HYDROCARBON_MISCIBLE</c>.</summary>
    public const string HydrocarbonMiscible = "HYDROCARBON_MISCIBLE";

    /// <summary>Polymer flooding. Aligns with seed code <c>POLYMER</c>.</summary>
    public const string Polymer = "POLYMER";

    /// <summary>Alkaline–surfactant–polymer (ASP). Aligns with seed code <c>ASP</c>.</summary>
    public const string ASP = "ASP";

    /// <summary>Chemical EOR (general). Aligns with seed code <c>CHEMICAL</c>.</summary>
    public const string Chemical = "CHEMICAL";

    /// <summary>Steam / thermal drive. Aligns with seed code <c>STEAM</c>.</summary>
    public const string Steam = "STEAM";

    /// <summary>Thermal recovery (general). Aligns with seed code <c>THERMAL</c>.</summary>
    public const string Thermal = "THERMAL";

    /// <summary>Injection (fluid unspecified). Aligns with seed code <c>INJECTION</c>.</summary>
    public const string Injection = "INJECTION";

    /// <summary>
    /// Determines if the given EOR type string matches water flooding methods.
    /// Checks for "WATER" or "FLOOD" substrings (case-insensitive).
    /// </summary>
    public static bool IsWaterFlood(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        var t = eorType.Trim();
        return t.Contains("WATER", StringComparison.OrdinalIgnoreCase)
            || t.Contains("FLOOD", StringComparison.OrdinalIgnoreCase)
            || string.Equals(t, WaterFlood, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the given EOR type string matches CO₂ miscible methods.
    /// Checks for "CO2" substring (case-insensitive).
    /// </summary>
    public static bool IsCO2Miscible(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        return eorType.Trim().Contains("CO2", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eorType.Trim(), CO2Miscible, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the given EOR type string matches gas injection methods.
    /// Checks for miscible, gas injection, or gas+inject combinations.
    /// </summary>
    public static bool IsGasInjection(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        var t = eorType.Trim();
        return t.Contains("MISCIBLE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(t, GasInjection, StringComparison.OrdinalIgnoreCase)
            || (t.Contains("GAS", StringComparison.OrdinalIgnoreCase) && t.Contains("INJECT", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines if the given EOR type string matches chemical EOR methods.
    /// Checks for polymer, ASP, or chemical substrings.
    /// </summary>
    public static bool IsChemicalEor(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        var t = eorType.Trim();
        return t.Contains("POLYMER", StringComparison.OrdinalIgnoreCase)
            || t.Contains("ASP", StringComparison.OrdinalIgnoreCase)
            || t.Contains("CHEMICAL", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the given EOR type string matches thermal/steam methods.
    /// Checks for "STEAM" or "THERMAL" substrings.
    /// </summary>
    public static bool IsThermal(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        var t = eorType.Trim();
        return t.Contains("STEAM", StringComparison.OrdinalIgnoreCase)
            || t.Contains("THERMAL", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the given EOR type string matches generic injection.
    /// </summary>
    public static bool IsInjection(string? eorType)
    {
        if (string.IsNullOrWhiteSpace(eorType)) return false;
        return string.Equals(eorType.Trim(), Injection, StringComparison.OrdinalIgnoreCase);
    }
}
