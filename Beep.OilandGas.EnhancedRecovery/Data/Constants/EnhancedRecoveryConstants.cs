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
}
