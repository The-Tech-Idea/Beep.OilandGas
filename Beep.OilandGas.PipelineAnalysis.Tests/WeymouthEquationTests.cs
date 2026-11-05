using Beep.OilandGas.Models.Data.PipelineAnalysis;
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
        double specificGravity = 0.65;

        // Act
        var capacity = PipelineAnalysis.Calculations.PipelineCapacityCalculator
            .CalculateGasPipelineCapacity(Properties(diameter, length, pInlet, pOutlet,temperature,specificGravity));

        // Assert — flow should be positive and in reasonable range for a 12" pipe
        Assert.True(capacity.MAXIMUM_FLOW_RATE * 1000m > 0, "Flow capacity should be positive");
        Assert.True(capacity.MAXIMUM_FLOW_RATE * 1000m > 1_000_000, "12-inch pipe should flow > 1 MMscfd");
        Assert.True(capacity.MAXIMUM_FLOW_RATE * 1000m < 1_000_000_000, "Flow should be < 1 Bscfd for this configuration");
    }

    [Fact]
    public void Weymouth_ZeroPressureDrop_ReturnsZeroFlow()
    {
        double diameter = 6.0;
        double length = 50.0;
        double pInlet = 1000.0;
        double pOutlet = 1000.0;
        double temperature = 520.0;
        double specificGravity = 0.65;

        var capacity = PipelineAnalysis.Calculations.PipelineCapacityCalculator
            .CalculateGasPipelineCapacity(Properties(diameter, length, pInlet, pOutlet,temperature,specificGravity));

        Assert.True(capacity.MAXIMUM_FLOW_RATE == 0, "Zero pressure drop should give zero flow");
    }

    [Fact]
    public void SwameeJain_ValidInput_ReturnsPositiveFrictionFactor()
    {
        double relativeRoughness = 0.00015 / 12.0; // commercial steel, 12-inch
        double reynoldsNumber = 1_000_000;          // fully turbulent

        var f = PipelineAnalysis.Calculations.PipelineCalculator
            .CalculateFrictionFactor((decimal)reynoldsNumber, (decimal)relativeRoughness, 1m);

        Assert.True(f > 0, "Friction factor should be positive");
        Assert.True(f < 0.1m, "Friction factor should be reasonable for turbulent flow");
        Assert.True(f > 0.01m, "Friction factor should be > 0.01 for commercial steel at Re=1e6");
    }

    [Fact]
    public void ErosionVelocity_Api14E_ReturnsReasonableValue()
    {
        double mixtureDensity = 10.0; // lb/ft³ (gas at moderate pressure)
        double cFactor = 100.0;         // API RP 14E solid service

        // The static method is on the ErosionPrediction class
        var result = PipelineAnalysis.Calculations.ErosionPrediction.CalculateErosionalVelocity(
            mixtureGasGravity:0.65, liquidFraction:1, liquidDensityLbFt3:mixtureDensity,
            operatingPressurePsia:100, operatingTempF:60, cFactor:cFactor, actualVelocityFtS:20);
        var ve=result.ErosionalVelocityFtS;
        Assert.Equal(mixtureDensity,result.MixtureDensityLbFt3,8);
        Assert.Equal(31.6227766017,ve,8);

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
            .CalculateGasPipelineCapacity(Properties(diameter, length, pInlet, pOutlet,520.0,sg));

        Assert.True(capacity.MAXIMUM_FLOW_RATE * 1000m > 0, $"Flow should be positive for D={diameter}\", L={length}mi");
    }
    [Fact]
    public void StandardConditionChangesPreserveTheSamePhysicalFlowingVelocity()
    {
        var baseline=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(Properties(12,10,800,600,520,0.65));
        var pressure=Properties(12,10,800,600,520,0.65);pressure.BASE_PRESSURE*=2;
        var atPressure=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(pressure);
        var temperature=Properties(12,10,800,600,520,0.65);temperature.BASE_TEMPERATURE*=2;
        var atTemperature=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(temperature);
        Assert.Equal(baseline.MAXIMUM_FLOW_RATE/2,atPressure.MAXIMUM_FLOW_RATE,8);
        Assert.Equal(baseline.MAXIMUM_FLOW_RATE*2,atTemperature.MAXIMUM_FLOW_RATE,8);
        Assert.Equal(baseline.FLOW_VELOCITY,atPressure.FLOW_VELOCITY,8);
        Assert.Equal(baseline.FLOW_VELOCITY,atTemperature.FLOW_VELOCITY,8);
        Assert.Equal(0.0037878788m,baseline.PRESSURE_GRADIENT,10); // 200 psi / (10 miles * 5280 ft/mile).
    }

    [Fact]
    public void WeymouthCapacityUsesEmpiricalDiameterAndLengthScalingWithoutAnExtraFrictionMultiplier()
    {
        var baseline=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(Properties(6,10,800,600,520,0.65));
        var doubled=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(Properties(12,10,800,600,520,0.65));
        var longer=PipelineAnalysis.Calculations.PipelineCapacityCalculator.CalculateGasPipelineCapacity(Properties(6,40,800,600,520,0.65));
        Assert.Equal(Math.Pow(2,2.667),(double)(doubled.MAXIMUM_FLOW_RATE/baseline.MAXIMUM_FLOW_RATE),8);
        Assert.Equal(baseline.MAXIMUM_FLOW_RATE/2,longer.MAXIMUM_FLOW_RATE,8);
    }

    private static GAS_PIPELINE_FLOW_PROPERTIES Properties(double diameter,double miles,double inlet,double outlet,double temperature,double gravity)
        =>new(){PIPELINE=new(){DIAMETER=(decimal)diameter,LENGTH=(decimal)miles,INLET_PRESSURE=(decimal)inlet,
            OUTLET_PRESSURE=(decimal)outlet,AVERAGE_TEMPERATURE=(decimal)temperature,ROUGHNESS=0.00015m},
            GAS_SPECIFIC_GRAVITY=(decimal)gravity,GAS_FLOW_RATE=1000m,BASE_PRESSURE=14.7m,BASE_TEMPERATURE=520m};
}
