using System.Linq;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.Models.Data.Calculations;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

/// <summary>
/// Guards LOV seed rows for <c>R_COMPRESSOR_ANALYSIS_REFERENCE_CODE</c> against duplication and drift from <see cref="CompressorAnalysisWellKnown"/>.
/// </summary>
public class CompressorAnalysisReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = CompressorAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverAnalysisTypeAndCompressorKindSets()
    {
        var sets = CompressorAnalysisReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(CompressorAnalysisReferenceSets.AnalysisType, sets);
        Assert.Contains(CompressorAnalysisReferenceSets.CompressorKind, sets);
    }

    [Fact]
    public void SeedRows_AlignWithCompressorAnalysisWellKnown_WireValues()
    {
        var rows = CompressorAnalysisReferenceCodeSeed.GetAllSeedRows().ToList();

        var analysisCodes = rows
            .Where(r => r.ReferenceSet == CompressorAnalysisReferenceSets.AnalysisType)
            .Select(r => r.ReferenceCode)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Power, analysisCodes);
        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Pressure, analysisCodes);
        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Efficiency, analysisCodes);

        var kindCodes = rows
            .Where(r => r.ReferenceSet == CompressorAnalysisReferenceSets.CompressorKind)
            .Select(r => r.ReferenceCode)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(CompressorAnalysisWellKnown.CompressorType.Centrifugal, kindCodes);
        Assert.Contains(CompressorAnalysisWellKnown.CompressorType.Reciprocating, kindCodes);
    }
}
