using Beep.OilandGas.ProductionOperations.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionOperationsReferenceSeedCatalogTests
{
    [Fact]
    public void MonitoringRows_AreUnique_BySetAndCode()
    {
        var rows = ProductionOperationsReferenceCodeSeed.GetMonitoringReferenceRows().ToList();
        var duplicates = rows
            .GroupBy(row => (row.ReferenceSet, row.ReferenceCode))
            .Where(group => group.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void MonitoringRows_CoverRequiredReferenceFamilies()
    {
        var sets = ProductionOperationsReferenceCodeSeed.GetMonitoringReferenceRows()
            .Select(row => row.ReferenceSet)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(ProductionOperationsReferenceSets.EquipmentActivityType, sets);
        Assert.Contains(ProductionOperationsReferenceSets.MeasurementType, sets);
        Assert.Contains(ProductionOperationsReferenceSets.MeasurementQuality, sets);
        Assert.Contains(ProductionOperationsReferenceSets.MeasurementUom, sets);
    }
}
