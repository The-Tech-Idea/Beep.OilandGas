namespace Beep.OilandGas.EnhancedRecovery.Constants;

/// <summary>
/// Module-owned literals for PDEN rows and defaults — not shared cross-domain enums in Models.
/// </summary>
public static class EnhancedRecoveryConstants
{
    /// <summary><see cref="Beep.OilandGas.PPDM39.Models.PDEN"/> source tag for app-created schemes.</summary>
    public const string PdenSourceEnhancedRecovery = "ENHANCED_RECOVERY";

    /// <summary>Default active indicator for new rows.</summary>
    public const string ActiveYes = "Y";

    // ─────────────────────────────────────────────────────────────────
    // Screening-Level Recovery Factor Defaults (% OOIP)
    // Used by GetScreeningRecoveryFactorPercent when volumetric history is not integrated.
    // Not decline-curve or material-balance recovery factors.
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Default recovery factor when EOR type is unknown or unspecified (% OOIP).</summary>
    public const decimal RecoveryFactorDefault = 20m;

    /// <summary>Recovery factor for water flooding methods (% OOIP).</summary>
    public const decimal RecoveryFactorWaterFlood = 25m;

    /// <summary>Recovery factor for CO₂ miscible methods (% OOIP).</summary>
    public const decimal RecoveryFactorCO2Miscible = 20m;

    /// <summary>Recovery factor for gas injection methods (% OOIP).</summary>
    public const decimal RecoveryFactorGasInjection = 18m;

    /// <summary>Recovery factor for chemical EOR methods (% OOIP).</summary>
    public const decimal RecoveryFactorChemical = 22m;

    /// <summary>Recovery factor for thermal/steam methods (% OOIP).</summary>
    public const decimal RecoveryFactorThermal = 30m;

    /// <summary>Recovery factor for generic injection (% OOIP).</summary>
    public const decimal RecoveryFactorInjection = 15m;

    // ─────────────────────────────────────────────────────────────────
    // PDEN Subtype Filters
    // Used for querying PDEN records by operation type
    // ─────────────────────────────────────────────────────────────────

    /// <summary>PDEN subtype for water flood operations.</summary>
    public const string PdenSubtypeWaterFlood = "WATER_FLOOD";

    /// <summary>PDEN subtype for gas injection operations.</summary>
    public const string PdenSubtypeGasInjection = "GAS_INJECTION";

    // ─────────────────────────────────────────────────────────────────
    // Product Types
    // Used in PDEN_FLOW_MEASUREMENT product type classification
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Product type for water injection/production measurements.</summary>
    public const string ProductTypeWater = "WATER";

    // ─────────────────────────────────────────────────────────────────
    // Screening windows per EOR method (RES-04)
    //
    // Basis: Taber, Martin & Seright, "EOR Screening Criteria Revisited" (SPE Reservoir Engineering, August 1997),
    // Table 3 - the recommended ranges of current projects. Only the properties that table states are used; where it
    // states none (water flooding, which is secondary recovery) the window is empty and says so. The CO2 depth rule is
    // that paper's own depth-versus-gravity table.
    //
    // These are screening thresholds, not reserve estimates: they decide whether a method is worth studying, not what
    // it will recover. The recovery factors above are the module's own screening defaults and stay separate.
    // ─────────────────────────────────────────────────────────────────

    /// <summary>One method's screening window; a null bound means that property is not part of the screen.</summary>
    public readonly record struct ScreeningWindow(
        decimal? MinimumApiGravity,
        decimal? MaximumApiGravity,
        decimal? MaximumViscosityCp,
        decimal? MinimumDepthFt,
        string Basis);

    /// <summary>
    /// The window each method is screened against, keyed by the <see cref="EorMethodConstants"/> codes.
    /// </summary>
    public static IReadOnlyDictionary<string, ScreeningWindow> ScreeningWindows { get; } =
        new Dictionary<string, ScreeningWindow>(StringComparer.OrdinalIgnoreCase)
        {
            [EorMethodConstants.WaterFlood] = new(
                null, null, null, null,
                "water flooding is secondary recovery: the screening basis sets no API or viscosity window"),

            [EorMethodConstants.GasInjection] = new(
                12m, null, 600m, null,
                "immiscible gas: API gravity at least 12 and oil viscosity at most 600 cp"),

            [EorMethodConstants.CO2Miscible] = new(
                22m, null, 10m, 2500m,
                "CO2 miscible: API gravity at least 22, oil viscosity at most 10 cp, and a depth that reaches the " +
                "miscibility pressure for that oil"),

            [EorMethodConstants.HydrocarbonMiscible] = new(
                23m, null, 3m, null,
                "hydrocarbon miscible: API gravity at least 23 and oil viscosity at most 3 cp"),

            [EorMethodConstants.Polymer] = new(
                15m, 40m, 150m, null,
                "polymer flooding: API gravity 15 to 40 and oil viscosity at most 150 cp"),

            [EorMethodConstants.ASP] = new(
                20m, null, 35m, null,
                "ASP: API gravity at least 20 and oil viscosity at most 35 cp"),

            [EorMethodConstants.Chemical] = new(
                20m, null, 35m, null,
                "chemical flooding: API gravity at least 20 and oil viscosity at most 35 cp"),

            [EorMethodConstants.Steam] = new(
                8m, 25m, 200000m, null,
                "steam flooding: API gravity 8 to 25 (heavy oil) and oil viscosity at most 200000 cp"),

            [EorMethodConstants.Thermal] = new(
                10m, 25m, 5000m, null,
                "thermal recovery: API gravity 10 to 25 and oil viscosity at most 5000 cp"),

            [EorMethodConstants.Injection] = new(
                12m, null, 600m, null,
                "injection with the fluid unspecified: the immiscible gas window"),
        };

    /// <summary>
    /// The depth CO2 miscible flooding needs for a given oil gravity, from the same paper's depth-versus-gravity
    /// table: 2,500 ft above 40 API, 2,800 ft from 32 to 39.9, 3,300 ft from 28 to 31.9, 4,000 ft from 22 to 27.9.
    /// </summary>
    public static decimal? Co2MinimumDepthFor(decimal apiGravity) => apiGravity switch
    {
        > 40m => 2500m,
        >= 32m => 2800m,
        >= 28m => 3300m,
        >= 22m => 4000m,
        _ => null,
    };
}
