using System.Linq;
using Beep.OilandGas.LeaseAcquisition.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class LeaseAcquisitionReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUniqueByReferenceSetAndCode()
    {
        var duplicates = LeaseReferenceCodeSeed.GetAllSeedRows()
            .GroupBy(r => (r.ReferenceSet, r.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedCatalog_CoversOperationalStatusFamily()
    {
        var rows = LeaseReferenceCodeSeed.GetAllSeedRows()
            .Where(r => r.ReferenceSet == LeaseReferenceSets.LandRightOperationalStatus)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(LeaseReferenceCodes.Active, rows);
        Assert.Contains(LeaseReferenceCodes.Inactive, rows);
        Assert.Contains(LeaseReferenceCodes.Pending, rows);
        Assert.Contains(LeaseReferenceCodes.Terminated, rows);
    }
}
