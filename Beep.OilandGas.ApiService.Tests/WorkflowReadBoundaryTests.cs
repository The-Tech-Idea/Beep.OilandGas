using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowReadBoundaryTests
{
    [Theory]
    [InlineData("selected", true)]
    [InlineData("foreign", false)]
    [InlineData("", false)]
    public async Task InstanceAndHistoryRequireTheSelectedField(string instanceField, bool allowed)
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        var instance = new ProcessInstance { InstanceId = "instance", FieldId = instanceField };
        process.Setup(x => x.GetProcessInstanceAsync("instance")).ReturnsAsync(instance);
        if (allowed) process.Setup(x => x.GetProcessHistoryAsync("instance")).ReturnsAsync(new List<ProcessHistoryEntry>());
        var resolutions = 0;
        var bound = new BoundProcessService(() => Task.FromResult(++resolutions == 1 ? "target" : "wrong"), target =>
        {
            Assert.Equal("target", target);
            return process.Object;
        });
        var field = new Mock<IFieldOrchestrator>();
        field.SetupGet(x => x.CurrentFieldId).Returns("selected");
        var controller = new BusinessProcessController(field.Object, bound, NullLogger<BusinessProcessController>.Instance);
        var detail = await controller.GetInstanceAsync("instance");
        if (allowed) Assert.Same(instance, Assert.IsType<OkObjectResult>(detail.Result).Value);
        else Assert.IsType<ForbidResult>(detail.Result);
        Assert.Equal(1, resolutions);
        resolutions = 0;
        var history = await controller.GetHistoryAsync("instance");
        if (allowed) Assert.IsType<OkObjectResult>(history.Result);
        else Assert.IsType<ForbidResult>(history.Result);
        Assert.Equal(1, resolutions);
        process.Verify(x => x.GetProcessInstanceAsync("instance"), Times.Exactly(2));
        process.Verify(x => x.GetProcessHistoryAsync("instance"), allowed ? Times.Once() : Times.Never());
        process.VerifyNoOtherCalls();
    }
}
