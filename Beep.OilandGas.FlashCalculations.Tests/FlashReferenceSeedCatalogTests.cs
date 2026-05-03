using System.Linq;
using Beep.OilandGas.FlashCalculations.Constants;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

public class FlashReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = FlashReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverCoreReferenceSets()
    {
        var sets = FlashReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(FlashReferenceSets.EosModel, sets);
        Assert.Contains(FlashReferenceSets.CalculationCategory, sets);
        Assert.Contains(FlashReferenceSets.SolverPreset, sets);
        Assert.Contains(FlashReferenceSets.FlashSpecification, sets);
        Assert.Contains(FlashReferenceSets.PhaseState, sets);
        Assert.Contains(FlashReferenceSets.PropertyKind, sets);
    }

    [Fact]
    public void SeedRows_IncludePrimaryEosAndPtSpecification()
    {
        var rows = FlashReferenceCodeSeed.GetAllSeedRows().ToList();

        var eos = rows
            .Where(r => r.ReferenceSet == FlashReferenceSets.EosModel)
            .Select(r => r.ReferenceCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("PR", eos);
        Assert.Contains("SRK", eos);

        var specs = rows
            .Where(r => r.ReferenceSet == FlashReferenceSets.FlashSpecification)
            .Select(r => r.ReferenceCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("PT_SPECIFIED", specs);
    }
}
