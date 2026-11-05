using Beep.OilandGas.PermitsAndApplications.Models;
using PermitApplicationSummary = Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationSummary;
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
    public async Task List_returns_only_recorded_current_field_and_rejects_unknown_status()
    {
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var workflow=new Mock<IPermitApplicationWorkflowService>();var compliance=new Mock<IPermitComplianceCheckService>();var history=new Mock<IPermitStatusHistoryService>();var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns("A");
        lifecycle.Setup(x=>x.GetByStatusAsync(PermitApplicationStatus.Submitted)).ReturnsAsync(new[]{new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",FIELD_ID="A",STATUS=PermitApplicationStatus.Submitted},new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="foreign",FIELD_ID="B",STATUS=PermitApplicationStatus.Submitted}});
        var controller=CreateController(lifecycle,workflow,compliance,history,field,"actor");var response=await controller.GetAllAsync("Submitted",null);
        var rows=Assert.IsType<List<PermitApplicationSummary>>(Assert.IsType<OkObjectResult>(response.Result).Value);var row=Assert.Single(rows);Assert.Equal("local",row.PermitApplicationId);Assert.Equal("A",row.FieldId);
        Assert.IsType<BadRequestObjectResult>((await controller.GetAllAsync("invented",null)).Result);
    }
    [Fact]
    public async Task Foreign_detail_is_not_returned_and_does_not_read_history_or_compliance()
    {
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var workflow=new Mock<IPermitApplicationWorkflowService>();var compliance=new Mock<IPermitComplianceCheckService>();var history=new Mock<IPermitStatusHistoryService>();var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns("A");
        lifecycle.Setup(x=>x.GetByIdAsync("foreign")).ReturnsAsync(new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="foreign",FIELD_ID="B"});
        var response=await CreateController(lifecycle,workflow,compliance,history,field,"actor").GetByIdAsync("foreign");Assert.IsType<NotFoundObjectResult>(response.Result);history.VerifyNoOtherCalls();compliance.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Foreign_application_cannot_reach_compliance_operations()
    {
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var workflow=new Mock<IPermitApplicationWorkflowService>();var compliance=new Mock<IPermitComplianceCheckService>();var history=new Mock<IPermitStatusHistoryService>();var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns("A");
        lifecycle.Setup(x=>x.GetByIdAsync("foreign")).ReturnsAsync(new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="foreign",FIELD_ID="B"});var controller=CreateController(lifecycle,workflow,compliance,history,field,"actor");
        Assert.IsType<NotFoundObjectResult>((await controller.CheckComplianceAsync("foreign")).Result);
        compliance.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenMissing()
    {
        var lifecycle = new Mock<IPermitApplicationLifecycleService>(MockBehavior.Strict);
        var workflow = new Mock<IPermitApplicationWorkflowService>(MockBehavior.Strict);
        var compliance = new Mock<IPermitComplianceCheckService>(MockBehavior.Strict);
        var history = new Mock<IPermitStatusHistoryService>(MockBehavior.Strict);
        var field = new Mock<IFieldOrchestrator>(MockBehavior.Strict);

        field.SetupGet(x => x.CurrentFieldId).Returns("FIELD-100");
        lifecycle.Setup(x => x.GetByIdAsync("missing-id")).ReturnsAsync((PERMIT_APPLICATION?)null);

        var controller = CreateController(lifecycle, workflow, compliance, history, field, "permits-user");
        var result = await controller.GetByIdAsync("missing-id");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        lifecycle.VerifyAll();
    }

    [Fact]
    public async Task ReviewedSubmissionUsesRetainedOriginalAndCurrentClaimActor()
    {
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var workflow=new Mock<IPermitApplicationWorkflowService>();var compliance=new Mock<IPermitComplianceCheckService>();var history=new Mock<IPermitStatusHistoryService>();var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns("A");
        var context=Scope("A","review-user");
        var review=PermitApplicationSourceReview.Capture(context,new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",FIELD_ID="A",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",APPLICANT_ID="recorded applicant",COUNTRY=Country.Kuwait},[]);
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"SUBMIT","Reviewed recorded evidence");var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);
        lifecycle.Setup(x=>x.RecordCommandAsync(original)).ReturnsAsync(outcome);
        var result=await CreateController(lifecycle,workflow,compliance,history,field,"review-user").RecordCommandAsync(original);
        Assert.Equal(outcome,Assert.IsType<OkObjectResult>(result.Result).Value);lifecycle.Verify(x=>x.RecordCommandAsync(original),Times.Once);
    }

    private static PermitApplicationDataScope Scope(string field,string actor)=>new(new(new("tenant","company"),new("core","binding"),actor,"selection",field),new("permits","permit-binding"));

    private static PermitsController CreateController(
        Mock<IPermitApplicationLifecycleService> lifecycle,
        Mock<IPermitApplicationWorkflowService> workflow,
        Mock<IPermitComplianceCheckService> compliance,
        Mock<IPermitStatusHistoryService> history,
        Mock<IFieldOrchestrator> field,
        string userId)
    {
        ICompanyPermitApplicationDataAccess Access()
        {
            var access=new Mock<ICompanyPermitApplicationDataAccess>();access.Setup(x=>x.RequireReadScopeAsync()).ReturnsAsync(()=>Scope(field.Object.CurrentFieldId,userId));return access.Object;
        }
        var controller = new PermitsController(
            lifecycle.Object,
            workflow.Object,
            compliance.Object,
            history.Object,
            field.Object, Access());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("party_id", userId)
                ], "TestAuth"))
            }
        };

        return controller;
    }
}
