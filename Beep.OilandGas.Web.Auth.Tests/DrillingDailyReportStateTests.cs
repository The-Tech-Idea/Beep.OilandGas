using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingDailyReportStateTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DrillingDailyReportNativeTests.Fixture Native = new();
        internal CompanyClientContext Company = new();
        internal Mock<IPersonaContextService> Personas = new();
        internal Mock<IDrillingServiceClient> Client = new();
        internal DevelopmentDraftWorkspaceTests.Journal Journal = new();
        internal DrillingDailyReportState State = null!;
        internal DrillingDailyReportPage History = null!;
        internal async Task Prepare(bool recorded = false)
        {
            await Native.Prepare(); if (recorded) await Native.Save();
            History = await Native.Store.Service.ReadDailyReportPageAsync(Native.Execution, 0, DrillingDailyReportPage.DefaultPageSize, null, Native.Guard, () => Task.CompletedTask);
            var basis = Native.Basis; var scope = basis.Scope();
            Company.TrySelect(Company.Clear(), scope.ActorUserId, new(scope.Company, basis.RecorderRoles.ToArray(),
                ["Drilling.View", "Drilling.Edit", "Drilling.ViewDailyReports", "Drilling.ManageWellProgram", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View"]));
            Personas.SetupGet(p => p.CurrentPersona).Returns(new AppPersona { Code = basis.PersonaCode!, Name = "Actual site reporting workspace" });
            Personas.SetupGet(p => p.CurrentProfile).Returns(new AppUserPersona { UserId = scope.ActorUserId, PersonaCode = basis.PersonaCode! });
            Personas.Setup(p => p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => History);
            Client.Setup(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => Native.Basis);
            State = new(Company, Personas.Object, Client.Object, Journal, (_, _, text) => text);
        }
        internal async Task Inspect() { await State.LoadAsync(Native.Execution.Basis.ReviewInstanceId); await State.InspectAsync(); Assert.NotNull(State.Basis); }
        internal void Freeze(string? start = null, decimal? depth = 125, string? provenance = "Observed site log 471") => State.Prepare(
            Native.Command.RecorderRole, start ?? Native.Command.PeriodStartUtc.ToString("O"), Native.Command.PeriodEndUtc.ToString("O"),
            100, depth, Native.Command.DepthUnit, null, null, null, provenance, "Observed actual operations", null);
        public void Dispose() { State?.Dispose(); Company.Dispose(); Native.Dispose(); }
    }
    private static DrillingDailyReportReceipt Receipt(DrillingDailyReportInput original, bool recorded = true) =>
        new(original.Revision(), original.ReportId(), recorded ? original.ToRecord(DateTime.UtcNow) : null, DateTimeOffset.UtcNow);

    [Theory] [InlineData("")] [InlineData("2026-10-26T12:00:00")] [InlineData("2026-10-26T12:00:00+03:00")]
    public async Task ExplicitUtcIsRequiredBeforeFreezingOrTransport(string start)
    {
        using var f = new Fixture(); await f.Prepare(); await f.Inspect(); f.Freeze(start);
        Assert.Null(f.State.Prepared); Assert.NotNull(f.State.Error); await f.State.SubmitAsync();
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Theory] [InlineData("depth")] [InlineData("provenance")]
    public async Task MissingMeasurementsAndSourceCannotBecomeObservedProgress(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); await f.Inspect();
        if (fault == "depth") f.Freeze(depth: null); else f.Freeze(provenance: " ");
        Assert.Null(f.State.Prepared); Assert.NotNull(f.State.Error);
    }
    [Fact] public async Task FrozenReportRetainsBeforeTransportAndClearsOnlyItsExactReceipt()
    {
        using var f = new Fixture(); await f.Prepare(); await f.Inspect(); f.Freeze();
        f.State.Prepared!.Basis.Execution.Recorded!.STATUS = "Consumer mutation";
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput original, CancellationToken _) =>
        {
            Assert.Equal(original.Revision(), f.Journal.DailyReportOriginal!.Revision()); Assert.Equal("STARTED", original.Basis.Execution.Recorded!.STATUS); return Task.FromResult(Receipt(original));
        });
        await f.State.SubmitAsync(); Assert.NotNull(f.State.Receipt!.Recorded); Assert.Null(f.Journal.DailyReportOriginal);
        Assert.Null(f.State.Page); Assert.False(f.State.CanPrepare); Assert.Null(f.State.Pending);
    }
    [Theory] [InlineData("retain")] [InlineData("response")] [InlineData("receipt")] [InlineData("clear")]
    public async Task UnconfirmedReportPreservesOriginalWithoutBlindRetry(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); await f.Inspect(); f.Freeze(); var original = f.State.Prepared!;
        f.Journal.FailRetain = fault == "retain"; f.Journal.FailClear = fault == "clear";
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput input, CancellationToken _) =>
            fault == "response" ? Task.FromException<DrillingDailyReportReceipt>(new HttpRequestException("Lost report response")) :
                Task.FromResult(fault == "receipt" ? Receipt(input) with { IntentRevision = new('F', 64) } : Receipt(input)));
        await f.State.SubmitAsync(); Assert.Equal(original.Revision(), f.State.Pending!.Revision()); Assert.NotNull(f.State.Error);
        Assert.Null(f.State.Page); Assert.Null(f.State.Basis);
        if(fault=="clear") Assert.NotNull(f.State.Receipt!.Recorded); else Assert.Null(f.State.Receipt);
        Assert.Equal(fault is "response" or "receipt",f.State.PendingTabCopyConfirmed); Assert.Equal(fault!="retain",f.State.PendingSubmissionMayHaveBeenSent);
        Assert.Contains(fault=="retain"?"No submission was sent":fault=="clear"?"last validated receipt":"submission may have reached",f.State.PendingExplanation!,StringComparison.OrdinalIgnoreCase);
        await f.State.SubmitAsync();
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), fault == "retain" ? Times.Never() : Times.Once());
        f.Journal.FailClear = f.Journal.FailRetain = false;
        f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original, fault != "retain"));
        await f.State.RecoverAsync(); Assert.Null(f.State.Pending); Assert.Null(f.Journal.DailyReportOriginal); Assert.NotNull(f.State.Receipt);
    }
    [Fact] public async Task NewOwnerViewOnlyRecoveryRunsBeforeSourceOrUnmappedProfileLookup()
    {
        using var f = new Fixture(); await f.Prepare(); f.Journal.DailyReportOriginal = f.Native.Command; var scope = f.Native.Basis.Scope();
        f.Company.TrySelect(f.Company.Clear(), scope.ActorUserId, new(scope.Company, ["ReaderRole"], ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"]));
        f.Personas.SetupGet(p => p.CurrentPersona).Returns(new AppPersona { Code = "UNMAPPED" });
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); Assert.True(f.State.CanRecover); Assert.False(f.State.CanPrepare);
        f.Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(f.Native.Command));
        await f.State.RecoverAsync(); Assert.NotNull(f.State.Receipt!.Recorded);
        f.Client.Verify(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never());
        f.Personas.Verify(p => p.EnsureLoadedAsync(), Times.Never());
    }
    [Theory] [InlineData("company")] [InlineData("persona")] [InlineData("route")] [InlineData("dispose")]
    public async Task LateHistoryCannotPublishAfterContextOrRouteChanges(string fault)
    {
        using var f = new Fixture(); await f.Prepare();
        var waiting = new TaskCompletionSource<DrillingDailyReportPage>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.GetDailyReportPageAsync(f.Native.Execution.Basis.ReviewInstanceId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns(waiting.Task);
        f.Client.Setup(c => c.GetDailyReportPageAsync(DrillingProgramReviewStart.InstancePrefix + "other", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        var read = f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId);
        if (fault == "company") f.Company.Clear(); else if (fault == "persona") f.Personas.Raise(p => p.Changed += null!);
        else if (fault == "route") await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix + "other"); else f.State.Dispose();
        waiting.SetResult(f.History); await read; Assert.Null(f.State.Page); Assert.Null(f.State.Basis); Assert.False(f.State.Busy);
    }
    [Theory] [InlineData("recorded")] [InlineData("empty")] [InlineData("read-only")] [InlineData("unavailable")] [InlineData("no-access")]
    public async Task ActualReportPageShowsObservedHistoryAndDistinctReadStates(string mode)
    {
        using var f = new Fixture(); await f.Prepare(mode is "recorded" or "read-only");
        if (mode == "read-only") { var s = f.Company.Selection!; f.Company.TrySelect(f.Company.Clear(), s.UserId, new(s.Scope, ["ReaderRole"], ["Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports"])); }
        if (mode == "no-access") f.Company.Clear();
        if (mode == "unavailable") f.Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company); ui.Services.AddSingleton(f.Personas.Object); ui.Services.AddSingleton(f.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId=" + Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();
        if (mode == "unavailable") { page.WaitForAssertion(() => Assert.Contains("could not be confirmed", page.Markup)); Assert.DoesNotContain("No observed reports", page.Markup); }
        else if (mode == "no-access") Assert.Contains("Choose the company", page.Markup);
        else if (mode == "empty") page.WaitForAssertion(() => Assert.Contains("No observed reports", page.Markup));
        else { page.WaitForAssertion(() => Assert.Contains(f.Native.Command.Provenance, page.Markup)); Assert.Contains("Unknown / Unknown", page.Markup); Assert.Contains("execution-recorder", page.Markup); if (mode == "read-only") Assert.DoesNotContain("Inspect current report basis", page.Markup); }
        f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact] public async Task ActualProtectedTabSeparatesReportFromStartCompanyActorAndOtherOriginals()
    {
        using var f = new Fixture(); await f.Prepare(); var original = f.Native.Command; var scope = original.Basis.Scope();
        var tab = new DevelopmentDraftIntentStoreTests.TabStorage(); var protection = new EphemeralDataProtectionProvider();
        DevelopmentDraftIntentStore Store() => new(new ProtectedSessionStorage(tab, protection));
        await Store().RetainDailyReportAsync(original); var encrypted = Assert.Single(tab.Values).Value;
        Assert.DoesNotContain(original.Provenance, encrypted); Assert.DoesNotContain(original.IntentId.ToString(), encrypted);
        Assert.Equal(original.Revision(), (await Store().ReadDailyReportAsync(scope.Company, scope.ActorUserId))!.Revision());
        Assert.Null(await Store().ReadDailyReportAsync(new("tenant", "other"), scope.ActorUserId)); Assert.Null(await Store().ReadDailyReportAsync(scope.Company, "other"));
        Assert.Null(await Store().ReadExecutionAsync(scope.Company, scope.ActorUserId));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainDailyReportAsync(original with { IntentId = Guid.NewGuid() }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearDailyReportAsync(original with { Summary = "Another report" }));
        await Store().ClearDailyReportAsync(original); Assert.Empty(tab.Values);
        await Store().RetainDailyReportAsync(original); tab.Values[Assert.Single(tab.Values).Key] = "corrupted";
        await Assert.ThrowsAnyAsync<Exception>(() => Store().ReadDailyReportAsync(scope.Company, scope.ActorUserId));
    }

    [Theory] [InlineData("company")] [InlineData("persona")] [InlineData("route")] [InlineData("dispose")]
    public async Task LateRecordedAcknowledgementPreservesItsJournalAfterContextChanges(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); await f.Inspect(); f.Freeze();
        var original = f.State.Prepared!; var waiting = new TaskCompletionSource<DrillingDailyReportReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns(waiting.Task);
        var save = f.State.SubmitAsync(); Assert.NotNull(f.Journal.DailyReportOriginal);
        if (fault == "company") f.Company.Clear(); else if (fault == "persona") f.Personas.Raise(p => p.Changed += null!);
        else if (fault == "route") await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix + "other"); else f.State.Dispose();
        waiting.SetResult(Receipt(original)); await save; Assert.Null(f.State.Receipt); Assert.Null(f.State.Confirmation);
        Assert.Equal(original.Revision(), f.Journal.DailyReportOriginal!.Revision()); Assert.False(f.State.Busy);
    }

    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("role")] [InlineData("persona")] [InlineData("permission")]
    public async Task WrongCurrentCompanyAccountRolePersonaOrPermissionCannotPrepareReport(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); var current = f.Company.Selection!;
        if (fault == "persona") f.Personas.SetupGet(p => p.CurrentPersona).Returns(new AppPersona { Code = "OTHER" });
        else f.Company.TrySelect(f.Company.Clear(), fault == "actor" ? "other" : current.UserId,
            new(fault == "company" ? new("tenant", "foreign") : current.Scope, fault == "role" ? ["OTHER"] : current.Roles.ToArray(), fault == "permission" ? [] : current.Permissions.ToArray()));
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId); await f.State.InspectAsync(); Assert.False(f.State.CanPrepare);
        await f.State.SubmitAsync(); f.Client.Verify(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    [Fact] public async Task SavingAndSendingExposeDifferentKnowledgeBeforeReceipt()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();
        var retain=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sent=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var answer=new TaskCompletionSource<DrillingDailyReportReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Journal.BeforeReviewRetain=()=>retain.Task;
        f.Client.Setup(c=>c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput input,CancellationToken _)=>{sent.SetResult();return answer.Task;});
        var original=f.State.Prepared!;var save=f.State.SubmitAsync();
        Assert.Contains("Saving original",f.State.PendingTitle);Assert.False(f.State.PendingTabCopyConfirmed);Assert.False(f.State.PendingSubmissionMayHaveBeenSent);Assert.Null(f.State.Error);
        f.Client.Verify(c=>c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>()),Times.Never());
        retain.SetResult();await sent.Task;Assert.True(f.State.PendingTabCopyConfirmed);Assert.True(f.State.PendingSubmissionMayHaveBeenSent);Assert.Contains("saved in this tab",f.State.PendingExplanation);Assert.Null(f.State.Receipt);
        answer.SetResult(Receipt(original));await save;Assert.NotNull(f.State.Receipt!.Recorded);Assert.Null(f.State.Pending);Assert.Null(f.State.PendingExplanation);
    }
    [Fact] public async Task UnconfirmedReadbackDoesNotPromiseTabPersistenceOrNoSubmissionAfterReload()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();f.Journal.FailRetainReadback=true;
        await f.State.SubmitAsync();Assert.NotNull(f.Journal.DailyReportOriginal);Assert.False(f.State.PendingTabCopyConfirmed);Assert.False(f.State.PendingSubmissionMayHaveBeenSent);
        Assert.Contains("No submission was sent",f.State.PendingExplanation);Assert.Contains("kept on this page",f.State.PendingTitle);
        f.Client.Verify(c=>c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>()),Times.Never());
        f.State.Dispose();using var next=new DrillingDailyReportState(f.Company,f.Personas.Object,f.Client.Object,f.Journal,(_,_,text)=>text);
        await next.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId);Assert.True(next.PendingTabCopyConfirmed);Assert.True(next.PendingSubmissionMayHaveBeenSent);
        Assert.DoesNotContain("No submission was sent",next.PendingExplanation);Assert.True(next.CanRecover);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ValidatedReceiptSurvivesCleanupFailureAndAnotherFailedRecovery(bool recorded)
    {
        using var f=new Fixture();await f.Prepare();var original=f.Native.Command;f.Journal.DailyReportOriginal=original;f.Journal.FailClear=true;
        f.Client.Setup(c=>c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,recorded));
        await f.State.LoadAsync(f.Native.Execution.Basis.ReviewInstanceId);await f.State.RecoverAsync();
        Assert.NotNull(f.State.Receipt);Assert.Equal(recorded,f.State.Receipt!.Recorded is not null);Assert.NotNull(f.State.Pending);Assert.False(f.State.PendingTabCopyConfirmed);Assert.Null(f.State.Confirmation);
        Assert.Contains("last validated receipt",f.State.PendingExplanation);Assert.Contains(recorded?"was recorded":"was absent",f.State.PendingExplanation);
        f.Client.Setup(c=>c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Recovery unavailable"));
        await f.State.RecoverAsync();Assert.Equal(recorded,f.State.Receipt!.Recorded is not null);Assert.NotNull(f.State.Error);Assert.NotNull(f.Journal.DailyReportOriginal);Assert.False(f.State.CanPrepare);
        f.Client.Verify(c=>c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Journal.FailClear=false;f.Client.Setup(c=>c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,recorded));
        await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.Null(f.Journal.DailyReportOriginal);Assert.NotNull(f.State.Confirmation);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualReportPageShowsValidatedOutcomeAlongsideUnconfirmedInputCleanup(bool recorded)
    {
        using var f=new Fixture();await f.Prepare();f.Journal.DailyReportOriginal=f.Native.Command;f.Journal.FailClear=true;
        f.Client.Setup(c=>c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(f.Native.Command,recorded));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reports?reviewId="+Uri.EscapeDataString(f.Native.Execution.Basis.ReviewInstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingDailyReports>();page.WaitForAssertion(()=>Assert.Contains("Retained original drilling report",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Recover original drilling report")).Click();
        page.WaitForAssertion(()=>Assert.Contains("awaiting input cleanup",page.Markup));Assert.Contains("last validated receipt",page.Markup);Assert.Contains("Outcome checked",page.Markup);
        Assert.Contains(recorded?"RECORDED":"confirmed absent",page.Markup);Assert.NotNull(f.Journal.DailyReportOriginal);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("dispose")]
    public async Task ContextChangeAfterReceiptPublicationCannotClearTheOriginalInput(string change)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();
        f.Client.Setup(c=>c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(),It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput input,CancellationToken _)=>Task.FromResult(Receipt(input)));
        var changed=false;f.State.Changed+=()=>{if(changed || f.State.Receipt is null)return;changed=true;if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);else f.State.Dispose();};
        await f.State.SubmitAsync();Assert.True(changed);Assert.Null(f.State.Receipt);Assert.Null(f.State.Confirmation);Assert.NotNull(f.Journal.DailyReportOriginal);Assert.False(f.State.Busy);Assert.Null(f.State.PendingExplanation);
    }
}
#endif
