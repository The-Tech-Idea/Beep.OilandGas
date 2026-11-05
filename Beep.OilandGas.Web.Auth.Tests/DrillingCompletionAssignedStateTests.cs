#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingCompletionAssignedStateTests
{
    internal static async Task<DrillingCompletionAssignedReview> Packet(DrillingCompletionReviewStartNativeTests.Fixture f)
    {
        var recorded=await f.Recorded();var original=recorded.Original;var receiver=original.Workflow.Stages.Single(stage=>stage.Definition.StepId==recorded.Process.CurrentStepId).Authority.Recipients[0];
        var source=original.Source.Execution.Basis with{Source=DrillingProgramAssignedReviewTests.AsReader(original.Source.Execution.Basis.Source,receiver.UserId),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};
        source=source with{Revision=source.ContentRevision()};var execution=f.Current.History.Execution with{Basis=source};execution=execution with{Revision=execution.ContentRevision()};
        var history=f.Current.History with{Execution=execution};history=history with{Revision=history.ContentRevision()};var page=f.Current with{History=history};page=page with{Revision=page.ContentRevision()};
        var task=recorded.Tasks.Single(row=>row.TARGET_PERSONA_CODE==receiver.PersonaCode && row.TASK_STATUS=="PENDING");
        var packet=new DrillingCompletionAssignedReview(original,page,task.CROSS_TASK_ID,task.PROCESS_STEP_INSTANCE_ID,task.ASSIGNED_ROLE,receiver.PersonaCode,
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(recorded.Process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),recorded.NativeRevision,"",DateTimeOffset.UtcNow);
        packet=packet with{Revision=packet.ContentRevision()};packet.Require(original.Scope().Company,receiver.UserId,original.InstanceId());return packet;
    }
    [Theory,InlineData("current"),InlineData("role"),InlineData("persona"),InlineData("actor"),InlineData("company"),InlineData("unavailable"),InlineData("late")]
    public async Task IndependentRecipientRequiresExactCompanyActorRoleWorkspaceAndLateContextFence(string mode)
    {
        var native=new DrillingCompletionReviewStartNativeTests.Fixture();await native.Prepare();await native.Start();var packet=await Packet(native);var scope=packet.Source.History.Execution.Basis.Scope();
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),mode=="actor"?"other":scope.ActorUserId,new(mode=="company"?new("tenant","foreign"):scope.Company,[mode=="role"?"OtherRole":packet.AssignedRole],["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        var personas=new Mock<IPersonaContextService>();personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=packet.PersonaCode});personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=mode=="persona"?"OTHER":packet.PersonaCode});
        var client=new Mock<IDrillingServiceClient>();client.Setup(c=>c.GetAssignedCompletionReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="unavailable")throw new IOException("Native unavailable");if(mode=="late")company.Clear();return Task.FromResult(packet);});
        using var state=new DrillingCompletionAssignedState(company,personas.Object,client.Object,new DevelopmentDraftWorkspaceTests.Journal(),(_,_,text)=>text);await state.LoadAsync(packet.InstanceId);
        if(mode=="current"){Assert.Equal(packet.Revision,state.Current!.Revision);Assert.True(state.Current.IsCurrentClaim);}else Assert.Null(state.Current);
        Assert.Equal(4,native.Lifecycle.Native.Writes);client.Verify(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task ActualRecipientPageShowsOriginalReportedEvidenceAndHistoricalClaimWithoutAcceptingOrClosingTask()
    {
        var native=new DrillingCompletionReviewStartNativeTests.Fixture();await native.Prepare();await native.Start();native.Append(true);var packet=await Packet(native);Assert.False(packet.IsCurrentClaim);
        var scope=packet.Source.History.Execution.Basis.Scope();using var company=new CompanyClientContext();company.TrySelect(company.Clear(),scope.ActorUserId,new(scope.Company,[packet.AssignedRole],["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        var personas=new Mock<IPersonaContextService>();personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=packet.PersonaCode});personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=packet.PersonaCode});
        var client=new Mock<IDrillingServiceClient>();client.Setup(c=>c.GetAssignedCompletionReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(packet);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(client.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(WorkflowTaskRoute.For("FIELD",packet.InstanceId,"DRILLING_DAILY_REPORT"));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionAssigned>();page.WaitForAssertion(()=>Assert.Contains("Your pending independent review task",page.Markup));
        Assert.Contains("historical or withdrawn",page.Markup);Assert.Contains("Reported completion evidence",page.Markup);Assert.Contains("Pending independent decision",page.Markup);
        Assert.Contains(scope.FieldId,page.Markup);Assert.Contains(packet.Source.History.Execution.Basis.Source.Basis.Well.Uwi,page.Markup);Assert.Contains(packet.Original.Source.ReportedCompletion!.RecorderPersonaCode,page.Markup);
        company.Clear();page.WaitForAssertion(()=>{Assert.DoesNotContain("Signed test certificate",page.Markup);Assert.Contains("Choose your company, then reopen",page.Markup);});Assert.Equal(4,native.Lifecycle.Native.Writes);
    }
}
#endif
