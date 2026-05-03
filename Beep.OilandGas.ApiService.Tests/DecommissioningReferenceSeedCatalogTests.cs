using System.Linq;
using Beep.OilandGas.Decommissioning.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class DecommissioningReferenceSeedCatalogTests
{
    [Fact]
    public void GetAllSeedRows_HasUniqueReferenceSetReferenceCodePairs()
    {
        var rows = DecommissioningReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicateKeys = rows
            .GroupBy(r => (r.ReferenceSet, r.ReferenceCode))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.ReferenceSet}/{g.Key.ReferenceCode}")
            .ToList();

        Assert.Empty(duplicateKeys);
    }

    [Fact]
    public void GetAllSeedRows_ContainsWellStatusFamilies()
    {
        var rows = DecommissioningReferenceCodeSeed.GetAllSeedRows().ToList();
        Assert.Contains(rows, r => r.ReferenceSet == DecommissioningReferenceSets.WellStatusType &&
                                   r.ReferenceCode == DecommissioningReferenceDefaults.DecommissioningStatusType);
        Assert.Contains(rows, r => r.ReferenceSet == DecommissioningReferenceSets.WellStatus &&
                                   r.ReferenceCode == "P_AND_A_PLANNED");
        Assert.Contains(rows, r => r.ReferenceSet == DecommissioningReferenceSets.WellStatus &&
                                   r.ReferenceCode == "P_AND_A_VERIFIED");
    }
}
