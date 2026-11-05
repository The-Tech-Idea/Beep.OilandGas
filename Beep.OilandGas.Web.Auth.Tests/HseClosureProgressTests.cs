using Bunit;
using Microsoft.AspNetCore.Components;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed partial class HseClosureReasonValidationTests
{
    [Theory]
    [InlineData("approval", false)]
    [InlineData("approval", true)]
    [InlineData("correction", false)]
    [InlineData("correction", true)]
    [InlineData("resolution", false)]
    [InlineData("resolution", true)]
    public Task ClosureMutationProgressRetainsOriginalUntilOutcome(string flow, bool lostReply) => ExerciseMutationProgressAsync(flow, lostReply, false);

    [Theory]
    [InlineData("approval")]
    [InlineData("correction")]
    [InlineData("resolution")]
    public Task ClosureConfirmedOutcomeSurvivesDetailRefreshFailure(string flow) => ExerciseMutationProgressAsync(flow, false, true);

    private async Task ExerciseMutationProgressAsync(string flow, bool lostReply, bool refreshFailure)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = ProgressReview(context, flow); var store = new ReasonStore(); var client = new Mock<IHSEServiceClient>();
        client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.ReviewCorrectionReturnAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        if (flow == "resolution")
        {
            store.Closure = new(Guid.NewGuid(), review, "Previously reviewed and approved final incident closure.");
            client.Setup(x => x.ReviewClosureResolutionAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentClosureResolutionReview(context, review.LifecycleDatabase, "case", "plan", store.Closure.IntentId, new string('D', 64), review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Changed native history note", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]));
        }
        var closureReply = new TaskCompletionSource<CompanyHseClosureOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
        var correctionReply = new TaskCompletionSource<CompanyHseCorrectionReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resolutionReply = new TaskCompletionSource<IncidentClosedReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        client.Setup(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original, CancellationToken _) => { Assert.Equal(original, store.Closure); calls++; return closureReply.Task; });
        client.Setup(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseCorrectionCommand original, CancellationToken _) => { Assert.Equal(original, store.Correction); calls++; return correctionReply.Task; });
        client.Setup(x => x.ResolveClosureAsync(It.IsAny<IncidentClosureResolution>(), It.IsAny<CancellationToken>())).Returns((IncidentClosureResolution original, CancellationToken _) => { Assert.Equal(original, store.Resolution); calls++; return resolutionReply.Task; });
        using var ui = ProgressUi(company, context, store, client);
        var refreshStarted = false; var refreshReply = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var refresh = EventCallback.Factory.Create(this, (Func<Task>)(() => { refreshStarted = true; return refreshReply.Task; }));
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true).Add(x => x.OnConfirmed, refresh));
        var reviewLabel = flow switch { "correction" => "Review rejected verification for correction", "resolution" => "Review changed native incident history", _ => "Review assigned closure stage" };
        page.WaitForAssertion(() => Assert.Contains(reviewLabel, page.Markup));
        await page.FindAll("button").Single(b => b.TextContent.Trim() == reviewLabel).ClickAsync(new());
        await page.Find("textarea").ChangeAsync(new() { Value = "Independent evidence reviewed before this explicit recorded decision." });
        var submitLabel = flow switch { "correction" => "Return rejected verification to corrective work", "resolution" => "Resolve reviewed history and record CLOSED status", _ => "Approve final closure and record CLOSED status" };
        var submission = page.FindAll("button").Single(b => b.TextContent.Trim() == submitLabel).ClickAsync(new());
        var progress = flow switch { "correction" => "Recording the return to corrective work.", "resolution" => "Recording the reviewed native history resolution.", _ => "Confirming lifecycle approval and native incident closure." };
        page.WaitForAssertion(() => Assert.Equal(1, calls));
        Assert.Equal("true", page.Find("section").GetAttribute("aria-busy"));
        Assert.Equal(progress, page.Find("[role=status]").TextContent);
        Assert.Equal("polite", page.Find("[role=status]").GetAttribute("aria-live"));
        Assert.NotEmpty(page.FindAll("[role=progressbar]"));
        Assert.All(page.FindAll("button"), b => Assert.True(b.HasAttribute("disabled")));
        Assert.DoesNotContain("Incident closure confirmed", page.Markup); Assert.DoesNotContain("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
        var originalClosure = store.Closure; var originalCorrection = store.Correction; var originalResolution = store.Resolution;
        if (lostReply)
        {
            var error = new HttpRequestException("Controlled reply loss");
            if (flow == "approval") closureReply.SetException(error); else if (flow == "correction") correctionReply.SetException(error); else resolutionReply.SetException(error);
        }
        else if (flow == "correction")
        {
            var original = store.Correction!;
            correctionReply.SetResult(new(original, new(original.IntentId, 1, context.Company, "instance", "action-step", "step-instance", "closure-step", review.Evidence.Actions.Revision), "history", "approval", ["verification-task"], ["correction-task"], new string('D', 64)));
        }
        else
        {
            var original = store.Closure!;
            var receipt = new CompanyHseClosureReceipt(original.IntentId, context.Company, "actor", "instance", "step-instance", "INC_CLOSURE", "history", "approval", null, ["task"], [], new string('D', 64));
            if (flow == "approval") closureReply.SetResult(new(original, receipt, new(original, receipt, 4)));
            else resolutionReply.SetResult(new(original, receipt, 4) { Resolution = store.Resolution });
        }
        if (!lostReply)
        {
            page.WaitForAssertion(() => Assert.True(refreshStarted));
            page.WaitForAssertion(() => Assert.Contains("Refreshing recorded incident details.", page.Markup));
            Assert.Equal("true", page.Find("section").GetAttribute("aria-busy"));
            Assert.Null(store.Closure); Assert.Null(store.Correction); Assert.Null(store.Resolution);
            Assert.Contains(flow == "correction" ? "Return to corrective work confirmed" : "Incident closure confirmed", page.Markup);
            if (refreshFailure) refreshReply.SetException(new InvalidOperationException("Controlled incident detail refresh failure")); else refreshReply.SetResult(true);
        }
        else Assert.False(refreshStarted);
        await submission;
        page.WaitForAssertion(() => Assert.Equal("false", page.Find("section").GetAttribute("aria-busy")));
        Assert.Empty(page.FindAll("[role=progressbar]")); Assert.Equal(1, calls);
        if (lostReply)
        {
            Assert.Contains("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(originalClosure, store.Closure); Assert.Equal(originalCorrection, store.Correction); Assert.Equal(originalResolution, store.Resolution);
            Assert.DoesNotContain("Incident closure confirmed", page.Markup);
        }
        else
        {
            Assert.Contains(flow == "correction" ? "Return to corrective work confirmed" : "Incident closure confirmed", page.Markup);
            Assert.Null(store.Closure); Assert.Null(store.Correction); Assert.Null(store.Resolution);
            Assert.DoesNotContain("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
            if (refreshFailure) Assert.Contains("Details could not be refreshed. Reload the incident", page.Markup);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosureReviewProgressDiscardsLateContextResult(bool terminal)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var store = new ReasonStore(); var client = new Mock<IHSEServiceClient>(); var pending = new TaskCompletionSource<CompanyHseClosureReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).Returns(pending.Task);
        using var ui = ProgressUi(company, context, store, client);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Review assigned closure stage", page.Markup));
        var request = page.FindAll("button").Single(b => b.TextContent.Trim() == "Review assigned closure stage").ClickAsync(new());
        page.WaitForAssertion(() => Assert.Equal("true", page.Find("section").GetAttribute("aria-busy")));
        Assert.Contains("Reviewing the assigned closure stage.", page.Markup);
        if (terminal)
        {
            page.SetParametersAndRender(p => p.Add(x => x.CurrentIncidentState, IncidentState.Closed));
            page.WaitForAssertion(() => Assert.Contains("Waiting for an earlier request to finish.", page.Markup));
        }
        pending.SetResult(ProgressReview(context, "approval")); await request;
        page.WaitForAssertion(() => Assert.Equal("false", page.Find("section").GetAttribute("aria-busy")));
        Assert.Empty(page.FindAll("[role=progressbar]"));
        if (terminal) Assert.DoesNotContain("Stage approval evidence and reason", page.Markup);
        else Assert.Contains("Stage approval evidence and reason", page.Markup);
        Assert.Null(store.Closure); client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    private static CompanyHseClosureReview ProgressReview(FieldSelectionReview context, string flow)
    {
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Recorded valve inspection evidence");
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]);
        var decision = new CAVerificationDecision(Guid.NewGuid(), actions, flow == "correction" ? "REJECTED" : "VERIFIED", "Independent inspection recorded the valve evidence.");
        return new(new(actions, [decision]), new("lifecycle", "binding"), "instance", "step-instance", flow == "correction" ? "INC_VERIFICATION" : "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64));
    }
    private static TestContext ProgressUi(CompanyClientContext company, FieldSelectionReview context, ReasonStore store, Mock<IHSEServiceClient> client)
    {
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store.Client.Object); return ui;
    }
}
