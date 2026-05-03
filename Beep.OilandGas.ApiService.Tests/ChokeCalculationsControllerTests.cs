using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Regression tests for <c>POST /api/Calculations/choke</c>.
/// </summary>
public class ChokeCalculationsControllerTests
{
    [Fact]
    public async Task PerformChokeAnalysis_ReturnsBadRequest_WhenBodyMissing()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var actionResult = await controller.PerformChokeAnalysis(request: null!, userId: null);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        calc.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformChokeAnalysis_ReturnsOk_WhenServiceSucceeds()
    {
        var expected = new ChokeAnalysisResult
        {
            CalculationId = "CHK-1",
            AnalysisType = "DOWNHOLE",
            CalculationDate = DateTime.UtcNow
        };

        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ReturnsAsync(expected);

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new ChokeAnalysisRequest { AnalysisType = "DOWNHOLE" };
        var actionResult = await controller.PerformChokeAnalysis(request, userId: null);

        var ok = Assert.IsType<ActionResult<ChokeAnalysisResult>>(actionResult);
        var okResult = Assert.IsType<OkObjectResult>(ok.Result);
        var body = Assert.IsType<ChokeAnalysisResult>(okResult.Value);
        Assert.Equal("CHK-1", body.CalculationId);
        calc.Verify(s => s.PerformChokeAnalysisAsync(It.Is<ChokeAnalysisRequest>(r => r.AnalysisType == "DOWNHOLE")), Times.Once);
    }

    [Fact]
    public async Task PerformChokeAnalysis_AppliesQueryUserId_WhenProvided()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        ChokeAnalysisRequest? captured = null;
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .Callback<ChokeAnalysisRequest>(r => captured = r)
            .ReturnsAsync(new ChokeAnalysisResult());

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new ChokeAnalysisRequest();
        await controller.PerformChokeAnalysis(request, userId: "user-42");

        Assert.NotNull(captured);
        Assert.Equal("user-42", captured!.UserId);
    }

    [Fact]
    public async Task PerformChokeAnalysis_ReturnsBadRequest_OnArgumentException()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(new ArgumentException("bad"));

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var actionResult = await controller.PerformChokeAnalysis(new ChokeAnalysisRequest(), userId: null);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task PerformChokeAnalysis_Rethrows_WhenCanceled()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(new OperationCanceledException());

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            controller.PerformChokeAnalysis(new ChokeAnalysisRequest(), userId: null));
        calc.VerifyAll();
    }
}
