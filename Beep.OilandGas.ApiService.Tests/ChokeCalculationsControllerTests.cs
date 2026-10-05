using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
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

    /// <summary>
    /// OILGAS-CATCH-01: the calculation's refusal reaches the API's handler as itself (answered 400 with its sentence); the
    /// controller no longer answers an <see cref="ArgumentException"/> — which the framework throws too — as a refusal.
    /// </summary>
    [Fact]
    public async Task PerformChokeAnalysis_PassesTheCalculationsRefusalThrough()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        var refusal = RefusalException.Invalid("bad");
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(refusal);

        var controller = CreateController(calc.Object);

        Assert.Same(refusal, await Refusals.RefusedAsync(RefusalKind.Invalid,
            () => controller.PerformChokeAnalysis(new ChokeAnalysisRequest())));
    }

    [Fact]
    public async Task PerformChokeAnalysis_LetsAFrameworkArgumentExceptionFailAsItself()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformChokeAnalysisAsync(It.IsAny<ChokeAnalysisRequest>()))
            .ThrowsAsync(new ArgumentException("bad"));

        var controller = CreateController(calc.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.PerformChokeAnalysis(new ChokeAnalysisRequest()));
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
