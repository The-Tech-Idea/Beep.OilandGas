using System.Linq;
using Beep.OilandGas.EnhancedRecovery.Constants;
using Xunit;

namespace Beep.OilandGas.EnhancedRecovery.Tests;

/// <summary>
/// Guards LOV seed rows for <c>R_ENHANCED_RECOVERY_REFERENCE_CODE</c> against duplication and drift from <see cref="EnhancedRecoveryReferenceSets"/>.
/// </summary>
public class EnhancedRecoveryReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = EnhancedRecoveryReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverEorMethodCategoryAndScreeningClassSets()
    {
        var sets = EnhancedRecoveryReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(EnhancedRecoveryReferenceSets.EorMethodCategory, sets);
        Assert.Contains(EnhancedRecoveryReferenceSets.ScreeningClass, sets);
    }

    [Fact]
    public void SeedRows_IncludeCorePdenSubtypeCodes()
    {
        var methodCodes = EnhancedRecoveryReferenceCodeSeed.GetAllSeedRows()
            .Where(r => r.ReferenceSet == EnhancedRecoveryReferenceSets.EorMethodCategory)
            .Select(r => r.ReferenceCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("WATER_FLOOD", methodCodes);
        Assert.Contains("GAS_INJECTION", methodCodes);
        Assert.Contains("CO2_MISCIBLE", methodCodes);
        Assert.Contains("INJECTION", methodCodes);
    }
}
