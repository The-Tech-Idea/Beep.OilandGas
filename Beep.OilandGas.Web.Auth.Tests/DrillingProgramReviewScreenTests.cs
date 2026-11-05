#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingProgramReviewScreenTests
{
    internal static DrillingProgramReviewReadiness Enable(DrillingProgramDraftStateTests.Fixture f,string mode="ready")
    {
        var current=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),current.UserId,new(current.Scope,current.Roles.ToArray(),current.Permissions.Concat(new[]{"Workflow.Start"}).ToArray()));var review=DrillingProgramReviewTests.Review(f.Native,mode);var row=review.Source.Program;
        f.Client.Setup(c=>c.GetProgramDraftsAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>{var value=new DrillingProgramDraftList(f.Native.Current,[row],"",DateTimeOffset.UtcNow);return value with{Revision=value.ContentRevision()};});
        f.Client.Setup(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),row.PROGRAM_ID,row.PROGRAM_DRAFT_REVISION,It.IsAny<CancellationToken>())).ReturnsAsync(review);return review;
    }
    [Theory][InlineData("ready")][InlineData("blocked")][InlineData("empty")]
    public async Task ActualOperationsParentShowsOriginalProgramAndExplicitIndependentRoutingWithoutApproval(string mode)
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();await f.Prepare();var review=Enable(f,mode);using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/operations/drillingoperations?wellUwi="+Uri.EscapeDataString(f.Native.Current.Well.Uwi));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Operations.DrillingOperations>();page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();page.WaitForAssertion(()=>Assert.Contains("Inspect recorded review and routing for",page.Markup));page.FindAll("button").Single(b=>b.TextContent.StartsWith("Inspect recorded review and routing for")).Click();
        page.WaitForAssertion(()=>Assert.Contains("Saved program and independent review routing",page.Markup));Assert.Contains(review.Source.Program.PROGRAM_ID,page.Markup);Assert.Contains(f.Native.Command.Objective,page.Markup);Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("Routing readiness creates no assignment or approval",page.Markup);Assert.Contains("DRAFT",page.Markup);
        Assert.Contains(mode switch{"ready"=>"Eligible reviewer program-reviewer","blocked"=>"No independent company reviewer",_=>"No configured DRILLING_PROGRAM approval workflow"},page.Markup);Assert.DoesNotContain(page.FindAll("button"),button=>button.TextContent is "Approve program" or "Submit program review");Assert.Null(f.Journal.ProgramOriginal);f.Client.Verify(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LatePrivateRoutingCannotSurviveCompanyPersonaRouteOrDisposalChanges(string change)
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();await f.Prepare();var review=Enable(f);await f.Inspect();var waiting=new TaskCompletionSource<DrillingProgramReviewReadiness>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var read=f.State.ReviewRoutingAsync(review.Source.Program.PROGRAM_ID);
        if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);else if(change=="route")await f.State.LoadAsync("other-well");else f.State.Dispose();waiting.SetResult(review);await read;Assert.Null(f.State.Routing);
    }
    [Fact]public async Task FailedRefreshClearsEarlierReadinessAndMissingStartPermissionCannotExposePrivateProposals()
    {
        using var f=new DrillingProgramDraftStateTests.Fixture();await f.Prepare();var review=Enable(f);await f.Inspect();await f.State.ReviewRoutingAsync(review.Source.Program.PROGRAM_ID);Assert.NotNull(f.State.Routing);
        f.Client.Setup(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());await f.State.ReviewRoutingAsync(review.Source.Program.PROGRAM_ID);Assert.Null(f.State.Routing);Assert.NotNull(f.State.Error);
        f.Select(true);await f.State.LoadAsync(f.Native.Current.Well.Uwi);await f.State.InspectAsync(f.Native.Current.Funding.Source.AfeId,null);Assert.True(f.State.CanReviewRouting);f.Client.Setup(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);await f.State.ReviewRoutingAsync(review.Source.Program.PROGRAM_ID);f.Client.Verify(c=>c.ReviewProgramRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Exactly(3));Assert.Null(f.State.Routing);
    }
}
#endif
