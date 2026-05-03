using System.Linq;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ChokeAnalysisReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = ChokeAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverRequiredFamilies()
    {
        var sets = ChokeAnalysisReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(ChokeAnalysisReferenceSets.AnalysisStatus, sets);
        Assert.Contains(ChokeAnalysisReferenceSets.ChokeEquipmentType, sets);
        Assert.Contains(ChokeAnalysisReferenceSets.CorrelationMethod, sets);
        Assert.Contains(ChokeAnalysisReferenceSets.FlowRegimeLabel, sets);
    }

    [Fact]
    public void SeedRows_IncludeStatusCorrelationAndRegimeCodes()
    {
        var rows = ChokeAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();

        var statuses = rows
            .Where(r => r.ReferenceSet == ChokeAnalysisReferenceSets.AnalysisStatus)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(ChokeAnalysisReferenceCodes.AnalysisCompleted, statuses);
        Assert.Contains(ChokeAnalysisReferenceCodes.AnalysisRunning, statuses);
        Assert.Contains(ChokeAnalysisReferenceCodes.AnalysisFailed, statuses);

        var correlations = rows
            .Where(r => r.ReferenceSet == ChokeAnalysisReferenceSets.CorrelationMethod)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase, correlations);
        Assert.Contains(ChokeAnalysisReferenceCodes.CorrelationMultiphase, correlations);
        Assert.Contains(ChokeAnalysisReferenceCodes.CorrelationGilbert, correlations);
        Assert.Contains(ChokeAnalysisReferenceCodes.CorrelationBaxendell, correlations);

        var regimes = rows
            .Where(r => r.ReferenceSet == ChokeAnalysisReferenceSets.FlowRegimeLabel)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(ChokeAnalysisReferenceCodes.RegimeSonic, regimes);
        Assert.Contains(ChokeAnalysisReferenceCodes.RegimeSubsonic, regimes);
    }
}
