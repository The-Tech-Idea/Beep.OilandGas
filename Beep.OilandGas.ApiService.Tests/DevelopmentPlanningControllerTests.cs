using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentPlanningControllerTests
{
    private static CreateDevelopmentPlan Input()=>new(){SelectionContext=new(new("tenant","company"),new("core","binding"),"planner","revision","field"),
        PlanningDatabase=new("planning","planning-binding"),IntentId=Guid.NewGuid(),PlanName="Reviewed draft",FieldId="field"};
    [Theory][InlineData("list")][InlineData("options")][InlineData("save")][InlineData("recover")]
    public async Task EveryEndpointUsesRequiredPermissionFieldModeAndClosingAuthority(string operation)
    {
        var input=Input();var calls=new List<string>();Func<Task> check=()=>{calls.Add("check");return Task.CompletedTask;};
        var scope=new DevelopmentPlanRequestScope(input.SelectionContext,input.PlanningDatabase,check,()=>{calls.Add("confirm");return Task.CompletedTask;});
        var service=new Mock<IDevelopmentPlanService>(MockBehavior.Strict);var authority=new Mock<IDevelopmentPlanRequestAuthority>(MockBehavior.Strict);
        var controller=new DevelopmentPlanningController(service.Object,authority.Object){ControllerContext=new(){HttpContext=new DefaultHttpContext()}};
        var permission=operation=="list"?PermissionConstants.DevelopmentPlanning.View:PermissionConstants.DevelopmentPlanning.Create;
        authority.Setup(a=>a.CaptureAsync(controller.User,permission,(operation=="options" || operation=="save"),(operation=="save" || operation=="recover")?input.SelectionContext:null,operation=="save"))
            .ReturnsAsync(scope);
        service.Setup(s=>s.GetDevelopmentPlansAsync(scope.Context,scope.PlanningDatabase,check)).Returns(async()=>{await check();return new(scope.Context,scope.PlanningDatabase,[],DateTimeOffset.UtcNow);});
        service.Setup(s=>s.GetDraftOptionsAsync(scope.Context,scope.PlanningDatabase,check)).Returns(async()=>{await check();return new(scope.Context,scope.PlanningDatabase,["KWD"]);});
        service.Setup(s=>s.CreateDevelopmentPlanAsync(input,check)).Returns(async()=>{await check();return new FIELD_DEVELOPMENT_PLAN();});
        service.Setup(s=>s.RecoverDevelopmentPlanAsync(input,scope.Context,scope.PlanningDatabase,check)).Returns(async()=>{await check();return new(scope.Context,scope.PlanningDatabase,input.IntentId,DevelopmentPlanDraftReview.RequestRevision(input),null,DateTimeOffset.UtcNow);});
        object? result=operation switch{"list"=>(await controller.GetPlansAsync()).Result,"options"=>(await controller.GetDraftOptionsAsync()).Result,"save"=>(await controller.CreatePlanAsync(input)).Result,_=>(await controller.RecoverPlanAsync(input)).Result};
        Assert.IsType<OkObjectResult>(result);Assert.Equal(new[]{"check","confirm"},calls);authority.VerifyAll();
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task ChangedPlanningBindingNeverReachesNativeOwner(bool recovery)
    {
        var input=Input();var service=new Mock<IDevelopmentPlanService>(MockBehavior.Strict);var authority=new Mock<IDevelopmentPlanRequestAuthority>();
        authority.Setup(a=>a.CaptureAsync(It.IsAny<ClaimsPrincipal>(),It.IsAny<string>(),It.IsAny<bool>(),It.IsAny<FieldSelectionReview>(),It.IsAny<bool>()))
            .ReturnsAsync(new DevelopmentPlanRequestScope(input.SelectionContext,new("planning","changed"),()=>Task.CompletedTask,()=>Task.CompletedTask));
        var controller=new DevelopmentPlanningController(service.Object,authority.Object){ControllerContext=new(){HttpContext=new DefaultHttpContext()}};
        await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(async()=>{if(recovery)await controller.RecoverPlanAsync(input);else await controller.CreatePlanAsync(input);});
        Assert.Empty(service.Invocations);
    }
    [Fact] public void ControllerExposesOnlyQualifiedDraftOperations()
    {
        var methods=typeof(DevelopmentPlanningController).GetMethods().Where(m=>m.DeclaringType==typeof(DevelopmentPlanningController)).Select(m=>m.Name).Order().ToArray();
        Assert.Equal(new[]{"ApproveBaselineAsync","CreatePlanAsync","DecideReviewAsync","GetAssignedReviewAsync","GetBaselineAsync","GetDraftOptionsAsync","GetPlanAsync","GetPlanReviewStatusAsync","GetPlansAsync","GetReviewProgressAsync","GetReviewWorkflowsAsync","PrepareReviewAsync","RecoverBaselineAsync","RecoverPlanAsync","RecoverReviewAsync","RecoverReviewDecisionAsync","SubmitReviewAsync"},methods);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task AssignedSourceRefusalsRetainPermissionAndConflictMeaning(bool changed)
    {
        var reader=new Mock<IDevelopmentAssignedReviewReader>();reader.Setup(service=>service.ReadAsync(It.IsAny<ClaimsPrincipal>(),"process"))
            .ThrowsAsync(changed?new InvalidOperationException("private native evidence"):new UnauthorizedAccessException("private account evidence"));
        var controller=new DevelopmentPlanningController(Mock.Of<IDevelopmentPlanService>(),Mock.Of<IDevelopmentPlanRequestAuthority>()){ControllerContext=new(){HttpContext=new DefaultHttpContext()}};
        var failures=new Mock<TheTechIdeaWeb.Diagnostics.IFailureReporter>();
        var refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.GetAssignedReviewAsync("process",reader.Object,failures.Object));
        Assert.Equal(changed?Beep.OilandGas.Models.Core.Refusals.RefusalKind.Conflict:Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden,refusal.Kind);
        Assert.DoesNotContain("private",refusal.Sentence);Assert.Equal(changed?1:0,failures.Invocations.Count);
    }
}
