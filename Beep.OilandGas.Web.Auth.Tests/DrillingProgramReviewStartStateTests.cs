using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.PermitsAndApplications.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingProgramReviewStartStateTests
{
    internal static DrillingProgramReviewStartReceipt Receipt(DrillingProgramReviewStart original,bool recorded=true)=>new(original.Revision(),original.InstanceId(),recorded,recorded?DrillingProgramReviewStartReceipt.StepId(original.InstanceId(),original.Workflow.Stages[0].Definition.StepId):null,recorded?["recorded-program-task"]:[],new('A',64),DateTimeOffset.UtcNow);
    private static async Task<DrillingProgramReviewStart> Prepare(DrillingProgramDraftStateTests.Fixture f)
    {await f.Prepare();var review=DrillingProgramReviewScreenTests.Enable(f);await f.Inspect();await f.State.ReviewRoutingAsync(review.Source.Program.PROGRAM_ID);f.State.PrepareReview(review.Options[0].ProcessId);return Assert.IsType<DrillingProgramReviewStart>(f.State.ReviewPrepared);}
    [Fact]public async Task OnlyExplicitFrozenConfirmationRetainsTheExactSourceBeforeNativeSubmission()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();var original=await Prepare(f);f.Client.Verify(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());Assert.False(f.State.CanPrepare);f.Client.Setup(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).Returns((DrillingProgramReviewStart input,CancellationToken _) =>{Assert.Equal(input.Revision(),f.Journal.ProgramReviewOriginal!.Revision());return Task.FromResult(Receipt(input));});await f.State.SubmitReviewAsync();Assert.Null(f.Journal.ProgramReviewOriginal);Assert.Null(f.State.ReviewPending);Assert.True(f.State.ReviewReceipt!.Recorded);Assert.Contains("program remains DRAFT",f.State.Confirmation);Assert.Equal(original.InstanceId(),f.State.ReviewReceipt.InstanceId);
    }
    [Theory][InlineData("retain")][InlineData("transport")][InlineData("receipt")][InlineData("clear")]
    public async Task UncertainAssignmentPreservesOriginalAndCannotBlindlyResubmit(string fault)
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();var original=await Prepare(f);f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="clear";
        f.Client.Setup(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).Returns((DrillingProgramReviewStart body,CancellationToken _) =>fault=="transport"?Task.FromException<DrillingProgramReviewStartReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Receipt(body) with{IntentRevision=new('F',64)}:Receipt(body)));
        await f.State.SubmitReviewAsync();Assert.Equal(original.Revision(),f.State.ReviewPending!.Revision());Assert.Null(f.State.ReviewReceipt);Assert.NotNull(f.State.Error);Assert.False(f.State.CanInspect);Assert.False(f.State.CanPrepare);await f.State.SubmitReviewAsync();f.Client.Verify(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(c=>c.RecoverProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,fault!="retain"));await f.State.RecoverReviewAsync();Assert.Null(f.State.ReviewPending);Assert.Null(f.Journal.ProgramReviewOriginal);
    }
    [Fact]public async Task NewReadOnlyOwnerRecoversRetainedReviewBeforeReadingMoreFundingOrRouting()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();var original=await Prepare(f);f.Journal.ProgramReviewOriginal=original;f.Select(false);using var state=f.NewState();await state.LoadAsync(original.Source.Basis.Well.Uwi);Assert.True(state.CanRecoverReview);Assert.False(state.CanSubmitReview);Assert.False(state.CanInspect);f.Client.Setup(c=>c.RecoverProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original));await state.RecoverReviewAsync();Assert.True(state.ReviewReceipt!.Recorded);f.Afes.Verify(c=>c.QueueAsync(It.IsAny<CancellationToken>()),Times.Once);f.Client.Verify(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateSubmissionCannotAcknowledgeOrClearRetainedPrivateInputAfterContextChanges(string change)
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();var original=await Prepare(f);var waiting=new TaskCompletionSource<DrillingProgramReviewStartReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var submit=f.State.SubmitReviewAsync();
        if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(c=>c.Changed+=null);else if(change=="route")await f.State.LoadAsync("other-well");else f.State.Dispose();waiting.SetResult(Receipt(original));await submit;Assert.Null(f.State.ReviewReceipt);Assert.NotNull(f.Journal.ProgramReviewOriginal);
    }
    [Fact]public async Task BothRetainedCommandKindsRecoverIndependentlyAndTheLatestOutcomeOwnsTheScreen()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();var original=await Prepare(f);f.Journal.ProgramReviewOriginal=original;f.Journal.ProgramOriginal=f.Native.Command;using var state=f.NewState();await state.LoadAsync(original.Source.Basis.Well.Uwi);Assert.True(state.CanRecoverReview);Assert.NotNull(state.Pending);
        f.Client.Setup(c=>c.RecoverProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original));await state.RecoverReviewAsync();Assert.NotNull(state.ReviewReceipt);Assert.NotNull(state.Pending);
        f.Client.Setup(c=>c.RecoverProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync(DrillingProgramDraftStateTests.Fixture.Receipt(f.Native.Command));await state.RecoverAsync();Assert.Null(state.ReviewReceipt);Assert.NotNull(state.Receipt!.Recorded);Assert.Equal(f.Native.Command.ProgramId(),state.Receipt.ProgramId);Assert.Null(f.Journal.ProgramReviewOriginal);Assert.Null(f.Journal.ProgramOriginal);
    }
    [Fact]public async Task ActualOperationsParentConfirmsRetainsSubmitsAndShowsOnlyRecordedReceivingAssignment()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();await f.Prepare();var review=DrillingProgramReviewScreenTests.Enable(f);f.Client.Setup(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>())).Returns((DrillingProgramReviewStart body,CancellationToken _) =>{Assert.Equal(body.Revision(),f.Journal.ProgramReviewOriginal!.Revision());return Task.FromResult(Receipt(body));});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/operations/drillingoperations?wellUwi="+Uri.EscapeDataString(f.Native.Current.Well.Uwi));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Operations.DrillingOperations>();page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();page.WaitForAssertion(()=>Assert.Contains("Inspect recorded review and routing for",page.Markup));page.FindAll("button").Single(b=>b.TextContent.StartsWith("Inspect recorded review and routing for")).Click();page.WaitForAssertion(()=>Assert.Contains("Review submission for",page.Markup));page.FindAll("button").Single(b=>b.TextContent.StartsWith("Review submission for")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Confirm original independent program review",page.Markup));Assert.Contains("Independent reviewer program-reviewer",page.Markup);Assert.Null(f.Journal.ProgramReviewOriginal);f.Client.Verify(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());page.FindAll("button").Single(b=>b.TextContent=="Confirm and retain program review").Click();page.WaitForAssertion(()=>Assert.Contains("Original program review outcome",page.Markup));Assert.Contains("1 recorded receiving destinations",page.Markup);Assert.Contains("saved program remains DRAFT",page.Markup);Assert.Null(f.Journal.ProgramReviewOriginal);
    }
}
#endif
