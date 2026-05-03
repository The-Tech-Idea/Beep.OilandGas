using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.GasProperties.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

/// <summary>
/// Smoke tests for calculation paths that do not hit persistence (mocks satisfy DI only).
/// </summary>
public class GasPropertiesServiceTests
{
    private static GasPropertiesService CreateSut()
    {
        return new GasPropertiesService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            connectionName: "PPDM39",
            logger: null);
    }

    [Theory]
    [InlineData("standing-katz", 800, 580, 0.65)]
    [InlineData("Hall-Yarborough", 800, 580, 0.65)]
    [InlineData("BRILL-BEGGS", 800, 580, 0.65)]
    public void CalculateZFactor_delegates_to_library_correlations(string correlation, decimal p, decimal t, decimal g)
    {
        var sut = CreateSut();
        decimal expected = correlation.ToLowerInvariant() switch
        {
            "hall-yarborough" => ZFactorCalculator.CalculateHallYarborough(p, t, g),
            "brill-beggs" => ZFactorCalculator.CalculateBrillBeggs(p, t, g),
            _ => ZFactorCalculator.CalculateStandingKatz(p, t, g)
        };

        decimal actual = sut.CalculateZFactor(p, t, g, correlation);
        Assert.Equal(decimal.Round(expected, 6), decimal.Round(actual, 6));
    }

    [Fact]
    public void CalculateGasDensity_uses_service_constant_R()
    {
        var sut = CreateSut();
        const decimal R = 10.73m;
        decimal p = 2000m, t = 580m, z = 0.9m, mw = 18m;
        decimal expected = (p * mw) / (z * R * t);
        Assert.Equal(expected, sut.CalculateGasDensity(p, t, z, mw));
    }

    [Fact]
    public void CalculateFormationVolumeFactor_matches_formula()
    {
        var sut = CreateSut();
        decimal p = 2000m, t = 580m, z = 0.88m;
        const decimal c = 0.02827m;
        decimal expected = c * z * t / p;
        Assert.Equal(expected, sut.CalculateFormationVolumeFactor(p, t, z));
    }
}
