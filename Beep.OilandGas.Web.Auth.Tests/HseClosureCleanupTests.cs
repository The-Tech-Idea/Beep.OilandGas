using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed partial class HseClosureReasonValidationTests
{
    [Theory]
    [InlineData("approval", false)] [InlineData("approval", true)]
    [InlineData("correction", false)] [InlineData("correction", true)]
    [InlineData("resolution", false)] [InlineData("resolution", true)]
    public async Task ConfirmedOutcomeSurvivesSavedRequestCleanupFailure(string flow, bool afterDelete)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = ProgressReview(context, flow); var store = new ReasonStore(); var client = new Mock<IHSEServiceClient>(); var sent = new List<string>();
        client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.ReviewCorrectionReturnAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var approved = new CompanyHseClosureCommand(Guid.NewGuid(), review, "Previously reviewed and approved final incident closure.");
        if (flow == "resolution")
        {
            store.Closure = approved;
            client.Setup(x => x.ReviewClosureResolutionAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentClosureResolutionReview(context, review.LifecycleDatabase, "case", "plan", approved.IntentId, new string('D', 64), review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Changed native history note", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]));
        }
        CompanyHseClosureReceipt Receipt(CompanyHseClosureCommand original) => new(original.IntentId, context.Company, "actor", "instance", "step-instance", "INC_CLOSURE", "history", "approval", null, ["task"], [], new string('D', 64));
        client.Setup(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original, CancellationToken _) => { sent.Add(JsonSerializer.Serialize(original)); var receipt = Receipt(original); return Task.FromResult(new CompanyHseClosureOutcome(original, receipt, new(original, receipt, 4))); });
        client.Setup(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseCorrectionCommand original, CancellationToken _) => { sent.Add(JsonSerializer.Serialize(original)); return Task.FromResult(new CompanyHseCorrectionReceipt(original, new(original.IntentId, 1, context.Company, "instance", "action-step", "step-instance", "closure-step", review.Evidence.Actions.Revision), "history", "approval", ["verification-task"], ["correction-task"], new string('D', 64))); });
        client.Setup(x => x.ResolveClosureAsync(It.IsAny<IncidentClosureResolution>(), It.IsAny<CancellationToken>())).Returns((IncidentClosureResolution original, CancellationToken _) => { sent.Add(JsonSerializer.Serialize(original)); return Task.FromResult(new IncidentClosedReceipt(approved, Receipt(approved), 4) { Resolution = original }); });
        var clearCalls = 0;
        if (flow == "correction") store.Client.Setup(x => x.ClearCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>())).Returns((CompanyHseCorrectionCommand original) =>
        {
            if (++clearCalls == 1) { if (afterDelete) store.Correction = null; throw new InvalidOperationException("Controlled saved-request cleanup failure"); }
            if (store.Correction is not null) Assert.Equal(original, store.Correction); store.Correction = null; return Task.CompletedTask;
        });
        else store.Client.Setup(x => x.ClearClosureAsync(It.IsAny<CompanyHseClosureCommand>())).Returns((CompanyHseClosureCommand original) =>
        {
            if (++clearCalls == 1) { if (afterDelete) store.Closure = null; throw new InvalidOperationException("Controlled saved-request cleanup failure"); }
            if (store.Closure is not null) Assert.Equal(original, store.Closure); store.Closure = null; return Task.CompletedTask;
        });
        using var ui = ProgressUi(company, context, store, client);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true));
        var reviewLabel = flow switch { "correction" => "Review rejected verification for correction", "resolution" => "Review changed native incident history", _ => "Review assigned closure stage" };
        page.WaitForAssertion(() => Assert.Contains(reviewLabel, page.Markup)); await page.FindAll("button").Single(b => b.TextContent.Trim() == reviewLabel).ClickAsync(new());
        await page.Find("textarea").ChangeAsync(new() { Value = "Independent evidence reviewed before this explicit recorded decision." });
        var submitLabel = flow switch { "correction" => "Return rejected verification to corrective work", "resolution" => "Resolve reviewed history and record CLOSED status", _ => "Approve final closure and record CLOSED status" };
        await page.FindAll("button").Single(b => b.TextContent.Trim() == submitLabel).ClickAsync(new());
        Assert.Single(sent); Assert.Equal(1, clearCalls);
        Assert.Contains(flow == "correction" ? "Return to corrective work confirmed" : "Incident closure confirmed", page.Markup);
        Assert.Contains("Saved request cleanup could not be confirmed", page.Markup);
        Assert.DoesNotContain("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Review assigned closure stage", page.Markup);
        if (flow == "resolution") Assert.Null(store.Resolution);
        var reconcile = flow switch { "correction" => "Reconcile original correction return", "resolution" => "Reconcile original native resolution", _ => "Reconcile original closure stage" };
        await page.FindAll("button").Single(b => b.TextContent.Trim() == reconcile).ClickAsync(new());
        Assert.Equal(2, sent.Count); Assert.Equal(sent[0], sent[1]); Assert.Equal(2, clearCalls);
        Assert.Null(store.Closure); Assert.Null(store.Correction); Assert.Null(store.Resolution);
        Assert.DoesNotContain("Saved request cleanup could not be confirmed", page.Markup); Assert.DoesNotContain("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
        store.Client.Verify(x => x.RetainClosureAsync(It.IsAny<CompanyHseClosureCommand>()), flow == "approval" ? Times.Once : Times.Never);
        store.Client.Verify(x => x.RetainCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>()), flow == "correction" ? Times.Once : Times.Never);
        store.Client.Verify(x => x.RetainResolutionAsync(It.IsAny<IncidentClosureResolution>()), flow == "resolution" ? Times.Once : Times.Never);
    }
}
