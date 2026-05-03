using System;
using Beep.OilandGas.LifeCycle.Services.Calculations;
using Beep.OilandGas.CompressorAnalysis.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Validates compressor facility orchestration in <see cref="PPDMCalculationService"/> —
/// <see cref="CompressorAnalysisWellKnown.AnalysisType"/> branching and service delegation.
/// </summary>
public class PPDMCalculationServiceCompressorTests
{
    private static PPDMCalculationService CreateService(Mock<ICompressorAnalysisService> compressor)
    {
        return new PPDMCalculationService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            new Mock<IFieldMappingService>().Object,
            new Mock<IChokeAnalysisService>().Object,
            compressor.Object,
            connectionName: "PPDM39",
            logger: null);
    }

    private static CompressorAnalysisRequest BaseRequest() =>
        new()
        {
            FacilityId = "FAC-1",
            SuctionPressure = 100m,
            DischargePressure = 500m,
            SuctionTemperature = 520m,
            FlowRate = 5000m,
            GasSpecificGravity = 0.65m,
            PolytropicEfficiency = 0.75m
        };

    [Fact]
    public async Task PerformCompressorAnalysisAsync_Pressure_CallsCalculateRequiredPressureOnly()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateRequiredPressureAsync(
                It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>()))
            .ReturnsAsync(new COMPRESSOR_PRESSURE_RESULT
            {
                REQUIRED_DISCHARGE_PRESSURE = 480m,
                COMPRESSION_RATIO = 4.8m,
                REQUIRED_POWER = 42m,
                DISCHARGE_TEMPERATURE = 580m
            });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = CompressorAnalysisWellKnown.AnalysisType.Pressure;

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        Assert.Equal(42m, result.PowerRequired);
        Assert.Equal(480m, result.DischargePressure);
        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Once);
        compMock.Verify(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()), Times.Never);
        compMock.Verify(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()), Times.Never);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_PressureWhitespaceCaseInsensitive_UsesPressurePath()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateRequiredPressureAsync(
                It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>()))
            .ReturnsAsync(new COMPRESSOR_PRESSURE_RESULT { REQUIRED_POWER = 1m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = "  pressure  ";

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_PowerCentrifugal_CallsCentrifugalOnly()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()))
            .ReturnsAsync(new COMPRESSOR_POWER_RESULT
            {
                BRAKE_HORSEPOWER = 300m,
                POLYTROPIC_HEAD = 1000m,
                ADIABATIC_HEAD = 950m,
                DISCHARGE_TEMPERATURE = 650m,
                COMPRESSION_RATIO = 5m,
                OVERALL_EFFICIENCY = 0.72m
            });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = CompressorAnalysisWellKnown.AnalysisType.Power;
        request.CompressorType = CompressorAnalysisWellKnown.CompressorType.Centrifugal;

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        Assert.Equal(300m, result.PowerRequired);
        compMock.Verify(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()), Times.Once);
        compMock.Verify(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()), Times.Never);
        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_PowerReciprocating_CallsReciprocatingOnly()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()))
            .ReturnsAsync(new COMPRESSOR_POWER_RESULT
            {
                BRAKE_HORSEPOWER = 150m,
                POLYTROPIC_HEAD = 800m,
                ADIABATIC_HEAD = 780m,
                DISCHARGE_TEMPERATURE = 620m,
                COMPRESSION_RATIO = 4m,
                OVERALL_EFFICIENCY = 0.80m
            });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.CompressorType = CompressorAnalysisWellKnown.CompressorType.Reciprocating;

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        Assert.Equal(150m, result.PowerRequired);
        compMock.Verify(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()), Times.Once);
        compMock.Verify(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()), Times.Never);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_EfficiencyAnalysisType_UsesPowerPathLikePower()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()))
            .ReturnsAsync(new COMPRESSOR_POWER_RESULT { BRAKE_HORSEPOWER = 50m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = CompressorAnalysisWellKnown.AnalysisType.Efficiency;

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        compMock.Verify(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()), Times.Once);
        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_AdditionalParametersSpeed_PassedToCentrifugalProperties()
    {
        const decimal expectedSpeed = 7777m;
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.Is<CENTRIFUGAL_COMPRESSOR_PROPERTIES>(p => p.SPEED == expectedSpeed)))
            .ReturnsAsync(new COMPRESSOR_POWER_RESULT { BRAKE_HORSEPOWER = 1m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AdditionalParameters = new CompressorAnalysisOptions { Speed = expectedSpeed };

        await svc.PerformCompressorAnalysisAsync(request);

        compMock.Verify(c => c.CalculateCentrifugalPowerAsync(It.Is<CENTRIFUGAL_COMPRESSOR_PROPERTIES>(p => p.SPEED == expectedSpeed)), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_AdditionalParametersMaxHp_PassedToPressureCalculation()
    {
        const decimal maxHp = 333m;
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateRequiredPressureAsync(
                It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
                It.IsAny<decimal>(),
                maxHp,
                It.IsAny<decimal>()))
            .ReturnsAsync(new COMPRESSOR_PRESSURE_RESULT { REQUIRED_POWER = 1m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = CompressorAnalysisWellKnown.AnalysisType.Pressure;
        request.AdditionalParameters = new CompressorAnalysisOptions { MaxDriverHorsepower = maxHp };

        await svc.PerformCompressorAnalysisAsync(request);

        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            maxHp,
            It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_AnalysisTypeFromOptionsWhenUnset_UsesPressurePath()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateRequiredPressureAsync(
                It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>()))
            .ReturnsAsync(new COMPRESSOR_PRESSURE_RESULT { REQUIRED_POWER = 2m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.AnalysisType = string.Empty;
        request.AdditionalParameters = new CompressorAnalysisOptions
        {
            AnalysisType = CompressorAnalysisWellKnown.AnalysisType.Pressure
        };

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        Assert.Equal(CompressorAnalysisWellKnown.AnalysisType.Pressure, result.AnalysisType);
        compMock.Verify(c => c.CalculateRequiredPressureAsync(
            It.IsAny<COMPRESSOR_OPERATING_CONDITIONS>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_CompressorTypeFromOptionsWhenUnset_UsesReciprocatingPath()
    {
        var compMock = new Mock<ICompressorAnalysisService>(MockBehavior.Strict);
        compMock
            .Setup(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()))
            .ReturnsAsync(new COMPRESSOR_POWER_RESULT { BRAKE_HORSEPOWER = 9m });

        var svc = CreateService(compMock);
        var request = BaseRequest();
        request.CompressorType = string.Empty;
        request.AdditionalParameters = new CompressorAnalysisOptions
        {
            CompressorType = CompressorAnalysisWellKnown.CompressorType.Reciprocating
        };

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Success, result.Status);
        Assert.Equal(CompressorAnalysisWellKnown.CompressorType.Reciprocating, result.CompressorType);
        compMock.Verify(c => c.CalculateReciprocatingPowerAsync(It.IsAny<RECIPROCATING_COMPRESSOR_PROPERTIES>()), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_CompressorServiceThrows_SetsFailedStatusAndErrorMessage()
    {
        var compMock = new Mock<ICompressorAnalysisService>();
        compMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()))
            .ThrowsAsync(new InvalidOperationException("compressor service unavailable"));

        var svc = CreateService(compMock);
        var request = BaseRequest();

        var result = await svc.PerformCompressorAnalysisAsync(request);

        Assert.Equal(CalculationRunStatus.Failed, result.Status);
        Assert.Contains("compressor service unavailable", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_NullRequest_ThrowsArgumentNullException()
    {
        var svc = CreateService(new Mock<ICompressorAnalysisService>());

        await Assert.ThrowsAsync<ArgumentNullException>(() => svc.PerformCompressorAnalysisAsync(null!));
    }

    [Fact]
    public async Task PerformCompressorAnalysisAsync_CompressorServiceCanceled_RethrowsOperationCanceledException()
    {
        var compMock = new Mock<ICompressorAnalysisService>();
        compMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()))
            .ThrowsAsync(new OperationCanceledException());

        var svc = CreateService(compMock);
        var request = BaseRequest();

        await Assert.ThrowsAsync<OperationCanceledException>(() => svc.PerformCompressorAnalysisAsync(request));
    }
}
