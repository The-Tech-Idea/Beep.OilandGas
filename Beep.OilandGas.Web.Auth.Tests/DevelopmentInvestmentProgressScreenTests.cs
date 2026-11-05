#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentProgressScreenTests
{
    internal static async Task<DevelopmentInvestmentHandoffStatus> Status(DevelopmentInvestmentStageNativeTests.Fixture f)
    {
        var original=f.Handoff.Original;var progress=await DevelopmentInvestmentProgressTests.Read(f);var process=(await f.Handoff.Native.Service.GetProcessInstanceAsync(original.InstanceId()))!;
        var source=f.Handoff.Current with{Recorded=f.Current.Recorded};var result=new DevelopmentInvestmentHandoffStatus(source,original.InstanceId(),original.Workflow.ProcessName,original.Scope().ActorUserId,
            new DateTimeOffset(DateTime.SpecifyKind(process.StartDate,DateTimeKind.Utc)),progress,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(source.Current.Source.Review.Scope.Company,source.Current.Source.Review.Scope.ActorUserId,source.Current.Source.Review.InstanceId,source.AfeId);return result;
    }
    internal static TestContext Ui(DevelopmentInvestmentStageNativeTests.Fixture f)
    {
        var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Handoff.Source.Base.Company);ui.Services.AddSingleton(f.Handoff.Source.Base.Baseline.Reader.Personas.Object);ui.Services.AddSingleton(f.Handoff.Source.Base.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Handoff.Source.Base.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));return ui;
    }
    [Theory][InlineData("IN_REVIEW")][InlineData("RESERVED")][InlineData("FINANCIAL_RECORDED")][InlineData("APPROVED")][InlineData("REJECTED")][InlineData("RECONCILIATION_REQUIRED")]
    public async Task ActualReadOnlySourcePanelDistinguishesEachNativeFinancialStateWithoutWrites(string state)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(1,state=="REJECTED"?"REJECT":"APPROVE");
        if(state is "RESERVED" or "FINANCIAL_RECORDED" or "APPROVED" or "REJECTED")await f.Reserve();
        if(state is "FINANCIAL_RECORDED" or "APPROVED" or "REJECTED")f.Financial();
        if(state is "APPROVED" or "REJECTED")await f.Complete();
        if(state=="RECONCILIATION_REQUIRED"){var other=f.Command with{IntentId=Guid.NewGuid()};f.Current=f.Current with{Recorded=DevelopmentInvestmentCommand.Facts(other.FinancialCommand().ToRecord(DateTime.UtcNow))};}
        var status=await Status(f);var scope=status.Source.Current.Source.Review.Scope;
        f.Handoff.Source.Base.Company.TrySelect(f.Handoff.Source.Base.Company.Clear(),scope.ActorUserId,new(scope.Company,["InvestmentReader"],["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>status);var writes=f.Mutations;
        using var ui=Ui(f);var panel=ui.RenderComponent<DevelopmentInvestmentHandoffPanel>(p=>p.Add(c=>c.ProcessId,status.Source.Current.Source.Review.InstanceId).Add(c=>c.AfeId,status.Source.AfeId));
        panel.FindAll("button").Single(b=>b.TextContent=="Read recorded investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Investment review progress",panel.Markup));
        Assert.Contains("Stage 1 of 1",panel.Markup);Assert.Contains("Role InvestmentReviewer",panel.Markup);Assert.Contains("Workspaces FINANCE",panel.Markup);Assert.Contains("Pending review tasks: "+status.Progress!.PendingTasks,panel.Markup);
        if(state=="IN_REVIEW")Assert.Contains("Intermediate approvals do not establish final budget approval",panel.Markup);
        else if(state=="RESERVED")Assert.Contains("It is not confirmed complete",panel.Markup);
        else if(state=="FINANCIAL_RECORDED")Assert.Contains("native workflow completion is still pending",panel.Markup);
        else if(state=="RECONCILIATION_REQUIRED")Assert.Contains("Final workflow completion is not confirmed",panel.Markup);
        else{Assert.Contains(state=="APPROVED"?"Final investment approval":"Investment rejection",panel.Markup);Assert.Contains("Reviewer independent-reviewer",panel.Markup);Assert.Contains(f.Command.Rationale,panel.Markup);Assert.Contains("Approval investment-stage-approval-",panel.Markup);}
        Assert.DoesNotContain("Receiving task destinations:",panel.Markup);Assert.Equal(writes,f.Mutations);
        f.Handoff.Source.Base.Client.Verify(c=>c.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>()),Times.Never());f.Handoff.Source.Base.Client.Verify(c=>c.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Handoff.Source.Base.Company.Clear();panel.WaitForAssertion(()=>Assert.DoesNotContain("Investment review progress",panel.Markup));
    }
    [Fact]public async Task ActualAfeParentOpensTheTimelineForItsExactOriginalSource()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(2);await f.Reserve();await f.Complete();var status=await Status(f);f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(status);
        using var ui=Ui(f);ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/economics/afe?processId="+Uri.EscapeDataString(status.Source.Current.Source.Review.InstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Economics.AFEManagement>();
        page.WaitForAssertion(()=>Assert.Contains("Read recorded investment handoff",page.Markup));page.FindAll("button").Single(b=>b.TextContent=="Read recorded investment handoff").Click();page.WaitForAssertion(()=>Assert.Contains("Investment review progress",page.Markup));Assert.Contains(status.Source.Afe().Number,page.Markup);Assert.Contains("Stage 2 of 2",page.Markup);Assert.Contains("Pending review tasks: 1",page.Markup);Assert.Contains("Reviewer independent-reviewer",page.Markup);
        f.Handoff.Source.Base.Client.Verify(c=>c.ReadInvestmentHandoffAsync(status.Source.Current.Source.Review.InstanceId,status.Source.AfeId,It.IsAny<CancellationToken>()),Times.Once);
    }
    [Fact]public async Task FinancialEvidenceWithoutANativeAssignmentCannotLookLikeWorkflowCompletion()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare();f.Financial();var status=await Status(f);status=status with{HandoffId=null,WorkflowName=null,Sender=null,StartedAtUtc=null,Progress=null};status=status with{Revision=status.ContentRevision()};
        f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(status);using var ui=Ui(f);var panel=ui.RenderComponent<DevelopmentInvestmentHandoffPanel>(p=>p.Add(c=>c.ProcessId,status.Source.Current.Source.Review.InstanceId).Add(c=>c.AfeId,status.Source.AfeId));panel.FindAll("button").Single(b=>b.TextContent=="Read recorded investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Native review completion requires reconciliation",panel.Markup));Assert.DoesNotContain("Investment review progress",panel.Markup);Assert.DoesNotContain("Final investment approval and native workflow/task completion are confirmed",panel.Markup);
    }
    [Fact]public async Task ActualTimelineKeepsOrderedStageAuditAndEncodesOriginalRationale()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(3);f.Command=f.Command with{Rationale="Original stage audit <script>private text</script>"};await f.Reserve();await f.Complete();var status=await Status(f);
        f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(status);using var ui=Ui(f);var panel=ui.RenderComponent<DevelopmentInvestmentHandoffPanel>(p=>p.Add(c=>c.ProcessId,status.Source.Current.Source.Review.InstanceId).Add(c=>c.AfeId,status.Source.AfeId));
        panel.FindAll("button").Single(b=>b.TextContent=="Read recorded investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Stage 3 of 3",panel.Markup));Assert.Contains("Stage 1 of 3",panel.Markup);Assert.Contains("Stage 2 of 3",panel.Markup);Assert.Contains("Approved stage",panel.Markup);Assert.Contains("Awaiting independent review",panel.Markup);Assert.Contains("Not reached",panel.Markup);Assert.Contains("Pending review tasks: 1",panel.Markup);Assert.DoesNotContain("<script>",panel.Markup);Assert.Contains("&lt;script&gt;private text&lt;/script&gt;",panel.Markup);Assert.DoesNotContain("Final investment approval and native workflow/task completion are confirmed",panel.Markup);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateStatusTimelineCannotPublishAfterItsSourceContextChanges(string change)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare();var status=await Status(f);var scope=status.Source.Current.Source.Review.Scope;
        var waiting=new TaskCompletionSource<DevelopmentInvestmentHandoffStatus>(TaskCreationOptions.RunContinuationsAsynchronously);f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);
        using var state=new DevelopmentInvestmentHandoffState(f.Handoff.Source.Base.Company,f.Handoff.Source.Base.Baseline.Reader.Personas.Object,f.Handoff.Source.Base.Client.Object,f.Handoff.Source.Base.Journal,(_,_,text)=>text);await state.LoadAsync(status.Source.Current.Source.Review.InstanceId,status.Source.AfeId);var read=state.ReadStatusAsync();
        if(change=="company")f.Handoff.Source.Base.Company.Clear();else if(change=="persona")f.Handoff.Source.Base.Baseline.Reader.Personas.Raise(p=>p.Changed+=null);else if(change=="route")await state.LoadAsync("other-process","other-afe");else state.Dispose();waiting.SetResult(status);await read;Assert.Null(state.Status);
    }
    [Fact]public async Task InvalidProjectionCannotReplacePreviouslyConfirmedStatusOrAppearAsZeroPending()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare();var status=await Status(f);f.Handoff.Source.Base.Client.Setup(c=>c.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>status);
        using var state=new DevelopmentInvestmentHandoffState(f.Handoff.Source.Base.Company,f.Handoff.Source.Base.Baseline.Reader.Personas.Object,f.Handoff.Source.Base.Client.Object,f.Handoff.Source.Base.Journal,(_,_,text)=>text);await state.LoadAsync(status.Source.Current.Source.Review.InstanceId,status.Source.AfeId);await state.ReadStatusAsync();Assert.NotNull(state.Status);
        var broken=status.Progress! with{PendingTasks=0};broken=broken with{Revision=broken.ContentRevision()};status=status with{Progress=broken};status=status with{Revision=status.ContentRevision()};await state.ReadStatusAsync();Assert.Null(state.Status);Assert.NotNull(state.Error);
    }
}
#endif
