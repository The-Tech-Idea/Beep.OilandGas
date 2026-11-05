using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Globalization;
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
using MudBlazor;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingReportingClosureStateTests
{
    [Theory,InlineData(true),InlineData(false)]
    public async Task ClosedReportingShowsRecordedStartAndLinksCompletionHistoryOnlyForItsParticipants(bool participant)
    {
        using var f=new Fixture();await f.Prepare();f.State.Dispose();var original=f.Original;f.Server=original;
        var status=f.Status();var actor=participant?original.Scope().ActorUserId:original.Review.Responsibility.Root.Workflow.Authority.Recipients.First().UserId;
        Assert.Equal(participant,original.Review.Evidence.Acceptance.IsParticipant(actor));
        if(!participant)
        {
            var source=status.Execution;var basis=source.Basis with{Source=DrillingProgramAssignedReviewTests.AsReader(source.Basis.Source,actor),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};
            basis=basis with{Revision=basis.ContentRevision()};source=source with{Basis=basis};source=source with{Revision=source.ContentRevision()};
            status=status with{Execution=source,CheckedAtUtc=DateTimeOffset.UtcNow};status=status with{Revision=status.ContentRevision()};
            var selected=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),actor,new(selected.Scope,[original.Review.Responsibility.Workflow.Step.AssignedRole],Fixture.View));
        }
        else
        {
            var selected=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),actor,new(selected.Scope,selected.Roles.ToArray(),Fixture.View));
        }
        f.Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=actor,PersonaCode="UNMAPPED"});
        f.Personas.SetupGet(p=>p.CurrentPersona).Returns((AppPersona?)null);
        status.Require(f.Company.Selection!.Scope,actor,original.Review.Responsibility.Root.InstanceId());
        f.Client.Setup(client=>client.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(status);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reporting?assignmentId="+Uri.EscapeDataString(original.Review.Responsibility.Root.InstanceId()));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingReportingResponsibility>();page.WaitForAssertion(()=>Assert.Contains("Recorded reporting closure",page.Markup));
        Assert.Contains("Closed reporting responsibility",page.Markup);Assert.Contains("Last accountable manager",page.Markup);Assert.Contains("Actual reported spud",page.Markup);Assert.Contains(original.Review.Evidence.Acceptance.Source.History.Execution.Recorded!.DRILLING_OPERATION_ID,page.Markup);Assert.Contains("Pending reporting tasks 0",page.Markup);
        var link=page.FindAll("a").SingleOrDefault(row=>row.TextContent.Contains("Open independent completion history",StringComparison.Ordinal));
        if(participant)Assert.Contains(Uri.EscapeDataString(original.Review.Evidence.Acceptance.InstanceId),link!.GetAttribute("href"));else Assert.Null(link);
        Assert.DoesNotContain("Check accepted completion for closure",page.Markup);
    }
    [Fact]
    public async Task RetainedTransferAndClosureEachBlockPreparingTheCompetingCommandBeforeSourceLoading()
    {
        using var f=new Fixture();await f.Prepare();var close=f.Original;var source=close.Review.Evidence.Acceptance.Source.History.Execution;
        var review=new DrillingReportingReassignmentReview(close.Review.Responsibility,source,close.Review.ProcessRevision,close.Review.NativeRevision,close.Review.Responsibility.Workflow,"TransferManager",close.Review.ManagerPersona,close.Review.AuthorityRevision,"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};f.Journal.ReportingReassignmentOriginal=new(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Current manager takeover of continuing reporting",true);
        f.Client.Invocations.Clear();await f.State.LoadAsync(close.Review.Responsibility.Root.InstanceId());Assert.Equal(close.Review.Responsibility.Root.InstanceId(),f.State.BlockedTransferId);Assert.False(f.State.CanInspect);
        f.Client.Verify(c=>c.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Journal.ReportingReassignmentOriginal=null;f.Journal.ReportingClosureOriginal=close;using var transfer=new DrillingReportingReassignmentState(f.Company,f.Personas.Object,f.Client.Object,f.Journal,(_,_,fallback)=>fallback);
        await transfer.LoadAsync(close.Review.Responsibility.Root.InstanceId());Assert.Equal(close.Review.Responsibility.Root.InstanceId(),transfer.BlockedClosureId);Assert.False(transfer.CanReview);
        f.Client.Verify(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var store=new DevelopmentDraftIntentStore(new ProtectedSessionStorage(tab,new EphemeralDataProtectionProvider()));
        var retainedTransfer=new DrillingReportingReassignment(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Retain one exact accountable transfer",true);var scope=close.Scope();
        await store.RetainReportingClosureAsync(close);await Assert.ThrowsAsync<InvalidOperationException>(()=>store.RetainReportingReassignmentAsync(retainedTransfer));Assert.Equal(close.Revision(),(await store.ReadReportingClosureAsync(scope.Company,scope.ActorUserId))!.Revision());
        await store.ClearReportingClosureAsync(close);await store.RetainReportingReassignmentAsync(retainedTransfer);await Assert.ThrowsAsync<InvalidOperationException>(()=>store.RetainReportingClosureAsync(close));Assert.Equal(retainedTransfer.Revision(),(await store.ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId))!.Revision());
        await store.ClearReportingReassignmentAsync(retainedTransfer);Assert.Empty(tab.Values);
    }
    private static readonly Lazy<Task<DrillingReportingClosure>> Baseline=new(async()=>
    {
        var native=new DrillingReportingClosureNativeTests.Fixture();await native.Prepare();return CompanyInventorySourceReview.Copy(native.Command);
    });
    internal sealed class Fixture : IDisposable
    {
        internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();
        internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingReportingClosureState State=null!;internal DrillingReportingClosure Original=null!;internal DrillingReportingClosure? Server;
        internal static string[] View=["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"];
        internal async Task Prepare()
        {
            Original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=Original.Scope();
            Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[Original.Review.ManagerRole],[..View,"Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"]));
            Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=Original.Review.ManagerPersona});
            Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=Original.Review.ManagerPersona});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Client.Setup(c=>c.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(Status);
            Client.Setup(c=>c.ReviewReportingClosureAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Original.Review);
            Client.Setup(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingClosure value,CancellationToken _)=>
            {Assert.Equal(value.Revision(),Journal.ReportingClosureOriginal!.Revision());Server=CompanyInventorySourceReview.Copy(value);return Receipt(value,true);});
            Client.Setup(c=>c.RecoverReportingClosureAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingClosure value,CancellationToken _)=>
            {if(Server is not null)Assert.Equal(value.Revision(),Server.Revision());return Receipt(value,Server is not null);});
            State=NewState();await State.LoadAsync(Original.Review.Responsibility.Root.InstanceId(),Original.Review.Evidence.Acceptance.InstanceId);await State.InspectAsync();Assert.True(State.CanPrepare);
        }
        internal DrillingReportingClosureState NewState()=>new(Company,Personas.Object,Client.Object,Journal,(_,_,message)=>message);
        internal DrillingReportingClosureStatus Status()
        {
            var value=new DrillingReportingClosureStatus(Original.Review.Responsibility,Original.Review.Evidence.Acceptance.Source.History.Execution,[],Server,new('C',64),"",DateTimeOffset.UtcNow);
            return value with{Revision=value.ContentRevision()};
        }
        internal static DrillingReportingClosureReceipt Receipt(DrillingReportingClosure value,bool recorded)=>new(value.Revision(),value.Review.Responsibility.Root.InstanceId(),recorded,recorded?value.HistoryId():null,recorded?value.ClosedAtUtc:null,value.Review.Responsibility.TaskIds(),new('D',64),DateTimeOffset.UtcNow);
        internal string Utc()=>DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",CultureInfo.InvariantCulture);
        internal void Freeze()=>State.Prepare(Utc(),"Original accepted work complete; close ongoing reporting",true);
        internal void ReadOnly()
        {
            var selected=Company.Selection!;Company.TrySelect(Company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),View));
            Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=selected.UserId,PersonaCode="UNMAPPED"});
        }
        public void Dispose(){State?.Dispose();Company.Dispose();}
    }
    [Theory,InlineData("offset"),InlineData("before-review"),InlineData("future"),InlineData("reason"),InlineData("confirmation"),InlineData("read-only"),InlineData("profile")]
    public async Task ExplicitUtcReasonConfirmationAndCurrentManagementProfileAreRequired(string fault)
    {
        using var f=new Fixture();await f.Prepare();var utc=f.Utc();
        if(fault=="offset")utc="2026-10-27T10:00:00+03:00";if(fault=="before-review")utc=f.Original.Review.CheckedAtUtc.AddSeconds(-1).ToString("O",CultureInfo.InvariantCulture);
        if(fault=="future")utc=DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture);
        if(fault=="read-only")f.ReadOnly();if(fault=="profile")f.Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=f.Original.Scope().ActorUserId,PersonaCode="UNMAPPED"});
        f.State.Prepare(utc,fault=="reason"?" ":"Explicit closure reason",fault!="confirmation");Assert.Null(f.State.Prepared);await f.State.SubmitAsync();
        f.Client.Verify(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory,InlineData(false),InlineData(true)]public async Task FailedOrUnconfirmedRetentionNeverTransportsClosure(bool readback)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailRetain=!readback;f.Journal.FailRetainReadback=readback;await f.State.SubmitAsync();
        Assert.NotNull(f.State.Pending);Assert.Contains("No submission",f.State.PendingExplanation);f.Client.Verify(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task LostAcknowledgementReloadsRetainedOriginalBeforeSourceOrProfileAndRecoversWithReadOnlyRights()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();
        f.Client.Setup(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingClosure value,CancellationToken _)=>{Assert.Equal(value.Revision(),f.Journal.ReportingClosureOriginal!.Revision());f.Server=value;throw new HttpRequestException("Lost acknowledgement");});
        await f.State.SubmitAsync();Assert.NotNull(f.Journal.ReportingClosureOriginal);f.State.Dispose();f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();f.State=f.NewState();
        await f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId());Assert.True(f.State.CanRecover);Assert.Null(f.State.Status);
        f.Client.Verify(c=>c.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());
        await f.State.RecoverAsync();Assert.True(f.State.Receipt!.Recorded);Assert.Null(f.Journal.ReportingClosureOriginal);Assert.Contains("confirmed closed",f.State.Confirmation);
        f.Client.Verify(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task ConfirmedReceiptSurvivesCleanupFailureAndExactRecoveryFinishesCleanup()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();f.Journal.FailClear=true;await f.State.SubmitAsync();Assert.True(f.State.Receipt!.Recorded);Assert.NotNull(f.State.Pending);Assert.Contains("Clearing the tab input is not confirmed",f.State.PendingExplanation);
        f.Journal.FailClear=false;await f.State.RecoverAsync();Assert.Null(f.Journal.ReportingClosureOriginal);Assert.Null(f.State.Pending);
        f.Client.Verify(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateReceiptCannotPublishOrClearAnotherContext(string change)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var arrived=new TaskCompletionSource<DrillingReportingClosure>(TaskCreationOptions.RunContinuationsAsynchronously);var answer=new TaskCompletionSource<DrillingReportingClosureReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingClosure value,CancellationToken _)=>{arrived.TrySetResult(value);return answer.Task;});
        var sending=f.State.SubmitAsync();var original=await arrived.Task;
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);if(change=="route")await f.State.LoadAsync(DrillingExecutionAssignment.InstancePrefix+new string('A',64));if(change=="dispose")f.State.Dispose();
        answer.SetResult(Fixture.Receipt(original,true));await sending;Assert.Null(f.State.Receipt);Assert.NotNull(f.Journal.ReportingClosureOriginal);
    }
    [Fact]public async Task ContextChangeDuringRetentionPreventsSubmissionAndPreservesOriginalScopeInput()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var arrived=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Journal.BeforeReviewRetain=async()=>{arrived.SetResult();await release.Task;};var saving=f.State.SubmitAsync();await arrived.Task;f.Company.Clear();release.SetResult();await saving;
        Assert.NotNull(f.Journal.ReportingClosureOriginal);Assert.Null(f.State.Pending);f.Client.Verify(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task ActualProtectedStoreRoundTripsScopeAndRefusesReplacingOrClearingAnotherOriginal()
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=original.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainReportingClosureAsync(original);Assert.Equal(original.Revision(),(await Store().ReadReportingClosureAsync(scope.Company,scope.ActorUserId))!.Revision());
        Assert.Null(await Store().ReadReportingClosureAsync(new("tenant","foreign"),scope.ActorUserId));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainReportingClosureAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearReportingClosureAsync(original with{Reason="Another reason"}));await Store().ClearReportingClosureAsync(original);Assert.Empty(tab.Values);
    }
    [Theory,InlineData(false),InlineData(true)]public async Task ExistingAssignmentNavigationDiscoversOnlyTheSourceBoundCurrentCompletionReview(bool missing)
    {
        using var f=new Fixture();await f.Prepare();var history=f.Original.Review.Evidence.Acceptance.Source.History;
        var routing=new DrillingCompletionReviewReadiness(history,[],[],null,null,"",DateTimeOffset.UtcNow){ExistingReviewInstanceId=missing?null:f.Original.Review.Evidence.Acceptance.InstanceId};routing=routing with{Revision=routing.ContentRevision()};
        f.Client.Setup(c=>c.GetCompletionRoutingAsync(history.Execution.Basis.ReviewInstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(routing);
        await f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId());f.Client.Invocations.Clear();await f.State.InspectAsync();
        if(missing){Assert.Null(f.State.Review);Assert.NotNull(f.State.Error);f.Client.Verify(c=>c.ReviewReportingClosureAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());}
        else{Assert.True(f.State.CanPrepare);Assert.Equal(routing.ExistingReviewInstanceId,f.State.Review!.Evidence.Acceptance.InstanceId);}
        f.Client.Verify(c=>c.GetCompletionRoutingAsync(history.Execution.Basis.ReviewInstanceId,It.IsAny<CancellationToken>()),Times.Once());
    }
    [Fact]public async Task LoadingProfileEventKeepsReadinessBusyAndDoesNotPublishBeforeCurrentEvidenceReturns()
    {
        using var f=new Fixture();await f.Prepare();await f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId(),f.Original.Review.Evidence.Acceptance.InstanceId);
        f.Personas.Setup(p=>p.EnsureLoadedAsync()).Callback(()=>f.Personas.Raise(p=>p.Changed+=null)).Returns(Task.CompletedTask);
        var arrived=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<DrillingReportingClosureReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.ReviewReportingClosureAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns((string _,string __,CancellationToken token)=>{arrived.SetResult();return release.Task;});
        var checking=f.State.InspectAsync();await arrived.Task;Assert.True(f.State.Busy);Assert.False(f.State.CanPrepare);Assert.Null(f.State.Review);release.SetResult(f.Original.Review);await checking;Assert.True(f.State.CanPrepare);Assert.False(f.State.Busy);
    }
    [Fact]public async Task AcceptedNativeCalendarAndUtcEvidenceRemainExplicitUnderArabicCalendarCulture()
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);var decision=original.Review.Evidence.Acceptance.Stages.Last().AcceptedDecision!;var culture=CultureInfo.CurrentCulture;var uiCulture=CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("ar-SA");CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo("ar-SA");using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
            var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionAcceptanceEvidenceView>(p=>p.Add(component=>component.Decision,decision));
            foreach(var test in decision.Acceptance!.Tests)Assert.Contains("Native recorded date "+test.Native.TestDate.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture),panel.Markup);
            Assert.Contains(decision.Acceptance.AcceptedAtUtc.ToString("u",CultureInfo.InvariantCulture),panel.Markup);
        }
        finally{CultureInfo.CurrentCulture=culture;CultureInfo.CurrentUICulture=uiCulture;}
    }
    [Theory,InlineData(false),InlineData(true)]public async Task ActualManagementPageConfirmsOriginalAndRecoversAfterWholeOwnerReload(bool loseAcknowledgement)
    {
        using var f=new Fixture();await f.Prepare();f.State.Dispose();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reporting?assignmentId="+Uri.EscapeDataString(f.Original.Review.Responsibility.Root.InstanceId())+"&completionId="+Uri.EscapeDataString(f.Original.Review.Evidence.Acceptance.InstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingReportingResponsibility>();page.WaitForAssertion(()=>Assert.Contains("Current reporting responsibility",page.Markup));
        await page.FindAll("button").Single(button=>button.TextContent=="Check accepted completion for closure").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Current accepted completion checked",page.Markup));
        await page.FindComponents<MudTextField<string>>().Single(field=>field.Instance.Label=="Actual reporting closure UTC").Find("input").ChangeAsync(new ChangeEventArgs{Value=f.Utc()});
        await page.FindComponents<MudTextField<string>>().Single(field=>field.Instance.Label=="Closure rationale").Find("textarea").ChangeAsync(new ChangeEventArgs{Value="Original accepted work complete; close ongoing reporting"});
        await page.FindComponent<MudCheckBox<bool>>().Find("input").ChangeAsync(new ChangeEventArgs{Value=true});
        await page.FindAll("button").Single(button=>button.TextContent=="Review reporting closure").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Confirm original reporting closure",page.Markup));
        if(loseAcknowledgement)f.Client.Setup(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingClosure value,CancellationToken _)=>{f.Server=value;throw new HttpRequestException("Lost acknowledgement");});
        await page.FindAll("button").Single(button=>button.TextContent=="Confirm and retain reporting closure").ClickAsync(new MouseEventArgs());
        if(loseAcknowledgement)
        {
            page.Dispose();f.ReadOnly();f.Client.Invocations.Clear();f.Personas.Invocations.Clear();page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingReportingResponsibility>();page.WaitForAssertion(()=>Assert.Contains("Recover original reporting closure",page.Markup));
            await page.FindAll("button").Single(button=>button.TextContent=="Recover original reporting closure").ClickAsync(new MouseEventArgs());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());
        }
        page.WaitForAssertion(()=>Assert.Contains("Original reporting closure outcome",page.Markup));Assert.Null(f.Journal.ReportingClosureOriginal);
        await page.FindAll("button").Single(button=>button.TextContent=="Reload reporting status").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Recorded reporting closure",page.Markup));Assert.Contains("Pending reporting tasks 0",page.Markup);Assert.Contains("historical snapshot",page.Markup);
        var selection=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),selection.UserId,new(new("tenant","foreign"),selection.Roles.ToArray(),selection.Permissions.ToArray()));
        page.WaitForAssertion(()=>Assert.DoesNotContain("Original accepted work complete",page.Markup));Assert.Contains("Reporting evidence has not been loaded for this company/account",page.Markup);
    }

    [Fact]
    public async Task ActualManagementPageShowsCurrentCrewManagerAndTransferHistoryInsteadOfInitialCounts()
    {
        using var f=new Fixture();await f.Prepare();f.State.Dispose();var initial=f.Original.Review;var root=initial.Responsibility.Root;
        var destination=root.Workflow with { Step=root.Workflow.Step with { PersonaCodes=["REPLACEMENT-CREW"] },
            Authority=root.Workflow.Authority with { Recipients=[new("replacement-reporter","REPLACEMENT-CREW")],AuthorityRevision=new('E',64) } };
        var preview=new DrillingReportingReassignmentReview(initial.Responsibility,initial.Evidence.Acceptance.Source.History.Execution,initial.ProcessRevision,initial.NativeRevision,destination,
            "ReplacementManagementRole",initial.ManagerPersona,new('E',64),"",DateTimeOffset.UtcNow);
        preview=preview with { Revision=preview.ContentRevision() };var transfer=new DrillingReportingReassignment(preview,Guid.NewGuid(),DateTimeOffset.UtcNow,"Replacement crew takes reporting responsibility",true);transfer.Require();
        var responsibility=DrillingReportingGeneration.After(transfer);
        var review=initial with { Responsibility=responsibility,ManagerRole=responsibility.ManagerRole,AuthorityRevision=new('F',64),CheckedAtUtc=DateTimeOffset.UtcNow };
        review=review with { Revision=review.ContentRevision() };review.Require();
        var status=new DrillingReportingClosureStatus(responsibility,preview.Execution,[transfer],null,new('F',64),"",DateTimeOffset.UtcNow);status=status with { Revision=status.ContentRevision() };
        status.Require(root.Scope().Company,f.Company.Selection!.UserId,root.InstanceId());
        var selected=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),selected.UserId,new(selected.Scope,[responsibility.ManagerRole],selected.Permissions.ToArray()));
        f.Client.Setup(client=>client.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(status);
        f.Client.Setup(client=>client.ReviewReportingClosureAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reporting?assignmentId="+Uri.EscapeDataString(root.InstanceId())+"&completionId="+Uri.EscapeDataString(initial.Evidence.Acceptance.InstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingReportingResponsibility>();
        page.WaitForAssertion(()=>Assert.Contains("Recorded reporting transfers",page.Markup));Assert.Contains("Generation 2",page.Markup);Assert.Contains("ReplacementManagementRole",page.Markup);Assert.Contains("REPLACEMENT-CREW",page.Markup);Assert.Contains("Pending reporting tasks 1",page.Markup);
        await page.FindAll("button").Single(button=>button.TextContent=="Check accepted completion for closure").ClickAsync(new MouseEventArgs());
        page.WaitForAssertion(()=>Assert.Contains("Current accepted completion checked",page.Markup));Assert.Contains("Management role ReplacementManagementRole",page.Markup);
    }
}
#endif
