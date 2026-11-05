using Beep.OilandGas.PermitsAndApplications.Models;
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
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingCompletionSubmissionStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingCompletionReviewStartNativeTests.Fixture Native=new();
        internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();
        internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingDailyReportState State=null!;
        internal DrillingCompletionReviewReadiness Routing=null!;
        internal string ReviewId=>Native.Original.Source.Execution.Basis.ReviewInstanceId;
        internal async Task Prepare()
        {
            await Native.Prepare();var original=Native.Original;var scope=original.Scope();
            Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[original.SenderRole],["Drilling.View","Drilling.Edit","Drilling.ManageWellProgram","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Start"]));
            Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=original.SenderPersonaCode,Name="Current company sender"});
            Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=original.SenderPersonaCode});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Routing=new(original.Source,[original.Workflow],[original.SenderRole],original.SenderPersonaCode,original.SenderAuthorityRevision,"",DateTimeOffset.UtcNow);Routing=Routing with{Revision=Routing.ContentRevision()};
            Client.Setup(c=>c.GetDailyReportPageAsync(It.IsAny<string>(),It.IsAny<int>(),It.IsAny<int>(),It.IsAny<string?>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Native.Current);
            Client.Setup(c=>c.GetCompletionRoutingAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Routing);
            Client.Setup(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewStart input,CancellationToken _) =>{Native.Original=input;return await Native.Start();});
            Client.Setup(c=>c.RecoverCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewStart input,CancellationToken _) =>{Native.Original=input;return await Native.Start(true);});
            State=new(Company,Personas.Object,Client.Object,Journal,(_,_,message)=>message);await State.LoadAsync(ReviewId);await State.InspectCompletionRoutingAsync();
        }
        internal void Freeze(string? reason="Independent inspection and test evidence review")=>State.PrepareCompletionReview(Routing.Options[0].ProcessId,Native.Original.SenderRole,reason);
        internal void ReadOnly(){var selected=Company.Selection!;Company.TrySelect(Company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=selected.UserId,PersonaCode="UNMAPPED"});}
        public void Dispose(){State?.Dispose();Company.Dispose();}
    }
    [Theory,InlineData("reason"),InlineData("role"),InlineData("blocked"),InlineData("existing")]
    public async Task ExplicitCurrentSourceRoleAndReasonAreRequiredBeforeFreezingOrSending(string fault)
    {
        using var f=new Fixture();await f.Prepare();
        if(fault=="blocked"){var option=f.Routing.Options[0];var stage=option.Stages[0];stage=stage with{Definition=stage.Definition with{PersonaCodes=[]},Authority=stage.Authority with{Recipients=[]}};f.Routing=f.Routing with{Options=[option with{Stages=[stage]}]};f.Routing=f.Routing with{Revision=f.Routing.ContentRevision()};await f.State.InspectCompletionRoutingAsync();}
        if(fault=="existing"){f.Routing=f.Routing with{Options=[],ExistingReviewInstanceId=f.Native.Original.InstanceId()};f.Routing=f.Routing with{Revision=f.Routing.ContentRevision()};await f.State.InspectCompletionRoutingAsync();}
        if(fault=="role")f.State.PrepareCompletionReview(f.Routing.Options[0].ProcessId,"ForeignRole","Explicit reason");else if(fault!="existing")f.Freeze(fault=="reason"?" ":"Explicit reason");else Assert.False(f.State.CanPrepareCompletionReview);
        Assert.Null(f.State.CompletionPrepared);await f.State.SubmitCompletionReviewAsync();f.Client.Verify(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task FrozenConfirmationRetainsExactEncryptedIntentBeforeNativeTransportAndClearsOnlyValidatedReceipt()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var original=f.State.CompletionPrepared!;
        Assert.False(f.State.CanPrepare);Assert.False(f.State.CanNavigate);
        f.Client.Setup(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionReviewStart input,CancellationToken _) =>
        {Assert.Equal(input.Revision(),f.Journal.CompletionReviewOriginal!.Revision());f.Native.Original=input;return await f.Native.Start();});
        await f.State.SubmitCompletionReviewAsync();Assert.True(f.State.CompletionReceipt!.Recorded);Assert.Null(f.State.CompletionPending);Assert.Null(f.Journal.CompletionReviewOriginal);
        Assert.Equal(original.Revision(),f.State.CompletionReceipt.IntentRevision);Assert.Equal(4,f.Native.Lifecycle.Native.Writes);Assert.Contains("remain unaccepted",f.State.Confirmation);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task FailedOrUnconfirmedRetentionDoesNotSendAndKeepsOriginalOnThisPage(bool readback)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailRetain=!readback;f.Journal.FailRetainReadback=readback;
        await f.State.SubmitCompletionReviewAsync();Assert.NotNull(f.State.CompletionPending);Assert.Contains("No submission",f.State.CompletionPendingExplanation);
        f.Client.Verify(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());Assert.Equal(0,f.Native.Lifecycle.Native.Writes);
    }
    [Fact]
    public async Task LostAcknowledgementReloadsBeforeSourceOrProfileAndRecoversWithReadOnlyUnmappedAuthority()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Native.Lifecycle.Native.FailCommit=true;await f.State.SubmitCompletionReviewAsync();f.Native.Lifecycle.Native.FailCommit=false;
        var original=f.State.CompletionPending!;Assert.True(f.State.CompletionTabCopyConfirmed);f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();
        await f.State.LoadAsync(f.ReviewId);Assert.True(f.State.CanRecoverCompletionReview);Assert.False(f.State.CanPrepareCompletionReview);Assert.Null(f.State.Page);
        await f.State.RecoverCompletionReviewAsync();Assert.True(f.State.CompletionReceipt!.Recorded);Assert.Equal(original.Revision(),f.State.CompletionReceipt.IntentRevision);Assert.Equal(4,f.Native.Lifecycle.Native.Writes);Assert.Null(f.Journal.CompletionReviewOriginal);
        f.Client.Verify(c=>c.GetDailyReportPageAsync(It.IsAny<string>(),It.IsAny<int>(),It.IsAny<int>(),It.IsAny<string?>(),It.IsAny<CancellationToken>()),Times.Never());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());
    }
    [Fact]
    public async Task CleanupFailureKeepsValidatedReceiptAndRecoveryFinishesWithoutAnotherWrite()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailClear=true;await f.State.SubmitCompletionReviewAsync();
        Assert.True(f.State.CompletionReceipt!.Recorded);Assert.NotNull(f.State.CompletionPending);Assert.Contains("Clearing the tab input is not confirmed",f.State.CompletionPendingExplanation);
        f.Journal.FailClear=false;await f.State.RecoverCompletionReviewAsync();Assert.Null(f.State.CompletionPending);Assert.Equal(4,f.Native.Lifecycle.Native.Writes);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task ContextChangesDuringRetentionPreventSendAndNeverClearTheRetainedOriginal(string change)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.BeforeReviewRetain=async()=>{if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);if(change=="route")await f.State.LoadAsync("other");if(change=="dispose")f.State.Dispose();};
        await f.State.SubmitCompletionReviewAsync();Assert.NotNull(f.Journal.CompletionReviewOriginal);Assert.Null(f.State.CompletionPending);f.Client.Verify(c=>c.SubmitCompletionReviewAsync(It.IsAny<DrillingCompletionReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task ActualProtectedJournalEncryptsIsolatesRefusesReplacementAndClearsExactOriginal()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var original=f.State.CompletionPrepared!;var scope=original.Scope();
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainCompletionReviewAsync(original);var saved=Assert.Single(tab.Values);Assert.EndsWith(".drilling-completion-review",saved.Key);Assert.DoesNotContain(original.Reason,saved.Value);
        Assert.Equal(original.Revision(),(await Store().ReadCompletionReviewAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadCompletionReviewAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadCompletionReviewAsync(scope.Company,"other"));Assert.Null(await Store().ReadDailyReportAsync(scope.Company,scope.ActorUserId));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainCompletionReviewAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearCompletionReviewAsync(original with{Reason="Changed reason"}));
        await Store().ClearCompletionReviewAsync(original);Assert.Empty(tab.Values);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task ActualReportPageConfirmsFrozenSubmissionAndRecoversRetainedOriginalAfterReload(bool loseAcknowledgement)
    {
        using var f=new Fixture();await f.Prepare();
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId="+Uri.EscapeDataString(f.ReviewId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(()=>Assert.Contains("Inspect independent completion routing",page.Markup));
        await page.FindAll("button").Single(button=>button.TextContent=="Inspect independent completion routing").ClickAsync(new MouseEventArgs());
        Assert.Contains("Reason for independent completion review",page.Markup);Assert.DoesNotContain("Confirm original independent completion review",page.Markup);
        var panel=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionSubmissionPanel>();
        await page.InvokeAsync(()=>panel.Instance.State.PrepareCompletionReview(f.Routing.Options[0].ProcessId,f.Native.Original.SenderRole,"Signed completion evidence requires independent review"));
        page.WaitForAssertion(()=>Assert.Contains("Confirm original independent completion review",page.Markup));Assert.Contains(f.Native.Original.ClaimId,page.Markup);
        f.Native.Lifecycle.Native.FailCommit=loseAcknowledgement;
        await page.FindAll("button").Single(button=>button.TextContent=="Confirm and retain completion review").ClickAsync(new MouseEventArgs());
        f.Native.Lifecycle.Native.FailCommit=false;
        if(loseAcknowledgement)
        {
            Assert.NotNull(f.Journal.CompletionReviewOriginal);page.Dispose();f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();
            page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
            page.WaitForAssertion(()=>Assert.Contains("Recover original completion review",page.Markup));
            await page.FindAll("button").Single(button=>button.TextContent=="Recover original completion review").ClickAsync(new MouseEventArgs());
            f.Client.Verify(c=>c.GetDailyReportPageAsync(It.IsAny<string>(),It.IsAny<int>(),It.IsAny<int>(),It.IsAny<string?>(),It.IsAny<CancellationToken>()),Times.Never());
            f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());
        }
        page.WaitForAssertion(()=>Assert.Contains("Independent review recorded; first-stage approval tasks: 1",page.Markup));
        Assert.Contains("Completion and test evidence remain unaccepted",page.Markup);Assert.Null(f.Journal.CompletionReviewOriginal);Assert.Equal(4,f.Native.Lifecycle.Native.Writes);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain(f.Native.Original.ClaimId,page.Markup));
    }
}
#endif
