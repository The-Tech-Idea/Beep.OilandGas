using Beep.OilandGas.PermitsAndApplications.Constants;
using Xunit;

namespace Beep.OilandGas.PermitsAndApplications.Tests;

public class PermitsReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = PermitsReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverRequiredFamilies()
    {
        var sets = PermitsReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(PermitsReferenceSets.PermitStatus, sets);
        Assert.Contains(PermitsReferenceSets.PermitAuthorityCategory, sets);
        Assert.Contains(PermitsReferenceSets.ComplianceOutcome, sets);
        Assert.Contains(PermitsReferenceSets.ComplianceStatus, sets);
        Assert.Contains(PermitsReferenceSets.FormRequirementType, sets);
    }
}
