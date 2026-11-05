#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.Workflow;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingCompletionReportStateTests
{
    private static async Task<List<DRILLING_DAILY_REPORT>> Prepare(DrillingDailyReportStateTests.Fixture f)
    {
        await f.Prepare(); var rows = DrillingDailyReportPageNativeTests.Series(f.Native.Command, 3);
        f.Native.Store.Store.Native.Rows.AddRange(rows); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); return rows;
    }
    private static void Freeze(DrillingDailyReportState state, DrillingCompletionReport? completion, bool withdraw = false)
    {
        var target = state.CorrectionTarget!;
        state.Prepare("ConfiguredExecutionRecorder", target.PERIOD_START_UTC.ToString("O"), target.PERIOD_END_UTC.ToString("O"),
            target.DEPTH_START_MD, target.DEPTH_END_MD, target.DEPTH_OUOM, target.ROTATING_HOURS, target.NPT_HOURS, target.NPT_REASON,
            target.REPORT_PROVENANCE, target.DAILY_OPERATIONS_SUMMARY, target.NEXT_PERIOD_PLAN, "Explicitly report, amend or withdraw this completion claim", completion, withdraw);
    }
    [Fact]
    public async Task LatestObservedPeriodIsRequiredAndExplicitEvidenceIsFrozenRetainedThenRecorded()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        await f.State.InspectCompletionAsync(rows[0].DDR_ID); Assert.Null(f.State.CorrectionTarget); Assert.False(f.State.CompletionMode);
        await f.State.InspectCompletionAsync(rows[2].DDR_ID); Assert.True(f.State.CompletionMode);
        Freeze(f.State, DrillingCompletionReportNativeTests.Evidence()); var original = f.State.Prepared!; Assert.Equal(rows[2].DDR_ID, original.Correction!.ReportId);
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) =>
        {
            Assert.Equal(input.Revision(), f.Journal.DailyReportOriginal!.Revision()); f.Native.Command = input; return await f.Native.Save();
        });
        await f.State.SubmitAsync(); Assert.NotNull(f.State.Receipt!.Recorded); Assert.Null(f.Journal.DailyReportOriginal); Assert.False(f.State.CompletionMode);
        Assert.Contains("pending independent acceptance", f.State.Confirmation); Assert.Equal(original.Completion!.CompletedAtUtc.UtcDateTime, f.State.Receipt.Recorded!.REPORTED_COMPLETION_UTC);
    }
    [Theory, InlineData("missing"), InlineData("future"), InlineData("unreviewed")]
    public async Task MissingFutureOrUnreviewedCompletionCannotBeFrozenOrSent(string fault)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f); await f.State.InspectCompletionAsync(rows[2].DDR_ID);
        var evidence = DrillingCompletionReportNativeTests.Evidence(); Freeze(f.State, fault == "missing" ? null : fault == "future" ? evidence with { CompletedAtUtc = DateTimeOffset.UtcNow.AddHours(1) } : evidence with { OutstandingItemsReviewed = false });
        Assert.Null(f.State.Prepared); Assert.NotNull(f.State.Error); await f.State.SubmitAsync(); Assert.Null(f.Journal.DailyReportOriginal);
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LateCompletionPreparationCannotSurvivePrivateContextChanges(string change)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        var delayed = new TaskCompletionSource<DrillingDailyReportBasis>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(delayed.Task);
        var work = f.State.InspectCompletionAsync(rows[2].DDR_ID);
        if (change == "company") f.Company.Clear(); if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("other"); if (change == "dispose") f.State.Dispose();
        delayed.SetResult(f.Native.Basis); await work; Assert.False(f.State.CompletionMode); Assert.Null(f.State.Basis); Assert.Null(f.State.CorrectionTarget); Assert.False(f.State.Busy);
    }
    [Fact]
    public async Task ExistingCompletionCannotDisappearThroughAnOrdinaryCorrectionButExplicitWithdrawalWorks()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f); f.Native.Command = DrillingCompletionReportNativeTests.Complete(f.Native, rows[2]); await f.Native.Save(); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); await f.State.InspectCorrectionAsync(f.History.History.LatestObservedReport!.ReportId);
        Assert.True(f.State.CompletionMode); Freeze(f.State, null); Assert.Null(f.State.Prepared); Assert.NotNull(f.State.Error);
        Freeze(f.State, null, true); Assert.True(f.State.Prepared!.WithdrawCompletion); Assert.Null(f.State.Prepared.Completion);
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) => { f.Native.Command = input; return await f.Native.Save(); });
        await f.State.SubmitAsync(); Assert.Contains("withdrawal", f.State.Confirmation); Assert.Null(f.State.Pending);
    }
    [Fact]
    public async Task LostCompletionAcknowledgementRecoversReadOnlyBeforeSourceOrProfileLookup()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f); await f.State.InspectCompletionAsync(rows[2].DDR_ID); Freeze(f.State, DrillingCompletionReportNativeTests.Evidence());
        var original = f.State.Prepared!;
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) => { f.Native.Command = input; f.Native.Store.Store.Native.FailCommit = true; return await f.Native.Save(); });
        await f.State.SubmitAsync(); f.Native.Store.Store.Native.FailCommit = false; Assert.Equal(original.Revision(), f.State.Pending!.Revision()); var writes = f.Native.Store.Store.Native.Writes;
        var selected = f.Company.Selection!; f.Company.TrySelect(f.Company.Clear(), selected.UserId, new(selected.Scope, ["ReaderRole"], ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"]));
        f.Client.Invocations.Clear(); f.Personas.Invocations.Clear();
        f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) => { f.Native.Command = input; return await f.Native.Save(true); });
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); Assert.True(f.State.CanRecover); Assert.False(f.State.CanPrepare); await f.State.RecoverAsync();
        Assert.NotNull(f.State.Receipt!.Recorded); Assert.Null(f.Journal.DailyReportOriginal); Assert.Equal(writes, f.Native.Store.Store.Native.Writes);
        f.Client.Verify(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never()); f.Personas.Verify(p => p.EnsureLoadedAsync(), Times.Never());
    }
    [Fact]
    public void CompletionEditorDoesNotGuessDatesResultsOrOutstandingReviewAndCanReloadRecordedEvidence()
    {
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        var editor = ui.RenderComponent<DrillingCompletionReportEditor>(p => p.Add(x => x.TargetId, "latest-report"));
        Assert.ThrowsAny<Exception>(() => editor.Instance.Create(DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1)));
        Assert.True(string.IsNullOrEmpty(editor.FindComponent<MudBlazor.MudSelect<string>>().Instance.Value));
        Assert.False(editor.FindComponent<MudBlazor.MudCheckBox<bool>>().Instance.Value);
        Assert.True(string.IsNullOrEmpty(editor.FindComponents<MudBlazor.MudTextField<string>>().Single(field => field.Instance.Label == "Actual completion UTC").Instance.Value));
        var initial = DrillingCompletionReportNativeTests.Evidence(); editor.SetParametersAndRender(p => p.Add(x => x.TargetId, "recorded-completion").Add(x => x.Initial, initial));
        Assert.Equal(JsonSerializer.Serialize(initial), JsonSerializer.Serialize(editor.Instance.Create(DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1))));
    }
    [Fact]
    public async Task ActualPageShowsCompletionActionEvidenceAndPendingAcceptanceWithoutCompletingTheOperation()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("button").Where(button => button.TextContent.StartsWith("Report execution completion from entry"))));
        await page.FindAll("button").Single(button => button.TextContent == "Report execution completion from entry 3").ClickAsync(new MouseEventArgs());
        Assert.Contains("Report or amend execution completion", page.Markup); Assert.Contains("Completion reporting or amendment reason", page.Markup); Assert.Contains("Actual completion UTC", page.Markup); Assert.Contains("Test performed UTC 1", page.Markup); Assert.Contains("pending independent acceptance", page.Markup);
        f.Native.Command = DrillingCompletionReportNativeTests.Complete(f.Native, rows[2]); await f.Native.Save(); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        await page.FindAll("button").Single(button => button.TextContent == "Reload original report history").ClickAsync(new MouseEventArgs());
        Assert.Contains("Current completion report, pending independent acceptance", page.Markup); Assert.Contains("Pass (reported)", page.Markup); Assert.Contains("Independent inspection acceptance remains outstanding", page.Markup);
        Assert.Equal("STARTED", f.History.History.Execution.Recorded!.STATUS); f.Company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("Signed test certificate", page.Markup));
    }
}
#endif
