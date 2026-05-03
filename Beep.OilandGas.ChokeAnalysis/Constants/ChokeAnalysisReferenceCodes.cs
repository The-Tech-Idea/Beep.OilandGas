namespace Beep.OilandGas.ChokeAnalysis.Constants;

/// <summary>
/// Canonical REFERENCE_CODE values aligned with runtime enums / API strings where applicable.
/// </summary>
public static class ChokeAnalysisReferenceCodes
{
    public const string AnalysisCompleted = "COMPLETED";
    public const string AnalysisFailed = "FAILED";
    public const string AnalysisRunning = "RUNNING";

    public const string TypeBean = "BEAN";
    public const string TypeAdjustable = "ADJUSTABLE";
    public const string TypePositive = "POSITIVE";

    public const string CorrelationGasSinglePhase = "GAS_SINGLE_PHASE";
    /// <summary>Alias for empirical multiphase orchestration (same fallback family as <see cref="CorrelationGilbert"/>).</summary>
    public const string CorrelationMultiphase = "MULTIPHASE";
    public const string CorrelationGilbert = "GILBERT";
    public const string CorrelationRos = "ROS";
    public const string CorrelationAchong = "ACHONG";
    public const string CorrelationPilehvari = "PILEHVARI";
    public const string CorrelationSachdeva = "SACHDEVA";
    /// <summary>Baxendell correlation (multiphase empirical); not all deployments seed this in LOV.</summary>
    public const string CorrelationBaxendell = "BAXENDELL";

    public const string RegimeSonic = "SONIC";
    public const string RegimeSubsonic = "SUBSONIC";

    /// <summary>
    /// When true, packaged choke orchestration (e.g. <c>ICalculationService.PerformChokeAnalysisAsync</c>) uses the Gilbert-style multiphase pressure heuristic instead of
    /// <see cref="Beep.OilandGas.Models.Core.Interfaces.IChokeAnalysisService"/> single-phase gas.
    /// Unknown correlation codes do not enable this path — only values seeded under correlation reference sets (Gilbert, Ros, …) plus <see cref="CorrelationMultiphase"/>.
    /// </summary>
    public static bool UseMultiphaseOrchestration(string? correlationMethod)
    {
        if (string.IsNullOrWhiteSpace(correlationMethod))
            return false;

        var c = correlationMethod.Trim();
        if (string.Equals(c, CorrelationGasSinglePhase, StringComparison.OrdinalIgnoreCase))
            return false;

        return string.Equals(c, CorrelationGilbert, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationMultiphase, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationRos, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationAchong, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationPilehvari, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationSachdeva, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c, CorrelationBaxendell, StringComparison.OrdinalIgnoreCase);
    }
}
