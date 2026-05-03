using Beep.OilandGas.FlashCalculations.Services;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

public class FlashFeedCatalogMergeTests
{
    [Fact]
    public void FromLiquidComposition_CopiesCriticalData_WhenNameMatchesCatalog()
    {
        var catalog = new List<FLASH_COMPONENT>
        {
            new()
            {
                NAME = "C1",
                COMPONENT_NAME = "C1",
                MOLE_FRACTION = 0.5m,
                CRITICAL_TEMPERATURE = 343m,
                CRITICAL_PRESSURE = 667.8m,
                ACENTRIC_FACTOR = 0.008m,
                MOLECULAR_WEIGHT = 16.04m
            }
        };

        var liquid = new List<FlashComponentFraction>
        {
            new() { ComponentName = "C1", Fraction = 1.0m }
        };

        var feed = FlashFeedCatalogMerge.FromLiquidComposition(liquid, catalog);

        Assert.Single(feed);
        Assert.Equal(343m, feed[0].CRITICAL_TEMPERATURE);
        Assert.Equal(667.8m, feed[0].CRITICAL_PRESSURE);
        Assert.Equal(0.008m, feed[0].ACENTRIC_FACTOR);
        Assert.Equal(16.04m, feed[0].MOLECULAR_WEIGHT);
        Assert.Equal(1.0m, feed[0].MOLE_FRACTION);
    }

    [Fact]
    public void FromLiquidComposition_MatchesCatalog_CaseInsensitive()
    {
        var catalog = new List<FLASH_COMPONENT>
        {
            new()
            {
                NAME = "c1",
                COMPONENT_NAME = "c1",
                CRITICAL_TEMPERATURE = 200m,
                CRITICAL_PRESSURE = 100m,
                ACENTRIC_FACTOR = 0.1m,
                MOLECULAR_WEIGHT = 20m
            }
        };

        var liquid = new List<FlashComponentFraction>
        {
            new() { ComponentName = "C1", Fraction = 0.25m }
        };

        var feed = FlashFeedCatalogMerge.FromLiquidComposition(liquid, catalog);

        Assert.Single(feed);
        Assert.Equal(200m, feed[0].CRITICAL_TEMPERATURE);
    }

    [Fact]
    public void FromLiquidComposition_UnknownComponent_UsesZerosForPhysicalProps()
    {
        var liquid = new List<FlashComponentFraction>
        {
            new() { ComponentName = "X", Fraction = 1m }
        };

        var feed = FlashFeedCatalogMerge.FromLiquidComposition(liquid, new List<FLASH_COMPONENT>());

        Assert.Single(feed);
        Assert.Equal("X", feed[0].NAME);
        Assert.Equal("X", feed[0].COMPONENT_NAME);
        Assert.Equal(0m, feed[0].CRITICAL_PRESSURE);
    }
}
