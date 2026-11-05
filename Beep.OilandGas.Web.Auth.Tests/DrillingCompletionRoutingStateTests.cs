using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.Workflow;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingCompletionRoutingStateTests
{
    private static async Task Prepare(DrillingDailyReportStateTests.Fixture f, string mode = "ready")
    {
        await f.Prepare();
        // Current qualified native report history, not a fabricated accepted status.
        var rows = DrillingDailyReportPageNativeTests.Series(f.Native.Command, 3);
        f.Native.Store.Store.Native.Rows.AddRange(rows); await f.Native.Refresh();
        f.Native.Command = DrillingCompletionReportNativeTests.Complete(f.Native, rows[2]); await f.Native.Save(); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        var selected = f.Company.Selection!;
        f.Company.TrySelect(f.Company.Clear(), selected.UserId, new(selected.Scope, selected.Roles.ToArray(), [.. selected.Permissions, "Workflow.Start"]));
        f.Client.Setup(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => DrillingCompletionRoutingTests.Review(f.History.History, mode));
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId);
    }
    [Theory, InlineData("ready"), InlineData("blocked"), InlineData("empty"), InlineData("reader")]
    public async Task ExplicitRoutingReadDoesNotWriteOrTurnReadinessIntoCompletion(string mode)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f, mode); var writes = f.Native.Store.Store.Native.Writes;
        f.Client.Verify(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        Assert.True(f.State.CanInspectCompletionRouting); await f.State.InspectCompletionRoutingAsync();
        Assert.NotNull(f.State.CompletionRouting); Assert.Null(f.State.CompletionRoutingError); Assert.Null(f.State.Prepared); Assert.Null(f.State.Pending);
        Assert.Equal("COMPLETION_REPORTED", f.State.Page!.History.ProgressState); Assert.Equal(writes, f.Native.Store.Store.Native.Writes);
        var escaped = f.State.CompletionRouting!; escaped.Source.Execution.Recorded!.STATUS = "Forged accepted status";
        Assert.Equal("STARTED", f.State.CompletionRouting!.Source.Execution.Recorded!.STATUS);
        await f.State.InspectAsync(); Assert.Null(f.State.CompletionRouting);
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LatePrivateRoutingCannotPublishAfterContextChanges(string change)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f);
        var result = DrillingCompletionRoutingTests.Review(f.History.History); var later = new TaskCompletionSource<DrillingCompletionReviewReadiness>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(later.Task);
        var loading = f.State.InspectCompletionRoutingAsync(); Assert.True(f.State.Busy);
        if (change == "company") f.Company.Clear(); if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("other"); if (change == "dispose") f.State.Dispose();
        later.SetResult(result); await loading; Assert.Null(f.State.CompletionRouting);
    }
    [Theory, InlineData("unavailable"), InlineData("history"), InlineData("workspace")]
    public async Task UnavailableOrChangedProofIsAnErrorRatherThanEmptyRouting(string fault)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f);
        var result = DrillingCompletionRoutingTests.Review(f.History.History);
        if (fault == "unavailable") f.Client.Setup(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Source unavailable"));
        if (fault == "history")
        {
            var changed = result.Source with { ChainRevision = new('F', 64) }; changed = changed with { Revision = changed.ContentRevision() };
            result = result with { Source = changed }; result = result with { Revision = result.ContentRevision() };
            f.Client.Setup(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        }
        if (fault == "workspace") f.Personas.SetupGet(p => p.CurrentProfile).Returns(new AppUserPersona { UserId = f.Company.Selection!.UserId, PersonaCode = "FOREIGN" });
        await f.State.InspectCompletionRoutingAsync(); Assert.Null(f.State.CompletionRouting); Assert.NotNull(f.State.CompletionRoutingError); Assert.NotNull(f.State.Page);
    }
    [Theory, InlineData("ready"), InlineData("blocked"), InlineData("empty"), InlineData("reader")]
    public async Task ActualRoutingComponentShowsSourceStagesAndTruthfulReadinessWithoutAcceptance(string mode)
    {
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        var review = DrillingCompletionRoutingTests.Review(await DrillingCompletionRoutingTests.Source(), mode);
        var view = ui.RenderComponent<DrillingCompletionRoutingView>(p => p.Add(x => x.Review, review));
        Assert.Contains("Routing inspection only", view.Markup); Assert.Contains("reporting responsibility remains ongoing", view.Markup);
        if (mode == "ready") Assert.Contains("INDEPENDENT_COMPLETION / completion-inspector", view.Markup);
        if (mode == "blocked") Assert.Contains("No eligible independent recipient", view.Markup);
        if (mode == "empty") Assert.Contains("No completion approval workflow is configured", view.Markup);
        if (mode == "reader") Assert.Contains("original FIELD WRITE authority", view.Markup);
    }
    [Fact]
    public async Task ActualReportParentLoadsRoutingOnRequestAndHidesRecipientsWhenCompanyChanges()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(() => Assert.Contains("Inspect independent completion routing", page.Markup));
        Assert.DoesNotContain("completion-inspector", page.Markup);
        await page.FindAll("button").Single(button => button.TextContent == "Inspect independent completion routing").ClickAsync(new MouseEventArgs());
        page.WaitForAssertion(() => Assert.Contains("completion-inspector", page.Markup)); Assert.Contains("pending independent acceptance", page.Markup);
        f.Company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("completion-inspector", page.Markup));
    }
}
#endif
