using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using AssignedPage=Beep.OilandGas.Web.Pages.PPDM39.Development.DevelopmentAssignedReview;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentReviewDecisionStateTests
{
    internal static DevelopmentReviewDecisionReceipt Receipt(DevelopmentReviewDecision original,bool recorded=true)=>new(original.Revision(),original.Review.InstanceId,recorded,
        recorded?"approval":null,recorded?"history":null,null,recorded?"REVIEW_APPROVED":"INITIAL",new('D',64),DateTimeOffset.UtcNow);
    [Theory][InlineData("", "Rationale")][InlineData("APPROVE", " ")][InlineData("REJECT", "")]
    public async Task ExplicitChoiceAndRationaleAreRequiredBeforeAnyTransport(string decision,string notes)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();await f.State.LoadAsync(f.Review.InstanceId);f.State.PrepareDecision(decision,notes);
        Assert.Null(f.State.PreparedDecision);Assert.NotNull(f.State.DecisionError);await f.State.SubmitDecisionAsync();Assert.Null(f.Journal.DecisionOriginal);
        f.Client.Verify(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task FrozenOriginalIsRetainedBeforeDecisionTransportAndConfirmedReceiptClearsIt(string decision)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();await f.State.LoadAsync(f.Review.InstanceId);f.State.PrepareDecision(decision,"  Independent rationale  ");
        var original=f.State.PreparedDecision!;Assert.Equal("Independent rationale",original.Notes);original.Review.Plan.FDP_NAME="Consumer mutation";
        f.Client.Setup(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentReviewDecision command,CancellationToken _) =>{
            Assert.Equal(command.Revision(),f.Journal.DecisionOriginal!.Revision());Assert.Equal("Original <review> concept",command.Review.Plan.FDP_NAME);return Task.FromResult(Receipt(command));});
        await f.State.SubmitDecisionAsync();Assert.Null(f.Journal.DecisionOriginal);Assert.Null(f.State.Current);Assert.Null(f.State.PendingDecision);Assert.True(f.State.DecisionReceipt!.Recorded);
        Assert.NotNull(f.State.DecisionConfirmation);Assert.False(f.State.Busy);Assert.False(f.State.CanPrepareDecision);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("receipt")][InlineData("clear")]
    public async Task UncertainSubmissionPreservesOriginalAndNeverSilentlyRetries(string fault)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();await f.State.LoadAsync(f.Review.InstanceId);f.State.PrepareDecision("APPROVE","Independent rationale");var original=f.State.PreparedDecision!;
        f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="clear";
        f.Client.Setup(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentReviewDecision command,CancellationToken _) =>
            fault=="response"?Task.FromException<DevelopmentReviewDecisionReceipt>(new HttpRequestException("Lost response")):Task.FromResult(fault=="receipt"?Receipt(command) with{IntentRevision=new('F',64)}:Receipt(command)));
        await f.State.SubmitDecisionAsync();Assert.Equal(original.Revision(),f.State.PendingDecision!.Revision());Assert.NotNull(f.State.DecisionError);Assert.Null(f.State.DecisionReceipt);
        await f.State.SubmitDecisionAsync();f.Client.Verify(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(client=>client.RecoverDraftReviewDecisionAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,fault!="retain"));
        await f.State.RecoverDecisionAsync();Assert.Null(f.State.PendingDecision);Assert.Null(f.Journal.DecisionOriginal);Assert.NotNull(f.State.DecisionReceipt);Assert.False(f.State.CanPrepareDecision);
    }
    [Fact] public async Task NewPageOwnerRecoversOriginalWithoutRequiringAStillPendingTask()
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var original=new DevelopmentReviewDecision(f.Review,Guid.NewGuid(),"REJECT","Independent rejection");f.Journal.DecisionOriginal=original;
        f.Client.Setup(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Task already closed"));
        f.Client.Setup(client=>client.RecoverDraftReviewDecisionAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original));
        await f.State.LoadAsync(original.Review.InstanceId);Assert.Null(f.State.Error);Assert.True(f.State.CanRecoverDecision);await f.State.RecoverDecisionAsync();
        Assert.True(f.State.DecisionReceipt!.Recorded);f.Client.Verify(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task ContextChangesDuringRetentionStopUnsentDecisionAndRetainRecoveryInput(string change)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();await f.State.LoadAsync(f.Review.InstanceId);f.State.PrepareDecision("APPROVE","Independent rationale");
        var paused=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Journal.BeforeReviewRetain=()=>paused.Task;var submit=f.State.SubmitDecisionAsync();
        if(change=="company")f.Company.Clear();else if(change=="persona"){f.Persona=new(){Code="OTHER"};f.Personas.Raise(personas=>personas.Changed+=null);}else await f.State.LoadAsync("other-process");
        paused.SetResult();await submit;Assert.NotNull(f.Journal.DecisionOriginal);Assert.Null(f.State.DecisionReceipt);
        f.Client.Verify(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact] public async Task AnotherProcessCannotReplacePendingOriginal()
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var original=new DevelopmentReviewDecision(f.Review,Guid.NewGuid(),"APPROVE","Retained original");f.Journal.DecisionOriginal=original;
        await f.State.LoadAsync("other-process");Assert.Equal(original.Review.InstanceId,f.State.RetainedDecisionProcessId);Assert.False(f.State.CanPrepareDecision);Assert.False(f.State.CanRecoverDecision);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task LateDecisionResponseCannotClearRetainedOriginalOrPublishIntoChangedContext(string change)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();await f.State.LoadAsync(f.Review.InstanceId);f.State.PrepareDecision("APPROVE","Independent rationale");var original=f.State.PreparedDecision!;
        var paused=new TaskCompletionSource<DevelopmentReviewDecisionReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).Returns(paused.Task);var submit=f.State.SubmitDecisionAsync();
        Assert.NotNull(f.Journal.DecisionOriginal);
        if(change=="company")f.Company.Clear();else if(change=="persona"){f.Persona=new(){Code="OTHER"};f.Personas.Raise(personas=>personas.Changed+=null);}else await f.State.LoadAsync("other-process");
        paused.SetResult(Receipt(original));await submit;Assert.Equal(original.Revision(),f.Journal.DecisionOriginal!.Revision());Assert.Null(f.State.DecisionReceipt);Assert.Null(f.State.DecisionConfirmation);
    }
    [Theory][InlineData("response")][InlineData("receipt")][InlineData("clear")]
    public async Task UnconfirmedRecoveryCannotDiscardOriginalOrOfferAnotherDecision(string fault)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var original=new DevelopmentReviewDecision(f.Review,Guid.NewGuid(),"REJECT","Original rationale");f.Journal.DecisionOriginal=original;
        await f.State.LoadAsync(original.Review.InstanceId);f.Journal.FailClear=fault=="clear";
        f.Client.Setup(client=>client.RecoverDraftReviewDecisionAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).Returns(()=>fault=="response"?
            Task.FromException<DevelopmentReviewDecisionReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Receipt(original) with{InstanceId="another"}:Receipt(original)));
        await f.State.RecoverDecisionAsync();Assert.Equal(original.Revision(),f.Journal.DecisionOriginal!.Revision());Assert.Equal(original.Revision(),f.State.PendingDecision!.Revision());
        Assert.NotNull(f.State.DecisionError);Assert.False(f.State.CanPrepareDecision);Assert.Null(f.State.DecisionReceipt);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public void ActualReviewerFormRequiresExplicitReviewThenConfirmationAndDisplaysRecordedOutcome(string decision)
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();f.Client.Setup(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentReviewDecision command,CancellationToken _) =>{
            Assert.Equal(decision,command.Decision);Assert.Equal("Independent screen rationale",command.Notes);Assert.NotNull(f.Journal.DecisionOriginal);return Task.FromResult(Receipt(command));});
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/assigned?processId="+f.Review.InstanceId);
        var page=ui.RenderComponent<AssignedPage>();page.WaitForAssertion(()=>Assert.Contains("Record this review stage",page.Markup));
        page.InvokeAsync(()=>page.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync(decision));page.Find("textarea").Change("Independent screen rationale");
        page.FindAll("button").Single(button=>button.TextContent.Contains("Review stage decision")).Click();
        page.WaitForAssertion(()=>Assert.Contains(decision=="APPROVE"?"Confirm stage approval":"Confirm stage rejection",page.Markup));
        Assert.True(page.FindComponent<MudSelect<string>>().Instance.Disabled);Assert.True(page.FindComponent<MudTextField<string>>().Instance.Disabled);
        f.Client.Verify(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Confirm stage decision")).Click();page.WaitForAssertion(()=>Assert.Contains("Approval audit: approval",page.Markup));
        Assert.Contains(decision=="APPROVE"?"Investment and operations release remain separate":"Later review work was not approved or released",page.Markup);f.Client.Verify(client=>client.DecideDraftReviewAsync(It.IsAny<DevelopmentReviewDecision>(),It.IsAny<CancellationToken>()),Times.Once);
    }
}
