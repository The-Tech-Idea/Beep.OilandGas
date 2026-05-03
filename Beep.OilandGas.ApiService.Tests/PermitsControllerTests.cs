using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Permits;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class PermitsControllerTests
{
    [Fact]
    public async Task CreateAsync_UsesCurrentFieldAndClaimUser()
    {
        var lifecycle = new Mock<IPermitApplicationLifecycleService>(MockBehavior.Strict);
        var workflow = new Mock<IPermitApplicationWorkflowService>(MockBehavior.Strict);
        var compliance = new Mock<IPermitComplianceCheckService>(MockBehavior.Strict);
        var history = new Mock<IPermitStatusHistoryService>(MockBehavior.Strict);
        var field = new Mock<IFieldOrchestrator>(MockBehavior.Strict);

        field.SetupGet(x => x.CurrentFieldId).Returns("FIELD-100");
        lifecycle.Setup(x => x.CreateAsync(It.IsAny<PERMIT_APPLICATION>(), "permits-user"))
            .ReturnsAsync((PERMIT_APPLICATION app, string _) =>
            {
                app.PERMIT_APPLICATION_ID = "PERMIT-100";
                return app;
            });

        var controller = CreateController(lifecycle, workflow, compliance, history, field, "permits-user");
        var result = await controller.CreateAsync(new CreatePermitApplicationRequest
        {
            ApplicationType = "Drilling",
            RegulatoryAuthority = "BSEE",
            ApplicantName = "Operator A",
            Description = "Test request"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("PERMIT-100", created.Value);
        lifecycle.VerifyAll();
        field.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenMissing()
    {
        var lifecycle = new Mock<IPermitApplicationLifecycleService>(MockBehavior.Strict);
        var workflow = new Mock<IPermitApplicationWorkflowService>(MockBehavior.Strict);
        var compliance = new Mock<IPermitComplianceCheckService>(MockBehavior.Strict);
        var history = new Mock<IPermitStatusHistoryService>(MockBehavior.Strict);
        var field = new Mock<IFieldOrchestrator>(MockBehavior.Strict);

        lifecycle.Setup(x => x.GetByIdAsync("missing-id")).ReturnsAsync((PERMIT_APPLICATION?)null);

        var controller = CreateController(lifecycle, workflow, compliance, history, field, "permits-user");
        var result = await controller.GetByIdAsync("missing-id");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        lifecycle.VerifyAll();
    }

    [Fact]
    public async Task SubmitAsync_UsesClaimUserId()
    {
        var lifecycle = new Mock<IPermitApplicationLifecycleService>(MockBehavior.Strict);
        var workflow = new Mock<IPermitApplicationWorkflowService>(MockBehavior.Strict);
        var compliance = new Mock<IPermitComplianceCheckService>(MockBehavior.Strict);
        var history = new Mock<IPermitStatusHistoryService>(MockBehavior.Strict);
        var field = new Mock<IFieldOrchestrator>(MockBehavior.Strict);

        lifecycle.Setup(x => x.GetByIdAsync("PERMIT-200"))
            .ReturnsAsync(new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "PERMIT-200" });
        lifecycle.Setup(x => x.SubmitAsync("PERMIT-200", "review-user"))
            .ReturnsAsync(new PERMIT_APPLICATION
            {
                PERMIT_APPLICATION_ID = "PERMIT-200",
                STATUS = PermitApplicationStatus.Submitted
            });

        var controller = CreateController(lifecycle, workflow, compliance, history, field, "review-user");
        var result = await controller.SubmitAsync("PERMIT-200");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        lifecycle.VerifyAll();
    }

    private static PermitsController CreateController(
        Mock<IPermitApplicationLifecycleService> lifecycle,
        Mock<IPermitApplicationWorkflowService> workflow,
        Mock<IPermitComplianceCheckService> compliance,
        Mock<IPermitStatusHistoryService> history,
        Mock<IFieldOrchestrator> field,
        string userId)
    {
        var controller = new PermitsController(
            lifecycle.Object,
            workflow.Object,
            compliance.Object,
            history.Object,
            field.Object);

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
}
