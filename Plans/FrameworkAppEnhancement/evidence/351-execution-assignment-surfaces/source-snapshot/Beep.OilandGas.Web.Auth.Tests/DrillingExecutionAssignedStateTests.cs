#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingExecutionAssignedStateTests
{
    internal static DrillingExecutionAssignedWork Packet(DrillingExecutionAssignment original)
    {
        var recipient=original.Workflow.Authority.Recipients[0];var source=original.Source.Basis;
        source=source with{Source=DrillingProgramAssignedReviewTests.AsReader(source.Source,recipient.UserId),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};
        source=source with{Revision=source.ContentRevision()};var current=original.Source with{Basis=source};current=current with{Revision=current.ContentRevision()};
        var step=DrillingExecutionAssignmentReceipt.StepId(original.InstanceId(),original.Workflow.Step.StepId);
        var task="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=original.InstanceId(),Step=step,Persona=recipient.PersonaCode}));
        var result=new DrillingExecutionAssignedWork(original,current,task,step,original.Workflow.Step.AssignedRole,recipient.PersonaCode,new('A',64),new('B',64),new('C',64),"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(original.Scope().Company,recipient.UserId,original.InstanceId());return result;
    }
    [Theory,InlineData("current"),InlineData("company"),InlineData("actor"),InlineData("role"),InlineData("persona"),InlineData("late-company"),InlineData("unavailable")]
    public async Task ExactCurrentRecipientCanReadResponsibilityWhileForeignOrStaleSourcesStayUnavailable(string mode)
    {
        var native=new DrillingExecutionAssignmentNativeTests.Fixture();await native.Prepare();var packet=Packet(native.Original);using var company=new CompanyClientContext();
        var scope=packet.Source.Basis.Scope();company.TrySelect(company.Clear(),scope.ActorUserId,new(scope.Company,[mode=="role"?"OtherRole":packet.AssignedRole],[]));
        var personas=new Mock<IPersonaContextService>();personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=mode=="persona"?"OTHER":packet.PersonaCode});
        personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=mode=="persona"?"OTHER":packet.PersonaCode});
        var client=new Mock<IDrillingServiceClient>();client.Setup(c=>c.GetAssignedExecutionAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(()=>{
            if(mode=="unavailable")throw new HttpRequestException();if(mode=="late-company")company.Clear();
            var result=CompanyInventorySourceReview.Copy(packet);if(mode=="actor")company.TrySelect(company.Clear(),"another",new(scope.Company,[packet.AssignedRole],[]));
            if(mode=="company")company.TrySelect(company.Clear(),scope.ActorUserId,new(new("tenant","foreign"),[packet.AssignedRole],[]));return Task.FromResult(result);});
        using var state=new DrillingExecutionAssignedState(company,personas.Object,client.Object,(_,_,message)=>message);await state.LoadAsync(packet.InstanceId);
        if(mode=="current"){Assert.Equal(packet.Revision,state.Current!.Revision);Assert.Equal(scope.ActorUserId,state.Current.Source.Basis.Scope().ActorUserId);}
        else Assert.Null(state.Current);Assert.False(state.Busy);
    }
    [Fact]public async Task ActualReceiverPageAndSharedLandingPanelShowOriginalResponsibilityWithoutInventingProgressCounts()
    {
        var native=new DrillingExecutionAssignmentNativeTests.Fixture();await native.Prepare();var packet=Packet(native.Original);var scope=packet.Source.Basis.Scope();using var company=new CompanyClientContext();
        company.TrySelect(company.Clear(),scope.ActorUserId,new(scope.Company,[packet.AssignedRole],[]));var personas=new Mock<IPersonaContextService>();
        personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=packet.PersonaCode});personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=packet.PersonaCode,Name="Reporting site"});
        var client=new Mock<IDrillingServiceClient>();client.Setup(c=>c.GetAssignedExecutionAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(packet);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(client.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/drilling/program-execution/assigned?processId="+Uri.EscapeDataString(packet.InstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecutionAssigned>();
        page.WaitForAssertion(()=>Assert.Contains("Your original reporting responsibility",page.Markup));Assert.Contains(packet.Original.Reason,page.Markup);
        Assert.Contains("Open assigned observed progress",page.Markup);Assert.Contains("No daily-report deadline",page.Markup);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(i=>i.GetInboxAsync(packet.PersonaCode)).ReturnsAsync(new UnifiedInbox{Company=scope.Company,ActorUserId=scope.ActorUserId,PersonaCode=packet.PersonaCode,
            NormalTasks=[new(){TaskId=packet.TaskId,TaskType="DATA_ENTRY",Priority=3,Status="PENDING",AssignedRole=packet.AssignedRole,ScopeKind="FIELD",EntityType="DRILLING_OPERATION",EntityId=packet.Source.Recorded!.DRILLING_OPERATION_ID,WorkflowName=packet.InstanceId,StepName=packet.StepInstanceId,Route=WorkflowTaskRoute.For("FIELD",packet.InstanceId,"DRILLING_OPERATION"),ExecutionAssignmentRevision=packet.Revision}],Counts=new(){TotalPending=1,DataEntry=1}});
        using var state=new PersonaTaskInboxState(inbox.Object,company,personas.Object);await state.LoadAsync();state.SelectSource("DRILLING_OPERATION");Assert.Equal(1,state.SourceCount("DRILLING_OPERATION"));Assert.Equal(1,state.SelectedCounts.DataEntry);Assert.Equal(0,state.SelectedCounts.Reviews);
        var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p=>p.Add(x=>x.Review,state).Add(x=>x.WorkspaceName,"Reporting site"));
        Assert.Contains("Ongoing execution reporting responsibilities: 1",panel.Markup);Assert.Contains("Open assigned execution reporting",panel.Markup);
    }
}
#endif
