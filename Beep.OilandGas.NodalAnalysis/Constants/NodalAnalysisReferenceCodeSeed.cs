using System.Collections.Generic;

namespace Beep.OilandGas.NodalAnalysis.Constants;

public static class NodalAnalysisReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new(NodalAnalysisReferenceSets.AnalysisStatus, NodalAnalysisReferenceCodes.AnalysisCompleted, "Completed");
        yield return new(NodalAnalysisReferenceSets.AnalysisStatus, NodalAnalysisReferenceCodes.AnalysisRunning, "Running");
        yield return new(NodalAnalysisReferenceSets.AnalysisStatus, NodalAnalysisReferenceCodes.AnalysisFailed, "Failed");

        yield return new(NodalAnalysisReferenceSets.OptimizationType, NodalAnalysisReferenceCodes.OptimizationMaximizeProduction, "Maximize Production");
        yield return new(NodalAnalysisReferenceSets.OptimizationType, NodalAnalysisReferenceCodes.OptimizationMinimizePressure, "Minimize Pressure");
        yield return new(NodalAnalysisReferenceSets.OptimizationType, NodalAnalysisReferenceCodes.OptimizationEfficiency, "Optimize Efficiency");

        yield return new(NodalAnalysisReferenceSets.DiagnosisStatus, NodalAnalysisReferenceCodes.DiagnosisNormal, "Normal");
        yield return new(NodalAnalysisReferenceSets.DiagnosisStatus, NodalAnalysisReferenceCodes.DiagnosisAttention, "Needs Attention");

        yield return new(NodalAnalysisReferenceSets.LiftMethod, "ESP", "Electric Submersible Pump");
        yield return new(NodalAnalysisReferenceSets.LiftMethod, "GAS_LIFT", "Gas Lift");
        yield return new(NodalAnalysisReferenceSets.LiftMethod, "SUCKER_ROD", "Sucker Rod");
        yield return new(NodalAnalysisReferenceSets.LiftMethod, "PLUNGER_LIFT", "Plunger Lift");
        yield return new(NodalAnalysisReferenceSets.LiftMethod, "HYDRAULIC_JET", "Hydraulic Jet Pump");
        yield return new(NodalAnalysisReferenceSets.LiftMethod, "PCP", "Progressive Cavity Pump");
    }
}
