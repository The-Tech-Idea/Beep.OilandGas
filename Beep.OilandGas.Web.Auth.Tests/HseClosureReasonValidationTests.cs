using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using System.Security.Claims;
using Bunit;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.App;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed partial class HseClosureReasonValidationTests
{
    [Theory]
    [InlineData("approval")]
    [InlineData("correction")]
    [InlineData("resolution")]
    public async Task ClosureReasonsValidateBeforeRetentionAndTransport(string flow)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents", "Workflow.Approve"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>();
        fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Recorded valve inspection evidence");
        var actions = new CAVerificationReview(context, "case", "plan", new string('A', 64), [action]);
        var decision = new CAVerificationDecision(Guid.NewGuid(), actions, flow == "correction" ? "REJECTED" : "VERIFIED", "Independent inspection recorded the valve evidence.");
        var review = new CompanyHseClosureReview(new(actions, [decision]), new("lifecycle", "binding"), "instance", "step-instance", flow == "correction" ? "INC_VERIFICATION" : "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64));
        var store = new ReasonStore(); var client = new Mock<IHSEServiceClient>(); var writes = 0;
        client.Setup(x => x.ReviewClosureStageAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(x => x.ReviewCorrectionReturnAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var retainedApproval = flow == "resolution" ? new CompanyHseClosureCommand(Guid.NewGuid(), review, "Previously reviewed and approved final incident closure.") : null;
        store.Closure = retainedApproval;
        if (retainedApproval is not null)
            client.Setup(x => x.ReviewClosureResolutionAsync(context, "case", It.IsAny<CancellationToken>())).ReturnsAsync(new IncidentClosureResolutionReview(context, review.LifecycleDatabase, "case", "plan", retainedApproval.IntentId, new string('D', 64), review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Changed history note", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]));
        client.Setup(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original, CancellationToken _) =>
        {
            writes++; Assert.Equal(original, store.Closure); return Task.FromException<CompanyHseClosureOutcome>(new HttpRequestException("Reply lost after valid submission"));
        });
        client.Setup(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<CancellationToken>())).Returns((CompanyHseCorrectionCommand original, CancellationToken _) =>
        {
            writes++; Assert.Equal(original, store.Correction); return Task.FromException<CompanyHseCorrectionReceipt>(new HttpRequestException("Reply lost after valid submission"));
        });
        client.Setup(x => x.ResolveClosureAsync(It.IsAny<IncidentClosureResolution>(), It.IsAny<CancellationToken>())).Returns((IncidentClosureResolution original, CancellationToken _) =>
        {
            writes++; Assert.Equal(original, store.Resolution); return Task.FromException<IncidentClosedReceipt>(new HttpRequestException("Reply lost after valid submission"));
        });
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICorrectiveActionIntentStore>(store.Client.Object);
        var page = ui.RenderComponent<HseClosureWorkflow>(p => p.Add(x => x.Context, context).Add(x => x.IncidentId, "case").Add(x => x.CurrentIncidentState, IncidentState.PendingCloseOut).Add(x => x.CanApprove, true));
        var reviewLabel = flow switch { "correction" => "Review rejected verification for correction", "resolution" => "Review changed native incident history", _ => "Review assigned closure stage" };
        page.WaitForAssertion(() => Assert.Contains(reviewLabel, page.Markup));
        await page.FindAll("button").Single(b => b.TextContent.Trim() == reviewLabel).ClickAsync(new());
        var submitLabel = flow switch { "correction" => "Return rejected verification to corrective work", "resolution" => "Resolve reviewed history and record CLOSED status", _ => "Approve final closure and record CLOSED status" };
        page.WaitForAssertion(() => Assert.Contains(submitLabel, page.Markup));
        foreach (var invalid in new[] { new string('x', 19), new string('x', 2001), "Reviewed\tvalve evidence before final approval." })
        {
            await page.Find("textarea").ChangeAsync(new() { Value = invalid });
            Assert.NotEqual("true", page.Find("textarea").GetAttribute("aria-invalid"));
            await page.FindAll("button").Single(b => b.TextContent.Trim() == submitLabel).ClickAsync(new());
            Assert.Equal("true", page.Find("textarea").GetAttribute("aria-invalid"));
            var description = page.Find("textarea").GetAttribute("aria-describedby");
            Assert.False(string.IsNullOrWhiteSpace(description));
            Assert.Contains(flow, page.Find("#" + description).Id);
            Assert.Contains(invalid.Contains('\t') ? "Remove control characters" : "between 20 and 2000", page.Find("#" + description).TextContent);
            Assert.Contains(invalid.Contains('\t') ? "Remove control characters" : "between 20 and 2000", page.Markup);
            Assert.DoesNotContain("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, writes); Assert.Equal(retainedApproval, store.Closure); Assert.Null(store.Correction); Assert.Null(store.Resolution);
        }
        const string valid = "تمت مراجعة أدلة الصمام بشكل مستقل.\nIndependent evidence reviewed before approval.";
        await page.Find("textarea").ChangeAsync(new() { Value = valid });
        Assert.NotEqual("true", page.Find("textarea").GetAttribute("aria-invalid"));
        await page.FindAll("button").Single(b => b.TextContent.Trim() == submitLabel).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("unconfirmed", page.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, writes);
        Assert.Equal(valid, flow switch { "correction" => store.Correction!.Reason, "resolution" => store.Resolution!.Reason, _ => store.Closure!.Reason });
        if (flow == "resolution") Assert.Equal(retainedApproval, store.Closure);
    }

    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Safety")], "test"))));
    }
    private sealed class ReasonStore
    {
        public CompanyHseClosureCommand? Closure;
        public CompanyHseCorrectionCommand? Correction;
        public IncidentClosureResolution? Resolution;
        public Mock<ICorrectiveActionIntentStore> Client { get; } = new();
        public ReasonStore()
        {
            Client.Setup(x => x.ReadClosureAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>())).Returns(() => Task.FromResult(Closure));
            Client.Setup(x => x.ReadCorrectionAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>())).Returns(() => Task.FromResult(Correction));
            Client.Setup(x => x.ReadResolutionAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>())).Returns(() => Task.FromResult(Resolution));
            Client.Setup(x => x.RetainClosureAsync(It.IsAny<CompanyHseClosureCommand>())).Callback((CompanyHseClosureCommand x) => Closure = x).Returns(Task.CompletedTask);
            Client.Setup(x => x.RetainCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>())).Callback((CompanyHseCorrectionCommand x) => Correction = x).Returns(Task.CompletedTask);
            Client.Setup(x => x.RetainResolutionAsync(It.IsAny<IncidentClosureResolution>())).Callback((IncidentClosureResolution x) => Resolution = x).Returns(Task.CompletedTask);
            Client.Setup(x => x.ClearClosureAsync(It.IsAny<CompanyHseClosureCommand>())).Callback((CompanyHseClosureCommand x) => { if (Closure is not null) Assert.Equal(Closure, x); Closure = null; }).Returns(Task.CompletedTask);
            Client.Setup(x => x.ClearCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>())).Callback((CompanyHseCorrectionCommand x) => { if (Correction is not null) Assert.Equal(Correction, x); Correction = null; }).Returns(Task.CompletedTask);
            Client.Setup(x => x.ClearResolutionAsync(It.IsAny<IncidentClosureResolution>())).Callback((IncidentClosureResolution x) => { if (Resolution is not null) Assert.Equal(Resolution, x); Resolution = null; }).Returns(Task.CompletedTask);
        }
    }
}
