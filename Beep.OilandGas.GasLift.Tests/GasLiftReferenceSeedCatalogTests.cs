using System.Linq;
using Beep.OilandGas.GasLift.Constants;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = GasLiftReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverCoreReferenceSets()
    {
        var sets = GasLiftReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(GasLiftReferenceSets.PortSizeIn, sets);
        Assert.Contains(GasLiftReferenceSets.OperatingMode, sets);
        Assert.Contains(GasLiftReferenceSets.DesignMethod, sets);
        Assert.Contains(GasLiftReferenceSets.ValveService, sets);
        Assert.Contains(GasLiftReferenceSets.InjectionGasSource, sets);
        Assert.Contains(GasLiftReferenceSets.DesignLimit, sets);
    }

    [Fact]
    public void PortSizeSeed_AlignsWithGasLiftConstants()
    {
        var portCodes = GasLiftReferenceCodeSeed.GetAllSeedRows()
            .Where(r => r.ReferenceSet == GasLiftReferenceSets.PortSizeIn)
            .Select(r => r.ReferenceCode)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var inches in GasLiftConstants.StandardPortSizes)
        {
            var code = inches.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains(code, portCodes);
        }
    }
}
