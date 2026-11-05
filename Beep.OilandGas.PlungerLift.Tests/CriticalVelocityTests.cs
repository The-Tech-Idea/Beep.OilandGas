using Xunit;

namespace Beep.OilandGas.PlungerLift.Tests;

/// <summary>Verifies Turner critical velocity and plunge lift cycle calculations.</summary>
public class CriticalVelocityTests
{
    [Fact]
    public void TurnerVelocity_GasWell_ReturnsPositiveValue()
    {
        decimal surfaceTension = 20.0m;  // dyne/cm (water)
        decimal liquidDensity = 62.4m;   // lb/ft³
        decimal gasDensity = 5.0m;       // lb/ft³

        var vc = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateCriticalVelocity_Turner(gasDensity, liquidDensity, surfaceTension);

        Assert.True(vc > 0, "Critical velocity should be positive");
        Assert.True(vc > 1 && vc < 50, $"Critical velocity {vc:F1} ft/s should be reasonable");
    }

    [Fact]
    public void TurnerVelocity_DenserGas_RequiresLowerVelocity()
    {
        decimal st = 20.0m;
        decimal rhoL = 62.4m;

        var vcLow = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateCriticalVelocity_Turner(2.0m, rhoL, st);  // low-pressure gas
        var vcHigh = PlungerLift.Calculations.PlungerLiftCalculator
            .CalculateCriticalVelocity_Turner(15.0m, rhoL, st); // high-pressure gas

        Assert.True(vcHigh < vcLow, "Denser gas should have lower critical velocity");
    }
}
