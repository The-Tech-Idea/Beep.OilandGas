using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingDailyReportCorrectionStateTests
{
    private static async Task<List<DRILLING_DAILY_REPORT>> Prepare(DrillingDailyReportStateTests.Fixture f)
    {
        await f.Prepare(); var rows = DrillingDailyReportPageNativeTests.Series(f.Native.Command, 3);
        f.Native.Store.Store.Native.Rows.AddRange(rows); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); return rows;
    }
    private static void Freeze(DrillingDailyReportState state, DrillingDailyReportObservation target, string? reason = "Signed site log reconciles this observation", bool changed = true) =>
        state.Prepare("ConfiguredExecutionRecorder", target.PERIOD_START_UTC.ToString("O"), target.PERIOD_END_UTC.ToString("O"),
            target.DEPTH_START_MD, target.DEPTH_END_MD, target.DEPTH_OUOM, target.ROTATING_HOURS, target.NPT_HOURS, target.NPT_REASON,
            target.REPORT_PROVENANCE, changed ? "Corrected observed drilling activity" : target.DAILY_OPERATIONS_SUMMARY, target.NEXT_PERIOD_PLAN, reason);
    [Fact]
    public async Task ExplicitCorrectionFreezesCurrentTargetReasonAndMeasurementsThenRetainsBeforeNativeTransport()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        await f.State.InspectCorrectionAsync(rows[0].DDR_ID); var target = f.State.CorrectionTarget!;
        Assert.Equal(rows[0].REPORT_REVISION, target.REPORT_REVISION); Assert.Equal(4, f.State.Basis!.NextNumber);
        Freeze(f.State, target); var frozen = f.State.Prepared!; Assert.Equal(rows[0].DDR_ID, frozen.Correction!.ReportId);
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()))
            .Returns(async (DrillingDailyReportInput original, CancellationToken _) =>
            {
                Assert.Equal(original.Revision(), f.Journal.DailyReportOriginal!.Revision()); f.Native.Command = original; return await f.Native.Save();
            });
        await f.State.SubmitAsync(); Assert.NotNull(f.State.Receipt!.Recorded); Assert.Null(f.State.Pending); Assert.Null(f.Journal.DailyReportOriginal);
        Assert.Null(f.State.CorrectionTarget); Assert.Null(f.State.Page); Assert.Equal(frozen.Correction.Reason, f.State.Receipt.Recorded!.CORRECTION_REASON);
        Assert.Equal(rows[0].REPORT_REVISION, f.Native.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(r => r.DDR_ID == rows[0].DDR_ID).REPORT_REVISION);
    }
    [Theory, InlineData("reason"), InlineData("no-change")]
    public async Task CorrectionRequiresReasonAndAnActualChangeBeforeFreezing(string fault)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        await f.State.InspectCorrectionAsync(rows[1].DDR_ID); Freeze(f.State, f.State.CorrectionTarget!, fault == "reason" ? " " : "Explicit correction", fault != "no-change");
        Assert.Null(f.State.Prepared); Assert.NotNull(f.State.Error); Assert.Null(f.Journal.DailyReportOriginal);
        await f.State.SubmitAsync(); f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task LostCorrectionAcknowledgementRetainsExactTargetAndRecoversReadOnlyWithoutDuplicating()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        await f.State.InspectCorrectionAsync(rows[1].DDR_ID); Freeze(f.State, f.State.CorrectionTarget!); var original = f.State.Prepared!;
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) =>
        {
            f.Native.Command = input; f.Native.Store.Store.Native.FailCommit = true; return await f.Native.Save();
        });
        await f.State.SubmitAsync(); f.Native.Store.Store.Native.FailCommit = false; Assert.Equal(original.Revision(), f.State.Pending!.Revision());
        Assert.Null(f.State.Page); Assert.Null(f.State.CorrectionTarget); var writes = f.Native.Store.Store.Native.Writes;
        var selected = f.Company.Selection!; f.Company.TrySelect(f.Company.Clear(), selected.UserId, new(selected.Scope, ["ReaderRole"],
            ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"]));
        f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(async (DrillingDailyReportInput input, CancellationToken _) => { f.Native.Command = input; return await f.Native.Save(true); });
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); Assert.True(f.State.CanRecover); Assert.False(f.State.CanPrepare);
        await f.State.RecoverAsync(); Assert.NotNull(f.State.Receipt!.Recorded); Assert.Null(f.Journal.DailyReportOriginal); Assert.Equal(writes, f.Native.Store.Store.Native.Writes);
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LateCorrectionBasisCannotSurvivePrivateContextChange(string change)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        var delayed = new TaskCompletionSource<DrillingDailyReportBasis>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(delayed.Task);
        var work = f.State.InspectCorrectionAsync(rows[0].DDR_ID);
        if (change == "company") f.Company.Clear(); if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("other"); if (change == "dispose") f.State.Dispose();
        delayed.SetResult(f.Native.Basis); await work; Assert.Null(f.State.CorrectionTarget); Assert.Null(f.State.Basis); Assert.Null(f.State.Prepared); Assert.False(f.State.Busy);
    }
    [Fact]
    public async Task SupersededOrUnseenRowsAndReadOnlyUsersCannotPrepareCorrection()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        f.Native.Command = DrillingDailyReportCorrectionNativeTests.Correct(f.Native, rows[0]); await f.Native.Save(); await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); await f.State.InspectCorrectionAsync(rows[0].DDR_ID);
        await f.State.InspectCorrectionAsync("not-on-this-checked-page"); Assert.Null(f.State.CorrectionTarget);
        var selected = f.Company.Selection!; f.Company.TrySelect(f.Company.Clear(), selected.UserId, new(selected.Scope, ["ReaderRole"],
            ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"]));
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); await f.State.InspectCorrectionAsync(rows[1].DDR_ID); Assert.Null(f.State.CorrectionTarget); Assert.False(f.State.CanPrepare);
        f.Client.Verify(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task ActualReportScreenShowsImmutableCorrectionChainCountsAndExplicitReasonInput()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        f.Native.Command = DrillingDailyReportCorrectionNativeTests.Correct(f.Native, rows[0]); var correction = (await f.Native.Save()).Recorded!; await f.Native.Refresh();
        f.History = await f.Native.Store.Service.ReadDailyReportPageAsync(f.Native.Execution, 0, 25, null, f.Native.Guard, () => Task.CompletedTask);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(() => Assert.Contains("3 observed periods; 1 corrections; 4 immutable history entries", page.Markup));
        Assert.Contains("SUPERSEDED", page.Markup); Assert.Contains("CURRENT CORRECTION", page.Markup); Assert.Contains(correction.CORRECTION_REASON!, page.Markup);
        Assert.DoesNotContain(page.FindAll("button"), button => button.TextContent == "Correct observations from entry 1");
        await page.FindAll("button").Single(button => button.TextContent == "Correct observations from entry 2").ClickAsync(new MouseEventArgs());
        Assert.Contains("Correct recorded observations", page.Markup); Assert.Contains("Correction reason", page.Markup); Assert.Contains(rows[1].REPORT_REVISION, System.Text.Json.JsonSerializer.Serialize(f.History));
        Assert.Contains(rows[1].REPORT_PROVENANCE, page.Markup); Assert.Null(f.Journal.DailyReportOriginal);
        f.Company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain(correction.CORRECTION_REASON!, page.Markup));
    }
}
#endif
