#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
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

public sealed class DrillingAssignedCompletionStateTests
{
    internal static DrillingDailyReportPage Progress(DrillingExecutionAssignedWork packet)
    {
        var basis = new DrillingDailyReportBasis(packet.Source, 1, null, null, null, [packet.AssignedRole], packet.PersonaCode, new('B', 64), "", DateTimeOffset.UtcNow);
        basis = basis with { Revision = basis.ContentRevision() }; var spud = new DateTimeOffset(DateTime.SpecifyKind(packet.Source.Recorded!.SPUD_DATE!.Value, DateTimeKind.Utc));
        var first = new DrillingDailyReportInput(basis, Guid.NewGuid(), packet.AssignedRole, spud, DateTimeOffset.UtcNow, 0, 125,
            packet.Source.Basis.Source.Program.DEPTH_OUOM, null, null, null, "Observed site report", "Reported completed period", null);
        var row = first.ToRecord(DateTime.UtcNow); basis = basis with { NextNumber = 2, PreviousReportId = row.DDR_ID, PreviousReportRevision = row.REPORT_REVISION, PreviousPeriodEndUtc = first.PeriodEndUtc, CheckedAtUtc = DateTimeOffset.UtcNow };
        basis = basis with { Revision = basis.ContentRevision() };
        var completion = first with { Basis = basis, IntentId = Guid.NewGuid(), Correction = new(row.DDR_ID, row.REPORT_REVISION, 1, "Explicit completion report for current recipient review"), Completion = DrillingCompletionReportNativeTests.Evidence() };
        return DrillingDailyReportPageNativeTests.Page(packet.Source, [row, completion.ToRecord(DateTime.UtcNow)]);
    }
    private sealed class Fixture : IDisposable
    {
        internal DrillingExecutionAssignmentNativeTests.Fixture Native = new();
        internal CompanyClientContext Company = new(); internal Mock<IPersonaContextService> Personas = new(); internal Mock<IDrillingServiceClient> Client = new();
        internal DrillingExecutionAssignedWork Packet = null!; internal DrillingDailyReportPage Page = null!; internal DrillingExecutionAssignedState State = null!;
        internal async Task Prepare()
        {
            await Native.Prepare(); Packet = DrillingExecutionAssignedStateTests.Packet(Native.Original); Page = Progress(Packet); var scope = Packet.Source.Basis.Scope();
            Company.TrySelect(Company.Clear(), scope.ActorUserId, new(scope.Company, [Packet.AssignedRole], ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"]));
            Personas.Setup(p => p.EnsureLoadedAsync()).Returns(Task.CompletedTask); Personas.SetupGet(p => p.CurrentProfile).Returns(new AppUserPersona { UserId = scope.ActorUserId, PersonaCode = Packet.PersonaCode }); Personas.SetupGet(p => p.CurrentPersona).Returns(new AppPersona { Code = Packet.PersonaCode });
            Client.Setup(c => c.GetAssignedExecutionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Packet);
            Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => Page);
            State = new(Company, Personas.Object, Client.Object, (_, _, message) => message); await State.LoadAsync(Packet.InstanceId);
        }
        public void Dispose() { State?.Dispose(); Company.Dispose(); }
    }
    [Fact]
    public async Task ExactRecipientLoadsSourcedCompletionSeparatelyWithoutInventingTaskClosureOrWritingReports()
    {
        using var f = new Fixture(); await f.Prepare(); Assert.Null(f.State.Progress); await f.State.LoadProgressAsync();
        Assert.Equal("COMPLETION_REPORTED", f.State.Progress!.ProgressState); Assert.Equal(f.Packet.Revision, f.State.Current!.Revision);
        Assert.NotEmpty(f.State.Progress.ReportedCompletion!.Report.OutstandingItems); Assert.False(f.State.ProgressBusy); Assert.Null(f.State.ProgressError);
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Theory, InlineData("lookup"), InlineData("execution")]
    public async Task UnavailableOrWrongExecutionIsNotZeroProgressAndDoesNotEraseQualifiedResponsibility(string fault)
    {
        using var f = new Fixture(); await f.Prepare();
        f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Unavailable source"));
        if (fault == "execution")
        {
            using var other = new DrillingDailyReportNativeTests.Fixture(); await other.Prepare(); var page = await other.Store.Service.ReadDailyReportPageAsync(other.Execution, 0, 25, null, other.Guard, () => Task.CompletedTask);
            f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(page);
        }
        await f.State.LoadProgressAsync(); Assert.Null(f.State.Progress); Assert.NotNull(f.State.ProgressError); Assert.NotNull(f.State.Current); Assert.False(f.State.ProgressBusy);
    }
    [Theory, InlineData("company"), InlineData("persona"), InlineData("route"), InlineData("dispose")]
    public async Task LateProgressCannotPublishIntoAnotherPrivateResponsibilityContext(string change)
    {
        using var f = new Fixture(); await f.Prepare(); var delayed = new TaskCompletionSource<DrillingDailyReportPage>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns(delayed.Task); var work = f.State.LoadProgressAsync();
        if (change == "company") f.Company.Clear(); if (change == "persona") f.Personas.Raise(p => p.Changed += null!);
        if (change == "route") await f.State.LoadAsync("other"); if (change == "dispose") f.State.Dispose();
        delayed.SetResult(f.Page); await work; Assert.Null(f.State.Progress); Assert.False(f.State.ProgressBusy);
    }
    [Fact]
    public async Task ActualReceiverScreenLoadsReportedTestsAndKeepsOngoingResponsibilityExplicit()
    {
        using var f = new Fixture(); await f.Prepare(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/assigned?processId=" + Uri.EscapeDataString(f.Packet.InstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecutionAssigned>(); page.WaitForAssertion(() => Assert.Contains("Your current reporting responsibility", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent == "Load observed progress and completion evidence").ClickAsync(new MouseEventArgs());
        Assert.Contains("Current observed progress and reported completion", page.Markup); Assert.Contains("Pass (reported)", page.Markup);
        Assert.Contains("does not close this ongoing responsibility", page.Markup); Assert.Contains("Independent inspection acceptance remains outstanding", page.Markup);
        f.Company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("Signed test certificate", page.Markup));
    }
}
#endif
