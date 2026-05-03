using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Runtime.CompilerServices;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class DecommissioningControllerTests
{
    [Fact]
    public async Task GetAbandonedWells_ReturnsBadRequest_WhenNoActiveField()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns((string?)null);
        var process = CreateNoopProcessService();

        var controller = CreateController(orchestrator.Object, process.Object, "user-1");
        var result = await controller.GetAbandonedWells();

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AbandonWell_UsesClaimUser_WhenQueryUserMissing()
    {
        const string fieldId = "FIELD-D-1";
        const string wellId = "WELL-1";
        var expected = new WellAbandonmentResponse { WellId = wellId, Status = "PLANNED" };

        var decommissioning = new Mock<IFieldDecommissioningService>(MockBehavior.Strict);
        decommissioning.Setup(s => s.AbandonWellForFieldAsync(fieldId, wellId, It.IsAny<WellAbandonmentRequest>(), "claim-user"))
            .ReturnsAsync(expected);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);
        orchestrator.Setup(x => x.GetDecommissioningService()).Returns(decommissioning.Object);

        var process = CreateNoopProcessService();
        var controller = CreateController(orchestrator.Object, process.Object, "claim-user");

        var result = await controller.AbandonWell(wellId, new WellAbandonmentRequest(), null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<WellAbandonmentResponse>(ok.Value);
        decommissioning.VerifyAll();
        orchestrator.VerifyAll();
    }

    private static DecommissioningController CreateController(
        IFieldOrchestrator fieldOrchestrator,
        DecommissioningProcessService processService,
        string userId)
    {
        var controller = new DecommissioningController(
            fieldOrchestrator,
            processService,
            NullLogger<DecommissioningController>.Instance);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "TestAuth"))
            }
        };

        return controller;
    }

    private static Mock<DecommissioningProcessService> CreateNoopProcessService()
    {
        var processSvc = new Mock<IProcessService>(MockBehavior.Loose);
        var decommSvc = (PPDMDecommissioningService)RuntimeHelpers.GetUninitializedObject(typeof(PPDMDecommissioningService));
        return new Mock<DecommissioningProcessService>(MockBehavior.Loose, processSvc.Object, decommSvc, NullLogger<DecommissioningProcessService>.Instance);
    }
}
