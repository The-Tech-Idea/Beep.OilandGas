using Beep.OilandGas.DevelopmentPlanning.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.Workflow;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentStageScreenTests
{
    private static DevelopmentInvestmentStageReceipt Receipt(DevelopmentInvestmentStageDecision original,string status="RECORDED")
        =>new(original.Revision(),original.Review.InstanceId,status,status=="RECORDED"?"COMPLETED":"IN_PROGRESS",status=="RECORDED"?"approval":null,status=="RECORDED"?"history":null,null,
            status=="FINANCIAL_RECORDED" || status=="RECORDED" && original.Terminal()?DevelopmentInvestmentCommand.Facts(original.FinancialCommand().ToRecord(DateTime.UtcNow)):null,new('E',64),DateTimeOffset.UtcNow);
    private static async Task<DevelopmentInvestmentStageDecision> Prepare(DevelopmentInvestmentAssignedStateTests.Fixture f)
    {
        await f.State.LoadAsync(f.Review.InstanceId);Assert.True(f.State.CanPrepare);
        f.State.PrepareDecision(null,"Explicit independent rationale");Assert.Null(f.State.Prepared);
        f.State.PrepareDecision("APPROVE"," ");Assert.Null(f.State.Prepared);
        f.State.PrepareDecision("APPROVE","Private independent stage rationale / قرار مستقل");return Assert.IsType<DevelopmentInvestmentStageDecision>(f.State.Prepared);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ExactOriginalIsRetainedBeforeTransportAndOnlyValidCompletionClearsIt(string decision)
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();await Prepare(f);f.State.PrepareDecision(decision,"Explicit independent stage decision");var original=f.State.Prepared!;
        f.Source.Base.Client.Setup(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentStageDecision body,CancellationToken _) =>{
            Assert.Equal(original.Revision(),body.Revision());Assert.Equal(body.Revision(),f.Source.Base.Journal.StageOriginal!.Revision());return Task.FromResult(Receipt(body));});
        await f.State.SubmitDecisionAsync();Assert.Null(f.State.Pending);Assert.Null(f.Source.Base.Journal.StageOriginal);Assert.Equal("RECORDED",f.State.Receipt!.Status);Assert.Equal(decision,f.State.Receipt.Financial!.Decision);Assert.False(f.State.CanPrepare);
    }
    [Theory][InlineData("retain")][InlineData("transport")][InlineData("receipt")][InlineData("clear")]
    public async Task UnconfirmedDecisionPreservesItsExactOriginalForReadOnlyRecovery(string fault)
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=await Prepare(f);f.Source.Base.Journal.FailRetain=fault=="retain";f.Source.Base.Journal.FailClear=fault=="clear";
        f.Source.Base.Client.Setup(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentStageDecision body,CancellationToken _) =>fault=="transport"?Task.FromException<DevelopmentInvestmentStageReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Receipt(body) with{IntentRevision=new('F',64)}:Receipt(body)));
        await f.State.SubmitDecisionAsync();Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.Null(f.State.Receipt);Assert.NotNull(f.State.DecisionError);Assert.False(f.State.CanPrepare);await f.State.SubmitDecisionAsync();
        f.Source.Base.Client.Verify(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Source.Base.Journal.FailRetain=false;f.Source.Base.Journal.FailClear=false;f.Source.Base.Client.Setup(c=>c.RecoverAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,fault=="retain"?"ABSENT":"RECORDED"));
        await f.State.RecoverDecisionAsync();Assert.Null(f.State.Pending);Assert.Null(f.Source.Base.Journal.StageOriginal);Assert.NotNull(f.State.DecisionConfirmation);
    }
    [Theory][InlineData("RESERVED")][InlineData("FINANCIAL_RECORDED")]
    public async Task PartialRecoveryKeepsJournalAndOnlyExplicitContinuationReusesTheOriginalIntent(string status)
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=await Prepare(f);f.Source.Base.Journal.StageOriginal=original;
        await f.State.LoadAsync(original.Review.InstanceId);f.Source.Base.Client.Setup(c=>c.RecoverAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,status));
        await f.State.RecoverDecisionAsync();Assert.Equal(status,f.State.Receipt!.Status);Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.True(f.State.CanContinue);
        f.Source.Base.Client.Verify(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Source.Base.Client.Setup(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentStageDecision body,CancellationToken _) =>{Assert.Equal(original.Revision(),body.Revision());return Task.FromResult(Receipt(body));});
        await f.State.ContinueDecisionAsync();Assert.Equal("RECORDED",f.State.Receipt!.Status);Assert.Null(f.Source.Base.Journal.StageOriginal);
    }
    [Fact]public async Task NewViewOnlyOwnerRecoversBeforeReadingAPendingAssignmentOrLoadingPersona()
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=await Prepare(f);f.Source.Base.Journal.StageOriginal=original;f.Source.Select(false);f.State.Dispose();
        using var state=new DevelopmentInvestmentAssignedState(f.Source.Base.Company,f.Personas.Object,f.Source.Base.Client.Object,f.Source.Base.Journal,(_,_,text)=>text);await state.LoadAsync(original.Review.InstanceId);
        Assert.True(state.CanRecover);Assert.False(state.CanPrepare);Assert.False(state.CanContinue);f.Source.Base.Client.Setup(c=>c.RecoverAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original));await state.RecoverDecisionAsync();Assert.Equal("RECORDED",state.Receipt!.Status);
        f.Source.Base.Client.Verify(c=>c.ReadAssignedInvestmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once);f.Personas.Verify(c=>c.EnsureLoadedAsync(),Times.Once);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateCompletionCannotAcknowledgeOrClearOriginalAcrossContextChanges(string change)
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=await Prepare(f);var waiting=new TaskCompletionSource<DevelopmentInvestmentStageReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Source.Base.Client.Setup(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var work=f.State.SubmitDecisionAsync();
        if(change=="company")f.Source.Base.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);else if(change=="dispose")f.State.Dispose();else await f.State.LoadAsync("bad-route");
        waiting.SetResult(Receipt(original));await work;Assert.Null(f.State.Receipt);Assert.Null(f.State.AcknowledgedOriginal);Assert.Equal(original.Revision(),f.Source.Base.Journal.StageOriginal!.Revision());
    }
    [Theory][InlineData("original")][InlineData("absent")][InlineData("foreign")][InlineData("retain")]
    public async Task LostBrowserJournalCanRestoreOnlyItsVerifiedOwnOriginalBeforeAnyWrite(string kind)
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=await Prepare(f);await f.State.LoadAsync(original.Review.InstanceId);Assert.True(f.State.CanRestoreOriginal);f.Source.Base.Journal.FailRetain=kind=="retain";
        var input=new DevelopmentInvestmentStageRecoveryInput(original.Scope().Company,kind=="foreign"?"another":original.Scope().ActorUserId,original.Review.InstanceId,kind=="absent"?null:original,new('D',64));
        f.Source.Base.Client.Setup(c=>c.ReadOwnInvestmentStageOriginalAsync(original.Review.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(input);await f.State.RestoreOriginalAsync();
        if(kind=="original"){Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.Equal(original.Revision(),f.Source.Base.Journal.StageOriginal!.Revision());Assert.True(f.State.CanRecover);}
        else if(kind=="absent"){Assert.Null(f.State.Pending);Assert.Contains("No original stage decision",f.State.DecisionConfirmation);}
        else{Assert.NotNull(f.State.DecisionError);Assert.Null(f.Source.Base.Journal.StageOriginal);}
        f.Source.Base.Client.Verify(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task PersonaInitializationEventDoesNotDiscardJournalReadiness()
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();f.Personas.Setup(p=>p.EnsureLoadedAsync()).Callback(()=>f.Personas.Raise(p=>p.Changed+=null)).Returns(Task.CompletedTask);await Prepare(f);Assert.True(f.State.CanPrepare);
    }
    [Fact]public void ActualReviewerPanelRequiresExplicitSelectionThenDisplaysFrozenConfirmationAndPartialRecovery()
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Source.Base.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Source.Base.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Source.Base.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();
        DevelopmentInvestmentStageDecision? sent=null;f.Source.Base.Client.Setup(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentStageDecision body,CancellationToken _) =>{sent=body;return Task.FromException<DevelopmentInvestmentStageReceipt>(new HttpRequestException());});
        var panel=ui.RenderComponent<DevelopmentInvestmentAssignedPanel>(p=>p.Add(c=>c.InstanceId,f.Review.InstanceId));panel.WaitForAssertion(()=>Assert.Contains("Review this configured stage",panel.Markup));
        panel.FindComponents<MudSelect<string>>().Single().InvokeAsync(()=>panel.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync("REJECT")).GetAwaiter().GetResult();
        panel.FindComponent<MudTextField<string>>().InvokeAsync(()=>panel.FindComponent<MudTextField<string>>().Instance.ValueChanged.InvokeAsync("Independent explicit rationale")).GetAwaiter().GetResult();
        panel.FindAll("button").Single(b=>b.TextContent.Contains("Review stage decision")).Click();Assert.Contains("Confirm original investment stage decision",panel.Markup);Assert.Null(sent);
        panel.FindAll("button").Single(b=>b.TextContent.Contains("Confirm stage decision")).Click();panel.WaitForAssertion(()=>Assert.NotNull(sent));Assert.Contains("Retained original investment stage decision",panel.Markup);
        f.Source.Base.Client.Setup(c=>c.RecoverAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Receipt(sent!,"FINANCIAL_RECORDED"));panel.FindAll("button").Single(b=>b.TextContent.Contains("Recover original stage outcome")).Click();
        panel.WaitForAssertion(()=>Assert.Contains("assignment completion is still pending",panel.Markup));Assert.Contains("Continue original decision",panel.Markup);Assert.DoesNotContain("Recorded investment stage</",panel.Markup);Assert.NotNull(f.Source.Base.Journal.StageOriginal);
    }
}
#endif
