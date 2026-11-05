using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class HseWorkflowStartTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Safety")], "test"))));
    }
    private sealed class Store : ICompanyHseWorkflowIntentStore
    {
        public CompanyHseWorkflowStart? Value;
        public Task<CompanyHseWorkflowStart?> ReadAsync(FieldSelectionReview context, string incidentId) => Task.FromResult(Value);
        public Task RetainAsync(CompanyHseWorkflowStart original) { if (Value is not null && Value != original) throw new InvalidOperationException(); Value = original; return Task.CompletedTask; }
        public Task ClearAsync(CompanyHseWorkflowStart original) { Assert.Equal(original, Value); Value = null; return Task.CompletedTask; }
    }
    [Fact]
    public async Task WorkflowEligibilityRefusesNewReviewAndStart()
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var client = new Mock<IBusinessProcessServiceClient>(); var store = new Store();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new Mock<IFieldSelectionContext>().Object); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(store);
        var page = ui.RenderComponent<HseWorkflowStart>(p => p.Add(x => x.CanStart, false).Add(x => x.Context, context).Add(x => x.IncidentId, "incident").Add(x => x.CanManage, true));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        foreach (var name in new[] { "ReviewAsync", "StartAsync" }) await page.InvokeAsync(() => (Task)typeof(HseWorkflowStart).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        Assert.DoesNotContain("Review workflow", page.Markup); Assert.DoesNotContain("Start reviewed workflow", page.Markup);
        Assert.Empty(client.Invocations); Assert.Null(store.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReviewedWorkflowRetainsOriginalAndReconcilesLostResponse(bool loseResponse)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = new CompanyHseWorkflowReview(context, new("lifecycle", "binding"), "incident", new string('A', 64), new string('B', 64), "Incident review", "HSEOfficer");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var client = new Mock<IBusinessProcessServiceClient>(); client.Setup(x => x.ReviewHseWorkflowAsync("incident", It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var calls = new List<CompanyHseWorkflowStart>();
        client.Setup(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<CancellationToken>())).Returns((CompanyHseWorkflowStart original, CancellationToken _) =>
        {
            calls.Add(original);
            if (loseResponse && calls.Count == 1) throw new HttpRequestException("Response lost");
            return Task.FromResult(new CompanyHseWorkflowReceipt(context.Company, "actor", "field", original.IntentId, "incident", original.InstanceId(), "step", ["task"], new string('C', 64)));
        });
        var store = new Store(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(store);
        var confirmed = 0;
        var page = ui.RenderComponent<HseWorkflowStart>(p => p.Add(x => x.CanStart, true).Add(x => x.Context, context).Add(x => x.IncidentId, "incident").Add(x => x.CanManage, true).Add(x => x.OnConfirmed, EventCallback.Factory.Create(this, () => confirmed++)));
        page.WaitForAssertion(() => Assert.Contains("Review workflow", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Review workflow")).ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("first role: HSEOfficer", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Start reviewed workflow")).ClickAsync(new());
        if (loseResponse)
        {
            page.WaitForAssertion(() => Assert.Contains("Reconcile original workflow", page.Markup));
            Assert.NotNull(store.Value); Assert.Equal(0, confirmed);
            page.SetParametersAndRender(p => p.Add(x => x.CanStart, false).Add(x => x.HasWorkflow, true));
            await page.FindAll("button").Single(x => x.TextContent.Contains("Reconcile original workflow")).ClickAsync(new());
            Assert.Equal(calls[0], calls[1]);
        }
        Assert.Null(store.Value); Assert.Equal(1, confirmed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangedFieldAfterReviewDoesNotRetainOrSend(bool eligibilityChanged)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ManageIncidents"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var current = context;
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => new FieldResponse { FieldId = current.FieldId!, SelectionAvailable = true, SelectionContext = current });
        var client = new Mock<IBusinessProcessServiceClient>(); client.Setup(x => x.ReviewHseWorkflowAsync("incident", It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyHseWorkflowReview(context, new("lifecycle", "binding"), "incident", new string('A', 64), new string('B', 64), "Incident review", "HSEOfficer"));
        var store = new Store(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<ICompanyHseWorkflowIntentStore>(store);
        var page = ui.RenderComponent<HseWorkflowStart>(p => p.Add(x => x.CanStart, true).Add(x => x.Context, context).Add(x => x.IncidentId, "incident").Add(x => x.CanManage, true));
        page.WaitForAssertion(() => Assert.Contains("Review workflow", page.Markup));
        await page.FindAll("button").Single(x => x.TextContent.Contains("Review workflow")).ClickAsync(new());
        if (eligibilityChanged)
        {
            page.SetParametersAndRender(p => p.Add(x => x.CanStart, false));
            Assert.DoesNotContain("Start reviewed workflow", page.Markup);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            await page.InvokeAsync(() => (Task)typeof(HseWorkflowStart).GetMethod("StartAsync", flags)!.Invoke(page.Instance, null)!);
        }
        else
        {
            current = context with { FieldId = "other" };
            await page.FindAll("button").Single(x => x.TextContent.Contains("Start reviewed workflow")).ClickAsync(new());
        }
        Assert.Null(store.Value);
        client.Verify(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
#endif
