using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Pages.PPDM39.Development;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentReviewWorkspaceTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentDraftWorkspaceTests.Fixture Draft=new();
        internal DevelopmentReviewCatalog Catalog=null!;
        internal DevelopmentReviewStart Original=null!;
        internal int Submissions,Recoveries;
        internal bool FailResponse,MissingResult;
        internal async Task Prepare()
        {
            await Draft.Review();var input=Draft.State.Review!;Draft.Rows.Add(DevelopmentDraftWorkspaceTests.Recorded(input));
            Draft.Company.Clear();Draft.Company.TrySelect(Draft.Company.Version,"planner",new(Draft.Context.Company,["ConfiguredPlanner"],["DevelopmentPlanning.View","DevelopmentPlanning.Create","Workflow.View","Workflow.Start"]));
            var id=Draft.Rows[0].FDP_ID;await Draft.State.LoadAsync(id);var source=Draft.State.SourceReview!;
            var workflow=new DevelopmentReviewWorkflowChoice("technical-review","Configured technical review","2.0",new('A',64),[new("technical","Technical review",1,"TechnicalReviewer",["RESERVOIR_ENGINEER"],24)]);
            var lifecycle=new AssetDatabaseScope("lifecycle","lifecycle-binding");Catalog=new(source,lifecycle,[workflow],DevelopmentReviewCatalog.RevisionFor(source,lifecycle,[workflow]),DateTimeOffset.UtcNow);
            Original=new(source,lifecycle,workflow,[new("technical","TechnicalReviewer",[new("reviewer","RESERVOIR_ENGINEER")],new('B',64))],Guid.NewGuid());Original.Require();
            Draft.Client.Setup(client=>client.GetDraftReviewWorkflowsAsync(id,source.Revision,It.IsAny<CancellationToken>())).ReturnsAsync(Catalog);
            Draft.Client.Setup(client=>client.PrepareDraftReviewAsync(id,It.IsAny<DevelopmentReviewSelection>(),It.IsAny<CancellationToken>())).ReturnsAsync(Original);
            Draft.Client.Setup(client=>client.SubmitDraftReviewAsync(It.IsAny<DevelopmentReviewStart>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentReviewStart original,CancellationToken token)=>{
                Assert.Equal(original.Revision(),Draft.Store.ReviewOriginal?.Revision());Submissions++;if(FailResponse)throw new HttpRequestException("Lost outcome");return Receipt(original,true);});
            Draft.Client.Setup(client=>client.RecoverDraftReviewAsync(It.IsAny<DevelopmentReviewStart>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentReviewStart original,CancellationToken token)=>{Recoveries++;return Receipt(original,!MissingResult);});
            await Draft.State.LoadReviewWorkflowsAsync();
        }
        internal async Task Review(){await Prepare();await Draft.State.PrepareHandoffAsync(Original.Workflow.ProcessId);Assert.NotNull(Draft.State.PreparedHandoff);}
        public void Dispose()=>Draft.Dispose();
    }
    private static DevelopmentReviewStartReceipt Receipt(DevelopmentReviewStart original,bool recorded)=>new(original.Revision(),original.InstanceId(),recorded,
        recorded?"technical-step":null,recorded?["persona-task"]:[],new('C',64),DateTimeOffset.UtcNow);
    [Fact] public async Task PreparedReviewRetainsBeforeTransportAndConfirmsInitialHandoffWithoutApproval()
    {
        using var f=new Fixture();await f.Review();await f.Draft.State.SubmitHandoffAsync();Assert.Equal(1,f.Submissions);Assert.Null(f.Draft.Store.ReviewOriginal);
        Assert.True(f.Draft.State.HandoffReceipt!.Recorded);Assert.Contains("Independent approval is still required",f.Draft.State.HandoffConfirmation);Assert.False(f.Draft.State.CanRequestReview);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("clear")]
    public async Task UncertainSubmissionCannotBeRepeatedAndOriginalRecoveryIsReadOnly(string fault)
    {
        using var f=new Fixture();await f.Review();f.FailResponse=fault=="response";f.Draft.Store.FailRetain=fault=="retain";f.Draft.Store.FailClear=fault=="clear";
        await f.Draft.State.SubmitHandoffAsync();Assert.NotNull(f.Draft.State.PendingHandoff);Assert.NotNull(f.Draft.State.HandoffError);Assert.Null(f.Draft.State.HandoffConfirmation);
        await f.Draft.State.SubmitHandoffAsync();Assert.Equal(fault=="retain"?0:1,f.Submissions);
        f.Draft.Store.FailClear=false;f.MissingResult=fault=="retain";await f.Draft.State.RecoverHandoffAsync();Assert.Equal(1,f.Recoveries);Assert.Null(f.Draft.State.PendingHandoff);
        Assert.NotNull(f.Draft.State.HandoffConfirmation);Assert.Equal(fault=="retain"?0:1,f.Submissions);
    }
    [Fact] public async Task RetainedOriginalSurvivesPageOwnerReloadAndAnotherDraftCannotReplaceIt()
    {
        using var f=new Fixture();await f.Review();f.FailResponse=true;await f.Draft.State.SubmitHandoffAsync();
        f.Draft.State.Dispose();f.Draft.State=new(f.Draft.Company,f.Draft.Fields.Object,f.Draft.Client.Object,f.Draft.Store,(_,_,text)=>text);
        await f.Draft.State.LoadAsync(f.Original.Source.PlanId);Assert.Equal(f.Original.Revision(),f.Draft.State.PendingHandoff!.Revision());Assert.True(f.Draft.State.CanRecoverHandoff);
        await f.Draft.State.LoadAsync("other-draft");Assert.Null(f.Draft.State.PendingHandoff);Assert.Equal(f.Original.Source.PlanId,f.Draft.State.RetainedReviewPlanId);Assert.False(f.Draft.State.CanRequestReview);Assert.False(f.Draft.State.CanRecoverHandoff);
        await f.Draft.State.LoadAsync(f.Original.Source.PlanId);await f.Draft.State.RecoverHandoffAsync();Assert.Equal(1,f.Submissions);Assert.Equal(1,f.Recoveries);
    }
    [Fact] public async Task LatePreparedResultCannotPublishAfterCompanyChange()
    {
        using var f=new Fixture();await f.Prepare();var delayed=new TaskCompletionSource<DevelopmentReviewStart>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Draft.Client.Setup(client=>client.PrepareDraftReviewAsync(It.IsAny<string>(),It.IsAny<DevelopmentReviewSelection>(),It.IsAny<CancellationToken>())).Returns(delayed.Task);
        var pending=f.Draft.State.PrepareHandoffAsync(f.Original.Workflow.ProcessId);f.Draft.Company.Clear();delayed.SetResult(f.Original);await pending;
        Assert.Null(f.Draft.State.PreparedHandoff);Assert.Null(f.Draft.State.SourceReview);Assert.False(f.Draft.State.Busy);Assert.Equal(0,f.Submissions);
    }
    [Fact] public async Task ForeignLifecyclePreparationCannotReplaceDisplayedReview()
    {
        using var f=new Fixture();await f.Prepare();f.Draft.Client.Setup(client=>client.PrepareDraftReviewAsync(It.IsAny<string>(),It.IsAny<DevelopmentReviewSelection>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(f.Original with{LifecycleDatabase=new("foreign","foreign-binding")});
        await f.Draft.State.PrepareHandoffAsync(f.Original.Workflow.ProcessId);Assert.Null(f.Draft.State.PreparedHandoff);Assert.NotNull(f.Draft.State.HandoffError);Assert.Equal(0,f.Submissions);
    }
    [Fact] public async Task RouteChangeWhileRetainingOriginalCannotStartAnUnsentSubmission()
    {
        using var f=new Fixture();await f.Review();var retained=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Draft.Store.BeforeReviewRetain=()=>retained.Task;var submit=f.Draft.State.SubmitHandoffAsync();await f.Draft.State.LoadAsync("another-draft");
        retained.SetResult();await submit;Assert.Equal(0,f.Submissions);Assert.Null(f.Draft.State.SourceReview);Assert.Null(f.Draft.State.HandoffReceipt);
        Assert.Equal(f.Original.Revision(),f.Draft.Store.ReviewOriginal?.Revision());Assert.False(f.Draft.State.Busy);
    }
    [Fact] public async Task ActualReviewPageDrivesPrepareConfirmSubmitAndShowsRecordedIndependentAssignment()
    {
        using var f=new Fixture();await f.Prepare();using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Draft.Company);ui.Services.AddSingleton(f.Draft.Fields.Object);ui.Services.AddSingleton(f.Draft.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Draft.Store);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/review?planId="+Uri.EscapeDataString(f.Original.Source.PlanId));
        var page=ui.RenderComponent<DevelopmentPlanReview>();page.WaitForAssertion(()=>Assert.Contains("Inspect review workflows",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect review workflows")).Click();page.WaitForAssertion(()=>Assert.Contains("Prepare review request",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Prepare review request")).Click();page.WaitForAssertion(()=>Assert.Contains("Confirm independent review request",page.Markup));
        Assert.Contains("1 independent reviewer",page.Markup);page.FindAll("button").Single(button=>button.TextContent.Contains("Submit review request")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Development review requested",page.Markup));Assert.Equal(1,f.Submissions);Assert.Contains("Independent approval is still required",page.Markup);
    }
}
