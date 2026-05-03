using System.Linq;
using Beep.OilandGas.NodalAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class NodalAnalysisReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = NodalAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverRequiredFamilies()
    {
        var sets = NodalAnalysisReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(NodalAnalysisReferenceSets.AnalysisStatus, sets);
        Assert.Contains(NodalAnalysisReferenceSets.OptimizationType, sets);
        Assert.Contains(NodalAnalysisReferenceSets.DiagnosisStatus, sets);
        Assert.Contains(NodalAnalysisReferenceSets.LiftMethod, sets);
    }

    [Fact]
    public void SeedRows_IncludeCoreOptimizationAndStatusCodes()
    {
        var rows = NodalAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();

        var optimizationCodes = rows
            .Where(r => r.ReferenceSet == NodalAnalysisReferenceSets.OptimizationType)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(NodalAnalysisReferenceCodes.OptimizationMaximizeProduction, optimizationCodes);
        Assert.Contains(NodalAnalysisReferenceCodes.OptimizationMinimizePressure, optimizationCodes);
        Assert.Contains(NodalAnalysisReferenceCodes.OptimizationEfficiency, optimizationCodes);

        var analysisStatuses = rows
            .Where(r => r.ReferenceSet == NodalAnalysisReferenceSets.AnalysisStatus)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(NodalAnalysisReferenceCodes.AnalysisCompleted, analysisStatuses);
        Assert.Contains(NodalAnalysisReferenceCodes.AnalysisRunning, analysisStatuses);
        Assert.Contains(NodalAnalysisReferenceCodes.AnalysisFailed, analysisStatuses);
    }
}
