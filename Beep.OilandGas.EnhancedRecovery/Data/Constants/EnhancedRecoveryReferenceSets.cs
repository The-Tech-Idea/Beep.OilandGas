namespace Beep.OilandGas.EnhancedRecovery.Constants;

/// <summary>
/// <c>REFERENCE_SET</c> values stored in <c>R_ENHANCED_RECOVERY_REFERENCE_CODE</c>.
/// </summary>
public static class EnhancedRecoveryReferenceSets
{
    /// <summary>Aligns with <c>PDEN.PDEN_SUBTYPE</c> / <c>ENHANCED_RECOVERY_TYPE</c> vocabulary used in services.</summary>
    public const string EorMethodCategory = "EOR_METHOD_CATEGORY";

    /// <summary>Optional tags for analytics / UI (e.g. screening tier).</summary>
    public const string ScreeningClass = "EOR_SCREENING_CLASS";
}
