using System;
using System.Collections.Generic;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

/// <summary>
/// Locks a binary Wilson + Rachford–Rice vapor fraction against an independent reference (same thermodynamic closure computed externally).
/// </summary>
public class FlashIsothermalFlashGoldenVectorTests
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
    public void PerformIsothermalFlash_BinaryC1C2_MatchesExternalWilsonRrGoldenVector()
    {
        // External reference (Node, double precision): Wilson K with Ki = (Pc/P)*exp(5.37*(1+w)*(1-T/Tc)); Newton RR → V ≈ 0.6119682145527167
        const double goldenVaporFraction = 0.6119682145527167;

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

        var flash = FlashCalculator.PerformIsothermalFlash(conditions);

        Assert.True(
            Math.Abs((double)flash.VaporFraction - goldenVaporFraction) < 1e-5,
            $"Vapor fraction {(double)flash.VaporFraction} differs from golden {goldenVaporFraction}");
    }
}
