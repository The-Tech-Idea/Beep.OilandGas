using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.AspNetCore.Http;
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
        var controller = CreateController(calc.Object);

        var actionResult = await controller.PerformCompressorAnalysis(request: null!);

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

        var controller = CreateController(calc.Object);

        var request = new CompressorAnalysisRequest { FacilityId = "FAC-1" };
        var actionResult = await controller.PerformCompressorAnalysis(request);

        var wrapped = Assert.IsType<ActionResult<CompressorAnalysisResult>>(actionResult);
        var okResult = Assert.IsType<OkObjectResult>(wrapped.Result);
        var body = Assert.IsType<CompressorAnalysisResult>(okResult.Value);
        Assert.Equal("CMP-1", body.CalculationId);
        calc.Verify(s => s.PerformCompressorAnalysisAsync(It.Is<CompressorAnalysisRequest>(r => r.FacilityId == "FAC-1")), Times.Once);
    }

    [Fact]
    public async Task PerformCompressorAnalysis_RecordsTheSignedInAccount_NotTheUserIdTheBodyNames()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        CompressorAnalysisRequest? captured = null;
        calc.Setup(s => s.PerformCompressorAnalysisAsync(It.IsAny<CompressorAnalysisRequest>()))
            .Callback<CompressorAnalysisRequest>(r => captured = r)
            .ReturnsAsync(new CompressorAnalysisResult());

        var controller = CreateController(calc.Object, "user-99");

        var request = new CompressorAnalysisRequest { UserId = "someone-else" };
        await controller.PerformCompressorAnalysis(request);

        Assert.NotNull(captured);
        Assert.Equal("user-99", captured.UserId);
    }

    [Fact]
    public async Task PerformCompressorAnalysis_RethrowsOperationCanceledException()
    {
        var calc = new Mock<ICalculationService>();
        calc.Setup(s => s.PerformCompressorAnalysisAsync(It.IsAny<CompressorAnalysisRequest>()))
            .ThrowsAsync(new OperationCanceledException());

        var controller = CreateController(calc.Object);

        var request = new CompressorAnalysisRequest { FacilityId = "FAC-1" };

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.PerformCompressorAnalysis(request));
    }

    private static CalculationsController CreateController(ICalculationService calculationService, string userId = "user-1") =>
        new(calculationService, fieldOrchestrator: null, progressTracking: null, NullLogger<CalculationsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", userId)], "TestAuth"))
                }
            }
        };
}
