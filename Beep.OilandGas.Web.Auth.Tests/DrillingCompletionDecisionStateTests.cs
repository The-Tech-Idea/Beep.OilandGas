using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingCompletionDecisionStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingCompletionReviewDecisionNativeTests.Fixture Native=new();internal CompanyClientContext Company=new();
        internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingCompletionAssignedState State=null!;
        internal async Task Prepare(bool acceptanceReady=false)
        {
            await Native.Prepare(acceptanceReady:acceptanceReady);var packet=Native.Command.Review;var scope=Native.Command.Scope();
            Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[packet.AssignedRole],["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View","Drilling.Approve","Workflow.Approve"]));
            Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=packet.PersonaCode});Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=packet.PersonaCode});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Client.Setup(c=>c.GetAssignedCompletionReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Native.Command.Review);
            Client.Setup(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewDecision original,CancellationToken _)=>{Native.Command=original;return await Native.Decide();});
            Client.Setup(c=>c.RecoverCompletionReviewDecisionAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewDecision original,CancellationToken _)=>{Native.Command=original;return await Native.Decide(true);});
            State=new(Company,Personas.Object,Client.Object,Journal,(_,_,message)=>message);await State.LoadAsync(packet.InstanceId);
        }
        internal void Freeze()=>State.PrepareDecision("APPROVE","Signed independent review against the exact reported completion claim");
        internal void ReadOnly(){var selected=Company.Selection!;Company.TrySelect(Company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=selected.UserId,PersonaCode="UNMAPPED"});}
        public void Dispose(){State?.Dispose();Company.Dispose();}
    }
    [Theory,InlineData("decision"),InlineData("reason"),InlineData("reader")]
    public async Task ExplicitDecisionReasonAndCurrentApprovalAuthorityAreRequired(string mode)
    {
        using var f=new Fixture();await f.Prepare();if(mode=="reader")f.ReadOnly();
        f.State.PrepareDecision(mode=="decision"?"":"APPROVE",mode=="reason"?" ":"Explicit independent review reason");Assert.Null(f.State.PreparedDecision);
        await f.State.SubmitDecisionAsync();f.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());Assert.Equal(0,f.Native.Mutations);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task FailedOrUnconfirmedRetentionNeverSendsDecision(bool readback)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailRetain=!readback;f.Journal.FailRetainReadback=readback;await f.State.SubmitDecisionAsync();
        Assert.NotNull(f.State.PendingDecision);Assert.Contains("No submission",f.State.PendingExplanation);f.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());Assert.Equal(0,f.Native.Mutations);
    }
    [Fact]
    public async Task ExactOriginalIsRetainedBeforeNativeDecisionAndValidatedReceiptSurvivesCleanupFailure()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailClear=true;
        f.Client.Setup(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewDecision original,CancellationToken _)=>{Assert.Equal(original.Revision(),f.Journal.CompletionDecisionOriginal!.Revision());f.Native.Command=original;return await f.Native.Decide();});
        await f.State.SubmitDecisionAsync();Assert.True(f.State.DecisionReceipt!.Recorded);Assert.NotNull(f.State.PendingDecision);Assert.Contains("Clearing the tab input is not confirmed",f.State.PendingExplanation);
        var writes=f.Native.Mutations;f.Journal.FailClear=false;await f.State.RecoverDecisionAsync();Assert.Null(f.State.PendingDecision);Assert.Null(f.Journal.CompletionDecisionOriginal);Assert.Equal(writes,f.Native.Mutations);
    }
    [Fact]
    public async Task LostAcknowledgementReloadsRetainedDecisionBeforeSourceOrUnmappedProfile()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Native.Start.Lifecycle.Native.FailCommit=true;await f.State.SubmitDecisionAsync();f.Native.Start.Lifecycle.Native.FailCommit=false;
        Assert.NotNull(f.Journal.CompletionDecisionOriginal);var id=f.Journal.CompletionDecisionOriginal!.Review.InstanceId;var writes=f.Native.Mutations;f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();
        await f.State.LoadAsync(id);Assert.True(f.State.CanRecoverDecision);Assert.Null(f.State.Current);await f.State.RecoverDecisionAsync();Assert.True(f.State.DecisionReceipt!.Recorded);Assert.Equal(writes,f.Native.Mutations);
        f.Client.Verify(c=>c.GetAssignedCompletionReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());Assert.Null(f.Journal.CompletionDecisionOriginal);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task ContextChangesDuringRetentionNeverSendOrClearOriginal(string change)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.BeforeReviewRetain=async()=>{if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);if(change=="route")await f.State.LoadAsync(DrillingCompletionReviewStart.InstancePrefix+"other");if(change=="dispose")f.State.Dispose();};
        await f.State.SubmitDecisionAsync();Assert.NotNull(f.Journal.CompletionDecisionOriginal);Assert.Null(f.State.PendingDecision);Assert.Equal(0,f.Native.Mutations);
    }
    [Fact]
    public async Task ActualProtectedStoreEncryptsSeparatesAndClearsOnlyExactDecision()
    {
        using var f=new Fixture();await f.Prepare();var original=f.Native.Command;var scope=original.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainCompletionDecisionAsync(original);var saved=Assert.Single(tab.Values);Assert.EndsWith(".drilling-completion-decision",saved.Key);Assert.DoesNotContain(original.Rationale,saved.Value);
        Assert.Equal(original.Revision(),(await Store().ReadCompletionDecisionAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadCompletionDecisionAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadCompletionReviewAsync(scope.Company,scope.ActorUserId));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainCompletionDecisionAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearCompletionDecisionAsync(original with{Rationale="Changed original reason"}));await Store().ClearCompletionDecisionAsync(original);Assert.Empty(tab.Values);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task ActualReviewerParentConfirmsOriginalAndRecoversAfterWholeOwnerReload(bool loseAcknowledgement)
    {
        using var f=new Fixture();await f.Prepare();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(WorkflowTaskRoute.For("FIELD",f.Native.Command.Review.InstanceId,"DRILLING_DAILY_REPORT"));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionAssigned>();page.WaitForAssertion(()=>Assert.Contains("Review this reported completion claim",page.Markup));
        var panel=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionDecisionPanel>();await page.InvokeAsync(()=>panel.Instance.State.PrepareDecision("APPROVE","Independent recorded review with physical test acceptance remaining separate"));
        page.WaitForAssertion(()=>Assert.Contains("Confirm independent completion stage decision",page.Markup));f.Native.Start.Lifecycle.Native.FailCommit=loseAcknowledgement;
        await page.FindAll("button").Single(button=>button.TextContent=="Confirm and retain completion decision").ClickAsync(new MouseEventArgs());f.Native.Start.Lifecycle.Native.FailCommit=false;
        if(loseAcknowledgement){page.Dispose();f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionAssigned>();page.WaitForAssertion(()=>Assert.Contains("Recover original completion decision",page.Markup));await page.FindAll("button").Single(button=>button.TextContent=="Recover original completion decision").ClickAsync(new MouseEventArgs());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());}
        page.WaitForAssertion(()=>Assert.Contains("Original completion decision outcome",page.Markup));Assert.Contains("Physical test acceptance",page.Markup);Assert.Null(f.Journal.CompletionDecisionOriginal);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Independent recorded review with physical",page.Markup));
    }
    [Theory,InlineData("current"),InlineData("unavailable"),InlineData("company"),InlineData("late")]
    public async Task CompleteHistoryRequiresExactCurrentParticipantAndFencesUnavailableOrLateEvidence(string mode)
    {
        using var f=new Fixture();await f.Prepare();await f.Native.Decide();var graph=await f.Native.Start.Recorded();var progress=PPDMProcessService.CompletionReviewProgress(graph.Original,graph.Process,graph.Tasks,f.Native.Current,graph.NativeRevision);
        f.Client.Setup(c=>c.GetCompletionReviewProgressAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns((string _,CancellationToken token)=>{if(mode=="unavailable")throw new HttpRequestException("Unavailable history");if(mode=="late")f.Company.Clear();return Task.FromResult(progress);});
        if(mode=="company"){var selected=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),selected.UserId,new(new("tenant","foreign"),selected.Roles.ToArray(),selected.Permissions.ToArray()));}
        using var state=new DrillingCompletionProgressState(f.Company,f.Personas.Object,f.Client.Object,(_,_,message)=>message);await state.LoadAsync(progress.InstanceId);
        if(mode=="current"){Assert.Equal("APPROVED",state.Current!.State);Assert.Equal(0,state.Current.PendingTasks);}else Assert.Null(state.Current);
        Assert.Equal("STARTED",f.Native.Current.History.Execution.Recorded!.STATUS);
    }
    [Fact]
    public async Task ActualHistoryPageShowsRecordedDecisionsAndSeparateAcceptanceWithoutInventingCompletion()
    {
        using var f=new Fixture();await f.Prepare();await f.Native.Decide();var graph=await f.Native.Start.Recorded();var progress=PPDMProcessService.CompletionReviewProgress(graph.Original,graph.Process,graph.Tasks,f.Native.Current,graph.NativeRevision);
        f.Client.Setup(c=>c.GetCompletionReviewProgressAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(progress);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/completion-review/history?processId="+Uri.EscapeDataString(progress.InstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionHistory>();page.WaitForAssertion(()=>Assert.Contains("Complete ordered independent completion review stages",page.Markup));
        Assert.Contains("Independent review state APPROVED",page.Markup);Assert.Contains("No independent physical completion acceptance is recorded",page.Markup);Assert.False(progress.OriginalCompletionAccepted);Assert.DoesNotContain("Original physical completion acceptance evidence",page.Markup);Assert.Contains(f.Native.Command.Rationale,page.Markup);Assert.Contains("actual pending approval tasks 0",page.Markup);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain(f.Native.Command.Rationale,page.Markup));
    }
}
#endif
