using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionForecastingReferenceSeedCatalogTests
{
    [Fact]
    public void SeedRows_AreUnique_ByReferenceSetAndCode()
    {
        var rows = ProductionForecastingReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicates = rows
            .GroupBy(x => (x.ReferenceSet, x.ReferenceCode))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedRows_CoverRequiredFamilies()
    {
        var sets = ProductionForecastingReferenceCodeSeed.GetAllSeedRows()
            .Select(x => x.ReferenceSet)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        Assert.Contains(ProductionForecastingReferenceSets.ForecastMethod, sets);
        Assert.Contains(ProductionForecastingReferenceSets.ForecastRunStatus, sets);
        Assert.Contains(ProductionForecastingReferenceSets.ForecastRiskRating, sets);
    }

    [Fact]
    public void SeedCatalog_IncludesRepresentativeForecastMethods()
    {
        var codes = ProductionForecastingReferenceCodeSeed.GetAllSeedRows()
            .Where(r => r.ReferenceSet == ProductionForecastingReferenceSets.ForecastMethod)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(nameof(ForecastType.Hyperbolic), codes);
        Assert.Contains(nameof(ForecastType.Decline), codes);
        Assert.Contains(nameof(ForecastType.None), codes);
    }

    [Fact]
    public void SeedCatalog_IncludesRunStatusesAndRisk()
    {
        var rows = ProductionForecastingReferenceCodeSeed.GetAllSeedRows().ToList();

        var statuses = rows
            .Where(r => r.ReferenceSet == ProductionForecastingReferenceSets.ForecastRunStatus)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(ProductionForecastingReferenceCodes.RunStatusGenerated, statuses);
        Assert.Contains(ProductionForecastingReferenceCodes.RunStatusOk, statuses);
        Assert.Contains(ProductionForecastingReferenceCodes.RunStatusActive, statuses);
        Assert.Contains(ProductionForecastingReferenceCodes.RunStatusInactive, statuses);

        var risks = rows
            .Where(r => r.ReferenceSet == ProductionForecastingReferenceSets.ForecastRiskRating)
            .Select(r => r.ReferenceCode)
            .ToHashSet();

        Assert.Contains(ProductionForecastingReferenceCodes.RiskUnknown, risks);
    }
}
