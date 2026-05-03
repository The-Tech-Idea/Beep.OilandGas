using Beep.OilandGas.ApiService.Controllers.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
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

    [Fact]
    public void CalculateNPV_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        service.Setup(s => s.CalculateNPV(It.IsAny<CashFlow[]>(), 0.1))
            .Throws(new ArgumentException("Cash flows cannot be empty"));
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(new CalculateNPVRequest
        {
            CashFlows = new(),
            DISCOUNT_RATE = 0.1
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
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
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = await controller.SaveResult(request, "u-1");

        Assert.IsType<OkObjectResult>(result);
        service.VerifyAll();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenResultMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new EconomicAnalysisController(service.Object, NullLogger<EconomicAnalysisController>.Instance);

        var result = await controller.SaveResult(new SaveAnalysisResultRequest
        {
            AnalysisId = "EA-2",
            Result = null
        }, "u-1");

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
}
