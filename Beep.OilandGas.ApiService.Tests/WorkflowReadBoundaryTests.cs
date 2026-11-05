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
    [InlineData("type")]
    [InlineData("prefix")]
    [InlineData("payload")]
    public async Task GenericInstanceAndHistoryCannotExposeInvestmentEvidenceThroughFieldRead(string marker)
    {
        var id=marker=="prefix"?TheTechIdea.Data.OilGas.DevelopmentInvestmentHandoff.InstancePrefix+"owned":"instance";
        var instance=new ProcessInstance { InstanceId=id,FieldId="selected",EntityId="private-afe",EntityType=marker=="type"?"AFE":"WELL" };
        if(marker=="payload")instance.ProcessData=new(){Data=new(){[TheTechIdea.Data.OilGas.DevelopmentInvestmentHandoff.PayloadKey]=new { privateCompany=true }}};
        var process=new Mock<IProcessService>(MockBehavior.Strict);process.Setup(value=>value.GetProcessInstanceAsync(id)).ReturnsAsync(instance);
        var field=new Mock<IFieldOrchestrator>();field.SetupGet(value=>value.CurrentFieldId).Returns("selected");
        var resources=new Mock<IAccessControlService>(MockBehavior.Strict);
        var controller=new BusinessProcessController(field.Object,process.Object,NullLogger<BusinessProcessController>.Instance,resources.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),Mock.Of<IJournalEntryService>(),Mock.Of<IWorkOrderService>(),Mock.Of<IInspectionService>())
        {ControllerContext=new(){HttpContext=new Microsoft.AspNetCore.Http.DefaultHttpContext{User=new(new System.Security.Claims.ClaimsIdentity([new("party_id","actor")],"test"))}}};
        Assert.IsType<ForbidResult>((await controller.GetInstanceAsync(id)).Result);
        Assert.IsType<ForbidResult>((await controller.GetHistoryAsync(id)).Result);
        process.Verify(value=>value.GetProcessInstanceAsync(id),Times.Exactly(2));process.VerifyNoOtherCalls();resources.VerifyNoOtherCalls();
    }
    [Theory]
    [InlineData("selected", true)]
    [InlineData("foreign", false)]
    [InlineData("", false)]
    public async Task InstanceAndHistoryRequireTheSelectedField(string instanceField, bool allowed)
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        var instance = new ProcessInstance { InstanceId = "instance", FieldId = instanceField, EntityId="well", EntityType="WELL" };
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
        var resources = new Mock<IAccessControlService>();
        resources.Setup(x=>x.CheckAssetAccessAsync("actor", "selected", "FIELD", "READ", null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess=true });
        resources.Setup(x=>x.CheckAssetAccessAsync("actor", "well", "WELL", "READ", null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess=true });
        var controller = new BusinessProcessController(field.Object, bound, NullLogger<BusinessProcessController>.Instance, resources.Object, new Mock<TheTechIdeaWeb.Diagnostics.IFailureReporter>().Object, new Mock<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>().Object, new Mock<IWorkOrderService>().Object, new Mock<IInspectionService>().Object) {
            ControllerContext=new ControllerContext { HttpContext=new Microsoft.AspNetCore.Http.DefaultHttpContext { User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new("party_id","actor")],"test")) } }
        };
        var detail = await controller.GetInstanceAsync("instance");
        if (allowed) Assert.Same(instance, Assert.IsType<OkObjectResult>(detail.Result).Value);
        else Assert.IsType<ForbidResult>(detail.Result);
        Assert.Equal(1, resolutions);
        resolutions = 0;
        var history = await controller.GetHistoryAsync("instance");
        if (allowed) Assert.IsType<OkObjectResult>(history.Result);
        else Assert.IsType<ForbidResult>(history.Result);
        Assert.Equal(1, resolutions);
        process.Verify(x => x.GetProcessInstanceAsync("instance"), Times.Exactly(allowed ? 4 : 2));
        process.Verify(x => x.GetProcessHistoryAsync("instance"), allowed ? Times.Once() : Times.Never());
        process.VerifyNoOtherCalls();
    }
}
