using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.AspNetCore.Http;
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
        var controller = SignedIn(new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance));

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
    public async Task TransitionAsync_LeavesAnUnexpectedFailureToTheApiHandler()
    {
        const string incidentId = "INC-MISSING";
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.Setup(s => s.GetHSEService()).Returns(hse.Object);

        var controller = SignedIn(new HSEController(orchestrator.Object, NullLogger<HSEController>.Instance));
        var request = new TransitionIncidentRequest { Trigger = "investigate", Reason = null };

        // OILGAS-CATCH-01: the controller no longer answers a failure itself; it reaches the API's exception handler,
        // which reports it and answers 500 with its reference and never its text (ExceptionAnswerTests).
        await Assert.ThrowsAsync<MockException>(() => controller.TransitionAsync(incidentId, request));
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

    private static HSEController SignedIn(HSEController controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "hse-user")], "TestAuth"))
            }
        };
        return controller;
    }
}
