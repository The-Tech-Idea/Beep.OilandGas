using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class EconomicAnalysisControllerTests
{
    [Fact]
    public void CalculateNPV_ReturnsBadRequest_WhenPayloadMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(null!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void CalculateNPV_ReturnsOk_WithCalculatedValue()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        service.Setup(s => s.CalculateNPV(It.IsAny<CashFlow[]>(), 0.1)).Returns(123.45);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(new CalculateNPVRequest
        {
            CashFlows = new() { -1000, 700, 500 },
            DISCOUNT_RATE = 0.1
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(123.45, Assert.IsType<double>(ok.Value));
        service.VerifyAll();
    }

    // OILGAS-CATCH-01: the analysis's refusal reaches the API's handler as itself (answered 400 with its sentence).
    [Fact]
    public void CalculateNPV_PassesTheAnalysisRefusalThrough()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var refusal = RefusalException.Invalid("Cash flows cannot be empty");
        service.Setup(s => s.CalculateNPV(It.IsAny<CashFlow[]>(), 0.1)).Throws(refusal);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        Assert.Same(refusal, Refusals.Refused(RefusalKind.Invalid, () => controller.CalculateNPV(new CalculateNPVRequest
        {
            CashFlows = new(),
            DISCOUNT_RATE = 0.1
        })));
        service.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_ReturnsOk_WhenPersisted()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var request = new SaveAnalysisResultRequest
        {
            AnalysisId = "EA-1",
            Result = new EconomicResult { NPV = 1.0, IRR = 0.2, DiscountRate = 0.1 }
        };
        service.Setup(s => s.SaveAnalysisResultAsync("EA-1", request.Result!, "u-1")).Returns(Task.CompletedTask);
        var controller = SignedIn(new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance), "u-1");

        var result = await controller.SaveResult(request);

        Assert.IsType<OkObjectResult>(result);
        service.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenResultMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = SignedIn(new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance), "u-1");

        var result = await controller.SaveResult(new SaveAnalysisResultRequest
        {
            AnalysisId = "EA-2",
            Result = null
        });

        Assert.IsType<BadRequestObjectResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetResult_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        service.Setup(s => s.GetAnalysisResultAsync("missing")).ReturnsAsync((EconomicResult?)null);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = await controller.GetResult("missing");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetResult_ReturnsBadRequest_WhenIdMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = await controller.GetResult(string.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        service.VerifyNoOtherCalls();
    }

    private static EconomicAnalysisController SignedIn(EconomicAnalysisController controller, string userId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", userId)], "TestAuth"))
            }
        };
        return controller;
    }
}
