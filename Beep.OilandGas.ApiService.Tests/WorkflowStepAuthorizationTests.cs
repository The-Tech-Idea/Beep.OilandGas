using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowStepAuthorizationTests
{
    [Theory]
    [InlineData("Reader", null, "Reviewer", "field", "step", false)]
    [InlineData("Reviewer", "other-user", "Reviewer", "field", "step", false)]
    [InlineData("Reviewer", "actor", "Reviewer", "other-field", "step", false)]
    [InlineData("Reviewer", "actor", "Reviewer", "field", "other-step", false)]
    [InlineData("Reviewer", "actor", "Reviewer", "field", "step", true)]
    [InlineData("Reviewer", "Reviewer", "Reviewer", "field", "step", true)]
    [InlineData("Administrator", null, "Reviewer", "other-field", "step", false)]
    public async Task UpdatesRequireFieldCurrentStepAndAssignment(string role, string? assignee,
        string requiredRole, string instanceField, string currentStep, bool allowed)
    {
        var process = new Mock<IProcessService>();
        process.Setup(x => x.GetProcessInstanceAsync("instance")).ReturnsAsync(new ProcessInstance
        {
            InstanceId = "instance", ProcessId = "definition", FieldId = instanceField, CurrentStepId = currentStep,
            StepInstances = new() { new() { StepId = "step", AssignedTo = assignee, RequiredRole = requiredRole } }
        });
        process.Setup(x => x.GetProcessDefinitionAsync("definition")).ReturnsAsync(new ProcessDefinition
        {
            Steps = new() { new() { StepId = "step", RequiredRoles = new() { "Reviewer" } } }
        });
        var data = new PROCESS_STEP_DATA();
        process.Setup(x => x.ExecuteStepAsync("instance", "step", data, "actor")).ReturnsAsync(true);
        var resolutions = 0;
        var binding = new BoundProcessService(() => Task.FromResult(++resolutions == 1 ? "selected" : "wrong"), target =>
        {
            Assert.Equal("selected", target);
            return process.Object;
        });
        var field = new Mock<IFieldOrchestrator>();
        field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var controller = new BusinessProcessController(field.Object, binding, NullLogger<BusinessProcessController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "actor"),
                    new Claim(ClaimTypes.Role, role) }, "Test"))
            } }
        };
        var result = await controller.UpdateStepAsync("instance", "step", data);
        if (allowed) Assert.IsType<NoContentResult>(result);
        else Assert.IsType<ForbidResult>(result);
        Assert.Equal(1, resolutions);
        process.Verify(x => x.ExecuteStepAsync("instance", "step", data, "actor"), allowed ? Times.Once() : Times.Never());
        resolutions = 0;
        process.Setup(x => x.CanTransitionAsync("instance", "target")).ReturnsAsync(true);
        process.Setup(x => x.TransitionStateAsync("instance", "target", "actor")).ReturnsAsync(true);
        var transition = await controller.ExecuteTransitionAsync("instance", new ProcessTransitionRequest { Trigger = "advance", ToStateId = "target" });
        if (allowed) Assert.IsType<OkObjectResult>(transition.Result);
        else Assert.IsType<ForbidResult>(transition.Result);
        Assert.Equal(1, resolutions);
        process.Verify(x => x.CanTransitionAsync("instance", "target"), allowed ? Times.Once() : Times.Never());
        process.Verify(x => x.TransitionStateAsync("instance", "target", "actor"), allowed ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData("Reader", "actor", "field", true)]
    [InlineData("Reader", "other", "field", false)]
    [InlineData("Administrator", "other", "field", true)]
    [InlineData("Administrator", "other", "foreign", false)]
    public async Task CloseRequiresCreatorOrAdministratorWithinTheSelectedField(string role, string creator, string instanceField, bool allowed)
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(x => x.GetProcessInstanceAsync("instance")).ReturnsAsync(new ProcessInstance
        {
            InstanceId = "instance", FieldId = instanceField, StartedBy = creator
        });
        if (allowed) process.Setup(x => x.CancelProcessAsync("instance", "reason", "actor")).ReturnsAsync(true);
        var resolutions = 0;
        var binding = new BoundProcessService(() => Task.FromResult(++resolutions == 1 ? "selected" : "wrong"), target =>
        {
            Assert.Equal("selected", target);
            return process.Object;
        });
        var field = new Mock<IFieldOrchestrator>();
        field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var controller = new BusinessProcessController(field.Object, binding, NullLogger<BusinessProcessController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "actor"),
                    new Claim(ClaimTypes.Role, role) }, "Test"))
            } }
        };
        var result = await controller.CloseInstanceAsync("instance", new ProcessCloseRequest { Reason = "reason" });
        if (allowed) Assert.IsType<NoContentResult>(result);
        else Assert.IsType<ForbidResult>(result);
        Assert.Equal(1, resolutions);
        process.Verify(x => x.GetProcessInstanceAsync("instance"), Times.Once);
        process.Verify(x => x.CancelProcessAsync("instance", "reason", "actor"), allowed ? Times.Once() : Times.Never());
        process.VerifyNoOtherCalls();
    }
}
