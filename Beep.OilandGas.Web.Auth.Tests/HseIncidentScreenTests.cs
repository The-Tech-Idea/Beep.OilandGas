using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using Bunit;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Web.Pages.PPDM39.BusinessProcess;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Moq;
using MudBlazor.Services;
using MudBlazor;
using Beep.OilandGas.Client.App;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class HseIncidentScreenTests
{
    [Theory]
    [InlineData("valid")]
    [InlineData("definition")]
    [InlineData("instance")]
    public async Task AssignedIncidentSourceLinksExactWorkflowAndRefusesGenericActions(string fault)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        var source = new Beep.OilandGas.Models.Processes.ProcessInstance { InstanceId = fault == "instance" ? "unconfirmed" : "hse-process-original/شركة?", EntityId = "case/واحد?part#2", EntityType = CompanyHseWorkflowStart.EntityType, FieldId = "field", ProcessId = fault == "definition" ? "OTHER" : CompanyHseWorkflowStart.ProcessId };
        using var state = new ProcessInstanceDetailState(company, _ => Task.FromResult<Beep.OilandGas.Models.Processes.ProcessInstance?>(source), _ => Task.FromResult(new List<Beep.OilandGas.Models.Processes.ProcessHistoryEntry>()));
        await state.LoadAsync(source.InstanceId, () => Task.FromResult<string?>("field"));
        var attempted = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.LoadActionsAsync(_ => { attempted = true; throw new InvalidOperationException("Generic transport must not run."); }));
        Assert.False(attempted); Assert.Null(state.ActionReview); Assert.True(state.IsHseIncident);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        var page = ui.RenderComponent<Beep.OilandGas.Web.Components.BusinessProcess.RecordedProcessSourcePanel>(p => p.Add(x => x.State, state));
        if (fault == "valid")
        {
            var link = page.FindAll("a").Single(a => a.TextContent.Contains("Open recorded incident workflow"));
            Assert.Equal("/ppdm39/hse/incident-detail?incidentId=" + Uri.EscapeDataString(source.EntityId) + "&workflowInstanceId=" + Uri.EscapeDataString(source.InstanceId), link.GetAttribute("href"));
        }
        else { Assert.Null(state.HseIncidentRoute); Assert.Contains("workflow link could not be confirmed", page.Markup); Assert.Empty(page.FindAll("a")); }
        company.Clear(); page.Render(); Assert.DoesNotContain("Open recorded incident workflow", page.Markup); Assert.Null(state.HseIncidentRoute);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("mismatch")]
    [InlineData("missing")]
    [InlineData("invalid")]
    [InlineData("changed-query")]
    public void IncidentWorkflowDeepLinkRequiresExactCurrentRecordedInstance(string mode)
    {
        const string incidentId = "case/واحد?part#2", workflowId = "hse-process-recorded/شركة?";
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord { IncidentId = incidentId, FieldId = "field", Description = "Bound incident details", CurrentState = IncidentState.PendingCloseOut });
        incidents.Setup(x => x.GetIncidentCausesAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync([]); incidents.Setup(x => x.GetIncidentCorrectiveActionsAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync(mode == "missing" ? null : new Beep.OilandGas.Models.Processes.ProcessInstance { InstanceId = workflowId, EntityId = incidentId, FieldId = "field", EntityType = CompanyHseWorkflowStart.EntityType, ProcessId = CompanyHseWorkflowStart.ProcessId });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object);
        var navigation = ui.Services.GetRequiredService<NavigationManager>();
        string Link(string expected) => "/ppdm39/hse/incident-detail?incidentId=" + Uri.EscapeDataString(incidentId) + "&workflowInstanceId=" + Uri.EscapeDataString(expected);
        navigation.NavigateTo(Link(mode == "invalid" ? " " : mode == "mismatch" ? "another-workflow" : workflowId));
        var page = ui.RenderComponent<HSEIncidentDetail>();
        if (mode is "valid" or "changed-query")
        {
            page.WaitForAssertion(() => Assert.Contains("Bound incident details", page.Markup));
            if (mode == "changed-query") { navigation.NavigateTo(Link("another-workflow")); page.SetParametersAndRender(); }
        }
        if (mode != "valid")
        {
            page.WaitForAssertion(() => Assert.Contains("Could not confirm incident detail", page.Markup)); Assert.DoesNotContain("Bound incident details", page.Markup);
        }
        if (mode == "invalid") incidents.Verify(x => x.GetIncidentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AssignedHseProcessPageOffersReviewedIncidentInsteadOfGenericActions(bool confirmedIdentity)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        var id = confirmedIdentity ? "hse-process-recorded" : "unconfirmed";
        var process = new Beep.OilandGas.Models.Processes.ProcessInstance { InstanceId = id, EntityId = "case/شركة?", EntityType = CompanyHseWorkflowStart.EntityType, ProcessId = CompanyHseWorkflowStart.ProcessId, FieldId = "field" };
        var client = new Mock<IRecordedProcessClient>(); client.Setup(x => x.GetInstanceAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(process); client.Setup(x => x.GetHistoryAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldIdAsync()).ReturnsAsync("field");
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.ComponentFactories.AddStub<Beep.OilandGas.Web.Components.FieldSelector>();
        var page = ui.RenderComponent<ProcessInstanceDetail>(p => p.Add(x => x.Id, id));
        page.WaitForAssertion(() => Assert.Contains("Generic state actions are unavailable for an incident", page.Markup));
        Assert.DoesNotContain("Review source and available actions", page.Markup);
        if (confirmedIdentity) Assert.Contains("workflowInstanceId=hse-process-recorded", page.Markup);
        else { Assert.Contains("workflow link could not be confirmed", page.Markup); Assert.DoesNotContain("Open recorded incident workflow", page.Markup); }
        client.Verify(x => x.GetTransitionReviewAsync(It.IsAny<string>()), Times.Never);
    }

    private sealed class MemoryRcaStore : IIncidentRcaIntentStore
    {
        public IncidentRcaIntent? Value;
        public Task<IncidentRcaIntent?> ReadRcaAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Value);
        public Task RetainAsync(IncidentRcaIntent original) { if (Value is not null && Value != original) throw new InvalidOperationException(); Value = original; return Task.CompletedTask; }
        public Task ClearAsync(IncidentRcaIntent original) { Assert.Equal(original, Value); Value = null; return Task.CompletedTask; }
    }
    private sealed class MemoryRcaHandoffStore : IIncidentRcaHandoffIntentStore
    {
        public CompanyHseRcaHandoffCommand? Value;
        public Task<CompanyHseRcaHandoffCommand?> ReadHandoffAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Value);
        public Task RetainAsync(CompanyHseRcaHandoffCommand original) { if (Value is not null && Value != original) throw new InvalidOperationException(); Value = original; return Task.CompletedTask; }
        public Task ClearAsync(CompanyHseRcaHandoffCommand original) { Assert.Equal(original, Value); Value = null; return Task.CompletedTask; }
    }
    private sealed class MemoryActionStore : ICorrectiveActionIntentStore
    {
        public CompanyHseCorrectionCommand? Correction;
        public Task<CompanyHseCorrectionCommand?> ReadCorrectionAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Correction);
        public Task RetainCorrectionAsync(CompanyHseCorrectionCommand original) { Correction = original; return Task.CompletedTask; }
        public Task ClearCorrectionAsync(CompanyHseCorrectionCommand original) { Assert.Equal(JsonSerializer.Serialize(Correction), JsonSerializer.Serialize(original)); Correction = null; return Task.CompletedTask; }

        public CorrectiveActionOriginal? Value;
        public CorrectiveActionCompletionOriginal? Completion;
        public CAVerificationDecision? Verification;
        public CompanyHseClosureCommand? Closure;
        public IncidentClosureResolution? Resolution;
        public Task<IncidentClosureResolution?> ReadResolutionAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Resolution);
        public Task RetainResolutionAsync(IncidentClosureResolution original) { if (Resolution is not null && JsonSerializer.Serialize(Resolution) != JsonSerializer.Serialize(original)) throw new InvalidOperationException(); Resolution = original; return Task.CompletedTask; }
        public Task ClearResolutionAsync(IncidentClosureResolution original) { Resolution = null; return Task.CompletedTask; }
        public Task<CompanyHseClosureCommand?> ReadClosureAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Closure);
        public Task RetainClosureAsync(CompanyHseClosureCommand original) { if (Closure is not null && JsonSerializer.Serialize(Closure) != JsonSerializer.Serialize(original)) throw new InvalidOperationException(); Closure = original; return Task.CompletedTask; }
        public Task ClearClosureAsync(CompanyHseClosureCommand original) { Closure = null; return Task.CompletedTask; }
        public Task<CAVerificationDecision?> ReadVerificationAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Verification);
        public Task RetainVerificationAsync(CAVerificationDecision original) { if (Verification is not null && Verification != original) throw new InvalidOperationException(); Verification = original; return Task.CompletedTask; }
        public Task ClearVerificationAsync(CAVerificationDecision original) { Assert.Equal(original, Verification); Verification = null; return Task.CompletedTask; }
        public Task<CorrectiveActionCompletionOriginal?> ReadCompletionAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Completion);
        public Task RetainCompletionAsync(CorrectiveActionCompletionOriginal original) { if (Completion is not null && Completion != original) throw new InvalidOperationException(); Completion = original; return Task.CompletedTask; }
        public Task ClearCompletionAsync(CorrectiveActionCompletionOriginal original) { Assert.Equal(original, Completion); Completion = null; return Task.CompletedTask; }
        public Task<CorrectiveActionOriginal?> ReadAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Value);
        public Task RetainAsync(CorrectiveActionOriginal original) { if (Value is not null && Value != original) throw new InvalidOperationException(); Value = original; return Task.CompletedTask; }
        public Task ClearAsync(CorrectiveActionOriginal original) { Assert.Equal(original, Value); Value = null; return Task.CompletedTask; }
    }
    private sealed class ActionTransport(string id) : HttpMessageHandler
    {
        public AddCASubmission? Submission;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/field/current/hse/incidents/case/cas", request.RequestUri!.AbsolutePath);
            Submission = await request.Content!.ReadFromJsonAsync<AddCASubmission>(cancellationToken: token);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = System.Net.Http.Json.JsonContent.Create(new AddCAReceipt(Submission!.Context, "case", Submission.Request.ActionId, id)) };
        }
    }
    [Fact]
    public async Task VerificationScreenShowsRecordedEvidenceAndDoesNotAssertClosure()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var action = new CAVerificationAction(3, "step", "Inspect valve", "COMPLETED", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), "other-operator", "Tested valve");
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewVerificationAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, true).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true));
        page.WaitForAssertion(() => Assert.Contains("Review verification evidence", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Review verification evidence")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Tested valve", page.Markup));
        Assert.Contains("other-operator", page.Markup); Assert.Contains("incident remains open", page.Markup);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseCorrectiveAction).GetField("verificationDecision", flags)!.SetValue(page.Instance, "VERIFIED");
        typeof(HseCorrectiveAction).GetField("verificationReason", flags)!.SetValue(page.Instance, "Independent inspection confirmed the repair.");
        CAVerificationDecision? sent = null;
        client.Setup(x => x.RecordVerificationAsync(It.IsAny<CAVerificationDecision>(), It.IsAny<CancellationToken>())).Returns((CAVerificationDecision original, CancellationToken _) => { sent = original; return Task.FromResult(original); });
        await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod("RecordVerificationAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.WaitForAssertion(() => Assert.True(page.Markup.Contains("Verification decision recorded"), page.Markup));
        Assert.NotNull(sent); Assert.Equal("VERIFIED", sent.Decision);
        client.Setup(x => x.ReviewClosureAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentClosureReview(sent.Review, [sent]));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Review incident closure prerequisites")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Independent verification is recorded for the current action evidence", page.Markup));
        Assert.Contains("Lifecycle verification and incident closure approvals are separate recorded stages", page.Markup);
        Assert.Contains("This review does not close the incident", page.Markup);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerificationRecoveryAfterNewFieldReviewNeverReplaysOldDecision(bool recorded)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var prior = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "old-review", "field");
        var current = prior with { Revision = "new-review" };
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), "other", "Tested valve");
        var original = new CAVerificationDecision(Guid.NewGuid(), new(prior, "case", "plan", new string('A', 64), [action]), "VERIFIED", "Independent inspection confirmed the repair.");
        var store = new MemoryActionStore { Verification = original };
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = current });
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewClosureAsync(current, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentClosureReview(original.Review with { Context = current }, recorded ? [original] : []));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store);
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, true).Add(x => x.Context, current).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true));
        page.WaitForAssertion(() => Assert.Contains("Reconcile verification", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Reconcile verification")).ClickAsync(new());
        if (recorded) { page.WaitForAssertion(() => Assert.Contains("Verification decision recorded", page.Markup)); Assert.Null(store.Verification); }
        else { page.WaitForAssertion(() => Assert.Contains("original verification could not be confirmed", page.Markup)); Assert.Equal(original, store.Verification); }
        client.Verify(x => x.RecordVerificationAsync(It.IsAny<CAVerificationDecision>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CorrectiveActionEligibilityBlocksNewWorkAndDiscardsLateChoices(bool lateChoices)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var choices = new TaskCompletionSource<CorrectiveActionOptions>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewCorrectiveActionOptionsAsync(context, "case", It.IsAny<CancellationToken>())).Returns(choices.Task);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, lateChoices).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true).Add(x => x.OpenSequences, new[] { 3 }));
        page.WaitForAssertion(() => Assert.Contains("Review incident closure prerequisites", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; Task? review = null;
        if (lateChoices)
        {
            await page.InvokeAsync(() => { review = (Task)typeof(HseCorrectiveAction).GetMethod("ReviewOptionsAsync", flags)!.Invoke(page.Instance, null)!; });
            client.Verify(x => x.ReviewCorrectiveActionOptionsAsync(context, "case", It.IsAny<CancellationToken>()), Times.Once);
            page.SetParametersAndRender(p => p.Add(x => x.CanRecord, false));
            choices.SetResult(new(context, "case", "plan", new string('A', 64), ["CORRECTIVE"], ["worker"]));
            await review!; await page.InvokeAsync(() => page.Render());
        }
        foreach (var method in new[] { "ReviewOptionsAsync", "SubmitAsync", "ReviewVerificationAsync", "RecordVerificationAsync", "ReviewCompletionAsync", "CompleteAsync" })
            await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        Assert.DoesNotContain("Review recorded action choices", page.Markup); Assert.DoesNotContain("Record corrective action", page.Markup);
        Assert.DoesNotContain("Review verification evidence", page.Markup); Assert.DoesNotContain("Record action completion", page.Markup);
        Assert.Null(typeof(HseCorrectiveAction).GetField("options", flags)!.GetValue(page.Instance));
        client.Verify(x => x.ReviewCorrectiveActionOptionsAsync(context, "case", It.IsAny<CancellationToken>()), lateChoices ? Times.Once() : Times.Never());
        // No fresh recording route may be called even if a private event is invoked after controls disappear.
        Assert.DoesNotContain(client.Invocations, call => call.Method.Name is "AddCorrectiveActionAsync" or "CompleteCorrectiveActionAsync" or "RecordVerificationAsync" or "ReviewVerificationAsync" or "ReviewCompletionAsync");
    }

    [Theory]
    [InlineData("CLOSED")]
    [InlineData("CANCELLED")]
    [InlineData(null)]
    [InlineData("UNRECOGNIZED")]
    public async Task TerminalClosureScreenWithholdsNewWorkAndPreservesRecovery(string? state)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.RecoverClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync((CompanyHseClosureRecovery?)null);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.CurrentIncidentState, state).Add(x => x.IncidentId, "case").Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Recover recorded closure approval", page.Markup));
        Assert.DoesNotContain("Review assigned closure stage", page.Markup); Assert.DoesNotContain("Review rejected verification for correction", page.Markup);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        foreach (var method in new[] { "ReviewAsync", "ReviewCorrectionAsync", "SubmitAsync", "SubmitCorrectionAsync" })
            await page.InvokeAsync(() => (Task)typeof(HseClosureWorkflow).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        client.Verify(x => x.ReviewClosureStageAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.ReviewCorrectionReturnAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        await page.FindAll("button").Single(b => b.TextContent.Contains("Recover recorded closure approval")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("No recorded closure stage was found", page.Markup));
        client.Verify(x => x.RecoverClosureStageAsync(context, "case", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosureScreenDiscardsOpenReviewAfterIncidentBecomesTerminal()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var response = new TaskCompletionSource<CompanyHseClosureReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).Returns(response.Task);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.IncidentId, "case").Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Review assigned closure stage", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; Task? running = null;
        await page.InvokeAsync(() => { running = (Task)typeof(HseClosureWorkflow).GetMethod("ReviewAsync", flags)!.Invoke(page.Instance, null)!; });
        client.Verify(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>()), Times.Once);
        page.SetParametersAndRender(p => p.Add(x => x.CurrentIncidentState, IncidentState.Closed));
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [new(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Tested valve")]);
        response.SetResult(new(new(actions, [new(Guid.NewGuid(), actions, "VERIFIED", "Independent inspection confirmed the repair.")]), new("lifecycle", "binding"), "instance", "step-instance", "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64)));
        await running!; await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("Approve final closure and record CLOSED status", page.Markup); Assert.DoesNotContain("Review assigned closure stage", page.Markup);
        Assert.Null(typeof(HseClosureWorkflow).GetField("review", flags)!.GetValue(page.Instance));
        Assert.Contains("Recover recorded closure approval", page.Markup);
        client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosureScreenRequiresNativeStatusBeforeReportingClosed(bool confirmed)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Tested valve");
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]); var decision = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent inspection confirmed the repair.");
        var review = new CompanyHseClosureReview(new(actions, [decision]), new("lifecycle", "binding"), "instance", "step-instance", "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64));
        var store = new MemoryActionStore(); var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original, CancellationToken _) =>
        {
            Assert.NotNull(store.Closure); Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(store.Closure));
            var receipt = new CompanyHseClosureReceipt(original.IntentId, context.Company, "actor", "instance", "step-instance", "INC_CLOSURE", "history", "approval", null, ["task"], [], new string('D', 64));
            return Task.FromResult(new CompanyHseClosureOutcome(original, receipt, confirmed ? new(original, receipt, 4) : null));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Review assigned closure stage", page.Markup));
        await page.FindAll("button").Single(b => b.TextContent.Contains("Review assigned closure stage")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Approve final closure and record CLOSED status", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseClosureWorkflow).GetField("reason", flags)!.SetValue(page.Instance, "Independent evidence reviewed and final closure approved.");
        await page.InvokeAsync(() => (Task)typeof(HseClosureWorkflow).GetMethod("SubmitAsync", flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
        if (confirmed) { page.WaitForAssertion(() => Assert.Contains("Incident closure confirmed:", page.Markup)); Assert.Null(store.Closure); }
        else
        {
            page.WaitForAssertion(() => Assert.Contains("stage outcome is unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase)); Assert.NotNull(store.Closure); Assert.DoesNotContain("Incident closure confirmed:", page.Markup);
            var approved = store.Closure!;
            page.SetParametersAndRender(p => p.Add(x => x.CurrentIncidentState, IncidentState.Closed));
            page.WaitForAssertion(() => Assert.Contains("Reconcile original closure stage", page.Markup));
            var changed = new IncidentClosureResolutionReview(context, review.LifecycleDatabase, "case", "plan", approved.IntentId, new string('D', 64), review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Updated native history note", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]);
            client.Setup(x => x.ReviewClosureResolutionAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(changed);
            client.Setup(x => x.ResolveClosureAsync(It.IsAny<IncidentClosureResolution>(), It.IsAny<CancellationToken>())).Returns((IncidentClosureResolution original, CancellationToken _) =>
            {
                Assert.NotNull(store.Resolution); Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(store.Resolution));
                var receipt = new CompanyHseClosureReceipt(approved.IntentId, context.Company, "actor", "instance", "step-instance", "INC_CLOSURE", "history", "approval", null, ["task"], [], new string('D', 64));
                return Task.FromResult(new IncidentClosedReceipt(approved, receipt, 4) { Resolution = original });
            });
            await page.FindAll("button").Single(b => b.TextContent.Contains("Review changed native incident history")).ClickAsync(new());
            page.WaitForAssertion(() => Assert.Contains("Updated native history note", page.Markup));
            typeof(HseClosureWorkflow).GetField("resolutionReason", flags)!.SetValue(page.Instance, "Explicitly reviewed the changed native history and confirmed closure.");
            await page.InvokeAsync(() => (Task)typeof(HseClosureWorkflow).GetMethod("SubmitResolutionAsync", flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
            page.WaitForAssertion(() => Assert.Contains("Incident closure confirmed after explicit native history resolution", page.Markup)); Assert.Null(store.Closure); Assert.Null(store.Resolution);
            client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CorrectionReturnScreenProtectsOriginalAndReconcilesLostResponse(bool lostResponse)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Valve pressure test failed");
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]);
        var review = new CompanyHseClosureReview(new(actions, [new(Guid.NewGuid(), actions, "REJECTED", "Independent pressure inspection rejected the repair.")]), new("lifecycle", "binding"), "instance", "step-instance", "INC_VERIFICATION", "Safety", new string('B', 64), new string('C', 64));
        var store = new MemoryActionStore(); var client = new Mock<IHSEServiceClient>(); var calls = 0; CompanyHseCorrectionCommand? sent = null;
        client.Setup(x => x.ReviewCorrectionReturnAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseCorrectionCommand command, CancellationToken _) =>
        {
            Assert.Equal(JsonSerializer.Serialize(command), JsonSerializer.Serialize(store.Correction));
            if (sent is null) sent = command; else Assert.Equal(JsonSerializer.Serialize(sent), JsonSerializer.Serialize(command));
            if (++calls == 1 && lostResponse) throw new HttpRequestException("Response lost");
            return Task.FromResult(new CompanyHseCorrectionReceipt(command, new(command.IntentId, 1, context.Company, "instance", "action-step", "step-instance", "closure-step", actions.Revision), "history", "approval", ["verification-task"], ["correction-task"], new string('D', 64)));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Review rejected verification for correction", page.Markup));
        await page.FindAll("button").Single(b => b.TextContent.Contains("Review rejected verification for correction")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Independent pressure inspection rejected", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseClosureWorkflow).GetField("correctionReason", flags)!.SetValue(page.Instance, "Replace failed valve and independently pressure test the correction.");
        await page.InvokeAsync(() => (Task)typeof(HseClosureWorkflow).GetMethod("SubmitCorrectionAsync", flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
        if (lostResponse)
        {
            page.WaitForAssertion(() => Assert.Contains("Reconcile original correction return", page.Markup)); Assert.NotNull(store.Correction);
            Assert.DoesNotContain("Review assigned closure stage", page.Markup);
            await page.FindAll("button").Single(b => b.TextContent.Contains("Reconcile original correction return")).ClickAsync(new());
        }
        page.WaitForAssertion(() => Assert.Contains("Return to corrective work confirmed", page.Markup)); Assert.Null(store.Correction); Assert.Equal(lostResponse ? 2 : 1, calls);
        Assert.DoesNotContain("Incident closure confirmed", page.Markup);
        client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    private sealed class ClosureReplyTransport(string mode, CompanyHseClosureCommand approved) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Get) return new(HttpStatusCode.NotFound);
            if (request.RequestUri!.AbsolutePath.EndsWith("/return-for-correction", StringComparison.Ordinal))
            {
                var correction = (await request.Content!.ReadFromJsonAsync<CompanyHseCorrectionCommand>(cancellationToken: token))!;
                if (mode == "correction-mismatch") correction = correction with { IntentId = Guid.NewGuid() };
                var correctionReview = correction.Review;
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(new CompanyHseCorrectionReceipt(correction, new(correction.IntentId, 1, correctionReview.Context.Company, correctionReview.InstanceId, "action-step", correctionReview.StepInstanceId, "closure-step", correctionReview.Evidence.Actions.Revision), "history", "approval", ["verification-task"], ["correction-task"], new string('D', 64))) };
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/resolve-native-history", StringComparison.Ordinal))
            {
                var resolution = (await request.Content!.ReadFromJsonAsync<IncidentClosureResolution>(cancellationToken: token))!;
                if (mode == "resolution-mismatch") resolution = resolution with { IntentId = Guid.NewGuid() };
                var a = approved.Review; var approval = new CompanyHseClosureReceipt(approved.IntentId, a.Context.Company, a.Context.ActorUserId, a.InstanceId, a.StepInstanceId, a.Stage, "history", "approval", null, ["task"], [], new string('D', 64));
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(new IncidentClosedReceipt(approved, approval, 4) { Resolution = resolution }) };
            }
            Assert.Equal("/api/field/current/hse/workflow/closure/complete-stage", request.RequestUri!.AbsolutePath);
            var original = (await request.Content!.ReadFromJsonAsync<CompanyHseClosureCommand>(cancellationToken: token))!;
            if (mode == "changed-original") original = original with { Reason = "A different final approval reason from the server." };
            var r = original.Review; var receipt = new CompanyHseClosureReceipt(original.IntentId, r.Context.Company, r.Context.ActorUserId, r.InstanceId, r.StepInstanceId, r.Stage, "history", "approval", null, ["task"], [], new string('D', 64));
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new CompanyHseClosureOutcome(original, receipt, mode == "missing-native" ? null : new(original, receipt, 4))) };
        }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("missing-native")]
    [InlineData("changed-original")]
    [InlineData("missing-recovery")]
    [InlineData("resolution")]
    [InlineData("resolution-mismatch")]
    [InlineData("correction")]
    [InlineData("correction-mismatch")]
    public async Task ClosureTypedClientRequiresExactCompleteOutcome(string mode)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Tested valve");
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]); var evidence = new IncidentClosureReview(actions, [new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent inspection confirmed the repair.")]);
        var original = new CompanyHseClosureCommand(Guid.NewGuid(), new(evidence, new("lifecycle", "binding"), "instance", "step-instance", "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64)), "Independent final closure approval evidence.");
        using var transport = new ClosureReplyTransport(mode, original); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test") };
        var client = new HSEServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<HSEServiceClient>.Instance, Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        if (mode.StartsWith("correction", StringComparison.Ordinal))
        {
            var rejection = evidence.Decisions.Single() with { Decision = "REJECTED", Reason = "Independent inspection rejected this valve repair." };
            var correction = new CompanyHseCorrectionCommand(Guid.NewGuid(), original.Review with { Stage = "INC_VERIFICATION", Evidence = new(actions, [rejection]) }, "Replace valve and test the completed corrective work.");
            if (mode == "correction") (await client.ReturnForCorrectionAsync(correction)).Require();
            else await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReturnForCorrectionAsync(correction));
            return;
        }
        if (mode.StartsWith("resolution", StringComparison.Ordinal))
        {
            var resolution = new IncidentClosureResolution(Guid.NewGuid(), new(context, original.Review.LifecycleDatabase, "case", "plan", original.IntentId, new string('D', 64), original.Review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Native history changed", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]), "Explicitly reviewed the native history change for closure.");
            if (mode == "resolution") (await client.ResolveClosureAsync(resolution)).Require();
            else await Assert.ThrowsAsync<InvalidOperationException>(() => client.ResolveClosureAsync(resolution)); return;
        }
        if (mode == "valid") (await client.CompleteClosureStageAsync(original)).Require();
        else if (mode == "missing-recovery") Assert.Null(await client.RecoverClosureStageAsync(context, "case"));
        else await Assert.ThrowsAsync<InvalidOperationException>(() => client.CompleteClosureStageAsync(original));
    }
    private sealed class CompletionTransport(bool mismatch) : HttpMessageHandler
    {
        public CompleteCASubmission? Submission;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/field/current/hse/incidents/case/cas/3/complete", request.RequestUri!.AbsolutePath);
            Submission = await request.Content!.ReadFromJsonAsync<CompleteCASubmission>(cancellationToken: token);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = System.Net.Http.Json.JsonContent.Create(new CompleteCAReceipt(Submission!.Review.Context, "case", Submission.Review.PlanId, 3, Submission.Review.StepId, mismatch ? "other" : Submission.Notes)) };
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletionClientRequiresTheExactReceipt(bool mismatch)
    {
        using var company = new CompanyClientContext();
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["Safety"], ["HSE.ManageIncidents"])));
        using var transport = new CompletionTransport(mismatch); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test") };
        var client = new HSEServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<HSEServiceClient>.Instance, Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        var review = new CACompletionReview(context, "case", "plan", 3, "step", "Inspect", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new string('A', 64));
        if (mismatch) await Assert.ThrowsAsync<InvalidOperationException>(() => client.CompleteCorrectiveActionAsync(review, "Valve inspected"));
        else await client.CompleteCorrectiveActionAsync(review, "Valve inspected");
        Assert.Equal(new CompleteCASubmission(review, "Valve inspected"), transport.Submission);
    }
    [Fact]
    public async Task CompletionRequiresEvidenceAndRetainsItAcrossLostResponse()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var store = new MemoryActionStore(); var client = new Mock<IHSEServiceClient>(); var calls = 0;
        var review = new CACompletionReview(context, "case", "plan", 3, "step", "Inspect", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new string('A', 64));
        client.Setup(x => x.ReviewCompletionAsync(context, "case", 3, It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.CompleteCorrectiveActionAsync(review, "Valve inspected", It.IsAny<CancellationToken>())).Returns(() =>
        {
            Assert.Equal("Valve inspected", store.Completion!.Notes);
            if (++calls == 1) throw new HttpRequestException("Response lost");
            return Task.CompletedTask;
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store);
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, true).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true).Add(x => x.OpenSequences, new[] { 3 }));
        page.WaitForAssertion(() => Assert.Contains("Review action for completion", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseCorrectiveAction).GetField("completionSequence", flags)!.SetValue(page.Instance, 3);
        await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod("ReviewCompletionAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod("CompleteAsync", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, calls);
        typeof(HseCorrectiveAction).GetField("completionNotes", flags)!.SetValue(page.Instance, "Valve inspected");
        await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod("CompleteAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.WaitForAssertion(() => Assert.Contains("Reconcile completion", page.Markup));
        page.SetParametersAndRender(p => p.Add(x => x.CanRecord, false));
        page.WaitForAssertion(() => Assert.Contains("Reconcile completion", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Reconcile completion")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Completion recorded", page.Markup));
        Assert.Equal(2, calls); Assert.Null(store.Completion);
    }
    [Fact]
    public async Task CorrectiveActionScreenRejectsChoicesFromAnotherCompany()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var client = new Mock<IHSEServiceClient>(); client.Setup(x => x.ReviewCorrectiveActionOptionsAsync(context, "case", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CorrectiveActionOptions(context with { Company = new("tenant", "other") }, "case", "plan", new string('A', 64), ["TYPE"], ["worker"]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, true).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true));
        page.WaitForAssertion(() => Assert.Contains("Review recorded action choices", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Review recorded action choices")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Could not confirm a corrective plan", page.Markup));
        Assert.DoesNotContain("Record corrective action", page.Markup);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CorrectiveActionClientSendsReviewedContextAndRequiresExactRecordedId(bool mismatched)
    {
        using var company = new CompanyClientContext();
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["Safety"], ["HSE.ManageIncidents"])));
        var action = new AddCARequest(Guid.NewGuid(), "Repair the recorded valve", "CORRECTIVE", new DateTime(2027, 1, 2, 0, 0, 0, DateTimeKind.Utc), "worker");
        using var transport = new ActionTransport(mismatched ? Guid.NewGuid().ToString("N") : action.ActionId.ToString("N")); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test") };
        var client = new HSEServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<HSEServiceClient>.Instance, Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        if (mismatched) await Assert.ThrowsAsync<InvalidOperationException>(() => client.AddCorrectiveActionAsync(context, "case", action, "plan", new string('A', 64)));
        else Assert.Equal(action.ActionId.ToString("N"), await client.AddCorrectiveActionAsync(context, "case", action, "plan", new string('A', 64)));
        Assert.Equal(new AddCASubmission(context, action, "plan", new string('A', 64)), transport.Submission);
    }
    [Fact]
    public async Task CorrectiveActionRetainsExactRequestAfterLostResponseAndDoesNotAdvanceWorkflow()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var store = new MemoryActionStore(); var client = new Mock<IHSEServiceClient>(); var calls = 0; AddCARequest? retained = null;
        client.Setup(x => x.ReviewCorrectiveActionOptionsAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new CorrectiveActionOptions(context, "case", "plan", new string('A', 64), ["CORRECTIVE"], ["worker"]));
        client.Setup(x => x.AddCorrectiveActionAsync(It.IsAny<FieldSelectionReview>(), "case", It.IsAny<AddCARequest>(), "plan", new string('A', 64), It.IsAny<CancellationToken>())).Returns((FieldSelectionReview current, string _, AddCARequest request, string _, string _, CancellationToken _) =>
        {
            Assert.Equal(store.Value!.Request, request); retained ??= request; Assert.Equal(retained, request);
            Assert.Equal(calls == 0 ? "revision" : "new-review", current.Revision);
            if (++calls == 1) throw new HttpRequestException("Response lost");
            return Task.FromResult(request.ActionId.ToString("N"));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store);
        var page = ui.RenderComponent<HseCorrectiveAction>(p => p.Add(x => x.CanRecord, true).Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true));
        page.WaitForAssertion(() => Assert.Contains("Review recorded action choices", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Review recorded action choices")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Record corrective action", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseCorrectiveAction).GetField("description", flags)!.SetValue(page.Instance, "Inspect and repair valve");
        typeof(HseCorrectiveAction).GetField("type", flags)!.SetValue(page.Instance, "CORRECTIVE");
        typeof(HseCorrectiveAction).GetField("responsible", flags)!.SetValue(page.Instance, "worker");
        typeof(HseCorrectiveAction).GetField("due", flags)!.SetValue(page.Instance, new DateTime(2026, 12, 1));
        await page.InvokeAsync(() => (Task)typeof(HseCorrectiveAction).GetMethod("SubmitAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.WaitForAssertion(() => Assert.Contains("Reconcile original action", page.Markup));
        Assert.NotNull(store.Value); Assert.Equal(DateTimeKind.Utc, store.Value!.Request.DueDate.Kind);
        var refreshed = context with { Revision = "new-review" };
        fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = refreshed });
        await page.InvokeAsync(() => page.SetParametersAndRender(p => p.Add(x => x.CanRecord, false).Add(x => x.Context, refreshed).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true)));
        page.WaitForAssertion(() => Assert.Contains("Reconcile original action", page.Markup));
        await page.FindAll("button").Single(button => button.TextContent.Contains("Reconcile original action")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Corrective action recorded", page.Markup));
        Assert.Equal(2, calls); Assert.Null(store.Value);
    }
    [Fact]
    public async Task CauseRecordingRetainsOriginalAndReconcilesAnUnconfirmedResponse()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var review = new IncidentRcaSourceReview(context, "case", new string('A', 64), 1, [], [option]);
        var store = new MemoryRcaStore(); var handoffs = new MemoryRcaHandoffStore(); var client = new Mock<IBusinessProcessServiceClient>(); var calls = 0; var handoffCalls = 0;
        client.Setup(x => x.ReviewHseIncidentRcaAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.RecordHseIncidentCauseAsync(It.IsAny<IncidentRcaIntent>(), It.IsAny<CancellationToken>())).Returns((IncidentRcaIntent original, CancellationToken _) =>
        {
            Assert.Same(store.Value, original); calls++;
            if (calls == 1) throw new HttpRequestException("Response lost");
            return Task.FromResult(new IncidentRcaReceipt(context.Company, "actor", "field", original.IntentId, "case", 1, HSE_INCIDENT_RCA_INTENT.Create(original).COMMAND_HASH));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<IIncidentRcaIntentStore>(store); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(handoffs);
        var page = ui.RenderComponent<HseCauseReview>(parameters => parameters.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CanManage, true).Add(x => x.CanRecord, true));
        page.WaitForAssertion(() => Assert.Contains("Review recorded causes", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Review recorded causes")).ClickAsync(new());
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(HseCauseReview).GetField("SelectedCode", flags)!.SetValue(page.Instance, "PROCESS");
        typeof(HseCauseReview).GetField("Summary", flags)!.SetValue(page.Instance, "Recorded cause requiring corrective action.");
        await page.InvokeAsync(() => (Task)typeof(HseCauseReview).GetMethod("RecordAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.WaitForAssertion(() => Assert.Contains("Reconcile original cause", page.Markup));
        var retained = Assert.IsType<IncidentRcaIntent>(store.Value);
        await page.FindAll("button").Single(x => x.TextContent.Contains("Reconcile original cause")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Cause recorded. Workflow completion requires a separate lifecycle step", page.Markup));
        Assert.Equal(2, calls); Assert.Null(store.Value); Assert.Equal("case", retained.Source.IncidentId);
        client.Setup(x => x.ReadRecordedHseCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([retained]);
        await page.FindAll("button").Single(x => x.TextContent.Contains("Find my recorded causes")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Recorded cause requiring corrective action.", page.Markup));
        var shared = new IncidentRcaRecordedCause(context.Company, context.Database, "field", "case", Guid.NewGuid(), 2, "PROCESS",
            "Another investigator recorded this cause.", "other-investigator", new string('A', 64));
        client.Setup(x => x.ReadSharedHseCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([shared]);
        await page.FindAll("button").Single(x => x.TextContent.Contains("View shared recorded causes")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Another investigator recorded this cause.", page.Markup));
        Assert.Contains("other-investigator", page.Markup);
        client.Setup(x => x.ReviewHseRcaHandoffAsync(It.IsAny<IncidentRcaRecordedCause>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IncidentRcaRecordedCause item, CancellationToken _) => new CompanyHseRcaHandoffReview(context, new("lifecycle", "binding"), item, "instance", "step", "Safety", new string('B', 64)));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Check workflow readiness")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("RCA step assigned to Safety", page.Markup));
        client.Setup(x => x.CompleteHseRcaHandoffAsync(It.IsAny<CompanyHseRcaHandoffCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseRcaHandoffCommand command, CancellationToken _) =>
        {
            Assert.Same(handoffs.Value, command); handoffCalls++;
            if (handoffCalls == 1) throw new HttpRequestException("Response lost");
            return Task.FromResult(new CompanyHseRcaHandoffReceipt(context.Company, "actor", command.IntentId, "instance", "step", "history", "next", ["closed"], ["next-task"], new string('C', 64)));
        });
        await page.FindAll("button").Single(x => x.TextContent.Contains("Submit reviewed RCA handoff")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Reconcile original RCA handoff", page.Markup));
        var savedHandoff = Assert.IsType<CompanyHseRcaHandoffCommand>(handoffs.Value);
        await page.FindAll("button").Single(x => x.TextContent.Contains("Reconcile original RCA handoff")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("RCA handoff confirmed. The next workflow task is ready", page.Markup));
        Assert.DoesNotContain("Check workflow readiness", page.Markup);
        Assert.Equal(2, handoffCalls); Assert.Null(handoffs.Value);
        var savedReceipt = new CompanyHseRcaHandoffReceipt(context.Company, "actor", savedHandoff.IntentId, "instance", "step", "history", "next", ["closed"], ["next-task"], new string('C', 64));
        client.Setup(x => x.RecoverHseRcaHandoffAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyHseRcaRecoveredHandoff(savedHandoff, savedReceipt));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Find my recorded RCA handoff")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("RCA handoff confirmed for cause 2", page.Markup));
    }
    [Fact]
    public async Task IncidentDetailReviewsRecordedCauseClassificationsAndClearsOnFieldChange()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View", "HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord { IncidentId = "case", FieldId = "field", Description = "Recorded incident" });
        incidents.Setup(x => x.GetIncidentCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]); incidents.Setup(x => x.GetIncidentCorrectiveActionsAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var client = new Mock<IBusinessProcessServiceClient>();
        client.Setup(x => x.GetHseIncidentAsync("case", It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));
        client.Setup(x => x.ReviewHseIncidentRcaAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentRcaSourceReview(context, "case", new string('A', 64), 2,
            [new(1, "ROOT", "PROCESS", "Recorded finding", true)], [new("ROOT", "PROCESS", "Process classification")]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");
        var page = ui.RenderComponent<HSEIncidentDetail>();
        page.WaitForAssertion(() => Assert.Contains("Review recorded causes and classifications", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Review recorded causes and classifications")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Recorded finding", page.Markup));
        Assert.Contains("Process classification", page.Markup);
        fields.Raise(x => x.CurrentFieldChanged += null!, "other");
        page.WaitForAssertion(() => Assert.DoesNotContain("Recorded finding", page.Markup));
    }

    [Fact]
    public async Task IncidentJurisdictionCanBeSearchedAndClearsOnFieldChange()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>();
        fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = review });
        var record = new HSEIncidentRecord { IncidentId = "case", FieldId = "field", IncidentType = "INJURY", Description = "Incident for jurisdiction review", Jurisdiction = "الكويت" };
        var other = new HSEIncidentRecord { IncidentId = "other", FieldId = "field", IncidentType = "INJURY", Description = "Norway incident", Jurisdiction = "Norway" };
        var incidents = new Mock<IHSEServiceClient>();
        incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([record, other]);
        incidents.Setup(x => x.GetIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        incidents.Setup(x => x.GetIncidentCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        incidents.Setup(x => x.GetIncidentCorrectiveActionsAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>();
        processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([new ProcessInstanceSummary { EntityId = "case", EntityType = "HSE_INCIDENT" }, new ProcessInstanceSummary { EntityId = "other", EntityType = "HSE_INCIDENT" }]);
        processes.Setup(x => x.GetHseIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance { EntityId = "case", EntityType = "HSE_INCIDENT", FieldId = "field", ProcessId = "HSE_INCIDENT_REPORTING" });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object);
        ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var list = ui.RenderComponent<HSEIncidents>();
        list.WaitForAssertion(() => Assert.Contains("الكويت", list.Markup));
        Assert.Contains("Norway incident", list.Markup);
        await list.InvokeAsync(() => { typeof(HSEIncidents).GetField("_search", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(list.Instance, "الكويت"); list.Render(); });
        Assert.Contains("Incident for jurisdiction review", list.Markup);
        Assert.DoesNotContain("Norway incident", list.Markup);
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");
        var detail = ui.RenderComponent<HSEIncidentDetail>();
        detail.WaitForAssertion(() => Assert.Contains("الكويت", detail.Markup));
        Assert.Contains("Reported jurisdiction", detail.Markup);
        fields.Raise(x => x.CurrentFieldChanged += null!, "other");
        list.WaitForAssertion(() => Assert.DoesNotContain("الكويت", list.Markup));
        detail.WaitForAssertion(() => Assert.DoesNotContain("الكويت", detail.Markup));
    }
    private sealed class MemoryIncidentStore : IIncidentReportIntentStore
    {
        private IncidentReportIntent? saved;
        public Task<IncidentReportIntent?> ReadAsync(FieldSelectionReview context) => Task.FromResult(saved is not null && saved.Context.Company == context.Company && saved.Context.ActorUserId == context.ActorUserId && saved.Context.Database == context.Database && saved.Context.FieldId == context.FieldId ? saved : null);
        public Task RetainAsync(IncidentReportIntent original) { if (saved is not null && saved != original) throw new InvalidOperationException(); saved = original; return Task.CompletedTask; }
        public Task ClearAsync(IncidentReportIntent original) { if (saved is not null && saved != original) throw new InvalidOperationException(); saved = null; return Task.CompletedTask; }
    }
    private sealed class MemoryWorkflowStore : ICompanyHseWorkflowIntentStore
    {
        public CompanyHseWorkflowStart? Value;
        public Task<CompanyHseWorkflowStart?> ReadAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Value);
        public Task RetainAsync(CompanyHseWorkflowStart original) { Value = original; return Task.CompletedTask; }
        public Task ClearAsync(CompanyHseWorkflowStart original) { Assert.Equal(original, Value); Value = null; return Task.CompletedTask; }
    }
    [Fact]
    public async Task IncidentListUsesReviewedWorkflowAndRetainsBeforeSending()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View", "HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([new HSEIncidentRecord { IncidentId = "case", FieldId = "field", Description = "Recorded incident" }]);
        var client = new Mock<IBusinessProcessServiceClient>(); client.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([new ProcessInstanceSummary { EntityId = "case", EntityType = "HSE_INCIDENT" }]);
        var review = new CompanyHseWorkflowReview(context, new("lifecycle", "binding"), "case", new string('A', 64), new string('B', 64), "Incident reporting", "HSEOfficer");
        client.Setup(x => x.ReviewHseWorkflowAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var store = new MemoryWorkflowStore();
        client.Setup(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<CancellationToken>())).Returns((CompanyHseWorkflowStart original, CancellationToken _) =>
        {
            Assert.Equal(original, store.Value);
            return Task.FromResult(new CompanyHseWorkflowReceipt(context.Company, "actor", "field", original.IntentId, "case", original.InstanceId(), "step", ["task"], new string('C', 64)));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(store); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HSEIncidents>();
        page.WaitForAssertion(() => Assert.Contains("Review workflow", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Review workflow")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("first role: HSEOfficer", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Start reviewed workflow")).ClickAsync(new());
        Assert.Null(store.Value);
        client.Verify(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Theory]
    [InlineData("CLOSED")]
    [InlineData("CANCELLED")]
    [InlineData("UNRECOGNIZED")]
    [InlineData(null)]
    public void IncidentDetailOffersRecoveryEvenWhenWorkflowExists(string? state)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View", "HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord { IncidentId = "case", FieldId = "field", Description = "Recorded incident", CurrentState = state! });
        incidents.Setup(x => x.GetIncidentCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]); incidents.Setup(x => x.GetIncidentCorrectiveActionsAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var client = new Mock<IBusinessProcessServiceClient>(); client.Setup(x => x.GetHseIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance { InstanceId = "saved", EntityId = "case", EntityType = "HSE_INCIDENT", FieldId = "field", ProcessId = "HSE_INCIDENT_REPORTING" });
        var store = new MemoryWorkflowStore { Value = new(context, new("lifecycle", "binding"), Guid.NewGuid(), "case", new string('A', 64), new string('B', 64)) };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(store); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");
        var page = ui.RenderComponent<HSEIncidentDetail>();
        page.WaitForAssertion(() => Assert.Contains("Reconcile original workflow", page.Markup));
        Assert.DoesNotContain("Review recorded action choices", page.Markup);
        Assert.DoesNotContain("Review verification evidence", page.Markup);
        Assert.DoesNotContain("Incident close-out", page.Markup);
        Assert.DoesNotContain("Review assigned closure stage", page.Markup);
        Assert.False(page.FindComponent<HseWorkflowStart>().Instance.CanStart);
        Assert.False(page.FindComponent<HseCorrectiveAction>().Instance.CanRecord);
    }
    private static void SetReportOptions(HSEIncidents page, FieldSelectionReview review)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var option = new IncidentReportOption("KUWAIT", "INJURY", "TIER_2");
        typeof(HSEIncidents).GetField("_reportOptions", flags)!.SetValue(page, new IncidentReportOptions(review, new string('A', 64), [option]));
        typeof(HSEIncidents).GetField("_rOption", flags)!.SetValue(page, option);
    }
    private sealed class Auth : AuthenticationStateProvider
    {
        public ClaimsPrincipal Principal = new(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Safety")], "test"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(Principal));
        public void Change(ClaimsPrincipal principal){Principal=principal;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    [Fact]
    public async Task IncidentDecisionAllowsConfiguredRoleAndRechecksItBeforeSending()
    {
        using var company = new CompanyClientContext();
        var roles = new[] { "Safety" };
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), roles, ["HSE.View", "HSE.ManageIncidents"])));
        ClaimsPrincipal Principal(params string[] selectedRoles) => new(new ClaimsIdentity(new[] { new Claim("party_id", "actor") }.Concat(selectedRoles.Select(role => new Claim(ClaimTypes.Role, role))), "test"));
        var auth = new Auth { Principal = Principal("Safety", "Manager") };
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = field });
        var incidents = new Mock<IHSEServiceClient>();
        incidents.Setup(x => x.GetIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord { IncidentId = "case", FieldId = "field", Description = "Incident for review", CurrentState = IncidentState.Reported });
        incidents.Setup(x => x.GetIncidentCausesAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        incidents.Setup(x => x.GetIncidentCorrectiveActionsAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync([
            new CAStatus(1, "Repair valve", "CORRECTIVE", "OPEN", DateTime.UtcNow, false, "worker", null),
            new CAStatus(2, "Check alarm", "CORRECTIVE", "COMPLETED", DateTime.UtcNow, false, "worker", DateTime.UtcNow)]);
        var processes = new Mock<IBusinessProcessServiceClient>();
        processes.Setup(x => x.GetHseIncidentAsync("case", It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance { EntityId = "case", EntityType = "HSE_INCIDENT", FieldId = "field", ProcessId = "HSE_INCIDENT_REPORTING" });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");
        var page = ui.RenderComponent<HSEIncidentDetail>(); page.WaitForAssertion(() => Assert.Contains("Incident for review", page.Markup));
        Assert.Contains("Review recorded causes", page.Markup);
        Assert.Contains("Independent verification and reviewed closure are required", page.Markup);
        Assert.Contains("Recorded corrective actions: 2; not completed: 1", page.Markup);
        Assert.DoesNotContain("Close Incident", page.Markup);
    }
    [Theory][InlineData("role")][InlineData("actor")][InlineData("permission")]
    public void IncidentDetailRefusesUnmatchedCompanyAuthorityBeforeReading(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],fault=="permission"?[]:["HSE.View"])));
        var auth=new Auth();if(fault!="permission")auth.Principal=new(new ClaimsIdentity([new Claim("party_id",fault=="actor"?"other":"actor"),new Claim(ClaimTypes.Role,fault=="role"?"Other":"Safety")],"test"));
        var incidents=new Mock<IHSEServiceClient>();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<AuthenticationStateProvider>(auth);ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());ui.Services.AddSingleton(company);ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");var page=ui.RenderComponent<HSEIncidentDetail>();page.WaitForAssertion(()=>Assert.Contains("Could not confirm incident detail",page.Markup));incidents.Verify(x=>x.GetIncidentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact]
    public void IncidentReaderWithoutManagePermissionCannotStartCorrectiveAction()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord{IncidentId="case",FieldId="field",Description="Read only"});incidents.Setup(x=>x.GetIncidentCausesAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);incidents.Setup(x=>x.GetIncidentCorrectiveActionsAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{EntityId="case",EntityType="HSE_INCIDENT",FieldId="field",ProcessId="HSE_INCIDENT_REPORTING"});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");var page=ui.RenderComponent<HSEIncidentDetail>();page.WaitForAssertion(()=>Assert.Contains("Read only",page.Markup));Assert.DoesNotContain("Record corrective action",page.Markup);
    }
    [Fact]
    public void IncidentDetailReceivesTheExactIdentifierFromTheQuery()
    {
        const string id="case/واحد?part#2";
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View","HSE.ManageIncidents"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentAsync(id,It.IsAny<CancellationToken>())).ReturnsAsync(new HSEIncidentRecord{IncidentId=id,FieldId="field",Description="Exact incident"});incidents.Setup(x=>x.GetIncidentCausesAsync(id,It.IsAny<CancellationToken>())).ReturnsAsync([]);incidents.Setup(x=>x.GetIncidentCorrectiveActionsAsync(id,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentAsync(id,It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{EntityId=id,EntityType="HSE_INCIDENT",FieldId="field",ProcessId="HSE_INCIDENT_REPORTING"});
        var auth=new Auth();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<AuthenticationStateProvider>(auth);ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId="+Uri.EscapeDataString(id));
        var page=ui.RenderComponent<HSEIncidentDetail>();Assert.Equal(id,page.Instance.Id);page.WaitForAssertion(()=>Assert.Contains("Exact incident",page.Markup));incidents.Verify(x=>x.GetIncidentAsync(id,It.IsAny<CancellationToken>()),Times.Once);
        page.InvokeAsync(()=>auth.Change(new ClaimsPrincipal(new ClaimsIdentity())));page.WaitForAssertion(()=>Assert.DoesNotContain("Exact incident",page.Markup));
    }
    [Fact]
    public async Task LateIncidentDetailCannotReappearAfterCompanyChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View","HSE.ManageIncidents"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var pending=new TaskCompletionSource<HSEIncidentRecord?>(TaskCreationOptions.RunContinuationsAsynchronously);var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentAsync("case",It.IsAny<CancellationToken>())).Returns(pending.Task);incidents.Setup(x=>x.GetIncidentCausesAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);incidents.Setup(x=>x.GetIncidentCorrectiveActionsAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{EntityId="case",EntityType="HSE_INCIDENT",FieldId="field",ProcessId="HSE_INCIDENT_REPORTING"});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");var page=ui.RenderComponent<HSEIncidentDetail>();company.Clear();pending.SetResult(new HSEIncidentRecord{IncidentId="case",FieldId="field",Description="Old company incident"});await page.InvokeAsync(()=>Task.CompletedTask);
        page.WaitForAssertion(()=>Assert.Contains("Company or FIELD changed",page.Markup));Assert.DoesNotContain("Old company incident",page.Markup);Assert.DoesNotContain("Incident Details",page.Markup);
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("missing")][InlineData("workflow-foreign")]
    public void IncidentDetailShowsOnlyConfirmedCurrentFieldData(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View","HSE.ManageIncidents"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(mode=="missing"?null:new HSEIncidentRecord{IncidentId="case",FieldId=mode=="foreign"?"other":"field",IncidentType="INJURY",Description="Confirmed incident",CurrentState=IncidentState.Reported});
        incidents.Setup(x=>x.GetIncidentCausesAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);incidents.Setup(x=>x.GetIncidentCorrectiveActionsAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{EntityId="case",EntityType="HSE_INCIDENT",FieldId=mode=="workflow-foreign"?"other":"field",ProcessId="HSE_INCIDENT_REPORTING"});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/hse/incident-detail?incidentId=case");var page=ui.RenderComponent<HSEIncidentDetail>();
        if(mode=="valid"){page.WaitForAssertion(()=>Assert.Contains("Confirmed incident",page.Markup));fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Confirmed incident",page.Markup));Assert.Contains("Company or FIELD changed",page.Markup);}
        else {page.WaitForAssertion(()=>Assert.Contains("Could not confirm incident detail",page.Markup));Assert.DoesNotContain("Confirmed incident",page.Markup);Assert.DoesNotContain("Incident Details",page.Markup);}
    }
    [Fact]
    public async Task IncidentStatesDriveCountsAndFilterWithoutCountingCancelledOrUnknownAsOpen()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var rows=new[]{(Id:"one",Status:IncidentState.Reported),(Id:"two",Status:IncidentState.PendingRCA),(Id:"three",Status:IncidentState.Closed),(Id:"four",Status:IncidentState.Cancelled),(Id:"five",Status:"UNRECOGNIZED")};
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(rows.Select(x=>new HSEIncidentRecord{IncidentId=x.Id,FieldId="field",IncidentType="INJURY",Description="description "+x.Id,CurrentState=x.Status}).ToList());
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(rows.Select(x=>new ProcessInstanceSummary{EntityId=x.Id,EntityType="HSE_INCIDENT"}).ToList());
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page=ui.RenderComponent<HSEIncidents>();page.WaitForAssertion(()=>Assert.Contains("Open: 2",page.Markup));Assert.Contains("Closed: 1",page.Markup);Assert.Contains("Cancelled: 1",page.Markup);Assert.Contains("Other or unrecorded state: 1",page.Markup);
        var filter=typeof(HSEIncidents).GetField("_statusFilter",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
        await page.InvokeAsync(()=>{filter.SetValue(page.Instance,IncidentState.PendingRCA);page.Render();});
        Assert.Contains("description two",page.Markup);Assert.DoesNotContain("description one",page.Markup);Assert.DoesNotContain("description four",page.Markup);
    }
    private sealed class EmptyResponse : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("null",System.Text.Encoding.UTF8,"application/json")});
    }
    private sealed class IncidentReadRequests : HttpMessageHandler
    {
        public List<Uri> Requests = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Requests.Add(request.RequestUri!);
            var body=request.RequestUri!.AbsolutePath.EndsWith("/causes",StringComparison.Ordinal) || request.RequestUri.AbsolutePath.EndsWith("/cas",StringComparison.Ordinal) ? "[]" : "null";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body,System.Text.Encoding.UTF8,"application/json")});
        }
    }
    [Fact]
    public async Task IncidentReadAndWorkflowClientsCarryReservedAndNonAsciiIdentifiersInQuery()
    {
        const string id="case/واحد?part#2";
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        using var handler=new IncidentReadRequests();using var http=new HttpClient(handler){BaseAddress=new Uri("https://test.invalid/")};var api=new ApiClient(http,Mock.Of<IFailureReporter>(),company);
        var hse=new HSEServiceClient(api,NullLogger<HSEServiceClient>.Instance,Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        var process=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        await hse.GetIncidentAsync(id);await hse.GetIncidentCausesAsync(id);await hse.GetIncidentCorrectiveActionsAsync(id);await process.GetHseIncidentAsync(id);
        Assert.Equal(new[]{"/api/field/current/hse/incident","/api/field/current/hse/incident/causes","/api/field/current/hse/incident/cas","/api/field/current/process/hse/incident"},handler.Requests.Select(uri=>uri.AbsolutePath));
        Assert.All(handler.Requests,uri=>Assert.Equal(id,Uri.UnescapeDataString(uri.Query["?incidentId=".Length..])));
    }
    [Fact]
    public async Task MissingIncidentPopulationIsNotConvertedToAnEmptyList()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));using var http=new HttpClient(new EmptyResponse()){BaseAddress=new Uri("https://test.invalid/")};
        var client=new HSEServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<HSEServiceClient>.Instance,Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetIncidentsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetIncidentCausesAsync("case"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetIncidentCorrectiveActionsAsync("case"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetHazopNodesAsync("study"));
        var workflows=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>workflows.GetHseIncidentsAsync());
        var compliance=new ComplianceServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<ComplianceServiceClient>.Instance,Mock.Of<IFailureReporter>());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>compliance.GetAllObligationsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>compliance.GetUpcomingObligationsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>compliance.GetRecordedFieldObligationsAsync());
        var permits=new PermitServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>permits.GetAllAsync());
    }
    [Theory][InlineData("success")][InlineData("foreign")][InlineData("workflow-missing")][InlineData("workflow-foreign")][InlineData("unauthorized")][InlineData("forbidden")][InlineData("failure")]
    public void IncidentScreenShowsCountsOnlyForAConfirmedResponse(string mode)
    {
        var incidents=new Mock<IHSEServiceClient>();
        if(mode is "success" or "foreign" or "workflow-missing" or "workflow-foreign")incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync([new HSEIncidentRecord{IncidentId="case/واحد",FieldId=mode=="foreign"?"other":"field",IncidentType="INJURY",Tier=1,Description="Recorded incident",CurrentState="OPEN"}]);
        else incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("unavailable",null,mode=="unauthorized"?HttpStatusCode.Unauthorized:mode=="forbidden"?HttpStatusCode.Forbidden:HttpStatusCode.InternalServerError));
        var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(mode is "success" or "foreign"?[new ProcessInstanceSummary{EntityId="case/واحد",EntityType="HSE_INCIDENT"}]:mode=="workflow-foreign"?[new ProcessInstanceSummary{EntityId="other",EntityType="HSE_INCIDENT"}]:[]);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",FieldName="Field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page=ui.RenderComponent<HSEIncidents>();
        if(mode=="success"){page.WaitForAssertion(()=>Assert.Contains("Recorded tier 1: 1",page.Markup));Assert.DoesNotContain("fatal/PTD",page.Markup);Assert.Contains("/hse/incidents/investigator?incidentId="+Uri.EscapeDataString("case/واحد"),page.Markup);fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded tier 1:",page.Markup));}
        else {page.WaitForAssertion(()=>Assert.Contains(mode=="unauthorized"?"Sign in to load":mode=="forbidden"?"cannot view incidents":"Could not load HSE incidents",page.Markup));Assert.DoesNotContain("Recorded tier 1:",page.Markup);Assert.DoesNotContain("Recorded incident",page.Markup);}
    }
    [Fact]
    public async Task LateIncidentReadCannotAppearAfterCompanyChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var pending=new TaskCompletionSource<List<HSEIncidentRecord>>(TaskCreationOptions.RunContinuationsAsynchronously);var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).Returns(pending.Task);var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());var page=ui.RenderComponent<HSEIncidents>();company.Clear();pending.SetResult([new HSEIncidentRecord{IncidentId="case",FieldId="field",Description="Late incident"}]);await page.InvokeAsync(()=>Task.CompletedTask);page.WaitForAssertion(()=>Assert.Contains("Company or FIELD changed",page.Markup));Assert.DoesNotContain("Late incident",page.Markup);Assert.DoesNotContain("Recorded tier 1:",page.Markup);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReportFormUsesOnlyOptionsForItsCurrentField(bool wrongField)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ReportIncident"])));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = review });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        processes.Setup(x => x.GetHseReportOptionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentReportOptions(wrongField ? review with { FieldId = "other" } : review, new string('A', 64), [new("KUWAIT", "INJURY", "TIER_2")]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HSEIncidents>(); page.WaitForAssertion(() => Assert.Contains("Recorded tier 1:", page.Markup));
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        await page.InvokeAsync(() => (Task)typeof(HSEIncidents).GetMethod("OpenReportDialogAsync", flags)!.Invoke(page.Instance, null)!);
        var reviewed = (IncidentReportOptions?)typeof(HSEIncidents).GetField("_reportOptions", flags)!.GetValue(page.Instance);
        if (wrongField) Assert.Null(reviewed); else Assert.Equal("KUWAIT", Assert.Single(reviewed!.Options).IncidentSetId);
    }

    [Fact]
    public async Task ReportFormSendsTheExplicitMultinationalJurisdiction()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ReportIncident"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync([]);var processes=new Mock<IBusinessProcessServiceClient>();processes.Setup(x=>x.GetHseIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        IncidentReportIntent? sent=null;processes.Setup(x=>x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(),It.IsAny<CancellationToken>())).ReturnsAsync((IncidentReportIntent request,CancellationToken _)=>{sent=request;return new IncidentReportReceipt(request.Context.Company,request.Context.ActorUserId,request.Context.FieldId!,request.IntentId,request.IncidentId());});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());ui.Services.AddSingleton(processes.Object);ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());var page=ui.RenderComponent<HSEIncidents>();
        page.WaitForAssertion(()=>Assert.Contains("Recorded tier 1:",page.Markup));
        var type=typeof(HSEIncidents);var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        type.GetField("_rLocation",flags)!.SetValue(page.Instance,"Field site");type.GetField("_rDescription",flags)!.SetValue(page.Instance,"Recorded incident");type.GetField("_rJurisdiction",flags)!.SetValue(page.Instance,"الكويت");
        SetReportOptions(page.Instance, new(new("tenant","company"),new("core","binding"),"actor","revision","field"));type.GetField("_rIncidentTimestamp",flags)!.SetValue(page.Instance,"2026-10-27T15:30+03:00");
        await page.InvokeAsync(()=> (Task)type.GetMethod("SubmitReportAsync",flags)!.Invoke(page.Instance,null)!);Assert.NotNull(sent);Assert.Equal("field",sent!.Context.FieldId);Assert.Equal("الكويت",sent.Jurisdiction);Assert.Equal(new DateTime(2026,10,27,12,30,0,DateTimeKind.Utc),sent.IncidentUtc);Assert.Equal("TIER_2",sent.Option.SeverityId);Assert.Equal("KUWAIT",sent.Option.IncidentSetId);Assert.Equal(new string('A',64),sent.OptionsRevision);
    }

    [Theory]
    [InlineData("permission")]
    [InlineData("role")]
    [InlineData("actor")]
    public async Task IncidentReportDoesNotSendAfterCompanyAuthorityIsMissingOrChanged(string fault)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], fault == "permission" ? [] : ["HSE.ReportIncident"])));
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = new(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field") });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var auth = new Auth(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HSEIncidents>(); page.WaitForAssertion(() => Assert.Contains("Recorded tier 1:", page.Markup));
        var type = typeof(HSEIncidents); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        type.GetField("_rLocation", flags)!.SetValue(page.Instance, "Field site"); type.GetField("_rDescription", flags)!.SetValue(page.Instance, "Incident"); type.GetField("_rJurisdiction", flags)!.SetValue(page.Instance, "KW"); SetReportOptions(page.Instance, new(new("tenant","company"),new("core","binding"),"actor","revision","field")); type.GetField("_rIncidentTimestamp", flags)!.SetValue(page.Instance, "2026-10-27T15:30+03:00");
        if (fault != "permission") auth.Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", fault == "actor" ? "other" : "actor"), new Claim(ClaimTypes.Role, fault == "role" ? "Other" : "Safety")], "test"));
        await page.InvokeAsync(() => (Task)type.GetMethod("SubmitReportAsync", flags)!.Invoke(page.Instance, null)!);
        processes.Verify(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("lost-response")]
    [InlineData("wrong-field")]
    public async Task UnconfirmedIncidentReportCannotBeResubmittedInSameScreenSession(string mode)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ReportIncident"])));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = review });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        if (mode == "lost-response") processes.Setup(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("response lost"));
        else processes.Setup(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>())).ReturnsAsync((IncidentReportIntent intent, CancellationToken _) => new IncidentReportReceipt(intent.Context.Company, intent.Context.ActorUserId, "other", intent.IntentId, intent.IncidentId()));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton<IIncidentReportIntentStore>(new MemoryIncidentStore()); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(new MemoryWorkflowStore()); ui.Services.AddSingleton<IIncidentRcaIntentStore>(new MemoryRcaStore()); ui.Services.AddSingleton<IIncidentRcaHandoffIntentStore>(new MemoryRcaHandoffStore()); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(new MemoryActionStore());
        var page = ui.RenderComponent<HSEIncidents>(); page.WaitForAssertion(() => Assert.Contains("Recorded tier 1:", page.Markup));
        var type = typeof(HSEIncidents); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        type.GetField("_rLocation", flags)!.SetValue(page.Instance, "Site"); type.GetField("_rDescription", flags)!.SetValue(page.Instance, "Incident"); type.GetField("_rJurisdiction", flags)!.SetValue(page.Instance, "KW"); SetReportOptions(page.Instance, review); type.GetField("_rIncidentTimestamp", flags)!.SetValue(page.Instance, "2026-10-27T15:30+03:00");
        async Task Submit() => await page.InvokeAsync(() => (Task)type.GetMethod("SubmitReportAsync", flags)!.Invoke(page.Instance, null)!);
        await Submit(); await page.InvokeAsync(() => page.Render());
        Assert.Contains("previous report submission could not be confirmed", page.Markup);
        await page.InvokeAsync(() => (Task)type.GetMethod("LoadIncidentsAsync", flags)!.Invoke(page.Instance, null)!);
        await Submit();
        await page.InvokeAsync(() => { var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ReportIncident"]))); });
        await page.InvokeAsync(() => (Task)type.GetMethod("LoadIncidentsAsync", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("previous report submission could not be confirmed", page.Markup);
        await Submit();
        processes.Verify(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReopenedScreenReconcilesTheSameReportAfterALostResponse()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ReportIncident"])));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = review });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var requests = new List<IncidentReportIntent>();
        processes.Setup(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>())).Returns((IncidentReportIntent item, CancellationToken _) => {
            requests.Add(item);
            return requests.Count == 1 ? Task.FromException<IncidentReportReceipt>(new HttpRequestException("lost response"))
                : Task.FromResult(new IncidentReportReceipt(item.Context.Company, item.Context.ActorUserId, item.Context.FieldId!, item.IntentId, item.IncidentId()));
        });
        var store = new MemoryIncidentStore(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());
        ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<IIncidentReportIntentStore>(store);
        var page = ui.RenderComponent<HSEIncidents>(); page.WaitForAssertion(() => Assert.Contains("Recorded tier 1:", page.Markup));
        var type = typeof(HSEIncidents); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        type.GetField("_rLocation", flags)!.SetValue(page.Instance, "Site"); type.GetField("_rDescription", flags)!.SetValue(page.Instance, "Incident");
        type.GetField("_rJurisdiction", flags)!.SetValue(page.Instance, "Kuwait"); type.GetField("_rIncidentTimestamp", flags)!.SetValue(page.Instance, "2026-10-27T15:30+03:00"); SetReportOptions(page.Instance, review);
        await page.InvokeAsync(() => (Task)type.GetMethod("SubmitReportAsync", flags)!.Invoke(page.Instance, null)!);
        Assert.Single(requests); Assert.NotNull(await store.ReadAsync(review));
        page.Dispose(); var reopened = ui.RenderComponent<HSEIncidents>();
        reopened.WaitForAssertion(() => Assert.Contains("Reconcile original report", reopened.Markup));
        await reopened.InvokeAsync(() => (Task)type.GetMethod("RetryReportAsync", flags)!.Invoke(reopened.Instance, null)!);
        Assert.Equal(2, requests.Count); Assert.Equal(requests[0], requests[1]); Assert.Null(await store.ReadAsync(review));
    }

    [Theory]
    [InlineData("storage-failure")]
    [InlineData("company-change")]
    public async Task ReportNeverSendsIfRetentionFailsOrCompanyChangesDuringRetention(string fault)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ReportIncident"])));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = review });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var processes = new Mock<IBusinessProcessServiceClient>(); processes.Setup(x => x.GetHseIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var store = new Mock<IIncidentReportIntentStore>(); store.Setup(x => x.ReadAsync(It.IsAny<FieldSelectionReview>())).ReturnsAsync((IncidentReportIntent?)null);
        store.Setup(x => x.RetainAsync(It.IsAny<IncidentReportIntent>())).Returns(() => {
            if (fault == "company-change") { company.Clear(); return Task.CompletedTask; }
            return Task.FromException(new InvalidOperationException("storage unavailable"));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(processes.Object);
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(store.Object);
        var page = ui.RenderComponent<HSEIncidents>(); page.WaitForAssertion(() => Assert.Contains("Recorded tier 1:", page.Markup));
        var type = typeof(HSEIncidents); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        type.GetField("_rLocation", flags)!.SetValue(page.Instance, "Site"); type.GetField("_rDescription", flags)!.SetValue(page.Instance, "Incident");
        type.GetField("_rJurisdiction", flags)!.SetValue(page.Instance, "Kuwait"); type.GetField("_rIncidentTimestamp", flags)!.SetValue(page.Instance, "2026-10-27T15:30+03:00"); SetReportOptions(page.Instance, review);
        await page.InvokeAsync(() => (Task)type.GetMethod("SubmitReportAsync", flags)!.Invoke(page.Instance, null)!);
        store.Verify(x => x.RetainAsync(It.IsAny<IncidentReportIntent>()), Times.Once);
        processes.Verify(x => x.RecordHseIncidentAsync(It.IsAny<IncidentReportIntent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
#endif
