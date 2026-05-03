namespace Beep.OilandGas.ChokeAnalysis.Constants;

/// <summary>
/// Calculator branch for single-phase gas choke (sonic vs subsonic).
/// Persisted <c>CHOKE_FLOW_RESULT.FLOW_REGIME</c> uses <see cref="ChokeAnalysisReferenceCodes"/> string codes.
/// </summary>
public enum ChokeFlowRegime
{
    Subsonic,
    Sonic
}
