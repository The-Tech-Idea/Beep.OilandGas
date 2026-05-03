using Beep.OilandGas.GasProperties.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class GasPropertiesServiceAdvancedTests
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

    [Fact]
    public async Task CalculateRealGasPropertiesAsync_returns_pressure_sweep_rows()
    {
        var sut = CreateSut();
        var rows = await sut.CalculateRealGasPropertiesAsync(
            specificGravity: 0.65m,
            variablePressureStart: 500m,
            variablePressureEnd: 600m,
            step: 50m,
            constantTemperature: 580m);

        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.True(r.PRESSURE is >= 500m and <= 600m));
        Assert.All(rows, r => Assert.True(r.Z_FACTOR > 0m));
    }

    [Fact]
    public async Task CalculateRealGasPropertiesAsync_throws_when_specific_gravity_invalid()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.CalculateRealGasPropertiesAsync(0m, 500m, 600m, 50m, 580m));
    }

    [Fact]
    public async Task CalculateRealGasPropertiesAsync_throws_when_step_non_positive()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.CalculateRealGasPropertiesAsync(0.65m, 500m, 600m, 0m, 580m));
    }

    [Fact]
    public async Task CalculateRealGasPropertiesAsync_throws_when_end_before_start()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.CalculateRealGasPropertiesAsync(0.65m, 600m, 500m, 50m, 580m));
    }
}
