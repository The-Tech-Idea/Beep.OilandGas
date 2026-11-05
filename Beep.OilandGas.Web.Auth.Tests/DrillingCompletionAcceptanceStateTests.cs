#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingCompletionAcceptanceStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingCompletionDecisionStateTests.Fixture Core=new();internal DrillingCompletionAcceptanceNativeTests.Fixture Acceptance=new();
        internal async Task Prepare()
        {
            await Core.Prepare(true);Acceptance.Native=Core.Native;await Acceptance.AddEvidence();
            Core.Client.Setup(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>())).Returns((DrillingCompletionTestSelection selection,CancellationToken _)=>Acceptance.Inspect(selection));
            Core.Client.Setup(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DrillingCompletionReviewDecision command,CancellationToken _)=>{Core.Native.Command=command;return Acceptance.Decide();});
            Core.Client.Setup(c=>c.RecoverCompletionReviewDecisionAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DrillingCompletionReviewDecision command,CancellationToken _)=>{Core.Native.Command=command;return Acceptance.Decide(true);});
        }
        internal Task Inspect(int index)=>Core.State.InspectTestAsync(index,Acceptance.Physical.Row.SOURCE,Acceptance.Physical.Row.TEST_TYPE,Acceptance.Physical.Row.RUN_NUM,"test-"+(index+1));
        internal void Sign()=>Core.State.SignTest("Approved physical acceptance criteria 42","Certificate UTC evidence 42","","Independently inspected the exact native certificate and criteria",true,true);
        internal async Task Freeze()
        {for(int index=0;index<2;index++){await Inspect(index);Sign();}Core.State.PrepareAcceptance(DateTimeOffset.UtcNow.ToString("O"),"Independent physical completion accepted after all source-bound tests and no unresolved work",true);Assert.NotNull(Core.State.PreparedDecision?.Acceptance);}
        public void Dispose()=>Core.Dispose();
    }
    [Theory,InlineData("criteria"),InlineData("passing"),InlineData("utc"),InlineData("utc-reference")]
    public async Task ExplicitCriteriaCertificateUtcAndPassingSignoffAreRequiredPerTest(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect(0);f.Core.State.SignTest(fault=="criteria"?"":"Approved criteria",fault=="utc-reference"?"":"Signed UTC certificate","","Independent test acceptance",fault!="passing",fault!="utc");
        Assert.Empty(f.Core.State.AcceptedTests);Assert.NotNull(f.Core.State.Error);Assert.False(f.Core.State.CanPrepareAcceptance);f.Core.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory,InlineData("local"),InlineData("future"),InlineData("confirmation")]
    public async Task ExplicitActualUtcAndWholeCompletionConfirmationCannotBeGuessed(string fault)
    {
        using var f=new Fixture();await f.Prepare();for(int index=0;index<2;index++){await f.Inspect(index);f.Sign();}
        f.Core.State.PrepareAcceptance(fault=="local"?"2026-10-27T06:00:00":fault=="future"?DateTimeOffset.UtcNow.AddDays(1).ToString("O"):DateTimeOffset.UtcNow.ToString("O"),"Independent completion acceptance",fault!="confirmation");
        Assert.Null(f.Core.State.PreparedDecision);Assert.NotNull(f.Core.State.Error);Assert.Null(f.Core.Journal.CompletionDecisionOriginal);
    }
    [Fact]public async Task EveryTestMustBeIncludedAndReinspectionInvalidatesItsPriorSignoff()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect(0);f.Sign();Assert.False(f.Core.State.CanPrepareAcceptance);await f.Inspect(1);f.Sign();Assert.True(f.Core.State.CanPrepareAcceptance);
        await f.Inspect(0);Assert.Single(f.Core.State.AcceptedTests);Assert.False(f.Core.State.CanPrepareAcceptance);f.Sign();f.Core.State.PrepareAcceptance(DateTimeOffset.UtcNow.ToString("O"),"All independent criteria and source evidence confirmed",true);
        Assert.Equal(2,f.Core.State.PreparedDecision!.Acceptance!.Tests.Count);Assert.Empty(f.Core.State.AcceptedTests);Assert.Null(f.Core.State.TestInspection);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task FailedAcceptanceRetentionOrReadbackCannotSendThePhysicalDecision(bool readback)
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();f.Core.Journal.FailRetain=!readback;f.Core.Journal.FailRetainReadback=readback;await f.Core.State.SubmitDecisionAsync();
        Assert.NotNull(f.Core.State.PendingDecision?.Acceptance);f.Core.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());Assert.Equal(0,f.Core.Native.Mutations);
    }
    [Fact]public async Task KnownAcceptanceReceiptSurvivesCleanupFailureAndExactRecoveryDoesNotRepeatAcceptance()
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();f.Core.Journal.FailClear=true;await f.Core.State.SubmitDecisionAsync();Assert.True(f.Core.State.DecisionReceipt!.CompletionAccepted);Assert.Equal(2,f.Core.State.PendingDecision!.Acceptance!.Tests.Count);Assert.NotNull(f.Core.Journal.CompletionDecisionOriginal);
        var writes=f.Core.Native.Mutations;f.Acceptance.Physical.NullRows=true;f.Core.Journal.FailClear=false;await f.Core.State.RecoverDecisionAsync();Assert.Equal(writes,f.Core.Native.Mutations);Assert.True(f.Core.State.DecisionReceipt!.CompletionAccepted);Assert.Null(f.Core.Journal.CompletionDecisionOriginal);
    }
    [Fact]public async Task ActualProtectedJournalRoundtripsEveryAcceptanceSignoffAndClearsOnlyTheExactOriginal()
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();var original=f.Core.State.PreparedDecision!;var scope=original.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainCompletionDecisionAsync(original);var encrypted=Assert.Single(tab.Values);Assert.DoesNotContain(original.Acceptance!.Tests[0].CriteriaReference,encrypted.Value);Assert.DoesNotContain(original.Acceptance.Tests[1].UtcCertificateReference,encrypted.Value);
        var restored=await Store().ReadCompletionDecisionAsync(scope.Company,scope.ActorUserId);Assert.Equal(original.Revision(),restored!.Revision());Assert.Equal(2,restored.Acceptance!.Tests.Count);Assert.Null(await Store().ReadCompletionDecisionAsync(new("tenant","foreign"),scope.ActorUserId));
        var tests=original.Acceptance.Tests.ToArray();tests[0]=tests[0] with{CriteriaReference="Another independently referenced criterion"};var changed=original with{Acceptance=original.Acceptance with{Tests=tests}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainCompletionDecisionAsync(changed));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearCompletionDecisionAsync(changed));Assert.NotEmpty(tab.Values);await Store().ClearCompletionDecisionAsync(original);Assert.Empty(tab.Values);
    }
    [Fact]public async Task ActualParticipantHistoryShowsImmutableAcceptanceWithoutClaimingCurrentExternalReadiness()
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();await f.Core.State.SubmitDecisionAsync();var progress=await f.Acceptance.Progress();f.Acceptance.Physical.NullRows=true;
        f.Core.Client.Setup(c=>c.GetCompletionReviewProgressAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(progress);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Core.Company);ui.Services.AddSingleton(f.Core.Personas.Object);ui.Services.AddSingleton(f.Core.Client.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/completion-review/history?processId="+Uri.EscapeDataString(progress.InstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionHistory>();
        page.WaitForAssertion(()=>Assert.Contains("Original physical completion acceptance is recorded",page.Markup));Assert.Contains("Approved physical acceptance criteria 42",page.Markup);Assert.Contains("Certificate UTC evidence 42",page.Markup);Assert.Contains("Current external test readiness must be rechecked",page.Markup);Assert.Contains("production release require their own",page.Markup);
        f.Core.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Certificate UTC evidence 42",page.Markup));f.Core.Client.Verify(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>()),Times.Exactly(2));
    }
    [Fact]public async Task LostAcknowledgementReloadsOriginalAcceptanceBeforeUnmappedProfileOrNativeTestLookup()
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();f.Core.Native.Start.Lifecycle.Native.FailCommit=true;await f.Core.State.SubmitDecisionAsync();f.Core.Native.Start.Lifecycle.Native.FailCommit=false;Assert.NotNull(f.Core.Journal.CompletionDecisionOriginal?.Acceptance);
        var writes=f.Core.Native.Mutations;f.Core.ReadOnly();f.Acceptance.Physical.NullRows=true;f.Core.Personas.Invocations.Clear();f.Core.Client.Invocations.Clear();
        using var restarted=new DrillingCompletionAssignedState(f.Core.Company,f.Core.Personas.Object,f.Core.Client.Object,f.Core.Journal,(_,_,message)=>message);await restarted.LoadAsync(f.Core.Native.Command.Review.InstanceId);await restarted.RecoverDecisionAsync();
        Assert.True(restarted.DecisionReceipt!.CompletionAccepted);Assert.Equal(writes,f.Core.Native.Mutations);f.Core.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());f.Core.Client.Verify(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateAcceptanceReceiptCannotPublishOrClearAnotherScope(string context)
    {
        using var f=new Fixture();await f.Prepare();await f.Freeze();var completion=new TaskCompletionSource<DrillingCompletionReviewDecisionReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Core.Client.Setup(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>())).Returns((DrillingCompletionReviewDecision command,CancellationToken _)=>{f.Core.Native.Command=command;return completion.Task;});
        var submission=f.Core.State.SubmitDecisionAsync();Assert.NotNull(f.Core.Journal.CompletionDecisionOriginal?.Acceptance);
        if(context=="company")f.Core.Company.Clear();if(context=="persona")f.Core.Personas.Raise(p=>p.Changed+=null);if(context=="route")await f.Core.State.LoadAsync("another-review");if(context=="dispose")f.Core.State.Dispose();
        completion.SetResult(await f.Acceptance.Decide());await submission;Assert.Null(f.Core.State.DecisionReceipt);Assert.NotNull(f.Core.Journal.CompletionDecisionOriginal?.Acceptance);
    }
    [Fact]public async Task ActualReviewerParentFreezesEveryTestAndConfirmsRetainedPhysicalAcceptance()
    {
        using var f=new Fixture();await f.Prepare();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Core.Company);ui.Services.AddSingleton(f.Core.Personas.Object);ui.Services.AddSingleton(f.Core.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Core.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(WorkflowTaskRoute.For("FIELD",f.Core.Native.Command.Review.InstanceId,"DRILLING_DAILY_REPORT"));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionAssigned>();page.WaitForAssertion(()=>Assert.Contains("Independent physical completion acceptance",page.Markup));
        async Task Text(string label,string value){var input=page.FindComponents<MudTextField<string>>().Single(component=>component.Instance.Label==label);await page.InvokeAsync(()=>input.Instance.ValueChanged.InvokeAsync(value));}
        for(int index=0;index<2;index++)
        {
            var selected=index;var lookup=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionTestPanel>();await page.InvokeAsync(()=>lookup.Instance.State.InspectTestAsync(selected,f.Acceptance.Physical.Row.SOURCE,f.Acceptance.Physical.Row.TEST_TYPE,f.Acceptance.Physical.Row.RUN_NUM,"test-"+(selected+1)));
            await Text("Acceptance criteria reference","Approved criteria for certificate "+index);await Text("Certificate evidence for reported UTC","Signed UTC certificate "+index);await Text("Independent test acceptance reason","Independent native result and certificate verified");
            var panel=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionAcceptancePanel>();var checks=panel.FindComponents<MudCheckBox<bool>>();await page.InvokeAsync(()=>checks[0].Instance.ValueChanged.InvokeAsync(true));await page.InvokeAsync(()=>checks[1].Instance.ValueChanged.InvokeAsync(true));
            await page.FindAll("button").Single(button=>button.TextContent=="Include independently verified test").ClickAsync(new MouseEventArgs());
        }
        await Text("Actual physical acceptance UTC (Z or +00:00)",DateTimeOffset.UtcNow.ToString("O"));await Text("Independent completion acceptance rationale","All checked physical tests and current source completion independently accepted");var confirmation=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionAcceptancePanel>().FindComponents<MudCheckBox<bool>>().Last();await page.InvokeAsync(()=>confirmation.Instance.ValueChanged.InvokeAsync(true));
        await page.FindAll("button").Single(button=>button.TextContent=="Review physical completion acceptance").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Confirm independent physical completion acceptance",page.Markup));Assert.Contains("Original physical completion acceptance evidence",page.Markup);Assert.Contains("Approved criteria for certificate 1",page.Markup);
        await page.FindAll("button").Single(button=>button.TextContent=="Confirm and retain physical completion acceptance").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("original independent physical completion acceptance was confirmed recorded",page.Markup));Assert.True((await f.Acceptance.Progress()).OriginalCompletionAccepted);Assert.Null(f.Core.Journal.CompletionDecisionOriginal);
        f.Core.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Approved criteria for certificate",page.Markup));
    }
}
#endif
