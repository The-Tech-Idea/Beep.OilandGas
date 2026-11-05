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

public sealed class DrillingDailyReportPageStateTests
{
    private static async Task<List<DRILLING_DAILY_REPORT>> Prepare(DrillingDailyReportStateTests.Fixture f)
    {
        await f.Prepare(); var rows = DrillingDailyReportPageNativeTests.Series(f.Native.Command, 27);
        f.History = DrillingDailyReportPageNativeTests.Page(f.Native.Execution, rows);
        f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, int after, int size, string? expected, CancellationToken token) =>
            {
                var result = DrillingDailyReportPageNativeTests.Page(f.Native.Execution, rows, after, size);
                if (expected is not null && result.History.Revision != expected) throw new InvalidOperationException("Changed history"); return result;
            });
        var head = f.History.History.Head!; var basis = f.Native.Basis with { NextNumber = 28, CheckedAtUtc = DateTimeOffset.UtcNow, PreviousReportId = head.ReportId,
            PreviousReportRevision = head.NativeRevision, PreviousPeriodEndUtc = head.LatestPeriodEndUtc };
        f.Native.Basis = basis with { Revision = basis.ContentRevision() }; f.Native.Basis.Require();
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); return rows;
    }
    [Fact]
    public async Task NavigationUsesCheckedRevisionAndNewReportUsesCompleteHeadRatherThanVisibleLastRow()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f);
        Assert.Equal(25, f.State.Page!.Reports.Count); Assert.Equal(27, f.State.Page.History.Count);
        await f.State.InspectAsync(); Assert.Equal(28, f.State.Basis!.NextNumber); Assert.Equal(27, f.State.Page.History.Head!.Number);
        await f.State.NextPageAsync(); Assert.Null(f.State.Basis); Assert.Equal(26, f.State.Page!.Reports[0].REPORT_NUMBER);
        Assert.False(f.State.Page.HasNext); await f.State.PreviousPageAsync(); Assert.Equal(0, f.State.Page!.AfterNumber);
        f.Client.Verify(c => c.GetDailyReportPageAsync(It.IsAny<string>(), 25, 25, It.Is<string>(x => x == f.History.History.Revision), It.IsAny<CancellationToken>()), Times.Once());
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Theory, InlineData("lookup"), InlineData("revision"), InlineData("range")]
    public async Task FailedOrChangedPageRefusesMixingAndRequiresExplicitReload(string fault)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), 25, 25, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns((string _, int after, int size, string? expected, CancellationToken token) =>
            {
                if (fault == "lookup") return Task.FromException<DrillingDailyReportPage>(new HttpRequestException());
                if (fault == "range") return Task.FromResult(f.History);
                var changed = rows.ToList(); changed.RemoveAt(changed.Count - 1);
                return Task.FromResult(DrillingDailyReportPageNativeTests.Page(f.Native.Execution, changed, 25, 25));
            });
        await f.State.NextPageAsync(); Assert.Null(f.State.Page); Assert.Null(f.State.Basis); Assert.NotNull(f.State.Error);
        Assert.False(f.State.CanPrepare); Assert.False(f.State.CanNavigate); Assert.Null(f.Journal.DailyReportOriginal);
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LatePageCannotPublishAfterContextChange(string change)
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); var rows = await Prepare(f);
        var delayed = new TaskCompletionSource<DrillingDailyReportPage>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), 25, 25, It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns(delayed.Task);
        var work = f.State.NextPageAsync();
        if (change == "company") f.Company.Clear(); if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("other"); if (change == "dispose") f.State.Dispose();
        delayed.SetResult(DrillingDailyReportPageNativeTests.Page(f.Native.Execution, rows, 25, 25)); await work;
        Assert.Null(f.State.Page); Assert.Null(f.State.Basis); Assert.False(f.State.Busy);
    }
    [Fact]
    public async Task ActualScreenShowsCheckedTotalAndBoundedPreviousNextRowsWithoutPostingReports()
    {
        using var f = new DrillingDailyReportStateTests.Fixture(); await Prepare(f);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        page.WaitForAssertion(() => Assert.Contains("Showing history entries 1 to 25 of 27", page.Markup));
        var renderedText = page.Find(".mud-container").TextContent;
        Assert.Contains("\u00b7", renderedText); Assert.DoesNotContain("\u00c2", renderedText); Assert.DoesNotContain("\ufffd", renderedText);
        Assert.Contains("Current checked head: entry 27", page.Markup); Assert.DoesNotContain("Qualified observed site log 26", page.Markup);
        await page.FindAll("button").Single(b => b.TextContent == "Next reports").ClickAsync(new MouseEventArgs());
        Assert.Contains("Showing history entries 26 to 27 of 27", page.Markup); Assert.DoesNotContain("Qualified observed site log 25", page.Markup);
        Assert.True(page.FindAll("button").Single(b => b.TextContent == "Next reports").HasAttribute("disabled"));
        await page.FindAll("button").Single(b => b.TextContent == "Previous reports").ClickAsync(new MouseEventArgs());
        Assert.Contains("Showing history entries 1 to 25 of 27", page.Markup);
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
#endif
