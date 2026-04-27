using System;
using System.Collections.Generic;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProspectIdentificationControllerWorkflowTests
{
    private static ProspectIdentificationController CreateSut(
        out Mock<IProspectIdentificationService> prospectService,
        out Mock<IProspectTechnicalMaturationService> technical,
        out Mock<IProspectRiskEconomicAnalysisService> riskEconomic,
        out Mock<IProspectPortfolioOptimizationService> portfolio)
    {
        prospectService = new Mock<IProspectIdentificationService>();
        technical = new Mock<IProspectTechnicalMaturationService>();
        riskEconomic = new Mock<IProspectRiskEconomicAnalysisService>();
        portfolio = new Mock<IProspectPortfolioOptimizationService>();
        var logger = NullLogger<ProspectIdentificationController>.Instance;
        return new ProspectIdentificationController(
            prospectService.Object,
            technical.Object,
            riskEconomic.Object,
            portfolio.Object,
            logger);
    }

    [Fact]
    public async Task EstimateResources_Delegates_ToTechnicalMaturation_AndReturnsOk()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var expected = new ResourceEstimationResult { ProspectId = "P-001", EstimationMethod = "Volumetric" };
        technical.Setup(t => t.EstimateResourcesAsync(
                "P-001",
                1_000_000m,
                0.75m,
                0.22m,
                0.35m,
                "analyst-1"))
            .ReturnsAsync(expected);

        var request = new ResourceEstimationRequest
        {
            ProspectId = "P-001",
            GrossRockVolume = 1_000_000m,
            NetToGrossRatio = 0.75m,
            Porosity = 0.22m,
            WaterSaturation = 0.35m,
            EstimatedBy = "analyst-1"
        };

        var actionResult = await sut.EstimateResources(request);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        technical.Verify(t => t.EstimateResourcesAsync(
            "P-001",
            1_000_000m,
            0.75m,
            0.22m,
            0.35m,
            "analyst-1"), Times.Once);
    }

    [Fact]
    public async Task EstimateResources_WithNullBody_ReturnsBadRequest()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var actionResult = await sut.EstimateResources(null!);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        technical.Verify(
            t => t.EstimateResourcesAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OptimizePortfolioWorkflow_WithEmptyRankings_ReturnsBadRequest()
    {
        var sut = CreateSut(out _, out _, out _, out var portfolio);
        var actionResult = await sut.OptimizePortfolioWorkflow(new PortfolioOptimizationWorkflowRequest
        {
            RankedProspects = new(),
            RiskTolerance = 1m,
            CapitalBudget = 1_000_000m
        });
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        portfolio.Verify(
            p => p.OptimizePortfolioAsync(It.IsAny<List<ProspectRanking>>(), It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public async Task PerformRiskAssessment_Delegates_ToRiskEconomic_AndReturnsOk()
    {
        var sut = CreateSut(out _, out _, out var riskEconomic, out _);
        var expected = new ProspectRiskAnalysisResult
        {
            ProspectId = "P-002",
            OverallRiskLevel = "Medium"
        };

        var scores = new Dictionary<string, decimal>
        {
            ["Trap"] = 0.3m,
            ["Seal"] = 0.2m
        };

        riskEconomic.Setup(r => r.PerformRiskAssessmentAsync("P-002", "geo-1", scores))
            .ReturnsAsync(expected);

        var actionResult = await sut.PerformRiskAssessment(new WorkflowRiskAssessmentRequest
        {
            ProspectId = "P-002",
            AssessedBy = "geo-1",
            RiskScores = scores
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        riskEconomic.Verify(r => r.PerformRiskAssessmentAsync("P-002", "geo-1", scores), Times.Once);
    }

    [Fact]
    public async Task AnalyzeSeismicInterpretation_Delegates_ToTechnicalMaturation_AndReturnsOk()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var expected = new SeismicInterpretationAnalysis
        {
            ProspectId = "P-050",
            SurveyId = "SEIS-42",
            InterpretationStatus = "Completed"
        };
        var horizons = new List<Horizon>();
        var faults = new List<Fault>();
        technical.Setup(t => t.AnalyzeSeismicInterpretationAsync("P-050", "SEIS-42", horizons, faults))
            .ReturnsAsync(expected);

        var actionResult = await sut.AnalyzeSeismicInterpretation(new SeismicInterpretationAnalysisRequest
        {
            ProspectId = "P-050",
            SurveyId = "SEIS-42",
            Horizons = horizons,
            Faults = faults
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        technical.Verify(t => t.AnalyzeSeismicInterpretationAsync("P-050", "SEIS-42", horizons, faults), Times.Once);
    }

    [Fact]
    public async Task AnalyzeSeismicInterpretation_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        technical.Setup(t => t.AnalyzeSeismicInterpretationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Horizon>>(),
                It.IsAny<List<Fault>>()))
            .ThrowsAsync(new ArgumentException("Prospect ID cannot be null or empty"));

        var actionResult = await sut.AnalyzeSeismicInterpretation(new SeismicInterpretationAnalysisRequest
        {
            ProspectId = string.Empty,
            SurveyId = "SEIS-001",
            Horizons = new(),
            Faults = new()
        });

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task PerformRiskAssessment_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var sut = CreateSut(out _, out _, out var riskEconomic, out _);
        riskEconomic.Setup(r => r.PerformRiskAssessmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, decimal>>()))
            .ThrowsAsync(new ArgumentException("Assessed by cannot be null or empty"));

        var actionResult = await sut.PerformRiskAssessment(new WorkflowRiskAssessmentRequest
        {
            ProspectId = "P-099",
            AssessedBy = string.Empty,
            RiskScores = new Dictionary<string, decimal> { ["Trap"] = 0.5m }
        });

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task OptimizePortfolioWorkflow_Delegates_ToPortfolioService_AndReturnsOk()
    {
        var sut = CreateSut(out _, out _, out _, out var portfolio);
        var ranked = new List<ProspectRanking>
        {
            new ProspectRanking { ProspectId = "P-100", Score = 80m, Rank = 1 }
        };
        var expected = new PortfolioOptimizationResult
        {
            OptimizationStrategy = "Risk-adjusted return maximization"
        };

        portfolio.Setup(p => p.OptimizePortfolioAsync(ranked, 0.6m, 2_000_000m))
            .ReturnsAsync(expected);

        var actionResult = await sut.OptimizePortfolioWorkflow(new PortfolioOptimizationWorkflowRequest
        {
            RankedProspects = ranked,
            RiskTolerance = 0.6m,
            CapitalBudget = 2_000_000m
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        portfolio.Verify(p => p.OptimizePortfolioAsync(ranked, 0.6m, 2_000_000m), Times.Once);
    }

    [Fact]
    public async Task AnalyzeTrapGeometry_Delegates_ToTechnicalMaturation_AndReturnsOk()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var expected = new TrapGeometryAnalysis { ProspectId = "P-010", TrapType = "Anticline" };
        technical.Setup(t => t.AnalyzeTrapGeometryAsync("P-010", "Anticline", 1200m, 1450m, 30m, 4m))
            .ReturnsAsync(expected);

        var actionResult = await sut.AnalyzeTrapGeometry(new TrapGeometryAnalysisRequest
        {
            ProspectId = "P-010",
            TrapType = "Anticline",
            CrestDepth = 1200m,
            SpillPointDepth = 1450m,
            Area = 30m,
            Volume = 4m
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        technical.Verify(t => t.AnalyzeTrapGeometryAsync("P-010", "Anticline", 1200m, 1450m, 30m, 4m), Times.Once);
    }

    [Fact]
    public async Task AnalyzeMigrationPath_Delegates_ToTechnicalMaturation_AndReturnsOk()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var expected = new MigrationPathAnalysis { ProspectId = "P-011", SourceRockId = "SRC-01" };
        technical.Setup(t => t.AnalyzeMigrationPathAsync("P-011", "SRC-01", 0.7m, 45m))
            .ReturnsAsync(expected);

        var actionResult = await sut.AnalyzeMigrationPath(new MigrationPathAnalysisRequest
        {
            ProspectId = "P-011",
            SourceRockId = "SRC-01",
            MaturityLevel = 0.7m,
            DistanceKm = 45m
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        technical.Verify(t => t.AnalyzeMigrationPathAsync("P-011", "SRC-01", 0.7m, 45m), Times.Once);
    }

    [Fact]
    public async Task AssessSealAndSource_Delegates_ToTechnicalMaturation_AndReturnsOk()
    {
        var sut = CreateSut(out _, out var technical, out _, out _);
        var expected = new SealSourceAssessment { ProspectId = "P-012", SealRockType = "Shale" };
        technical.Setup(t => t.AssessSealAndSourceAsync("P-012", "Shale", 80m, "SourceX", 0.65m))
            .ReturnsAsync(expected);

        var actionResult = await sut.AssessSealAndSource(new SealSourceAssessmentRequest
        {
            ProspectId = "P-012",
            SealRockType = "Shale",
            SealThickness = 80m,
            SourceRockType = "SourceX",
            SourceMaturity = 0.65m
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        technical.Verify(t => t.AssessSealAndSourceAsync("P-012", "Shale", 80m, "SourceX", 0.65m), Times.Once);
    }

    [Fact]
    public async Task AnalyzeEconomicViability_Delegates_ToRiskEconomic_AndReturnsOk()
    {
        var sut = CreateSut(out _, out _, out var riskEconomic, out _);
        var expected = new EconomicViabilityAnalysis { ProspectId = "P-020", ViabilityStatus = "Viable" };
        riskEconomic.Setup(r => r.AnalyzeEconomicViabilityAsync("P-020", 50m, 20m, 100m, 30m, 70m, 3m))
            .ReturnsAsync(expected);

        var actionResult = await sut.AnalyzeEconomicViability(new EconomicViabilityRequest
        {
            ProspectId = "P-020",
            EstimatedOil = 50m,
            EstimatedGas = 20m,
            CapitalCost = 100m,
            OperatingCost = 30m,
            OilPrice = 70m,
            GasPrice = 3m
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, ok.Value);
        riskEconomic.Verify(r => r.AnalyzeEconomicViabilityAsync("P-020", 50m, 20m, 100m, 30m, 70m, 3m), Times.Once);
    }

    [Fact]
    public async Task AnalyzeEconomicViability_WhenServiceThrowsUnexpectedException_Returns500()
    {
        var sut = CreateSut(out _, out _, out var riskEconomic, out _);
        riskEconomic.Setup(r => r.AnalyzeEconomicViabilityAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ThrowsAsync(new InvalidOperationException("Simulated downstream failure"));

        var actionResult = await sut.AnalyzeEconomicViability(new EconomicViabilityRequest
        {
            ProspectId = "P-021",
            EstimatedOil = 1m,
            EstimatedGas = 1m,
            CapitalCost = 1m,
            OperatingCost = 1m,
            OilPrice = 1m,
            GasPrice = 1m
        });

        var status = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, status.StatusCode);
    }
}
