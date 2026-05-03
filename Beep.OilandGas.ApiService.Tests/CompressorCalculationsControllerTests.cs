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
/// Regression tests for <c>POST /api/Calculations/compressor</c>.
/// </summary>
public class CompressorCalculationsControllerTests
{
    [Fact]
    public async Task PerformCompressorAnalysis_ReturnsBadRequest_WhenBodyMissing()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var actionResult = await controller.PerformCompressorAnalysis(request: null!, userId: null);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        calc.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformCompressorAnalysis_ReturnsOk_WhenServiceReturnsResult()
    {
        var expected = new CompressorAnalysisResult
        {
            CalculationId = "CMP-1",
            FacilityId = "FAC-1",
            Status = CalculationRunStatus.Success,
            CalculationDate = DateTime.UtcNow
        };

        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformCompressorAnalysisAsync(It.IsAny<CompressorAnalysisRequest>()))
            .ReturnsAsync(expected);

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new CompressorAnalysisRequest { FacilityId = "FAC-1" };
        var actionResult = await controller.PerformCompressorAnalysis(request, userId: null);

        var wrapped = Assert.IsType<ActionResult<CompressorAnalysisResult>>(actionResult);
        var okResult = Assert.IsType<OkObjectResult>(wrapped.Result);
        var body = Assert.IsType<CompressorAnalysisResult>(okResult.Value);
        Assert.Equal("CMP-1", body.CalculationId);
        calc.Verify(s => s.PerformCompressorAnalysisAsync(It.Is<CompressorAnalysisRequest>(r => r.FacilityId == "FAC-1")), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysis_AppliesQueryUserId_WhenProvided()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        CompressorAnalysisRequest? captured = null;
        calc.Setup(s => s.PerformCompressorAnalysisAsync(It.IsAny<CompressorAnalysisRequest>()))
            .Callback<CompressorAnalysisRequest>(r => captured = r)
            .ReturnsAsync(new CompressorAnalysisResult());

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new CompressorAnalysisRequest();
        await controller.PerformCompressorAnalysis(request, userId: "user-99");

        Assert.NotNull(captured);
        Assert.Equal("user-99", captured.UserId);
    }

    [Fact]
    public async Task PerformCompressorAnalysis_RethrowsOperationCanceledException()
    {
        var calc = new Mock<ICalculationService>();
        calc.Setup(s => s.PerformCompressorAnalysisAsync(It.IsAny<CompressorAnalysisRequest>()))
            .ThrowsAsync(new OperationCanceledException());

        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new CompressorAnalysisRequest { FacilityId = "FAC-1" };

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.PerformCompressorAnalysis(request, userId: null));
    }
}
