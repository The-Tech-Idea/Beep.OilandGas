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
/// Regression tests for <c>POST /api/Calculations/choke</c>.
/// </summary>
public class ChokeCalculationsControllerTests
{
    [Fact]
    public async Task PerformChokeAnalysis_ReturnsBadRequest_WhenBodyMissing()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        var controller = CreateController(calc.Object);

        var actionResult = await controller.PerformChokeAnalysis(request: null!);

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

        var controller = CreateController(calc.Object);

        var request = new ChokeAnalysisRequest { AnalysisType = "DOWNHOLE" };
        var actionResult = await controller.PerformChokeAnalysis(request);

        var ok = Assert.IsType<ActionResult<ChokeAnalysisResult>>(actionResult);
        var okResult = Assert.IsType<OkObjectResult>(ok.Result);
        var body = Assert.IsType<ChokeAnalysisResult>(okResult.Value);
        Assert.Equal("CHK-1", body.CalculationId);
        calc.Verify(s => s.PerformChokeAnalysisAsync(It.Is<ChokeAnalysisRequest>(r => r.AnalysisType == "DOWNHOLE")), Times.Once);
    }

    [Fact]
    public async Task PerformChokeAnalysis_RecordsTheSignedInAccount_NotTheUserIdTheBodyNames()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        ChokeAnalysisRequest? captured = null;
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .Callback<ChokeAnalysisRequest>(r => captured = r)
            .ReturnsAsync(new ChokeAnalysisResult());

        var controller = CreateController(calc.Object, "user-42");

        var request = new ChokeAnalysisRequest { UserId = "someone-else" };
        await controller.PerformChokeAnalysis(request);

        Assert.NotNull(captured);
        Assert.Equal("user-42", captured!.UserId);
    }

    [Fact]
    public async Task PerformChokeAnalysis_ReturnsBadRequest_OnArgumentException()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(new ArgumentException("bad"));

        var controller = CreateController(calc.Object);

        var actionResult = await controller.PerformChokeAnalysis(new ChokeAnalysisRequest());

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task PerformChokeAnalysis_Rethrows_WhenCanceled()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(new OperationCanceledException());

        var controller = CreateController(calc.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            controller.PerformChokeAnalysis(new ChokeAnalysisRequest()));
        calc.VerifyAll();
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
