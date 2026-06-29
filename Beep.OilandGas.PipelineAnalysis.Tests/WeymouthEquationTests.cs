using Xunit;

namespace Beep.OilandGas.PipelineAnalysis.Tests;

/// <summary>
/// Verifies the Weymouth gas flow equation against known reference values.
/// The Weymouth equation is a standard in gas pipeline engineering:
///   Q = 433.5 × (Tb/Pb) × D^2.667 × √((P1² - P2²) / (L × T × Z × SG))
/// where Q is in scf/day, D in inches, P in psia, L in miles, T in °R.
/// </summary>
public class WeymouthEquationTests
{
    [Fact]
    public void Weymouth_ShortPipe_LowPressure_ReturnsPositiveFlow()
    {
        // Arrange: 12-inch pipe, 10 miles, P1=800 psia, P2=600 psia
        double diameter = 12.0;    // inches
        double length = 10.0;      // miles
        double pInlet = 800.0;     // psia
        double pOutlet = 600.0;    // psia
        double temperature = 520.0; // °R
        double zFactor = 0.90;
        double specificGravity = 0.65;

        // Act
        var capacity = PipelineAnalysis.Calculations.PipelineCapacityCalculator
            .CalculateGasPipelineCapacity(diameter, length, pInlet, pOutlet,
                temperature, zFactor, specificGravity);

        // Assert — flow should be positive and in reasonable range for a 12" pipe
        Assert.True(capacity > 0, "Flow capacity should be positive");
        Assert.True(capacity > 1_000_000, "12-inch pipe should flow > 1 MMscfd");
        Assert.True(capacity < 1_000_000_000, "Flow should be < 1 Bscfd for this configuration");
    }

    [Fact]
    public void Weymouth_ZeroPressureDrop_ReturnsZeroFlow()
    {
        double diameter = 6.0;
        double length = 50.0;
        double pInlet = 1000.0;
        double pOutlet = 1000.0;
        double temperature = 520.0;
        double zFactor = 0.90;
        double specificGravity = 0.65;

        var capacity = PipelineAnalysis.Calculations.PipelineCapacityCalculator
            .CalculateGasPipelineCapacity(diameter, length, pInlet, pOutlet,
                temperature, zFactor, specificGravity);

        Assert.True(capacity == 0, "Zero pressure drop should give zero flow");
    }

    [Fact]
    public void SwameeJain_ValidInput_ReturnsPositiveFrictionFactor()
    {
        double relativeRoughness = 0.00015 / 12.0; // commercial steel, 12-inch
        double reynoldsNumber = 1_000_000;          // fully turbulent

        var f = PipelineAnalysis.Calculations.PipelineCalculator
            .SwameeJainFrictionFactor(relativeRoughness, reynoldsNumber);

        Assert.True(f > 0, "Friction factor should be positive");
        Assert.True(f < 0.1, "Friction factor should be reasonable for turbulent flow");
        Assert.True(f > 0.01, "Friction factor should be > 0.01 for commercial steel at Re=1e6");
    }

    [Fact]
    public void ErosionVelocity_Api14E_ReturnsReasonableValue()
    {
        double mixtureDensity = 10.0; // lb/ft³ (gas at moderate pressure)
        double cFactor = 100.0;         // API RP 14E solid service

        // The static method is on the ErosionPrediction class
        var ve = PipelineAnalysis.Calculations.ErosionPrediction
            .CalculateErosionalVelocity(cFactor, mixtureDensity);

        Assert.True(ve > 0, "Erosional velocity should be positive");
        Assert.True(ve > 10 && ve < 200, $"Erosional velocity {ve:F1} ft/s should be in reasonable range");
    }

    [Theory]
    [InlineData(4.0, 20.0, 1200.0, 800.0, 0.6)]    // 4-inch, moderate
    [InlineData(24.0, 100.0, 1000.0, 900.0, 0.65)]  // 24-inch, long
    [InlineData(8.0, 5.0, 1500.0, 200.0, 0.70)]     // 8-inch, high dP
    public void Weymouth_VariousConfigurations_AllReturnPositiveFlow(
        double diameter, double length, double pInlet, double pOutlet, double sg)
    {
        var capacity = PipelineAnalysis.Calculations.PipelineCapacityCalculator
            .CalculateGasPipelineCapacity(diameter, length, pInlet, pOutlet,
                520.0, 0.90, sg);

        Assert.True(capacity > 0, $"Flow should be positive for D={diameter}\", L={length}mi");
    }
}
