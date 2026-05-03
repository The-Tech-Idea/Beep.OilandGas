using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class NodalAnalysisServiceTests
{
    [Fact]
    public async Task PerformNodalAnalysis_ReturnsCompletedResult_WithCurvesAndOperatingPoint()
    {
        var service = CreateService();
        var parameters = BuildValidParameters();

        var result = await service.PerformNodalAnalysisAsync("UWI-100", parameters);

        Assert.Equal("UWI-100", result.WellUWI);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.IPRCurve);
        Assert.NotEmpty(result.VLPCurve);
        Assert.True(result.OptimalFlowRate >= 0);
        Assert.True(result.OptimalBottomholePressure > 0);
    }

    [Fact]
    public async Task OptimizeSystem_ReturnsDeterministicRecommendationPayload()
    {
        var service = CreateService();
        var goals = new OptimizationGoals
        {
            OptimizationType = "MaximizeProduction",
            TargetFlowRate = 2400m,
            TargetBottomholePressure = 2100m
        };

        var result = await service.OptimizeSystemAsync("UWI-100", goals);

        Assert.Equal("UWI-100", result.WellUWI);
        Assert.NotNull(result.RecommendedOperatingPoint);
        Assert.True(result.Recommendations.Count >= 4);
        Assert.Contains("score", result.Recommendations[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1.", result.Recommendations[0], StringComparison.Ordinal);
        Assert.True(result.ImprovementPercentage >= 0);
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
    }

    [Fact]
    public async Task AnalyzePerformanceMatching_ReturnsVersionedDiagnosticsContract()
    {
        var service = CreateService();
        var parameters = BuildValidParameters();

        var result = await service.AnalyzePerformanceMatchingAsync("UWI-600", parameters);

        Assert.Equal("UWI-600", result.WellUWI);
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
    }

    [Fact]
    public async Task SaveAnalysisResult_Throws_WhenUserIdMissing()
    {
        var service = CreateService();
        var payload = new NodalAnalysisRunResult { WellUWI = "UWI-100", AnalysisDate = DateTime.UtcNow };

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAnalysisResultAsync(payload, ""));
    }

    [Fact]
    public async Task SaveAnalysisResult_Throws_WhenWellUwiMissing()
    {
        var service = CreateService();
        var payload = new NodalAnalysisRunResult { WellUWI = "  ", AnalysisDate = DateTime.UtcNow };

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAnalysisResultAsync(payload, "user-1"));
    }

    [Fact]
    public async Task ForecastProduction_Throws_WhenForecastMonthsNotPositive()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.ForecastProductionAsync("UWI-700", 100m, 0.12m, 0));
    }

    [Fact]
    public async Task ForecastProduction_Throws_WhenDeclineRateOutOfRange()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.ForecastProductionAsync("UWI-701", 100m, 1.2m, 12));
    }

    [Fact]
    public async Task ForecastProduction_AverageUsesSimulatedMonths_WhenEconomicLimitEndsEarly()
    {
        var service = CreateService();

        var forecast = await service.ForecastProductionAsync("UWI-702", 100m, 1m, 36);

        Assert.Single(forecast.MONTHLY_PRODUCTION);
        Assert.Equal(100m, forecast.AVERAGE_MONTHLY_PRODUCTION);
        Assert.Equal(100m, forecast.TOTAL_CUMULATIVE_PRODUCTION);
        Assert.Equal(100m, forecast.INITIAL_PRODUCTION_RATE);
    }

    [Fact]
    public async Task RecommendArtificialLift_ReturnsExplainableScoredRanking()
    {
        var service = CreateService();

        var result = await service.RecommendArtificialLiftAsync(
            "UWI-200",
            currentProduction: 650m,
            targetProduction: 1400m,
            wellDepth: 11000m,
            waterCut: 0.35m);

        Assert.Equal("UWI-200", result.WellUWI);
        Assert.Contains("score", result.PrimaryRecommendation, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.AlternativeRecommendations);
        Assert.NotEmpty(result.CandidateScores);
        Assert.True(result.CandidateScores.ContainsKey("ESP - Electric Submersible Pump"));
        Assert.NotEmpty(result.ScoreBreakdown);
        Assert.Contains(result.ScoreBreakdown, line => line.StartsWith("policy.", StringComparison.Ordinal));
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
    }

    [Fact]
    public async Task PerformSensitivityAnalysis_ReturnsScenarioBundlesAndSweepDefinitions()
    {
        var service = CreateService();
        var parameters = BuildValidParameters();

        var result = await service.PerformSensitivityAnalysisAsync(
            "UWI-300",
            parameters,
            new System.Collections.Generic.List<string> { "WellheadPressure", "TubingDiameter", "ReservoirPressure" });

        Assert.Equal("UWI-300", result.WellUWI);
        Assert.Equal(3, result.ScenarioResults.Count);
        Assert.Equal(3, result.ScenarioOrder.Count);
        Assert.Equal(3, result.SweepDefinition.Count);
        Assert.Equal(5, result.PriceVariation.Count);
        Assert.NotEmpty(result.SensitivityFactors);
        Assert.False(string.IsNullOrWhiteSpace(result.MostSensitiveParameter));
        Assert.Contains(result.MostSensitiveParameter, result.SensitivityFactors.Keys);
        Assert.Contains(result.ScenarioResults, s => s.ScenarioName == "Base");
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
    }

    [Fact]
    public async Task DiagnoseWellPerformance_ReturnsVersionedDiagnosticsContract()
    {
        var service = CreateService();

        var result = await service.DiagnoseWellPerformanceAsync(
            "UWI-400",
            expectedProduction: 1000m,
            actualProduction: 700m,
            wellheadPressure: 2600m,
            bottomholePressure: 1900m);

        Assert.Equal("UWI-400", result.WellUWI);
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
        Assert.NotEmpty(result.IdentifiedIssues);
    }

    [Fact]
    public async Task AnalyzePressureMaintenance_ReturnsVersionedDiagnosticsContract()
    {
        var service = CreateService();

        var result = await service.AnalyzePressureMaintenanceAsync(
            "UWI-500",
            currentReservoirPressure: 2400m,
            bubblePointPressure: 2200m,
            productivityIndex: 2.5m);

        Assert.Equal("UWI-500", result.WellUWI);
        Assert.Equal("NODAL_DIAGNOSTICS_V1", result.ApiContractVersion);
    }

    [Fact]
    public async Task AnalyzePressureMaintenance_WithZeroProductivityIndex_DoesNotDivideByZero()
    {
        var service = CreateService();

        var result = await service.AnalyzePressureMaintenanceAsync(
            "UWI-800",
            currentReservoirPressure: 2400m,
            bubblePointPressure: 2200m,
            productivityIndex: 0m);

        Assert.Equal("UWI-800", result.WellUWI);
        Assert.Equal(0m, result.InjectionVolumeRequired);
    }

    [Fact]
    public async Task AnalyzePressureMaintenance_Throws_WhenProductivityIndexNegative()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.AnalyzePressureMaintenanceAsync("UWI-801", 2400m, 2200m, -0.5m));
    }

    private static NodalAnalysisService CreateService()
    {
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string table, string id) => $"{table}-{id}");

        return new NodalAnalysisService(
            Mock.Of<IDMEEditor>(),
            Mock.Of<ICommonColumnHandler>(),
            defaults.Object,
            Mock.Of<IPPDMMetadataRepository>(),
            "PPDM39",
            NullLogger<NodalAnalysisService>.Instance);
    }

    private static NodalAnalysisParameters BuildValidParameters()
    {
        return new NodalAnalysisParameters
        {
            NumberOfPoints = 30,
            MinFlowRate = 0,
            MaxFlowRate = 5000,
            ReservoirProperties = new ReservoirProperties
            {
                ReservoirPressure = 3500,
                BubblePointPressure = 2200,
                ProductivityIndex = 1.1,
                WaterCut = 0.25,
                GasOilRatio = 800,
                OilGravity = 35,
                FormationVolumeFactor = 1.2,
                OilViscosity = 1.4
            },
            WellboreProperties = new WellboreProperties
            {
                TubingDiameter = 2.875,
                TubingLength = 8200,
                WellheadPressure = 500,
                WaterCut = 0.25,
                GasOilRatio = 800,
                OilGravity = 35,
                GasSpecificGravity = 0.75,
                WellheadTemperature = 90,
                BottomholeTemperature = 180,
                TubingRoughness = 0.0006
            }
        };
    }
}
