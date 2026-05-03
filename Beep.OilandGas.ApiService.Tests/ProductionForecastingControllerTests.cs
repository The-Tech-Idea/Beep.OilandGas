using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using GenerateForecastRequest = Beep.OilandGas.Models.Data.ProductionForecasting.GenerateForecastRequest;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionForecastingControllerTests
{
    [Fact]
    public async Task GenerateForecast_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.GenerateForecast(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GenerateForecast_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        var expected = new ProductionForecastResult { ForecastId = "F-1", WellUWI = "UWI-1" };
        core.Setup(s => s.GenerateForecastAsync(It.Is<GenerateForecastRequest>(r =>
            r.WellUWI == "UWI-1" &&
            r.ForecastMethod == ForecastType.Hyperbolic &&
            r.ForecastPeriod == 12)))
            .ReturnsAsync(expected);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.GenerateForecast(new GenerateForecastRequest
        {
            WellUWI = "UWI-1",
            ForecastMethod = ForecastType.Hyperbolic,
            ForecastPeriod = 12
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task GenerateForecast_ForwardsExtendedRequestParameters()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.GenerateForecastAsync(It.Is<GenerateForecastRequest>(r =>
            r.WellUWI == "UWI-22" &&
            r.ForecastMethod == ForecastType.Hyperbolic &&
            r.ForecastPeriod == 24 &&
            r.InitialOilRateQi == 1500m &&
            r.InitialDeclineDi == 0.02m &&
            r.DeclineExponentB == 0.65m &&
            r.EconomicLimitOilRate == 25m &&
            r.TerminalDeclineDi == 0.0002m &&
            r.UseModifiedHyperbolic &&
            r.SkipHistoryFit)))
            .ReturnsAsync(new ProductionForecastResult { ForecastId = "F-22", WellUWI = "UWI-22" });

        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);
        var request = new GenerateForecastRequest
        {
            WellUWI = "UWI-22",
            ForecastMethod = ForecastType.Hyperbolic,
            ForecastPeriod = 24,
            InitialOilRateQi = 1500m,
            InitialDeclineDi = 0.02m,
            DeclineExponentB = 0.65m,
            EconomicLimitOilRate = 25m,
            TerminalDeclineDi = 0.0002m,
            UseModifiedHyperbolic = true,
            SkipHistoryFit = true
        };

        var result = await controller.GenerateForecast(request);
        Assert.IsType<OkObjectResult>(result.Result);
        core.VerifyAll();
    }

    [Fact]
    public async Task GenerateForecast_ReturnsBadRequest_WhenArgumentException()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.GenerateForecastAsync(It.Is<GenerateForecastRequest>(r =>
            r.WellUWI == "UWI-1" &&
            r.ForecastMethod == ForecastType.None &&
            r.ForecastPeriod == 12)))
            .ThrowsAsync(new ArgumentException("bad"));
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.GenerateForecast(new GenerateForecastRequest
        {
            WellUWI = "UWI-1",
            ForecastMethod = ForecastType.None,
            ForecastPeriod = 12
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyAll();
    }

    [Fact]
    public async Task GenerateForecast_Returns500_WhenUnexpectedException()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.GenerateForecastAsync(It.Is<GenerateForecastRequest>(r =>
            r.WellUWI == "UWI-1" &&
            r.ForecastMethod == ForecastType.Hyperbolic &&
            r.ForecastPeriod == 12)))
            .ThrowsAsync(new InvalidOperationException("db"));
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.GenerateForecast(new GenerateForecastRequest
        {
            WellUWI = "UWI-1",
            ForecastMethod = ForecastType.Hyperbolic,
            ForecastPeriod = 12
        });

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
        core.VerifyAll();
    }

    [Fact]
    public async Task GenerateForecast_Rethrows_WhenCanceled()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.GenerateForecastAsync(It.Is<GenerateForecastRequest>(r =>
            r.WellUWI == "UWI-1" &&
            r.ForecastMethod == ForecastType.Hyperbolic &&
            r.ForecastPeriod == 12)))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GenerateForecast(new GenerateForecastRequest
        {
            WellUWI = "UWI-1",
            ForecastMethod = ForecastType.Hyperbolic,
            ForecastPeriod = 12
        }));
        core.VerifyAll();
    }

    [Fact]
    public async Task PerformDeclineCurveAnalysis_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.PerformDeclineCurveAnalysis(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PerformDeclineCurveAnalysis_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var expected = new DeclineCurveAnalysis { AnalysisId = "A-1", WellUWI = "UWI-1" };
        core.Setup(s => s.PerformDeclineCurveAnalysisAsync("UWI-1", start, end))
            .ReturnsAsync(expected);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.PerformDeclineCurveAnalysis(new DeclineCurveAnalysisRequest
        {
            WellUWI = "UWI-1",
            StartDate = start,
            EndDate = end
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        core.VerifyAll();
    }

    [Fact]
    public async Task SaveForecast_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.SaveForecast(null);

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveForecast_ReturnsOk_WhenServiceSucceeds()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.SaveForecastAsync(It.IsAny<ProductionForecastResult>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);
        var http = new DefaultHttpContext();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-42")],
            authenticationType: "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = http };

        var forecast = new ProductionForecastResult { ForecastId = "F-1" };
        var result = await controller.SaveForecast(forecast, userId: null);

        Assert.IsType<OkObjectResult>(result);
        core.Verify(s => s.SaveForecastAsync(forecast, "user-42"), Times.Once);
    }

    [Fact]
    public async Task SaveForecast_ReturnsBadRequest_WhenArgumentException()
    {
        var core = new Mock<IProductionForecastingService>(MockBehavior.Strict);
        core.Setup(s => s.SaveForecastAsync(It.IsAny<ProductionForecastResult>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("bad forecast"));
        var controller = new ProductionForecastingController(core.Object, NullLogger<ProductionForecastingController>.Instance);

        var result = await controller.SaveForecast(new ProductionForecastResult { ForecastId = "F-1" }, userId: "u1");

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyAll();
    }
}
