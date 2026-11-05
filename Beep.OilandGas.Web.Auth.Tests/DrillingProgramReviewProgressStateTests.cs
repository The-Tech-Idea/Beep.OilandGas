using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
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
public sealed class DrillingProgramReviewProgressStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingProgramReviewDecisionNativeTests.Fixture Native=new();internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingProgramAssignedReviewState State=null!;internal DrillingProgramReviewProgress Progress=null!;
        internal async Task Prepare(string mode="initial")
        {
            await Native.Prepare(3,mode=="rejected-first"?"REJECT":"APPROVE");if(mode!="initial")await Native.Decide();if(mode is "approved" or "rejected-later"){Native.Command=await Native.Create(mode=="rejected-later"?"REJECT":"APPROVE");await Native.Decide();}if(mode=="approved"){Native.Command=await Native.Create();await Native.Decide();}Progress=await DrillingProgramReviewProgressTests.Read(Native,Native.Start.Original.Scope().ActorUserId);var scope=Progress.Source.Basis.Scope();Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,["CurrentReadOnlyRole"],["Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);Client.Setup(c=>c.GetProgramReviewProgressAsync(Progress.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Progress);State=new(Company,Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        }
        public void Dispose(){State?.Dispose();Company.Dispose();Native.Dispose();}
    }
    [Theory][InlineData("initial")][InlineData("intermediate")][InlineData("approved")][InlineData("rejected-first")][InlineData("rejected-later")]
    public async Task OriginalSenderWithViewRightsReloadsTheCompleteTimelineWithoutPendingTaskOrReviewRole(string mode)
    {
        using var f=new Fixture();await f.Prepare(mode);await f.State.LoadAsync(f.Progress.InstanceId);Assert.Equal(f.Progress.Revision,f.State.Progress!.Revision);Assert.Null(f.State.Current);Assert.False(f.State.CanPrepareDecision);Assert.Null(f.State.Error);f.State.Progress.Source.Program.PROGRAM_NAME="Consumer mutation";Assert.Equal(f.Progress.Source.Program.PROGRAM_NAME,f.State.Progress.Source.Program.PROGRAM_NAME);f.Client.Verify(c=>c.GetAssignedProgramReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LatePrivateOutcomeCannotSurviveCompanyPersonaRouteOrDisposalChange(string change)
    {
        using var f=new Fixture();await f.Prepare();var waiting=new TaskCompletionSource<DrillingProgramReviewProgress>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.GetProgramReviewProgressAsync(f.Progress.InstanceId,It.IsAny<CancellationToken>())).Returns(waiting.Task);f.Client.Setup(c=>c.GetProgramReviewProgressAsync(DrillingProgramReviewStart.InstancePrefix+"another",It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());var read=f.State.LoadAsync(f.Progress.InstanceId);if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);else if(change=="route")await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix+"another");else f.State.Dispose();waiting.SetResult(f.Progress);await read;Assert.Null(f.State.Progress);Assert.Null(f.State.Current);
    }
    [Fact]public async Task UnavailableOutcomeRefreshClearsEarlierHistoryAndNeverSubstitutesReceiptOrEmptySuccess()
    {
        using var f=new Fixture();await f.Prepare("approved");await f.State.LoadAsync(f.Progress.InstanceId);Assert.NotNull(f.State.Progress);f.Client.Setup(c=>c.GetProgramReviewProgressAsync(f.Progress.InstanceId,It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());await f.State.LoadAsync(f.Progress.InstanceId);Assert.Null(f.State.Progress);Assert.NotNull(f.State.Error);Assert.Null(f.State.DecisionReceipt);
    }
    [Fact]public async Task ChangedNativeRevisionBetweenHistoryAndAssignedPacketClearsBothReadViews()
    {
        using var f=new DrillingProgramAssignedReviewStateTests.Fixture();await f.Prepare();var packet=f.Packet with{NativeRevision=new('F',64)};packet=packet with{Revision=packet.ContentRevision()};f.Client.Setup(c=>c.GetAssignedProgramReviewAsync(f.Packet.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(packet);await f.State.LoadAsync(f.Packet.InstanceId);Assert.Null(f.State.Current);Assert.Null(f.State.Progress);Assert.NotNull(f.State.Error);
    }
    [Theory][InlineData("approved")][InlineData("rejected-first")][InlineData("intermediate")]
    public async Task ActualReadOnlyPageShowsSavedSourceAndCompleteRecordedRolesReviewersRationaleUtcAuditAndCounts(string mode)
    {
        using var f=new Fixture();await f.Prepare(mode);using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-review/assigned?processId="+Uri.EscapeDataString(f.Progress.InstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingProgramAssignedReview>();page.WaitForAssertion(()=>Assert.Contains("Recorded ordered program review stages",page.Markup));Assert.Contains(mode=="approved"?"Program review: Approved":mode=="rejected-first"?"Program review: Rejected":"Program review: In review",page.Markup);Assert.Contains("Pending receiving tasks: "+f.Progress.PendingTasks,page.Markup);Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("Planned MD 2500.25 m",page.Markup);Assert.Contains("Source status DRAFT",page.Markup);Assert.Equal(3,page.FindAll("tbody tr").Count);Assert.Contains("ConfiguredProgramReviewer",page.Markup);Assert.Contains("program-reviewer",page.Markup);Assert.Contains("Independent program design and original funding reviewed",page.Markup);Assert.Contains("Decision UTC",page.Markup);Assert.Contains(f.Progress.Stages[0].ApprovalId!,page.Markup);Assert.Contains(f.Progress.Stages[0].HistoryId!,page.Markup);Assert.DoesNotContain("Review rationale",page.Markup);Assert.DoesNotContain("Choose a company",page.Markup);Assert.DoesNotContain("Pending independent program review",page.Markup);
    }
    [Fact]public async Task ActualSavedProgramReadOnlyLookupLinksToTheDurableOriginalReviewWithoutPrivateProposedRecipients()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();await f.Prepare();var initial=DrillingProgramReviewScreenTests.Enable(f);var original=new DrillingProgramReviewStart(initial.Source,initial.Options[0],Guid.NewGuid());var view=initial with{Options=[],ExistingReviewInstanceId=original.InstanceId()};view=view with{Revision=view.ContentRevision()};f.Client.Setup(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(view);f.Select(false);using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/operations/drillingoperations?wellUwi="+Uri.EscapeDataString(f.Native.Current.Well.Uwi));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Operations.DrillingOperations>();page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();page.WaitForAssertion(()=>Assert.Contains("Inspect recorded review and routing for",page.Markup));page.FindAll("button").Single(b=>b.TextContent.StartsWith("Inspect recorded review and routing for")).Click();page.WaitForAssertion(()=>Assert.Contains("Open recorded program review history",page.Markup));Assert.Contains("/drilling/program-review/assigned?processId="+Uri.EscapeDataString(original.InstanceId()),page.Markup);Assert.DoesNotContain("Eligible reviewer",page.Markup);Assert.DoesNotContain("Review submission for",page.Markup);f.Client.Verify(c=>c.SubmitProgramReviewAsync(It.IsAny<DrillingProgramReviewStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
}
#endif
