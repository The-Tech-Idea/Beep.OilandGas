using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Moq;
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
    public async Task ApprovalAuthorityLossDiscardsUnsentAndLateReviews(string flow, bool delayed)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var store = new ReasonStore(); var client = new Mock<IHSEServiceClient>();
        var review = ProgressReview(context, flow);
        var reply = new TaskCompletionSource<CompanyHseClosureReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resolutionReply = new TaskCompletionSource<IncidentClosureResolutionReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).Returns(reply.Task);
        client.Setup(x => x.ReviewCorrectionReturnAsync(context, "case", It.IsAny<CancellationToken>())).Returns(reply.Task);
        IncidentClosureResolutionReview? resolutionReview = null;
        if (flow == "resolution")
        {
            store.Closure = new(Guid.NewGuid(), review, "Previously approved original retained across authority changes.");
            resolutionReview = new(context, review.LifecycleDatabase, "case", "plan", store.Closure.IntentId, new string('D', 64), review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Changed native history", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]);
            client.Setup(x => x.ReviewClosureResolutionAsync(context, "case", It.IsAny<CancellationToken>())).Returns(resolutionReply.Task);
        }
        if (!delayed) { reply.SetResult(review); if (resolutionReview is not null) resolutionReply.SetResult(resolutionReview); }
        var original = store.Closure;
        using var ui = ProgressUi(company, context, store, client);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true));
        var label = flow switch { "correction" => "Review rejected verification for correction", "resolution" => "Review changed native incident history", _ => "Review assigned closure stage" };
        page.WaitForAssertion(() => Assert.Contains(label, page.Markup));
        var request = page.FindAll("button").Single(b => b.TextContent.Trim() == label).ClickAsync(new());
        if (!delayed)
        {
            await request;
            await page.Find("textarea").ChangeAsync(new() { Value = "Private unsent reason must be discarded when approval authority is revoked." });
        }
        else page.WaitForAssertion(() => Assert.Equal("true", page.Find("section").GetAttribute("aria-busy")));
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, false));
        Assert.Empty(page.FindAll("textarea")); Assert.Empty(page.FindAll("button"));
        Assert.DoesNotContain("Private unsent reason", page.Markup);
        Assert.Contains("Current incident approval authority is unavailable", page.Markup);
        if (delayed) { reply.SetResult(review); if (resolutionReview is not null) resolutionReply.SetResult(resolutionReview); await request; }
        Assert.Empty(page.FindAll("textarea"));
        Assert.Equal(original, store.Closure); Assert.Null(store.Correction); Assert.Null(store.Resolution);
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains(label, page.Markup));
        Assert.Empty(page.FindAll("textarea")); // Restored authority never revives a transient review.
        await page.FindAll("button").Single(b => b.TextContent.Trim() == label).ClickAsync(new());
        Assert.Equal("", page.Find("textarea").TextContent);
        client.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.ResolveClosureAsync(It.IsAny<IncidentClosureResolution>(), It.IsAny<CancellationToken>()), Times.Never);
        store.Client.Verify(x => x.RetainClosureAsync(It.IsAny<CompanyHseClosureCommand>()), Times.Never);
    }

    [Fact]
    public async Task AuthorityLossDuringRetainedReadStopsRemainingPrivateSlotReads()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var store = new ReasonStore { Closure = new(Guid.NewGuid(), ProgressReview(context, "approval"), "Retained original protected while approval authority is unavailable.") };
        var held = new TaskCompletionSource<CompanyHseClosureCommand?>();
        store.Client.Setup(x => x.ReadClosureAsync(context, "case")).Returns(held.Task);
        var client = new Mock<IHSEServiceClient>();
        using var ui = ProgressUi(company, context, store, client);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => store.Client.Verify(x => x.ReadClosureAsync(context, "case"), Times.Once));
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, false));
        await page.InvokeAsync(() => held.SetResult(store.Closure));
        page.WaitForAssertion(() => Assert.Equal("false", page.Find("section").GetAttribute("aria-busy")));
        Assert.DoesNotContain(store.Closure.Reason, page.Markup);
        store.Client.Verify(x => x.ReadResolutionAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>()), Times.Never);
        store.Client.Verify(x => x.ReadCorrectionAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>()), Times.Never);
        store.Client.Setup(x => x.ReadClosureAsync(context, "case")).ReturnsAsync(store.Closure);
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains(store.Closure.Reason, page.Markup));
        client.VerifyNoOtherCalls();
        store.Client.Verify(x => x.ClearClosureAsync(It.IsAny<CompanyHseClosureCommand>()), Times.Never);
    }

    [Fact]
    public void RetainedClosureWaitsForRestoredApprovalAuthorityWithoutReplay()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var store = new ReasonStore { Closure = new(Guid.NewGuid(), ProgressReview(context, "approval"), "Saved original must survive temporary loss of approval authority.") };
        var original = store.Closure; var client = new Mock<IHSEServiceClient>();
        using var ui = ProgressUi(company, context, store, client);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, false));
        store.Client.Verify(x => x.ReadClosureAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>()), Times.Never);
        Assert.Empty(page.FindAll("button")); Assert.DoesNotContain(original.Reason, page.Markup);
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Reconcile original closure stage", page.Markup));
        Assert.Contains(original.Reason, page.Markup); Assert.Equal(original, store.Closure);
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, false));
        Assert.DoesNotContain(original.Reason, page.Markup); Assert.Equal(original, store.Closure);
        page.SetParametersAndRender(p => p.Add(x => x.CanApprove, true));
        page.WaitForAssertion(() => Assert.Contains("Reconcile original closure stage", page.Markup));
        client.VerifyNoOtherCalls();
        store.Client.Verify(x => x.ClearClosureAsync(It.IsAny<CompanyHseClosureCommand>()), Times.Never);
    }
}
