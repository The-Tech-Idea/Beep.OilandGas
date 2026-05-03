namespace Beep.OilandGas.NodalAnalysis.Constants;

/// <summary>
/// Canonical REFERENCE_CODE values for nodal analysis statuses, optimization, lift, and diagnostics.
/// </summary>
public static class NodalAnalysisReferenceCodes
{
    public const string AnalysisCompleted = "Completed";
    public const string AnalysisFailed = "Failed";
    public const string AnalysisRunning = "Running";

    public const string OptimizationMaximizeProduction = "MaximizeProduction";
    public const string OptimizationMinimizePressure = "MinimizePressure";
    public const string OptimizationEfficiency = "OptimizeEfficiency";

    public const string DiagnosisNormal = "Normal";
    public const string DiagnosisAttention = "NeedsAttention";
}
