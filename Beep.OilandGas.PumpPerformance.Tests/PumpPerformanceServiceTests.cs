using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Services;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class PumpPerformanceServiceTests
{
    private readonly PumpPerformanceService _sut = new(logger: null);

    [Fact]
    public async Task CalculateCFactorAsync_MatchesPowerDividedByQCubed()
    {
        double c = await _sut.CalculateCFactorAsync(200, 100, 50);
        Assert.Equal(200 / (100.0 * 100 * 100), c, 12);
    }

    [Fact]
    public async Task GetEfficiencyAsync_DefaultSg_IsWater()
    {
        double eta = await _sut.GetEfficiencyAsync(300, 75, 230);
        double expected = (300 * 75 * PumpConstants.WaterSpecificGravity)
            / (PumpConstants.HorsepowerConversionFactor * 230);
        Assert.Equal(expected, eta, 9);
    }

    [Fact]
    public async Task GetEfficiencyAsync_WithSpecificGravity_Scales()
    {
        const double sg = 0.88;
        double eta = await _sut.GetEfficiencyAsync(300, 75, 230, sg);
        double expected = (300 * 75 * sg) / (PumpConstants.HorsepowerConversionFactor * 230);
        Assert.Equal(expected, eta, 9);
    }

    [Fact]
    public async Task GetEfficiencyAsync_WhenFlowZero_Throws()
    {
        await Assert.ThrowsAsync<InvalidInputException>(() => _sut.GetEfficiencyAsync(0, 75, 230));
    }

    [Fact]
    public async Task GeneratePerformanceCurveAsync_StoresSpecificGravityAndScalesEfficiency()
    {
        var curveWater = await _sut.GeneratePerformanceCurveAsync("P1", 100, 80, 150, 1.0);
        var curveOil = await _sut.GeneratePerformanceCurveAsync("P1", 100, 80, 150, 0.85);

        Assert.Equal(1.0, curveWater.SpecificGravity);
        Assert.Equal(0.85, curveOil.SpecificGravity);
        Assert.Equal(curveWater.FlowRates.Length, curveOil.Efficiencies.Length);
        Assert.True(curveOil.Efficiencies[4] < curveWater.Efficiencies[4]);
    }

    [Fact]
    public async Task AnalyzePerformanceAsync_UsesSpecificGravity()
    {
        var point = new PumpOperatingPoint
        {
            PumpId = "T1",
            FlowRate = 300,
            Head = 75,
            BrakeHorsepower = 230,
            SpecificGravity = 0.9
        };

        var analysis = await _sut.AnalyzePerformanceAsync(point);
        double expectedEta = (300 * 75 * 0.9) / (PumpConstants.HorsepowerConversionFactor * 230);
        Assert.Equal(expectedEta, analysis.ActualEfficiency, 9);
    }
}
