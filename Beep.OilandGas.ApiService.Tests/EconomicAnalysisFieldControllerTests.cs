using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using FieldEconomicAnalysisController = Beep.OilandGas.ApiService.Controllers.Field.EconomicAnalysisController;

namespace Beep.OilandGas.ApiService.Tests;

public class EconomicAnalysisFieldControllerTests
{
    [Fact]
    public void CalculateNPV_ReturnsBadRequest_WhenPayloadMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(null!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void CalculateNPV_ReturnsOk_WithCalculatedValue()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        service.Setup(s => s.CalculateNPV(It.IsAny<CashFlow[]>(), 0.1)).Returns(77.7);
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(new CalculateNPVRequest
        {
            CashFlows = new() { -1000, 900, 400 },
            DISCOUNT_RATE = 0.1
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(77.7, Assert.IsType<double>(ok.Value));
        service.VerifyAll();
    }

    [Fact]
    public void CalculateNPV_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        service.Setup(s => s.CalculateNPV(It.IsAny<CashFlow[]>(), 0.1))
            .Throws(new ArgumentException("Discount rate is invalid"));
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = controller.CalculateNPV(new CalculateNPVRequest
        {
            CashFlows = new(),
            DISCOUNT_RATE = 0.1
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetResult_ReturnsBadRequest_WhenIdMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = await controller.GetResult(string.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenAnalysisIdMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = await controller.SaveResult(new SaveAnalysisResultRequest
        {
            AnalysisId = "",
            Result = new EconomicResult()
        }, "u-1");

        Assert.IsType<BadRequestObjectResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveResult_ReturnsBadRequest_WhenResultMissing()
    {
        var service = new Mock<IEconomicAnalysisService>(MockBehavior.Strict);
        var controller = new FieldEconomicAnalysisController(service.Object, NullLogger<FieldEconomicAnalysisController>.Instance);

        var result = await controller.SaveResult(new SaveAnalysisResultRequest
        {
            AnalysisId = "EA-9",
            Result = null
        }, "u-1");

        Assert.IsType<BadRequestObjectResult>(result);
        service.VerifyNoOtherCalls();
    }
}
