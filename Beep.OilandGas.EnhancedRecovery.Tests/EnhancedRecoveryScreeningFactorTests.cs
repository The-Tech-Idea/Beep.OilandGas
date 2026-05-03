using Beep.OilandGas.EnhancedRecovery.Services;
using Xunit;

namespace Beep.OilandGas.EnhancedRecovery.Tests;

public class EnhancedRecoveryScreeningFactorTests
{
    [Fact]
    public void GetScreeningRecoveryFactorPercent_NullOrEmpty_ReturnsDefault()
    {
        Assert.Equal(20m, EnhancedRecoveryService.GetScreeningRecoveryFactorPercent(null));
        Assert.Equal(20m, EnhancedRecoveryService.GetScreeningRecoveryFactorPercent(""));
        Assert.Equal(20m, EnhancedRecoveryService.GetScreeningRecoveryFactorPercent("   "));
    }

    [Theory]
    [InlineData("WATER_FLOOD", 25)]
    [InlineData("water flood", 25)]
    [InlineData("GAS_INJECTION", 18)]
    [InlineData("CO2_MISCIBLE", 20)]
    [InlineData("STEAM", 30)]
    [InlineData("THERMAL", 30)]
    [InlineData("POLYMER", 22)]
    [InlineData("INJECTION", 15)]
    public void GetScreeningRecoveryFactorPercent_MatchesEorClass(string eorType, decimal expectedPercent)
    {
        Assert.Equal(expectedPercent, EnhancedRecoveryService.GetScreeningRecoveryFactorPercent(eorType));
    }
}
