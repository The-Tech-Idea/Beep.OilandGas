using System;
using System.Collections.Generic;
using Beep.OilandGas.GasProperties.Calculations;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class AveragePropertiesCalculatorTests
{
    private static decimal BrillBeggs(decimal p, decimal t, decimal g) =>
        ZFactorCalculator.CalculateBrillBeggs(p, t, g);

    [Fact]
    [Trait("Scenario", "Reservoir")]
    public void CalculatePressureWeightedAverage_matches_manual_weights()
    {
        var pressures = new List<decimal> { 1000m, 2000m };
        var temperatures = new List<decimal> { 580m, 580m };
        decimal g = 0.65m;
        decimal z1 = ZFactorCalculator.CalculateBrillBeggs(1000m, 580m, g);
        decimal z2 = ZFactorCalculator.CalculateBrillBeggs(2000m, 580m, g);
        decimal w1 = 1000m, w2 = 2000m;
        decimal expectedZ = (z1 * w1 + z2 * w2) / (w1 + w2);
        decimal expectedP = (1000m * w1 + 2000m * w2) / (w1 + w2);

        var avg = AveragePropertiesCalculator.CalculatePressureWeightedAverage(
            pressures, temperatures, g, BrillBeggs);

        Assert.Equal(decimal.Round(expectedP, 5), decimal.Round(avg.AVERAGE_PRESSURE, 5));
        Assert.Equal(580m, avg.AVERAGE_TEMPERATURE);
        Assert.Equal(decimal.Round(expectedZ, 6), decimal.Round(avg.AVERAGE_Z_FACTOR, 6));
    }

    [Fact]
    public void CalculateArithmeticAverage_uses_Z_at_mean_pressure_and_temperature()
    {
        var pressures = new List<decimal> { 1000m, 2000m };
        var temperatures = new List<decimal> { 560m, 600m };
        decimal g = 0.65m;
        decimal meanP = 1500m;
        decimal meanT = 580m;
        decimal expectedZ = ZFactorCalculator.CalculateBrillBeggs(meanP, meanT, g);

        var avg = AveragePropertiesCalculator.CalculateArithmeticAverage(
            pressures, temperatures, g, BrillBeggs);

        Assert.Equal(meanP, avg.AVERAGE_PRESSURE);
        Assert.Equal(meanT, avg.AVERAGE_TEMPERATURE);
        Assert.Equal(decimal.Round(expectedZ, 6), decimal.Round(avg.AVERAGE_Z_FACTOR, 6));
    }

    [Fact]
    [Trait("Scenario", "Facility")]
    public void CalculateAverageOverRange_returns_mid_pressure_and_sample_mean_Z_and_viscosity()
    {
        decimal minP = 500m, maxP = 1500m, t = 580m, g = 0.65m;
        int n = 3;

        decimal z0 = ZFactorCalculator.CalculateBrillBeggs(500m, t, g);
        decimal z1 = ZFactorCalculator.CalculateBrillBeggs(1000m, t, g);
        decimal z2 = ZFactorCalculator.CalculateBrillBeggs(1500m, t, g);
        decimal mu0 = GasViscosityCalculator.CalculateCarrKobayashiBurrows(500m, t, g, z0);
        decimal mu1 = GasViscosityCalculator.CalculateCarrKobayashiBurrows(1000m, t, g, z1);
        decimal mu2 = GasViscosityCalculator.CalculateCarrKobayashiBurrows(1500m, t, g, z2);

        var avg = AveragePropertiesCalculator.CalculateAverageOverRange(
            minP, maxP, t, g, n,
            BrillBeggs,
            GasViscosityCalculator.CalculateCarrKobayashiBurrows);

        Assert.Equal(1000m, avg.AVERAGE_PRESSURE);
        Assert.Equal(t, avg.AVERAGE_TEMPERATURE);
        Assert.Equal(decimal.Round((z0 + z1 + z2) / 3m, 6), decimal.Round(avg.AVERAGE_Z_FACTOR, 6));
        Assert.Equal(decimal.Round((mu0 + mu1 + mu2) / 3m, 6), decimal.Round(avg.AVERAGE_VISCOSITY, 6));
    }

    [Fact]
    public void CalculatePressureWeightedAverage_throws_when_counts_mismatch()
    {
        Assert.Throws<ArgumentException>(() =>
            AveragePropertiesCalculator.CalculatePressureWeightedAverage(
                new List<decimal> { 1m },
                new List<decimal> { 1m, 2m },
                0.65m,
                BrillBeggs));
    }
}
