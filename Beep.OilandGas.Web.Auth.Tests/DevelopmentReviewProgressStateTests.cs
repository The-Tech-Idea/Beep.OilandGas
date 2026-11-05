using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using HistoryPage=Beep.OilandGas.Web.Pages.PPDM39.Development.DevelopmentReviewHistory;
using SourcePage=Beep.OilandGas.Web.Pages.PPDM39.Development.DevelopmentPlanReview;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentReviewProgressStateTests
{
    internal static DevelopmentReviewProgress Progress(DevelopmentPlanSourceReview source,string actor,string state="REVIEW_APPROVED")
    {
        var at=DateTimeOffset.UtcNow;var rejected=state=="REVIEW_REJECTED";
        var stages=new[]{new DevelopmentReviewStageProgress("technical","technical-step",1,"Independent technical review","ConfiguredReviewRole",rejected?"FAILED":"COMPLETED",rejected?"REJECT":"APPROVE","reviewer","Recorded <independent> rationale / مراجعة","approval-audit","decision-history",at)};
        DevelopmentReviewHistory[] history=[new("start-history",null,"PROCESS_STARTED","planner","Review submitted",at.AddMinutes(-1)),new("decision-history","technical-step",rejected?"DEVELOPMENT_REVIEW_REJECTED":"DEVELOPMENT_REVIEW_APPROVED","reviewer",stages[0].Rationale!,at)];
        var result=new DevelopmentReviewProgress(new(source.Context.Company,actor,source.Context.FieldId!,source.Context.Database,source.PlanningDatabase,new("lifecycle","lifecycle-binding")),source.Plan!,source.Revision,
            "development-process","Configured technical review","2.0","planner",state,stages,history,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(source.Context.Company,actor,result.InstanceId);return result;
    }
    private static DevelopmentPlanSourceReview Source(DevelopmentAssignedReview review)=>new(new(review.Scope.Company,review.Scope.CoreDatabase,"reviewer","reader-preference",review.Scope.FieldId),review.Scope.PlanningDatabase,review.Plan.FDP_ID,review.Plan,review.SourceRevision,DateTimeOffset.UtcNow);
    [Fact] public async Task ViewOnlyHistoryIsIndependentOfPendingAssignmentJournalAndActiveField()
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();f.Company.TrySelect(f.Company.Clear(),"reviewer",new(f.Review.Scope.Company,["ConfiguredReviewRole"],["DevelopmentPlanning.View","Workflow.View"]));var progress=Progress(Source(f.Review),"reviewer");
        f.Client.Setup(client=>client.GetDraftReviewProgressAsync(progress.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(progress);
        using var state=new DevelopmentReviewProgressState(f.Company,f.Personas.Object,f.Client.Object,(_,_,text)=>text);await state.LoadAsync(progress.InstanceId);
        Assert.Null(state.Error);Assert.Equal("REVIEW_APPROVED",state.Current!.WorkflowState);state.Current.Plan.FDP_NAME="Consumer mutation";Assert.NotEqual("Consumer mutation",state.Current.Plan.FDP_NAME);
        f.Client.Verify(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);Assert.Null(f.Journal.DecisionOriginal);
    }
    [Theory][InlineData("failed")][InlineData("empty")][InlineData("foreign")][InlineData("changed-source")][InlineData("incomplete-audit")]
    public async Task UnqualifiedHistoryCannotAppearAsSuccessfulRecordedOutcome(string fault)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var progress=Progress(Source(f.Review),"reviewer");
        if(fault=="foreign")progress=progress with{Scope=progress.Scope with{Company=new("tenant","foreign")}};
        if(fault=="changed-source")progress.Plan.FDP_NAME="Changed source";
        if(fault=="incomplete-audit"){progress=progress with{Stages=[progress.Stages[0] with{ApprovalId=null}]};progress=progress with{Revision=progress.ContentRevision()};}
        f.Client.Setup(client=>client.GetDraftReviewProgressAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(fault=="failed"?Task.FromException<DevelopmentReviewProgress>(new HttpRequestException()):Task.FromResult(fault=="empty"?null!:progress));
        using var state=new DevelopmentReviewProgressState(f.Company,f.Personas.Object,f.Client.Object,(_,_,text)=>text);await state.LoadAsync(progress.InstanceId);Assert.Null(state.Current);Assert.NotNull(state.Error);Assert.False(state.Loading);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task LateHistoricalOutcomeCannotPublishAfterContextOrRouteChanges(string change)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var progress=Progress(Source(f.Review),"reviewer");var paused=new TaskCompletionSource<DevelopmentReviewProgress>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(client=>client.GetDraftReviewProgressAsync(progress.InstanceId,It.IsAny<CancellationToken>())).Returns(paused.Task);
        using var state=new DevelopmentReviewProgressState(f.Company,f.Personas.Object,f.Client.Object,(_,_,text)=>text);var load=state.LoadAsync(progress.InstanceId);
        if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(personas=>personas.Changed+=null);else await state.LoadAsync("another-process");
        paused.SetResult(progress);await load;Assert.Null(state.Current);Assert.False(state.Loading);
    }
    [Theory][InlineData("REVIEW_APPROVED")][InlineData("REVIEW_REJECTED")]
    public void ActualHistoryPageShowsRecordedSourceStagesAuditAndTimelineWithoutARecoveryJournal(string outcome)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var progress=Progress(Source(f.Review),"reviewer",outcome);
        f.Client.Setup(client=>client.GetDraftReviewProgressAsync(progress.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(progress);
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/history?processId="+progress.InstanceId);
        var page=ui.RenderComponent<HistoryPage>();page.WaitForAssertion(()=>Assert.Contains("approval-audit",page.Markup));
        Assert.Contains(outcome=="REVIEW_APPROVED"?"All configured review stages approved":"Review rejected",page.Markup);Assert.Contains("Original &lt;review&gt; concept",page.Markup);
        Assert.Contains("Recorded &lt;independent&gt; rationale",page.Markup);Assert.Contains("Recorded review history",page.Markup);Assert.Contains("Review submitted",page.Markup);
        Assert.DoesNotContain("Confirm stage decision",page.Markup);f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("approval-audit",page.Markup));
    }
    private static async Task<DevelopmentPlanSourceReview> Planner(DevelopmentDraftWorkspaceTests.Fixture f)
    {
        await f.Review();f.Rows.Add(DevelopmentDraftWorkspaceTests.Recorded(f.State.Review!));f.Company.TrySelect(f.Company.Clear(),"planner",new(f.Context.Company,["ConfiguredPlanner"],["DevelopmentPlanning.View","DevelopmentPlanning.Create","Workflow.View","Workflow.Start"]));
        await f.State.LoadAsync(f.Rows[0].FDP_ID);return f.State.SourceReview!;
    }
    [Theory][InlineData("recorded")][InlineData("absent")][InlineData("failed")]
    public async Task ActualSenderSourcePageInspectsRecordedReviewAndDistinguishesCheckedAbsenceFromFailure(string result)
    {
        using var f=new DevelopmentDraftWorkspaceTests.Fixture();var source=await Planner(f);var progress=Progress(source,"planner");
        f.Client.Setup(client=>client.GetDraftReviewStatusAsync(source.PlanId,source.Revision,It.IsAny<CancellationToken>())).Returns(result=="failed"?Task.FromException<DevelopmentPlanReviewStatus>(new HttpRequestException()):Task.FromResult(new DevelopmentPlanReviewStatus(source,result=="recorded"?progress:null,DateTimeOffset.UtcNow)));
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Fields.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Store);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/review?planId="+source.PlanId);var page=ui.RenderComponent<SourcePage>();
        page.WaitForAssertion(()=>Assert.Contains("Inspect recorded review outcome",page.Markup));page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect recorded review outcome")).Click();
        page.WaitForAssertion(()=>Assert.Contains(result switch{"recorded"=>"approval-audit","absent"=>"Its absence was checked",_=>"could not be confirmed"},page.Markup));
        if(result=="failed")Assert.DoesNotContain("Its absence was checked",page.Markup);Assert.Contains("New &lt;development&gt; concept",page.Markup);
    }
    [Fact] public async Task RecordedProgressPreventsPreparingAnotherHandoffAndClearsOnFieldChange()
    {
        using var f=new DevelopmentDraftWorkspaceTests.Fixture();var source=await Planner(f);Assert.True(f.State.CanRequestReview);
        f.Client.Setup(client=>client.GetDraftReviewStatusAsync(source.PlanId,source.Revision,It.IsAny<CancellationToken>())).ReturnsAsync(new DevelopmentPlanReviewStatus(source,Progress(source,"planner"),DateTimeOffset.UtcNow));
        await f.State.InspectReviewStatusAsync();Assert.NotNull(f.State.ReviewStatus);Assert.False(f.State.CanRequestReview);f.Fields.Raise(fields=>fields.CurrentFieldChanged+=null,"another-field");Assert.Null(f.State.ReviewStatus);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task SubmissionAndRecoveryClearEarlierCheckedAbsenceInsteadOfDisplayingContradictoryOutcome(bool recover)
    {
        using var f=new DevelopmentReviewWorkspaceTests.Fixture();await f.Review();var source=f.Draft.State.SourceReview!;
        f.Draft.Client.Setup(client=>client.GetDraftReviewStatusAsync(source.PlanId,source.Revision,It.IsAny<CancellationToken>())).ReturnsAsync(new DevelopmentPlanReviewStatus(source,null,DateTimeOffset.UtcNow));
        if(recover){f.FailResponse=true;await f.Draft.State.SubmitHandoffAsync();}
        await f.Draft.State.InspectReviewStatusAsync();Assert.NotNull(f.Draft.State.ReviewStatus);Assert.Null(f.Draft.State.ReviewStatus!.Review);
        if(recover)await f.Draft.State.RecoverHandoffAsync();else await f.Draft.State.SubmitHandoffAsync();
        Assert.Null(f.Draft.State.ReviewStatus);Assert.True(f.Draft.State.HandoffReceipt!.Recorded);
    }
}
