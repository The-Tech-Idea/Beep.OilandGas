using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingProgramAssignedReviewStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingProgramReviewStartNativeTests.Fixture Native=new();internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();internal DrillingProgramAssignedReviewState State=null!;internal DrillingProgramAssignedReview Packet=null!;
        internal async Task Prepare()
        {
            await Native.Prepare();await Native.Start();Packet=await DrillingProgramAssignedReviewTests.Packet(Native);var scope=Packet.Source.Basis.Scope();Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[Packet.AssignedRole],["Drilling.View","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"]));
            Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=Packet.PersonaCode,Name="Program review workspace",DefaultRoute="/drilling/workspace"});Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=Packet.PersonaCode});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);Client.Setup(c=>c.GetAssignedProgramReviewAsync(Packet.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Packet);
            var graph=await Native.Native.Service.ReadRecordedProgramReviewAsync(scope.Company,Packet.InstanceId,Native.Guard);var outcome=Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService.ProgramReviewProgress(graph.Original,graph.Process,graph.Tasks,Packet.Source,graph.NativeRevision);Client.Setup(c=>c.GetProgramReviewProgressAsync(Packet.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>outcome);State=new(Company,Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        }
        public void Dispose(){State?.Dispose();Company.Dispose();Native.Dispose();}
    }
    [Fact]public async Task ExactReaderSourceAndTaskAreDefensiveAndFailedReloadClearsPriorEvidence()
    {
        using var f=new Fixture();await f.Prepare();await f.State.LoadAsync(f.Packet.InstanceId);Assert.Equal(f.Packet.Revision,f.State.Current!.Revision);f.State.Current.Source.Program.PROGRAM_NAME="Caller mutation";Assert.Equal(f.Packet.Source.Program.PROGRAM_NAME,f.State.Current.Source.Program.PROGRAM_NAME);
        f.Client.Setup(c=>c.GetAssignedProgramReviewAsync(f.Packet.InstanceId,It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());await f.State.LoadAsync(f.Packet.InstanceId);Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("role")][InlineData("persona")][InlineData("permission")]
    public async Task AnotherCurrentCompanyAccountRoleOrWorkspaceCannotDisplayTheOriginalProgram(string fault)
    {
        using var f=new Fixture();await f.Prepare();var scope=f.Packet.Source.Basis.Scope();if(fault is "company" or "actor" or "role" or "permission"){var selected=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),fault=="actor"?"other":selected.UserId,new(fault=="company"?new("tenant","foreign"):selected.Scope,fault=="role"?["OtherRole"]:selected.Roles.ToArray(),fault=="permission"?[]:selected.Permissions.ToArray()));}
        if(fault=="persona"){f.Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code="OTHER"});f.Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode="OTHER"});}await f.State.LoadAsync(f.Packet.InstanceId);Assert.Null(f.State.Current);if(fault=="permission")f.Client.Verify(c=>c.GetAssignedProgramReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());else if(fault is "role" or "persona"){Assert.NotNull(f.State.Progress);Assert.Null(f.State.Error);}else Assert.NotNull(f.State.Error);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateOriginalSourceCannotSurviveContextOrRouteChanges(string change)
    {
        using var f=new Fixture();await f.Prepare();var waiting=new TaskCompletionSource<DrillingProgramAssignedReview>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.GetAssignedProgramReviewAsync(f.Packet.InstanceId,It.IsAny<CancellationToken>())).Returns(waiting.Task);f.Client.Setup(c=>c.GetAssignedProgramReviewAsync(DrillingProgramReviewStart.InstancePrefix+"another",It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());var read=f.State.LoadAsync(f.Packet.InstanceId);
        if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);else if(change=="route")await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix+"another");else f.State.Dispose();waiting.SetResult(f.Packet);await read;Assert.Null(f.State.Current);
    }
    [Theory][InlineData("recorded")][InlineData("unavailable")][InlineData("no-access")]
    public async Task ActualAssignedRouteShowsExactSavedVersionFundingAndPendingRoleTaskOnly(string mode)
    {
        using var f=new Fixture();await f.Prepare();if(mode=="unavailable")f.Client.Setup(c=>c.GetAssignedProgramReviewAsync(f.Packet.InstanceId,It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());if(mode=="no-access")f.Company.Clear();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-review/assigned?processId="+Uri.EscapeDataString(f.Packet.InstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingProgramAssignedReview>();
        if(mode=="recorded")
        {
            page.WaitForAssertion(()=>Assert.Contains("Exact saved program version",page.Markup));Assert.Contains(f.Packet.Source.Program.PROGRAM_ID,page.Markup);Assert.Contains(f.Packet.TaskId,page.Markup);Assert.Contains("Assigned role ConfiguredProgramReviewer",page.Markup);Assert.Contains("Workspace DRILLING_REVIEW",page.Markup);Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("Pending independent program review",page.Markup);Assert.Contains("Source status DRAFT",page.Markup);Assert.Contains("Planned MD 2500.25 m",page.Markup);Assert.Contains("Planned calendar dates",page.Markup);
        }
        else if(mode=="unavailable"){page.WaitForAssertion(()=>Assert.Contains("could not be confirmed",page.Markup));Assert.DoesNotContain(f.Packet.Source.Program.PROGRAM_ID,page.Markup);}else{Assert.Contains("Choose a company",page.Markup);Assert.DoesNotContain(f.Packet.Source.Program.PROGRAM_ID,page.Markup);}
        Assert.DoesNotContain(page.FindAll("button"),b=>b.TextContent is "Approve program" or "Reject program" or "Complete review");f.Client.Verify(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
}
#endif
