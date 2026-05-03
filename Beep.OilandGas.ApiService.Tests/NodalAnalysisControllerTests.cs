using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class NodalAnalysisControllerTests
{
    [Fact]
    public async Task PerformAnalysis_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformAnalysis(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformAnalysis_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformAnalysis(new PerformNodalAnalysisRequest
        {
            WellUWI = "",
            AnalysisParameters = new NodalAnalysisParameters()
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformAnalysis_ReturnsBadRequest_WhenAnalysisParametersMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformAnalysis(new PerformNodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = null!
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformAnalysis_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new NodalAnalysisRunResult { AnalysisId = "N-1", WellUWI = "UWI-1" };
        var parameters = new NodalAnalysisParameters();
        core.Setup(s => s.PerformNodalAnalysisAsync("UWI-1", parameters)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformAnalysis(new PerformNodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = parameters
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task Optimize_ReturnsBadRequest_WhenOptimizationGoalsMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Optimize(new OptimizeSystemRequest
        {
            WellUWI = "UWI-1",
            OptimizationGoals = null!
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Optimize_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Optimize(new OptimizeSystemRequest
        {
            WellUWI = "  ",
            OptimizationGoals = new OptimizationGoals { OptimizationType = "MaximizeProduction" }
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Optimize_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var goals = new OptimizationGoals { OptimizationType = "MaximizeProduction" };
        var expected = new OptimizationResult { OptimizationId = "O-1", WellUWI = "UWI-1" };
        core.Setup(s => s.OptimizeSystemAsync("UWI-1", goals)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Optimize(new OptimizeSystemRequest
        {
            WellUWI = "UWI-1",
            OptimizationGoals = goals
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.SaveResult(new NodalAnalysisRunResult { WellUWI = "  ", AnalysisId = "A-1" }, userId: "user-1");

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveResult_ReturnsOk_WhenExplicitUserIdProvided()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var payload = new NodalAnalysisRunResult { AnalysisId = "A-42", WellUWI = "UWI-1" };
        core.Setup(s => s.SaveAnalysisResultAsync(
                It.Is<NodalAnalysisRunResult>(r => r.AnalysisId == "A-42" && r.WellUWI == "UWI-1"),
                "user-1"))
            .Returns(Task.CompletedTask);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.SaveResult(payload, userId: "user-1");

        Assert.IsType<OkObjectResult>(result);
        core.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var payload = new NodalAnalysisRunResult { AnalysisId = "A-1", WellUWI = "UWI-1" };
        core.Setup(s => s.SaveAnalysisResultAsync(It.IsAny<NodalAnalysisRunResult>(), "user-1"))
            .ThrowsAsync(new ArgumentException("Persist rejected for test."));
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.SaveResult(payload, userId: "user-1");

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_Rethrows_WhenCanceled()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var payload = new NodalAnalysisRunResult { AnalysisId = "A-1", WellUWI = "UWI-1" };
        core.Setup(s => s.SaveAnalysisResultAsync(It.IsAny<NodalAnalysisRunResult>(), "user-1"))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            controller.SaveResult(payload, userId: "user-1"));

        core.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_UsesUserFromClaims_WhenUserIdMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        core.Setup(s => s.SaveAnalysisResultAsync(It.IsAny<NodalAnalysisRunResult>(), "user-99"))
            .Returns(Task.CompletedTask);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);
        var http = new DefaultHttpContext();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-99")],
            authenticationType: "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        var payload = new NodalAnalysisRunResult { AnalysisId = "A-1", WellUWI = "UWI-1" };

        var result = await controller.SaveResult(payload, userId: null);

        Assert.IsType<OkObjectResult>(result);
        core.VerifyAll();
    }

    [Fact]
    public async Task GetHistory_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.GetHistory("");

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistory_ReturnsBadRequest_WhenWellUwiWhitespace()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.GetHistory("   ");

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistory_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new List<NodalAnalysisRunResult> { new() { AnalysisId = "A-1", WellUWI = "UWI-1" } };
        core.Setup(s => s.GetAnalysisHistoryAsync("UWI-1")).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.GetHistory("UWI-1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task PerformanceMatching_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformanceMatching(new PerformNodalAnalysisRequest
        {
            WellUWI = "",
            AnalysisParameters = new NodalAnalysisParameters()
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformanceMatching_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var parameters = new NodalAnalysisParameters();
        var expected = new PerformanceMatchingAnalysis { WellUWI = "UWI-1", AnalysisId = "P-1" };
        core.Setup(s => s.AnalyzePerformanceMatchingAsync("UWI-1", parameters)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformanceMatching(new PerformNodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = parameters
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task ProductionForecast_ReturnsBadRequest_WhenForecastMonthsInvalid()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ProductionForecast(new NodalProductionForecastRequest
        {
            WellUWI = "UWI-1",
            CurrentProduction = 100m,
            DeclineRate = 0.1m,
            ForecastMonths = 0
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProductionForecast_ReturnsBadRequest_WhenDeclineRateInvalid()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ProductionForecast(new NodalProductionForecastRequest
        {
            WellUWI = "UWI-1",
            CurrentProduction = 100m,
            DeclineRate = 1.5m,
            ForecastMonths = 12
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sensitivity_ReturnsBadRequest_WhenBaselineParametersMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Sensitivity(new NodalSensitivityAnalysisRequest
        {
            WellUWI = "UWI-1",
            BaselineParameters = null,
            ParametersToVary = null
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ArtificialLift_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ArtificialLift(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ArtificialLift_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ArtificialLift(new NodalArtificialLiftRequest
        {
            WellUWI = "  ",
            CurrentProduction = 100m,
            TargetProduction = 200m,
            WellDepth = 9000m,
            WaterCut = 0.3m
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ArtificialLift_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new ArtificialLiftRecommendation { WellUWI = "UWI-1", RecommendationId = "AL-1" };
        core.Setup(s => s.RecommendArtificialLiftAsync("UWI-1", 100m, 200m, 9000m, 0.3m)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ArtificialLift(new NodalArtificialLiftRequest
        {
            WellUWI = "UWI-1",
            CurrentProduction = 100m,
            TargetProduction = 200m,
            WellDepth = 9000m,
            WaterCut = 0.3m
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task Diagnostics_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Diagnostics(new NodalWellDiagnosticsRequest
        {
            WellUWI = "",
            ExpectedProduction = 1000m,
            ActualProduction = 700m,
            WellheadPressure = 100m,
            BottomholePressure = 200m
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PressureMaintenance_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PressureMaintenance(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PressureMaintenance_ReturnsBadRequest_WhenProductivityIndexNegative()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PressureMaintenance(new NodalPressureMaintenanceRequest
        {
            WellUWI = "UWI-1",
            CurrentReservoirPressure = 2400m,
            BubblePointPressure = 2200m,
            ProductivityIndex = -1m
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PressureMaintenance_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PressureMaintenance(new NodalPressureMaintenanceRequest
        {
            WellUWI = "  ",
            CurrentReservoirPressure = 2400m,
            BubblePointPressure = 2200m,
            ProductivityIndex = 2.5m
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformanceMatching_ReturnsBadRequest_WhenAnalysisParametersMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformanceMatching(new PerformNodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = null!
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Optimize_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Optimize(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.SaveResult(null, userId: "user-1");

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProductionForecast_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ProductionForecast(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProductionForecast_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ProductionForecast(new NodalProductionForecastRequest
        {
            WellUWI = "",
            CurrentProduction = 100m,
            DeclineRate = 0.1m,
            ForecastMonths = 12
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProductionForecast_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new PRODUCTION_FORECAST { WELL_UWI = "UWI-1", FORECAST_ID = "F-1" };
        core.Setup(s => s.ForecastProductionAsync("UWI-1", 500m, 0.12m, 12)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.ProductionForecast(new NodalProductionForecastRequest
        {
            WellUWI = "UWI-1",
            CurrentProduction = 500m,
            DeclineRate = 0.12m,
            ForecastMonths = 12
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task PerformanceMatching_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PerformanceMatching(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sensitivity_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Sensitivity(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sensitivity_ReturnsBadRequest_WhenWellUwiMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Sensitivity(new NodalSensitivityAnalysisRequest
        {
            WellUWI = "",
            BaselineParameters = new NodalAnalysisParameters(),
            ParametersToVary = new List<string>()
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sensitivity_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var baseline = new NodalAnalysisParameters();
        var expected = new EconomicSensitivityAnalysisResult { AnalysisId = "S-1", WellUWI = "UWI-1" };
        core.Setup(s => s.PerformSensitivityAnalysisAsync("UWI-1", baseline, It.IsAny<List<string>>())).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Sensitivity(new NodalSensitivityAnalysisRequest
        {
            WellUWI = "UWI-1",
            BaselineParameters = baseline,
            ParametersToVary = new List<string> { "WellheadPressure" }
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task Diagnostics_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Diagnostics(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Diagnostics_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new WellDiagnosticsResult { DiagnosisId = "D-1", WellUWI = "UWI-1" };
        core.Setup(s => s.DiagnoseWellPerformanceAsync("UWI-1", 1000m, 700m, 260m, 190m)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.Diagnostics(new NodalWellDiagnosticsRequest
        {
            WellUWI = "UWI-1",
            ExpectedProduction = 1000m,
            ActualProduction = 700m,
            WellheadPressure = 260m,
            BottomholePressure = 190m
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task PressureMaintenance_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var expected = new PressureMaintenanceStrategy { StrategyId = "PM-1", WellUWI = "UWI-1" };
        core.Setup(s => s.AnalyzePressureMaintenanceAsync("UWI-1", 2400m, 2200m, 2.5m)).ReturnsAsync(expected);
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        var result = await controller.PressureMaintenance(new NodalPressureMaintenanceRequest
        {
            WellUWI = "UWI-1",
            CurrentReservoirPressure = 2400m,
            BubblePointPressure = 2200m,
            ProductivityIndex = 2.5m
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task GetHistory_Rethrows_WhenCanceled()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        core.Setup(s => s.GetAnalysisHistoryAsync("UWI-1"))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GetHistory("UWI-1"));
        core.VerifyAll();
    }

    [Fact]
    public async Task Optimize_Rethrows_WhenCanceled()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var goals = new OptimizationGoals { OptimizationType = "MaximizeProduction" };
        core.Setup(s => s.OptimizeSystemAsync("UWI-1", goals))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.Optimize(new OptimizeSystemRequest
        {
            WellUWI = "UWI-1",
            OptimizationGoals = goals
        }));
        core.VerifyAll();
    }

    [Fact]
    public async Task PerformAnalysis_Rethrows_WhenCanceled()
    {
        var core = new Mock<INodalAnalysisService>(MockBehavior.Strict);
        var parameters = new NodalAnalysisParameters();
        core.Setup(s => s.PerformNodalAnalysisAsync("UWI-1", parameters))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new NodalAnalysisController(core.Object, NullLogger<NodalAnalysisController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.PerformAnalysis(new PerformNodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = parameters
        }));
        core.VerifyAll();
    }
}
