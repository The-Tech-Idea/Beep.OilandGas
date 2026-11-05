#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Services;
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

public sealed class DrillingDailyReportConflictStateTests
{
    private static DrillingDailyReportConflictReview Proof(DrillingDailyReportInput original)
    {
        var other = original with { IntentId = Guid.NewGuid(), Provenance = "Different recorded site log 472", Summary = "Different actual observations", DepthEndMd = 150 };
        var row = other.ToRecord(DateTime.UtcNow);
        var builder = new DrillingDailyReportHistoryBuilder(other.Basis.Execution, DateTimeOffset.UtcNow); builder.Add(row); var history = builder.Build();
        var result = new DrillingDailyReportConflictReview(original.Revision(), original.ReportId(), true, row, history.Revision, 1, "", DateTimeOffset.UtcNow);
        result = result with { Revision = result.ContentRevision() }; result.Require(original); return result;
    }
    private static async Task<DrillingDailyReportConflictReview> Pending(DrillingDailyReportStateTests.Fixture f)
    {
        await f.Prepare(); f.Journal.DailyReportOriginal = f.Native.Command;
        var selected = f.Company.Selection!;
        f.Company.TrySelect(f.Company.Clear(), selected.UserId, new(selected.Scope, selected.Roles.ToArray(),
            selected.Permissions.Where(p => p is not ("Drilling.Edit" or "Drilling.ManageWellProgram")).ToArray()));
        f.Personas.SetupGet(p => p.CurrentProfile).Returns(new AppUserPersona { UserId = selected.UserId, PersonaCode = "UNMAPPED" });
        var proof = Proof(f.Native.Command);
        f.Client.Setup(c => c.GetDailyReportConflictAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        f.Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); return proof;
    }
    [Fact]
    public async Task ReadOnlyUnmappedRecorderMustExplicitlyInspectAndRecheckBeforeClearing()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var proof = await Pending(f);
        Assert.True(f.State.CanInspectConflict); Assert.False(f.State.CanPrepare);
        await f.State.InspectConflictAsync(); Assert.NotNull(f.Journal.DailyReportOriginal); Assert.False(f.State.ConflictAcknowledged);
        f.State.Conflict!.Competing.REPORT_PROVENANCE = "Consumer mutation";
        Assert.Equal(proof.Competing.REPORT_PROVENANCE, f.State.Conflict!.Competing.REPORT_PROVENANCE);
        await f.State.AcknowledgeConflictAsync(); Assert.True(f.State.ConflictAcknowledged); Assert.Null(f.State.Pending);
        Assert.Null(f.State.Receipt); Assert.Null(f.Journal.DailyReportOriginal); Assert.NotNull(f.State.Conflict);
        f.Client.Verify(c => c.ConfirmDailyReportConflictAsync(It.Is<DrillingDailyReportConflictAcknowledgement>(x => x.ReviewRevision == proof.Revision), It.IsAny<CancellationToken>()), Times.Once());
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
        f.Client.Verify(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never());
        f.Personas.Verify(p => p.EnsureLoadedAsync(), Times.Never());
    }
    [Theory, InlineData("lookup"), InlineData("changed"), InlineData("clear")]
    public async Task LookupChangedHistoryOrClearFailurePreservesTheOriginal(string fault)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var proof = await Pending(f); await f.State.InspectConflictAsync();
        if (fault == "lookup") f.Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        if (fault == "changed") f.Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).ReturnsAsync(Proof(f.Native.Command));
        f.Journal.FailClear = fault == "clear";
        await f.State.AcknowledgeConflictAsync(); Assert.NotNull(f.State.Pending); Assert.NotNull(f.Journal.DailyReportOriginal);
        Assert.NotNull(f.State.Error); Assert.Equal(fault == "clear", f.State.ConflictAcknowledged); Assert.Null(f.State.Receipt);
        if (fault == "clear")
        {
            Assert.Contains("again to recheck and finish", f.State.PendingExplanation); var at = f.State.Conflict!.CheckedAtUtc;
            f.Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
            await f.State.AcknowledgeConflictAsync(); Assert.Equal(at, f.State.Conflict!.CheckedAtUtc); Assert.NotNull(f.Journal.DailyReportOriginal);
        }
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LateConfirmationCannotPublishOrClearAfterContextChange(string change)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var proof = await Pending(f); await f.State.InspectConflictAsync();
        var response = new TaskCompletionSource<DrillingDailyReportConflictReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).Returns(response.Task);
        var work = f.State.AcknowledgeConflictAsync();
        if (change == "company") f.Company.Clear();
        if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("another");
        if (change == "dispose") f.State.Dispose();
        response.SetResult(proof); await work; Assert.Null(f.State.Conflict); Assert.False(f.State.ConflictAcknowledged);
        Assert.NotNull(f.Journal.DailyReportOriginal); Assert.Null(f.State.Confirmation);
    }
    [Theory, InlineData(false), InlineData(true)]
    public async Task ActualReportScreenShowsDifferentObservationsAndOnlyExplicitCleanup(bool earlierAbsent)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var proof = await Pending(f); f.Journal.FailClear = true;
        if (earlierAbsent) f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DrillingDailyReportReceipt(f.Native.Command.Revision(), f.Native.Command.ReportId(), null, DateTimeOffset.UtcNow));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(() => Assert.Contains("Inspect a different recorded report", page.Markup));
        if (earlierAbsent)
        {
            await page.FindAll("button").Single(b => b.TextContent == "Recover original drilling report").ClickAsync(new MouseEventArgs());
            Assert.Contains("The original drilling report was confirmed absent.", page.Markup);
        }
        await page.FindAll("button").Single(b => b.TextContent == "Inspect a different recorded report").ClickAsync(new MouseEventArgs());
        Assert.Contains(proof.Competing.DDR_ID, page.Markup); Assert.Contains("Different recorded site log 472", page.Markup);
        Assert.Contains("Different actual observations", page.Markup); Assert.Contains("150", page.Markup); Assert.NotNull(f.Journal.DailyReportOriginal);
        await page.FindAll("button").Single(b => b.TextContent == "Keep recorded report and clear my unrecorded original").ClickAsync(new MouseEventArgs());
        Assert.Contains("Clearing the tab input is not confirmed", page.Markup); Assert.NotNull(f.Journal.DailyReportOriginal);
        f.Journal.FailClear = false;
        await page.FindAll("button").Single(b => b.TextContent == "Keep recorded report and clear my unrecorded original").ClickAsync(new MouseEventArgs());
        Assert.Null(f.Journal.DailyReportOriginal); Assert.Contains("You kept the exact different recorded report", page.Markup);
        Assert.DoesNotContain("Choose the company with drilling", page.Markup); Assert.DoesNotContain("Original report outcome", page.Markup);
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact]
    public async Task PositiveRecordedOriginalRemainsOnExactOriginalRecovery()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Pending(f); f.Journal.FailClear = true;
        f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DrillingDailyReportReceipt(f.Native.Command.Revision(), f.Native.Command.ReportId(), f.Native.Command.ToRecord(DateTime.UtcNow), DateTimeOffset.UtcNow));
        await f.State.RecoverAsync(); Assert.False(f.State.CanInspectConflict); await f.State.InspectConflictAsync(); Assert.Null(f.State.Conflict);
        f.Client.Verify(c => c.GetDailyReportConflictAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
#endif
