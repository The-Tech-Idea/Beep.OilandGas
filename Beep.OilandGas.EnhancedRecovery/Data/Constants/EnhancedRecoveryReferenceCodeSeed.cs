using System.Collections.Generic;

namespace Beep.OilandGas.EnhancedRecovery.Constants;

/// <summary>
/// Canonical seed rows for <c>R_ENHANCED_RECOVERY_REFERENCE_CODE</c> — method categories
/// and screening labels aligned with <c>EnhancedRecoveryService</c> PDEN filters.
/// </summary>
public static class EnhancedRecoveryReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "WATER_FLOOD", "Water flooding (secondary recovery)");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "GAS_INJECTION", "Gas injection (immiscible / vapor)");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "CO2_MISCIBLE", "CO₂ miscible / dense gas EOR");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "HYDROCARBON_MISCIBLE", "Hydrocarbon miscible gas");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "POLYMER", "Polymer flooding");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "ASP", "Alkaline–surfactant–polymer (ASP)");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "CHEMICAL", "Chemical EOR (general)");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "STEAM", "Steam / thermal drive");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "THERMAL", "Thermal recovery (general)");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.EorMethodCategory, "INJECTION", "Injection (fluid unspecified)");

        yield return new SeedRow(EnhancedRecoveryReferenceSets.ScreeningClass, "SCREENING_LEVEL_1", "Level 1 — screening correlation / analog");
        yield return new SeedRow(EnhancedRecoveryReferenceSets.ScreeningClass, "SCREENING_LEVEL_2", "Level 2 — calibrated to field pilots");
    }
}
