using System.Collections.Generic;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

/// <summary>
/// Guards trivial Rachford–Rice branches (all liquid / all vapor) and one interior two-phase point.
/// </summary>
public class FlashCalculatorRachfordRiceTests
{
    private static FLASH_COMPONENT Comp(
        string name,
        decimal z,
        decimal tcRankine,
        decimal pcPsia,
        decimal omega,
        decimal mw) =>
        new()
        {
            NAME = name,
            COMPONENT_NAME = name,
            MOLE_FRACTION = z,
            CRITICAL_TEMPERATURE = tcRankine,
            CRITICAL_PRESSURE = pcPsia,
            ACENTRIC_FACTOR = omega,
            MOLECULAR_WEIGHT = mw
        };

    [Fact]
    public void SolveRachfordRice_VeryHighPressure_ReturnsAllLiquid()
    {
        var feed = new List<FLASH_COMPONENT>
        {
            Comp("C1", 0.5m, 343m, 667.8m, 0.008m, 16.04m),
            Comp("C2", 0.5m, 549.6m, 707.8m, 0.098m, 30.07m)
        };

        var v = FlashCalculator.SolveRachfordRice(feed, 500_000m, 540m, out _, out var converged);

        Assert.Equal(0m, v);
        Assert.True(converged);
    }

    [Fact]
    public void SolveRachfordRice_LowPressureHighTemp_TwoPhaseInteriorVaporFraction()
    {
        var feed = new List<FLASH_COMPONENT>
        {
            Comp("C1", 0.5m, 343m, 667.8m, 0.008m, 16.04m),
            Comp("C2", 0.5m, 549.6m, 707.8m, 0.098m, 30.07m)
        };

        // Moderate K-values → two-phase region (not trivial bubble/dew shortcuts).
        var v = FlashCalculator.SolveRachfordRice(feed, 0.5m, 900m, out _, out var converged);

        Assert.InRange((double)v, 0.01, 0.99);
        Assert.True(converged);
    }

    [Fact]
    public void PerformIsothermalFlash_MatchesSolveRachfordRice_ForSameFeed()
    {
        var conditions = new FLASH_CONDITIONS
        {
            PRESSURE = 0.5m,
            TEMPERATURE = 900m,
            FEED_COMPOSITION = new List<FLASH_COMPONENT>
            {
                Comp("C1", 0.5m, 343m, 667.8m, 0.008m, 16.04m),
                Comp("C2", 0.5m, 549.6m, 707.8m, 0.098m, 30.07m)
            }
        };

        var rr = FlashCalculator.SolveRachfordRice(
            conditions.FEED_COMPOSITION,
            conditions.PRESSURE,
            conditions.TEMPERATURE,
            out var itRr,
            out var convRr);

        var flash = FlashCalculator.PerformIsothermalFlash(conditions);

        Assert.Equal(rr, flash.VaporFraction);
        Assert.Equal(itRr, flash.Iterations);
        Assert.Equal(convRr, flash.Converged);
    }
}
