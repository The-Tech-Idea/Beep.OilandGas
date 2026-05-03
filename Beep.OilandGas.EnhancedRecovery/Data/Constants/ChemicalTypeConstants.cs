namespace Beep.OilandGas.EnhancedRecovery.Constants;

/// <summary>
/// Chemical EOR type identifiers used in chemical flooding screening and economics.
/// These types are used in interfacial tension reduction, recovery increment, and cost calculations.
/// </summary>
public static class ChemicalTypeConstants
{
    /// <summary>Surfactant chemical type — provides highest IFT reduction (~1000x).</summary>
    public const string Surfactant = "SURFACTANT";

    /// <summary>Polymer chemical type — mobility control, slight IFT reduction.</summary>
    public const string Polymer = "POLYMER";

    /// <summary>Alkali chemical type — generates in-situ surfactants, moderate IFT reduction.</summary>
    public const string Alkali = "ALKALI";

    // ─────────────────────────────────────────────────────────────────
    // Interfacial Tension (IFT) Reduction Factors
    // Multiplier on original IFT — lower = more reduction
    // ─────────────────────────────────────────────────────────────────

    /// <summary>IFT reduction factor for surfactant (~1000x reduction).</summary>
    public const double IftReductionSurfactant = 0.001;

    /// <summary>IFT reduction factor for polymer (slight reduction).</summary>
    public const double IftReductionPolymer = 0.5;

    /// <summary>IFT reduction factor for alkali (~100x reduction).</summary>
    public const double IftReductionAlkali = 0.01;

    /// <summary>Default IFT reduction factor for unknown chemical types.</summary>
    public const double IftReductionDefault = 0.1;

    // ─────────────────────────────────────────────────────────────────
    // Recovery Increment Factors (% OOIP)
    // Base recovery increment before IFT adjustment
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Base recovery increment for surfactant (20% OOIP).</summary>
    public const double RecoveryIncrementSurfactant = 0.20;

    /// <summary>Base recovery increment for polymer (10% OOIP).</summary>
    public const double RecoveryIncrementPolymer = 0.10;

    /// <summary>Base recovery increment for alkali (15% OOIP).</summary>
    public const double RecoveryIncrementAlkali = 0.15;

    /// <summary>Default recovery increment for unknown chemical types (8% OOIP).</summary>
    public const double RecoveryIncrementDefault = 0.08;

    // ─────────────────────────────────────────────────────────────────
    // Chemical Cost Estimates ($/bbl recovered)
    // Base price per barrel of oil recovered
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Base chemical cost for surfactant ($/bbl).</summary>
    public const double CostSurfactant = 15.0;

    /// <summary>Base chemical cost for polymer ($/bbl).</summary>
    public const double CostPolymer = 8.0;

    /// <summary>Base chemical cost for alkali ($/bbl).</summary>
    public const double CostAlkali = 5.0;

    /// <summary>Default chemical cost for unknown types ($/bbl).</summary>
    public const double CostDefault = 10.0;

    /// <summary>
    /// Gets the IFT reduction factor for a given chemical type.
    /// </summary>
    /// <param name="chemicalType">Chemical type identifier.</param>
    /// <returns>IFT reduction multiplier (lower = more reduction).</returns>
    public static double GetIftReduction(string chemicalType)
    {
        return chemicalType.ToUpperInvariant() switch
        {
            Surfactant => IftReductionSurfactant,
            Polymer => IftReductionPolymer,
            Alkali => IftReductionAlkali,
            _ => IftReductionDefault,
        };
    }

    /// <summary>
    /// Gets the base recovery increment for a given chemical type.
    /// </summary>
    /// <param name="chemicalType">Chemical type identifier.</param>
    /// <returns>Base recovery increment as fraction of OOIP.</returns>
    public static double GetRecoveryIncrement(string chemicalType)
    {
        return chemicalType.ToUpperInvariant() switch
        {
            Surfactant => RecoveryIncrementSurfactant,
            Polymer => RecoveryIncrementPolymer,
            Alkali => RecoveryIncrementAlkali,
            _ => RecoveryIncrementDefault,
        };
    }

    /// <summary>
    /// Gets the base chemical cost for a given chemical type.
    /// </summary>
    /// <param name="chemicalType">Chemical type identifier.</param>
    /// <returns>Cost in $/bbl recovered.</returns>
    public static double GetCost(string chemicalType)
    {
        return chemicalType.ToUpperInvariant() switch
        {
            Surfactant => CostSurfactant,
            Polymer => CostPolymer,
            Alkali => CostAlkali,
            _ => CostDefault,
        };
    }
}
