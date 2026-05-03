using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HSEControllerTests
{
    [Fact]
    public async Task ReportAsync_ReturnsBadRequest_WhenBodyMissing()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        var controller = new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance);

        var result = await controller.ReportAsync(null!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        orchestrator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetIncidentAsync_ReturnsNotFound_WhenMissing()
    {
        const string incidentId = "INC-404";
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hse.Setup(s => s.GetIncidentAsync(incidentId)).ReturnsAsync((HSEIncidentRecord?)null);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.Setup(s => s.GetHSEService()).Returns(hse.Object);

        var controller = new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance);

        var result = await controller.GetIncidentAsync(incidentId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        hse.VerifyAll();
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task TransitionAsync_ReturnsServerError_WhenServiceThrowsUnexpected()
    {
        const string incidentId = "INC-MISSING";
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.Setup(s => s.GetHSEService()).Returns(hse.Object);

        var controller = new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance);
        var request = new TransitionIncidentRequest { Trigger = "investigate", Reason = null };

        var result = await controller.TransitionAsync(incidentId, request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task GetIncidentsAsync_ReturnsBadRequest_WhenCurrentFieldMissing()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(s => s.CurrentFieldId).Returns((string?)null);

        var controller = new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance);

        var result = await controller.GetIncidentsAsync(null, null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        orchestrator.VerifyAll();
    }
}
