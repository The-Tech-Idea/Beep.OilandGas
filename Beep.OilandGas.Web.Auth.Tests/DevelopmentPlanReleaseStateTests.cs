using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using HistoryPage=Beep.OilandGas.Web.Pages.PPDM39.Development.DevelopmentReviewHistory;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentPlanReleaseStateTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentAssignedReviewStateTests.Fixture Reader=new();
        internal DevelopmentReviewProgress Review;
        internal DevelopmentPlanReleaseState State;
        internal Fixture()
        {
            var assigned=Reader.Review;var source=new DevelopmentPlanSourceReview(new(assigned.Scope.Company,assigned.Scope.CoreDatabase,"reviewer","read-preference",assigned.Scope.FieldId),assigned.Scope.PlanningDatabase,assigned.Plan.FDP_ID,assigned.Plan,assigned.SourceRevision,DateTimeOffset.UtcNow);
            Review=DevelopmentReviewProgressStateTests.Progress(source,"reviewer");
            Reader.Client.Setup(client=>client.GetDraftReviewProgressAsync(Review.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            Reader.Client.Setup(client=>client.GetDraftBaselineAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>new DevelopmentPlanBaselineReview(Review,null,DateTimeOffset.UtcNow));
            State=new(Reader.Company,Reader.Personas.Object,Reader.Client.Object,Reader.Journal,(_,_,text)=>text);
        }
        internal async Task Prepare(){await State.LoadAsync(Review);await State.InspectAsync();State.Prepare("Independent baseline approval rationale");Assert.NotNull(State.Prepared);}
        internal static DevelopmentPlanReleaseReceipt Receipt(DevelopmentPlanRelease original,bool recorded=true)=>new(original.Revision(),new(original.Review,recorded?original.ToRecord(DateTime.UtcNow):null,DateTimeOffset.UtcNow));
        public void Dispose(){State.Dispose();Reader.Dispose();}
    }
    [Fact] public async Task CurrentCheckedAbsenceAndExplicitRationaleAreRequiredBeforePreparingApproval()
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review);Assert.False(f.State.CanPrepare);await f.State.InspectAsync();f.State.Prepare(" ");Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);
        f.State.Prepare("Explicit baseline rationale");Assert.NotNull(f.State.Prepared);Assert.Null(f.Reader.Journal.ReleaseOriginal);
        f.Reader.Client.Verify(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact] public async Task OriginalIsRetainedBeforeTransportAndCanonicalBaselineReceiptConfirmsVersionAndLineage()
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;
        f.Reader.Client.Setup(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).Returns((DevelopmentPlanRelease command,CancellationToken _) =>{
            Assert.Equal(command.Revision(),f.Reader.Journal.ReleaseOriginal!.Revision());return Task.FromResult(Fixture.Receipt(command));});
        await f.State.SubmitAsync();Assert.Null(f.Reader.Journal.ReleaseOriginal);Assert.Null(f.State.Pending);Assert.NotNull(f.State.Result!.Baseline);Assert.False(f.State.CanPrepare);
        Assert.Equal(2,f.State.Result.Baseline!.FDP_VERSION);Assert.Equal(original.Review.Plan.FDP_ID,f.State.Result.Baseline.SOURCE_DRAFT_FDP_ID);Assert.Equal("DRAFT",f.Review.Plan.FDP_STATUS);
        Assert.Contains("AFE and investment release remain separate",f.State.Confirmation);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("receipt")][InlineData("clear")]
    public async Task UncertainApprovalPreservesOriginalAndBlocksSilentDuplicate(string fault)
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;f.Reader.Journal.FailRetain=fault=="retain";f.Reader.Journal.FailClear=fault=="clear";
        f.Reader.Client.Setup(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).Returns((DevelopmentPlanRelease command,CancellationToken _) =>
            fault=="response"?Task.FromException<DevelopmentPlanReleaseReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Fixture.Receipt(command) with{IntentRevision=new('F',64)}:Fixture.Receipt(command)));
        await f.State.SubmitAsync();Assert.NotNull(f.State.Pending);Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.Null(f.State.Result);Assert.NotNull(f.State.Error);await f.State.SubmitAsync();
        f.Reader.Client.Verify(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Reader.Journal.FailRetain=false;f.Reader.Journal.FailClear=false;f.Reader.Client.Setup(client=>client.RecoverDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original,fault!="retain"));
        await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.Null(f.Reader.Journal.ReleaseOriginal);Assert.NotNull(f.State.Confirmation);Assert.False(f.State.CanPrepare);
    }
    [Fact] public async Task NewOwnerRecoversOriginalWithViewOnlyPermissionsAndNoApprovalOrPendingTaskRead()
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;f.Reader.Journal.ReleaseOriginal=original;
        f.Reader.Company.TrySelect(f.Reader.Company.Clear(),"reviewer",new(f.Review.Scope.Company,["ConfiguredReviewRole"],["DevelopmentPlanning.View","Workflow.View"]));
        f.State.Dispose();f.State=new(f.Reader.Company,f.Reader.Personas.Object,f.Reader.Client.Object,f.Reader.Journal,(_,_,text)=>text);await f.State.LoadAsync(f.Review);Assert.True(f.State.CanRecover);Assert.False(f.State.CanPrepare);
        f.Reader.Client.Setup(client=>client.RecoverDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original));
        await f.State.RecoverAsync();Assert.NotNull(f.State.Result!.Baseline);Assert.Null(f.Reader.Journal.ReleaseOriginal);
        f.Reader.Client.Verify(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task ContextChangeDuringRetentionStopsUnsentApprovalAndKeepsRecoveryInput(string change)
    {
        using var f=new Fixture();await f.Prepare();var paused=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Reader.Journal.BeforeReviewRetain=()=>paused.Task;
        var submit=f.State.SubmitAsync();if(change=="company")f.Reader.Company.Clear();else if(change=="persona")f.Reader.Personas.Raise(personas=>personas.Changed+=null);else {var other=f.Review with{InstanceId="other"};other=other with{Revision=other.ContentRevision()};await f.State.LoadAsync(other);}
        paused.SetResult();await submit;Assert.NotNull(f.Reader.Journal.ReleaseOriginal);Assert.Null(f.State.Result);
        f.Reader.Client.Verify(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task LateApprovalResponseCannotPublishOrClearOriginalAfterContextChanges(string change)
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;var paused=new TaskCompletionSource<DevelopmentPlanReleaseReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Reader.Client.Setup(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).Returns(paused.Task);var submit=f.State.SubmitAsync();
        if(change=="company")f.Reader.Company.Clear();else if(change=="persona")f.Reader.Personas.Raise(personas=>personas.Changed+=null);else {var other=f.Review with{InstanceId="other"};other=other with{Revision=other.ContentRevision()};await f.State.LoadAsync(other);}
        paused.SetResult(Fixture.Receipt(original));await submit;Assert.NotNull(f.Reader.Journal.ReleaseOriginal);Assert.Null(f.State.Result);Assert.Null(f.State.Confirmation);
    }
    [Theory][InlineData("failure")][InlineData("corrupt")]
    public async Task FailedOrCorruptBaselineReadCannotBecomeAbsenceOrEnableApproval(string fault)
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review);var original=new DevelopmentPlanRelease(f.Review,Guid.NewGuid(),"Original approval");var row=original.ToRecord(DateTime.UtcNow);row.APPROVED_BY="another";
        f.Reader.Client.Setup(client=>client.GetDraftBaselineAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(fault=="failure"?Task.FromException<DevelopmentPlanBaselineReview>(new HttpRequestException()):Task.FromResult(new DevelopmentPlanBaselineReview(f.Review,row,DateTimeOffset.UtcNow)));
        await f.State.InspectAsync();Assert.Null(f.State.Result);Assert.NotNull(f.State.Error);Assert.False(f.State.CanPrepare);
    }
    [Fact] public async Task RetainedApprovalForAnotherReviewCannotBeOverwritten()
    {
        using var f=new Fixture();f.Reader.Journal.ReleaseOriginal=new(f.Review,Guid.NewGuid(),"Original");var other=f.Review with{InstanceId="other"};other=other with{Revision=other.ContentRevision()};await f.State.LoadAsync(other);
        Assert.Equal(f.Review.InstanceId,f.State.RetainedProcessId);Assert.False(f.State.CanPrepare);Assert.False(f.State.CanRecover);
    }
    [Fact] public void ActualHistoryPageInspectsReviewsConfirmsAndDisplaysTheRecordedApprovedBaseline()
    {
        using var f=new Fixture();f.Reader.Client.Setup(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>())).Returns((DevelopmentPlanRelease command,CancellationToken _) =>{
            Assert.NotNull(f.Reader.Journal.ReleaseOriginal);Assert.Equal("Actual screen approval rationale",command.Rationale);return Task.FromResult(Fixture.Receipt(command));});
        using var ui=Ui(f);var page=ui.RenderComponent<HistoryPage>();page.WaitForAssertion(()=>Assert.Contains("Inspect FDP baseline",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect FDP baseline")).Click();page.WaitForAssertion(()=>Assert.Contains("Its absence was checked",page.Markup));
        page.Find("textarea").Change("Actual screen approval rationale");page.FindAll("button").Single(button=>button.TextContent.Contains("Review FDP baseline approval")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Confirm FDP baseline approval",page.Markup));Assert.True(page.Find("textarea").HasAttribute("disabled"));
        f.Reader.Client.Verify(client=>client.ApproveDraftBaselineAsync(It.IsAny<DevelopmentPlanRelease>(),It.IsAny<CancellationToken>()),Times.Never);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Confirm approved FDP baseline")).Click();page.WaitForAssertion(()=>Assert.Contains("Approved FDP baseline",page.Markup));
        Assert.Contains("Version 2",page.Markup);Assert.Contains("Recorded draft",page.Markup);Assert.Contains("Approved by: reviewer",page.Markup);Assert.Contains("Actual screen approval rationale",page.Markup);Assert.DoesNotContain("Its absence was checked",page.Markup);
        f.Reader.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Approved by: reviewer",page.Markup));
    }
    [Fact] public void ViewOnlyHistoryPageCanInspectApprovedBaselineWithoutShowingApprovalForm()
    {
        using var f=new Fixture();var command=new DevelopmentPlanRelease(f.Review,Guid.NewGuid(),"Recorded independent approval");var baseline=command.ToRecord(DateTime.UtcNow);
        f.Reader.Company.TrySelect(f.Reader.Company.Clear(),"reviewer",new(f.Review.Scope.Company,["ConfiguredReviewRole"],["DevelopmentPlanning.View","Workflow.View"]));
        f.Reader.Client.Setup(client=>client.GetDraftBaselineAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DevelopmentPlanBaselineReview(f.Review,baseline,DateTimeOffset.UtcNow));
        using var ui=Ui(f);var page=ui.RenderComponent<HistoryPage>();page.WaitForAssertion(()=>Assert.Contains("Inspect FDP baseline",page.Markup));page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect FDP baseline")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Approved by: reviewer",page.Markup));Assert.DoesNotContain("Review FDP baseline approval",page.Markup);
    }
    private static Bunit.TestContext Ui(Fixture f)
    {
        var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Reader.Company);ui.Services.AddSingleton(f.Reader.Personas.Object);ui.Services.AddSingleton(f.Reader.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Reader.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/history?processId="+f.Review.InstanceId);return ui;
    }
}
