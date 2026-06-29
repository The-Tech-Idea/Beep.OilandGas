using Xunit;

namespace Beep.OilandGas.PlungerLift.Tests;

/// <summary>Verifies Turner critical velocity and plunge lift cycle calculations.</summary>
public class CriticalVelocityTests
{
    [Fact]
    public void TurnerVelocity_GasWell_ReturnsPositiveValue()
    {
        double surfaceTension = 20.0;  // dyne/cm (water)
        double liquidDensity = 62.4;   // lb/ft³
        double gasDensity = 5.0;       // lb/ft³

        var vc = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateTurnerCriticalVelocity(surfaceTension, liquidDensity, gasDensity);

        Assert.True(vc > 0, "Critical velocity should be positive");
        Assert.True(vc > 1 && vc < 50, $"Critical velocity {vc:F1} ft/s should be reasonable");
    }

    [Fact]
    public void TurnerVelocity_DenserGas_RequiresHigherVelocity()
    {
        double st = 20.0;
        double rhoL = 62.4;

        var vcLow = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateTurnerCriticalVelocity(st, rhoL, 2.0);  // low-pressure gas
        var vcHigh = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateTurnerCriticalVelocity(st, rhoL, 15.0); // high-pressure gas

        Assert.True(vcHigh < vcLow, "Denser gas should have lower critical velocity");
    }
}
