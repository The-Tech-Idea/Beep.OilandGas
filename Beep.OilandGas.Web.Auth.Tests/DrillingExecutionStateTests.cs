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
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingExecutionStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingExecutionStartNativeTests.Fixture Native=new();internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingExecutionState State=null!;internal DrillingExecutionStatus Latest=null!;
        internal async Task Prepare(bool recorded=false)
        {
            await Native.Prepare();var source=Native.Current;var scope=source.Scope();Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,source.RecorderRoles.ToArray(),["Drilling.View","Drilling.Create","Drilling.ManageWellProgram","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=source.PersonaCode!,Name="Actual execution workspace"});Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=source.PersonaCode!});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);Latest=new(source,recorded?Native.Command.ToRecord(DateTime.UtcNow):null,"",DateTimeOffset.UtcNow);Latest=Latest with{Revision=Latest.ContentRevision()};Latest.Require();Client.Setup(c=>c.GetExecutionStatusAsync(source.ReviewInstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Latest);Client.Setup(c=>c.GetExecutionBasisAsync(source.ReviewInstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Native.Current);State=new(Company,Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        }
        internal async Task Inspect(){await State.LoadAsync(Native.Current.ReviewInstanceId);await State.InspectAsync();Assert.NotNull(State.Basis);}
        internal void Freeze(string? utc=null)=>State.Prepare(Native.Command.RecorderRole,utc??Native.Command.SpudAtUtc.ToString("O"),Native.Command.Rationale);
        public void Dispose(){State?.Dispose();Company.Dispose();Native.Dispose();}
    }
    internal static DrillingExecutionStartReceipt Receipt(DrillingExecutionStart command,bool recorded=true)=>new(command.Revision(),command.OperationId(),recorded?command.ToRecord(DateTime.UtcNow):null,DateTimeOffset.UtcNow);
    [Theory][InlineData("", "Rationale")][InlineData("2026-10-26T12:30:00", "Rationale")][InlineData("2026-10-26T12:30:00+03:00", "Rationale")][InlineData("valid", " ")]
    public async Task ExplicitActualUtcAndRationaleAreRequiredBeforeConfirmationOrTransport(string utc,string rationale)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.State.Prepare(f.Native.Command.RecorderRole,utc=="valid"?f.Native.Command.SpudAtUtc.ToString("O"):utc,rationale);Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);await f.State.SubmitAsync();Assert.Null(f.Journal.ExecutionOriginal);f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateSourceCannotSurviveContextOrRouteChanges(string change)
    {
        using var f=new Fixture();await f.Prepare();var waiting=new TaskCompletionSource<DrillingExecutionStatus>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.GetExecutionStatusAsync(f.Native.Current.ReviewInstanceId,It.IsAny<CancellationToken>())).Returns(waiting.Task);f.Client.Setup(c=>c.GetExecutionStatusAsync(DrillingProgramReviewStart.InstancePrefix+"other",It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());var read=f.State.LoadAsync(f.Native.Current.ReviewInstanceId);if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);else if(change=="route")await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix+"other");else f.State.Dispose();waiting.SetResult(f.Latest);await read;Assert.Null(f.State.Status);Assert.Null(f.State.Basis);Assert.False(f.State.Busy);
    }
    [Fact]public async Task AReportedFutureEventCannotBeConfirmedByTheScreen()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze(DateTimeOffset.UtcNow.AddDays(1).ToString("O"));Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task ExactOriginalStartIsFrozenRetainedBeforeTransportAndClearedOnlyAfterNativeReceipt()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;original.Basis.Source.Program.PROGRAM_NAME="Consumer mutation";f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionStart value,CancellationToken _)=>{Assert.Equal(value.Revision(),f.Journal.ExecutionOriginal!.Revision());Assert.NotEqual("Consumer mutation",value.Basis.Source.Program.PROGRAM_NAME);return Task.FromResult(Receipt(value));});await f.State.SubmitAsync();Assert.Null(f.Journal.ExecutionOriginal);Assert.Null(f.State.Pending);Assert.NotNull(f.State.Receipt!.Recorded);Assert.Null(f.State.Status);Assert.False(f.State.CanPrepare);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("receipt")][InlineData("clear")]
    public async Task UnconfirmedStartPreservesExactRecoveryInputAndNeverBlindlyResubmits(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="clear";f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionStart value,CancellationToken _)=>fault=="response"?Task.FromException<DrillingExecutionStartReceipt>(new HttpRequestException("Lost response")):Task.FromResult(fault=="receipt"?Receipt(value) with{IntentRevision=new('F',64)}:Receipt(value)));await f.State.SubmitAsync();Assert.Equal(original.Revision(),f.State.Pending!.Revision());if(fault=="clear")Assert.NotNull(f.State.Receipt!.Recorded);else Assert.Null(f.State.Receipt);Assert.NotNull(f.State.Error);Assert.Equal(fault is "response" or "receipt",f.State.PendingTabCopyConfirmed);Assert.Equal(fault!="retain",f.State.PendingSubmissionMayHaveBeenSent);Assert.Contains(fault=="retain"?"No submission was sent":fault=="clear"?"last validated receipt":"submission may have reached",f.State.PendingExplanation!,StringComparison.OrdinalIgnoreCase);await f.State.SubmitAsync();f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,fault!="retain"));await f.State.RecoverAsync();Assert.Null(f.Journal.ExecutionOriginal);Assert.Null(f.State.Pending);Assert.NotNull(f.State.Receipt);
    }
    [Fact]public async Task NewOwnerWithOnlyViewRightsRecoversBeforeAnySourceOrProfileLookup()
    {
        using var f=new Fixture();await f.Prepare();f.Journal.ExecutionOriginal=f.Native.Command;var current=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),current.UserId,new(current.Scope,["CurrentReaderRole"],["Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));f.Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code="UNMAPPED"});await f.State.LoadAsync(f.Native.Current.ReviewInstanceId);Assert.True(f.State.CanRecover);Assert.False(f.State.CanPrepare);f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(f.Native.Command));await f.State.RecoverAsync();Assert.NotNull(f.State.Receipt!.Recorded);Assert.Null(f.Journal.ExecutionOriginal);f.Client.Verify(c=>c.GetExecutionStatusAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateStartAcknowledgementCannotClearOriginalOrPublishAfterContextChange(string change)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;var waiting=new TaskCompletionSource<DrillingExecutionStartReceipt>();f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var write=f.State.SubmitAsync();Assert.NotNull(f.Journal.ExecutionOriginal);if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);else if(change=="route")await f.State.LoadAsync(DrillingProgramReviewStart.InstancePrefix+"other");else f.State.Dispose();waiting.SetResult(Receipt(original));await write;Assert.Null(f.State.Receipt);Assert.Null(f.State.Status);Assert.Equal(original.Revision(),f.Journal.ExecutionOriginal!.Revision());
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("permission")][InlineData("role")][InlineData("persona")]
    public async Task CurrentCompanyActorRolesPermissionsAndMappedProfileCannotBeSubstituted(string fault)
    {
        using var f=new Fixture();await f.Prepare();var current=f.Company.Selection!;if(fault=="persona")f.Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code="OTHER"});else f.Company.TrySelect(f.Company.Clear(),fault=="actor"?"other":current.UserId,new(fault=="company"?new("tenant","foreign"):current.Scope,fault=="role"?["OTHER"]:current.Roles.ToArray(),fault=="permission"?[]:current.Permissions.ToArray()));await f.State.LoadAsync(f.Native.Current.ReviewInstanceId);Assert.False(f.State.CanInspect);Assert.False(f.State.CanPrepare);if(fault is "company" or "actor" or "permission")Assert.Null(f.State.Status);else Assert.NotNull(f.State.Status);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualExecutionPageShowsRecordedOutcomeOrExplicitFrozenStartWithActualUtc(bool recorded)
    {
        using var f=new Fixture();await f.Prepare(recorded);f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionStart value,CancellationToken _)=>{Assert.NotNull(f.Journal.ExecutionOriginal);return Task.FromResult(Receipt(value));});using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution?reviewId="+Uri.EscapeDataString(f.Native.Current.ReviewInstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecution>();page.WaitForAssertion(()=>Assert.Contains("Exact independently approved program",page.Markup));Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("2 recorded independent approvals",page.Markup);Assert.Contains("Planned MD 2500.25 m",page.Markup);
        if(recorded){Assert.Contains("Recorded drilling execution",page.Markup);Assert.Contains("Actual reported spud",page.Markup);Assert.Contains("execution-recorder",page.Markup);Assert.DoesNotContain("Report actual execution start",page.Markup);Assert.DoesNotContain("Inspect actual execution start basis",page.Markup);return;}
        Assert.Contains("No execution start is recorded",page.Markup);page.FindAll("button").Single(b=>b.TextContent=="Inspect actual execution start basis").Click();page.WaitForAssertion(()=>Assert.Contains("Report actual execution start",page.Markup));var roles=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Current execution recorder role");await roles.InvokeAsync(()=>roles.Instance.ValueChanged.InvokeAsync(f.Native.Command.RecorderRole));var spud=page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Actual spud UTC");await spud.InvokeAsync(()=>spud.Instance.ValueChanged.InvokeAsync(f.Native.Command.SpudAtUtc.ToString("O")));var rationale=page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Execution start rationale");await rationale.InvokeAsync(()=>rationale.Instance.ValueChanged.InvokeAsync("Actual site source and UTC confirmed"));page.FindAll("button").Single(b=>b.TextContent=="Review actual execution start").Click();Assert.Contains("Confirm original execution start",page.Markup);Assert.Contains("Actual site source and UTC confirmed",page.Markup);f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());page.FindAll("button").Single(b=>b.TextContent=="Confirm and retain execution start").Click();page.WaitForAssertion(()=>Assert.Contains("Original execution start outcome",page.Markup));Assert.Contains("STARTED",page.Markup);Assert.Contains("Role ConfiguredExecutionRecorder",page.Markup);Assert.Contains("Workspace DRILLING_RECORDER",page.Markup);Assert.Null(f.Journal.ExecutionOriginal);
    }
    [Theory][InlineData("read-only")][InlineData("unavailable")][InlineData("no-access")]
    public async Task ActualPageSeparatesCurrentReadOnlyRecordedOutcomeUnavailableSourceAndMissingAccess(string mode)
    {
        using var f=new Fixture();await f.Prepare(true);if(mode=="read-only"){var current=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),current.UserId,new(current.Scope,["ReadOnlyRole"],["Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));}if(mode=="no-access")f.Company.Clear();if(mode=="unavailable")f.Client.Setup(c=>c.GetExecutionStatusAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution?reviewId="+Uri.EscapeDataString(f.Native.Current.ReviewInstanceId));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecution>();if(mode=="read-only"){page.WaitForAssertion(()=>Assert.Contains("Recorded drilling execution",page.Markup));Assert.Contains("Actual reported spud",page.Markup);Assert.DoesNotContain("Inspect actual execution start basis",page.Markup);}else if(mode=="unavailable"){page.WaitForAssertion(()=>Assert.Contains("could not be confirmed",page.Markup));Assert.DoesNotContain(f.Native.Current.Source.Program.PROGRAM_NAME,page.Markup);}else{Assert.Contains("Choose a company",page.Markup);Assert.DoesNotContain(f.Native.Current.Source.Program.PROGRAM_NAME,page.Markup);}f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task ActualEncryptedStoragePreservesStartAcrossOwnersAndSeparatesCompanyActorCommandKindsAndConflicts()
    {
        using var f=new Fixture();await f.Prepare();var original=f.Native.Command;var scope=original.Basis.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));await Store().RetainExecutionAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Rationale,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);Assert.Equal(original.Revision(),(await Store().ReadExecutionAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadExecutionAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadExecutionAsync(scope.Company,"other"));Assert.Null(await Store().ReadProgramDecisionAsync(scope.Company,scope.ActorUserId));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainExecutionAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearExecutionAsync(original with{Rationale="Changed original"}));await Store().ClearExecutionAsync(original);Assert.Empty(tab.Values);await Store().RetainExecutionAsync(original);tab.Values[Assert.Single(tab.Values).Key]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadExecutionAsync(scope.Company,scope.ActorUserId));
    }
    [Fact] public async Task SavingAndSendingExposeDifferentKnowledgeBeforeReceipt()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();
        var retain=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sent=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var answer=new TaskCompletionSource<DrillingExecutionStartReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Journal.BeforeReviewRetain=()=>retain.Task;
        f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionStart input,CancellationToken _)=>{sent.SetResult();return answer.Task;});
        var original=f.State.Prepared!;var save=f.State.SubmitAsync();
        Assert.Contains("Saving original",f.State.PendingTitle);Assert.False(f.State.PendingTabCopyConfirmed);Assert.False(f.State.PendingSubmissionMayHaveBeenSent);Assert.Null(f.State.Error);
        f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
        retain.SetResult();await sent.Task;Assert.True(f.State.PendingTabCopyConfirmed);Assert.True(f.State.PendingSubmissionMayHaveBeenSent);Assert.Contains("saved in this tab",f.State.PendingExplanation);Assert.Null(f.State.Receipt);
        answer.SetResult(Receipt(original));await save;Assert.NotNull(f.State.Receipt!.Recorded);Assert.Null(f.State.Pending);Assert.Null(f.State.PendingExplanation);
    }
    [Fact] public async Task UnconfirmedReadbackDoesNotPromiseTabPersistenceOrNoSubmissionAfterReload()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();f.Journal.FailRetainReadback=true;
        await f.State.SubmitAsync();Assert.NotNull(f.Journal.ExecutionOriginal);Assert.False(f.State.PendingTabCopyConfirmed);Assert.False(f.State.PendingSubmissionMayHaveBeenSent);
        Assert.Contains("No submission was sent",f.State.PendingExplanation);Assert.Contains("kept on this page",f.State.PendingTitle);
        f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
        f.State.Dispose();using var next=new DrillingExecutionState(f.Company,f.Personas.Object,f.Client.Object,f.Journal,(_,_,text)=>text);
        await next.LoadAsync(f.Native.Current.ReviewInstanceId);Assert.True(next.PendingTabCopyConfirmed);Assert.True(next.PendingSubmissionMayHaveBeenSent);
        Assert.DoesNotContain("No submission was sent",next.PendingExplanation);Assert.True(next.CanRecover);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ValidatedReceiptSurvivesCleanupFailureAndAnotherFailedRecovery(bool recorded)
    {
        using var f=new Fixture();await f.Prepare();var original=f.Native.Command;f.Journal.ExecutionOriginal=original;f.Journal.FailClear=true;
        f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,recorded));
        await f.State.LoadAsync(f.Native.Current.ReviewInstanceId);await f.State.RecoverAsync();
        Assert.NotNull(f.State.Receipt);Assert.Equal(recorded,f.State.Receipt!.Recorded is not null);Assert.NotNull(f.State.Pending);Assert.False(f.State.PendingTabCopyConfirmed);Assert.Null(f.State.Confirmation);
        Assert.Contains("last validated receipt",f.State.PendingExplanation);Assert.Contains(recorded?"was recorded":"was absent",f.State.PendingExplanation);
        f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Recovery unavailable"));
        await f.State.RecoverAsync();Assert.Equal(recorded,f.State.Receipt!.Recorded is not null);Assert.NotNull(f.State.Error);Assert.NotNull(f.Journal.ExecutionOriginal);Assert.False(f.State.CanPrepare);
        f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Journal.FailClear=false;f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(original,recorded));
        await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.Null(f.Journal.ExecutionOriginal);Assert.NotNull(f.State.Confirmation);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualStartPageShowsValidatedOutcomeAlongsideUnconfirmedInputCleanup(bool recorded)
    {
        using var f=new Fixture();await f.Prepare();f.Journal.ExecutionOriginal=f.Native.Command;f.Journal.FailClear=true;
        f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(Receipt(f.Native.Command,recorded));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution?reviewId="+Uri.EscapeDataString(f.Native.Current.ReviewInstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecution>();page.WaitForAssertion(()=>Assert.Contains("Retained original execution start",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Recover original execution start")).Click();
        page.WaitForAssertion(()=>Assert.Contains("awaiting input cleanup",page.Markup));Assert.Contains("last validated receipt",page.Markup);Assert.Contains("Outcome checked",page.Markup);
        Assert.Contains(recorded?"Recorded operation":"confirmed absent",page.Markup);Assert.NotNull(f.Journal.ExecutionOriginal);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("dispose")]
    public async Task ContextChangeAfterReceiptPublicationCannotClearTheOriginalInput(string change)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();
        f.Client.Setup(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionStart input,CancellationToken _)=>Task.FromResult(Receipt(input)));
        var changed=false;f.State.Changed+=()=>{if(changed || f.State.Receipt is null)return;changed=true;if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);else f.State.Dispose();};
        await f.State.SubmitAsync();Assert.True(changed);Assert.Null(f.State.Receipt);Assert.Null(f.State.Confirmation);Assert.NotNull(f.Journal.ExecutionOriginal);Assert.False(f.State.Busy);Assert.Null(f.State.PendingExplanation);
    }
}
#endif
