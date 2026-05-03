namespace Beep.OilandGas.FlashCalculations.Constants;

/// <summary>
/// <c>REFERENCE_SET</c> values for <c>R_FLASH_CALCULATION_REFERENCE_CODE</c>.
/// </summary>
public static class FlashReferenceSets
{
    /// <summary>Matches <see cref="Beep.OilandGas.Models.Data.Calculations.FlashCalculationOptions.EquationOfState"/> vocabulary.</summary>
    public const string EosModel = "FLASH_EOS_MODEL";

    /// <summary>High-level flash / PVT calculation paths exposed by the library.</summary>
    public const string CalculationCategory = "FLASH_CALC_CATEGORY";

    /// <summary>Optional UI tags for convergence / solver presets.</summary>
    public const string SolverPreset = "FLASH_SOLVER_PRESET";

    /// <summary>Specified variables for flash (PT, PH, etc.) — screening / API labels.</summary>
    public const string FlashSpecification = "FLASH_SPECIFICATION";

    /// <summary>Equilibrium phase labels for results and reporting.</summary>
    public const string PhaseState = "FLASH_PHASE_STATE";

    /// <summary>Derived PVT property categories (Z, density, enthalpy proxy).</summary>
    public const string PropertyKind = "FLASH_PROPERTY_KIND";
}
