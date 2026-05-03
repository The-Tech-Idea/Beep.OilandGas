using System.Collections.Generic;

namespace Beep.OilandGas.ChokeAnalysis.Constants;

public static class ChokeAnalysisReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new(ChokeAnalysisReferenceSets.AnalysisStatus, ChokeAnalysisReferenceCodes.AnalysisCompleted, "Completed");
        yield return new(ChokeAnalysisReferenceSets.AnalysisStatus, ChokeAnalysisReferenceCodes.AnalysisRunning, "Running");
        yield return new(ChokeAnalysisReferenceSets.AnalysisStatus, ChokeAnalysisReferenceCodes.AnalysisFailed, "Failed");

        yield return new(ChokeAnalysisReferenceSets.ChokeEquipmentType, ChokeAnalysisReferenceCodes.TypeBean, "Fixed bean / orifice");
        yield return new(ChokeAnalysisReferenceSets.ChokeEquipmentType, ChokeAnalysisReferenceCodes.TypeAdjustable, "Adjustable choke");
        yield return new(ChokeAnalysisReferenceSets.ChokeEquipmentType, ChokeAnalysisReferenceCodes.TypePositive, "Positive choke");

        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase, "Single-phase gas (isentropic orifice)");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationMultiphase, "Multiphase empirical (alias)");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationGilbert, "Gilbert (multiphase empirical)");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationRos, "Ros");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationAchong, "Achong");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationBaxendell, "Baxendell");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationPilehvari, "Pilehvari");
        yield return new(ChokeAnalysisReferenceSets.CorrelationMethod, ChokeAnalysisReferenceCodes.CorrelationSachdeva, "Sachdeva (subcritical multiphase)");

        yield return new(ChokeAnalysisReferenceSets.FlowRegimeLabel, ChokeAnalysisReferenceCodes.RegimeSonic, "Sonic (critical) flow");
        yield return new(ChokeAnalysisReferenceSets.FlowRegimeLabel, ChokeAnalysisReferenceCodes.RegimeSubsonic, "Subsonic flow");
    }
}
