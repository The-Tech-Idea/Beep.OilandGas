using System.Collections.Generic;

namespace Beep.OilandGas.FlashCalculations.Constants;

/// <summary>
/// Canonical seed rows for <c>R_FLASH_CALCULATION_REFERENCE_CODE</c> — EOS labels, calculation categories, default solver presets.
/// Keep aligned with <see cref="FlashConstants"/> numeric defaults and API options strings.
/// </summary>
public static class FlashReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new SeedRow(FlashReferenceSets.EosModel, "PR", "Peng–Robinson equation of state");
        yield return new SeedRow(FlashReferenceSets.EosModel, "SRK", "Soave–Redlich–Kwong equation of state");
        yield return new SeedRow(FlashReferenceSets.EosModel, "SRK_MODIFIED", "Modified SRK (custom alpha)");
        yield return new SeedRow(FlashReferenceSets.EosModel, "IDEAL_K", "Ideal K-value / simplified vapor-liquid split");

        yield return new SeedRow(FlashReferenceSets.CalculationCategory, "ISOTHERMAL_FLASH", "Isothermal flash (PT specified)");
        yield return new SeedRow(FlashReferenceSets.CalculationCategory, "MULTISTAGE_FLASH", "Multi-stage separator train flash");
        yield return new SeedRow(FlashReferenceSets.CalculationCategory, "PHASE_ENVELOPE", "Phase envelope / quality trace support");
        yield return new SeedRow(FlashReferenceSets.CalculationCategory, "RIGOROUS_FLASH", "Rigorous multi-component EOS flash");

        yield return new SeedRow(FlashReferenceSets.SolverPreset, "DEFAULT", $"Default iterations ({FlashConstants.MaximumIterations}) / tol ({FlashConstants.ConvergenceTolerance})");
        yield return new SeedRow(FlashReferenceSets.SolverPreset, "STRICT", "Tighter convergence — smaller tolerance");
        yield return new SeedRow(FlashReferenceSets.SolverPreset, "FAST", "Fewer iterations — screening only");

        // Flash specification (industry-style labels; aligns with isothermal / rigorous paths).
        yield return new SeedRow(FlashReferenceSets.FlashSpecification, "PT_SPECIFIED", "Pressure–temperature specified (isothermal flash)");
        yield return new SeedRow(FlashReferenceSets.FlashSpecification, "PH_SPECIFIED", "Pressure–enthalpy specified (future / extended EOS)");
        yield return new SeedRow(FlashReferenceSets.FlashSpecification, "TP_SPECIFIED", "Temperature–pressure specified (alias for PT path)");

        // Phase state (reporting / phase split).
        yield return new SeedRow(FlashReferenceSets.PhaseState, "OVERALL", "Overall mixture / combined stream");
        yield return new SeedRow(FlashReferenceSets.PhaseState, "VAPOR", "Vapor phase");
        yield return new SeedRow(FlashReferenceSets.PhaseState, "LIQUID", "Hydrocarbon liquid phase");
        yield return new SeedRow(FlashReferenceSets.PhaseState, "AQUEOUS", "Aqueous / water-rich phase");

        // Property kinds (outputs / diagnostics).
        yield return new SeedRow(FlashReferenceSets.PropertyKind, "COMPRESSIBILITY_Z", "Compressibility factor Z");
        yield return new SeedRow(FlashReferenceSets.PropertyKind, "MOLAR_VOLUME", "Molar volume");
        yield return new SeedRow(FlashReferenceSets.PropertyKind, "FUGACITY_COEFF", "Fugacity coefficient (component or phase)");
        yield return new SeedRow(FlashReferenceSets.PropertyKind, "K_VALUE", "Equilibrium K-value");
    }
}
