using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Validates choke orchestration in <see cref="PPDMCalculationService"/> — flow regime codes vs reference data.
/// </summary>
public class PPDMCalculationServiceChokeTests
{
    private static PPDMCalculationService CreateService(Mock<IChokeAnalysisService>? choke = null)
    {
        return new PPDMCalculationService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            new Mock<IFieldMappingService>().Object,
            choke?.Object ?? new Mock<IChokeAnalysisService>().Object,
            new Mock<ICompressorAnalysisService>().Object,
            connectionName: "PPDM39",
            logger: null);
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_GilbertFallback_MapsPressureRatioToSonicReferenceCode()
    {
        var svc = CreateService();

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions
            {
                CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationGilbert
            },
            UpstreamPressure = 1000m,
            DownstreamPressure = 400m,
            ChokeDiameter = 0.5m
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, result.FlowRegime);
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_MultiphaseAlias_MapsPressureRatioToSubsonicReferenceCode()
    {
        var svc = CreateService();

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions { CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationMultiphase },
            UpstreamPressure = 1000m,
            DownstreamPressure = 600m,
            ChokeDiameter = 0.5m
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, result.FlowRegime);
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_RosCorrelation_DoesNotInvokeSinglePhaseChokeService()
    {
        var chokeMock = new Mock<IChokeAnalysisService>(MockBehavior.Strict);

        var svc = CreateService(chokeMock);

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions { CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationRos },
            UpstreamPressure = 1000m,
            DownstreamPressure = 400m,
            ChokeDiameter = 0.5m
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, result.FlowRegime);
        chokeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_GasSinglePhaseCorrelation_DelegatesToChokeAnalysisService()
    {
        var chokeMock = new Mock<IChokeAnalysisService>(MockBehavior.Strict);
        chokeMock
            .Setup(c => c.CalculateDownholeChokeFlowAsync(It.IsAny<CHOKE_PROPERTIES>(), It.IsAny<GAS_CHOKE_PROPERTIES>()))
            .ReturnsAsync(new CHOKE_FLOW_RESULT
            {
                FLOW_RATE = 1234.56m,
                UPSTREAM_PRESSURE = 1000m,
                DOWNSTREAM_PRESSURE = 400m,
                PRESSURE_RATIO = 0.4m,
                CRITICAL_PRESSURE_RATIO = 0.55m,
                FLOW_REGIME = ChokeAnalysisReferenceCodes.RegimeSonic
            });

        var svc = CreateService(chokeMock);

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions { CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase },
            UpstreamPressure = 1000m,
            DownstreamPressure = 400m,
            ChokeDiameter = 0.5m,
            AnalysisType = "DOWNHOLE"
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(1234.56m, result.FlowRate);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, result.FlowRegime);
        chokeMock.Verify(c => c.CalculateDownholeChokeFlowAsync(It.IsAny<CHOKE_PROPERTIES>(), It.IsAny<GAS_CHOKE_PROPERTIES>()), Times.Once);
        chokeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_Ros_UsesRosEstimatedUpstream_WhenUpstreamOmitted()
    {
        const decimal liquidRate = 500m;
        const decimal glr = 500m;
        const decimal diameterInches = 0.5m;
        var expected = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, diameterInches).RosPressure;

        var svc = CreateService();

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions { CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationRos },
            FlowRate = liquidRate,
            ChokeDiameter = diameterInches,
            DownstreamPressure = 100m
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(expected, result.UpstreamPressure);
    }

    [Fact]
    public async Task PerformChokeAnalysisAsync_Multiphase_CriticalRatioOverride_RespectedForRegime()
    {
        var svc = CreateService();

        var request = new ChokeAnalysisRequest
        {
            AdditionalParameters = new ChokeAnalysisOptions
            {
                CorrelationMethod = ChokeAnalysisReferenceCodes.CorrelationGilbert,
                CriticalPressureRatioOverride = 0.45m
            },
            UpstreamPressure = 1000m,
            DownstreamPressure = 500m,
            FlowRate = 500m,
            ChokeDiameter = 0.5m
        };

        var result = await svc.PerformChokeAnalysisAsync(request);

        Assert.Equal(0.45m, result.CriticalPressureRatio);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, result.FlowRegime);
    }
}
