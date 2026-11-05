using System.Security.Claims;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Web.Pages.PPDM39.HSE;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class HseLandingAuthorityTests
{
    [Theory]
    [InlineData("anonymous")]
    [InlineData("foreign")]
    [InlineData("role")]
    [InlineData("permission")]
    [InlineData("silent")]
    public async Task LandingIncidentDataRequiresCurrentAccountAndRejectsLateResponses(string fault)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], fault == "permission" ? [] : ["HSE.View"])));
        var auth = new ChangingAuth { Principal = Principal(fault) };
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var pending = new TaskCompletionSource<List<HSEIncidentRecord>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var rows = new List<HSEIncidentRecord> { new() { IncidentId = "case?شركة", FieldId = "field", CurrentState = IncidentState.Closed } };
        var incidents = new Mock<IHSEServiceClient>(); var calls = 0;
        incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).Returns(() => ++calls == 1 ? Task.FromResult(rows) : pending.Task);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(incidents.Object);
        ui.ComponentFactories.AddStub<PersonaTaskInbox>(); ui.ComponentFactories.AddStub<PermitApplicationInbox>();
        var page = ui.RenderComponent<HSEDashboard>();
        page.WaitForAssertion(() => Assert.Contains("Current company HSE view authority is required", page.Markup));
        Assert.Equal(0, calls); fields.Verify(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()), Times.Never);
        auth.Change(Principal("valid"));
        if (fault == "permission") Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        await page.FindAll("button").Single(b => b.TextContent.Trim() == "Refresh").ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("Returned: 1", page.Markup));
        Assert.Contains("incidentId=" + Uri.EscapeDataString(rows[0].IncidentId), page.Markup); Assert.Contains("Closed: 1", page.Markup);
        await page.Find("input").ChangeAsync(new() { Value = "previous-company-study" });
        auth.Change(Principal(fault == "permission" ? "anonymous" : fault));
        page.WaitForAssertion(() => Assert.Contains("Account changed", page.Markup));
        Assert.DoesNotContain("Returned: 1", page.Markup); Assert.DoesNotContain("incidentId=", page.Markup); Assert.DoesNotContain("Checked ", page.Markup);
        Assert.DoesNotContain("previous-company-study", page.Markup);
        auth.Change(Principal("valid"));
        var refresh = page.FindAll("button").Single(b => b.TextContent.Trim() == "Refresh").ClickAsync(new());
        page.WaitForAssertion(() => Assert.Equal(2, calls));
        if (fault == "silent") auth.Principal = Principal("foreign");
        else auth.Change(Principal(fault == "permission" ? "anonymous" : fault));
        pending.SetResult(rows); await refresh;
        page.WaitForAssertion(() => Assert.DoesNotContain("Loading recorded incidents", page.Markup));
        Assert.DoesNotContain("Returned: 1", page.Markup); Assert.DoesNotContain("Closed: 1", page.Markup); Assert.DoesNotContain("incidentId=", page.Markup);
        Assert.Contains(fault == "silent" ? "Current company HSE view authority is required" : "Account changed", page.Markup);
        Assert.Equal(2, calls);
    }
    private static ClaimsPrincipal Principal(string fault) => fault switch
    {
        "anonymous" => new(new ClaimsIdentity()),
        "foreign" or "silent" => new(new ClaimsIdentity([new("party_id", "other-account"), new(ClaimTypes.Role, "Safety")], "test")),
        "role" => new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "OtherRole")], "test")),
        _ => new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Safety")], "test"))
    };
    private sealed class ChangingAuth : AuthenticationStateProvider
    {
        public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity());
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(Principal));
        public void Change(ClaimsPrincipal value) { Principal = value; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }
}
