using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class HseAssignedActionsTests
{
    [Fact]
    public async Task AssignedActionLandingPagesExactSourceLinksAndClearsOnCompanyChange()
    {
        using var f = new Fixture();
        f.Client.Setup(x => x.GetAssignedOverdueActionsAsync(f.Context, It.IsAny<CancellationToken>())).ReturnsAsync(f.Result(11));
        var page = f.Ui.RenderComponent<HseAssignedActions>();
        await page.Find("button").ClickAsync(new());
        page.WaitForAssertion(() => Assert.Contains("11 overdue assigned actions", page.Markup));
        Assert.Equal(10, page.FindAll("a").Count);
        Assert.Contains("incidentId=" + Uri.EscapeDataString("case/شركة?2") + "#hse-action-1", page.Markup);
        await page.FindAll("button").Single(x => x.TextContent.Trim() == "Next assigned actions").ClickAsync(new());
        Assert.Single(page.FindAll("a")); Assert.Contains("#hse-action-11", page.Markup);
        Assert.True(f.Company.TrySelect(f.Company.Clear(), "actor", new(new("other-tenant", "other"), ["Safety"], ["HSE.View"])));
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("a")));
        Assert.DoesNotContain("11 overdue", page.Markup); Assert.DoesNotContain("checked ", page.Markup);
    }

    [Theory]
    [InlineData("company")]
    [InlineData("account")]
    [InlineData("silent-account")]
    [InlineData("field")]
    [InlineData("foreign-result")]
    public async Task AssignedActionLandingRejectsLateOrForeignResults(string mode)
    {
        using var f = new Fixture();
        var pending = new TaskCompletionSource<CompanyHseAssignedActions>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(x => x.GetAssignedOverdueActionsAsync(f.Context, It.IsAny<CancellationToken>())).Returns(pending.Task);
        var page = f.Ui.RenderComponent<HseAssignedActions>();
        var click = page.Find("button").ClickAsync(new());
        page.WaitForAssertion(() => f.Client.Verify(x => x.GetAssignedOverdueActionsAsync(f.Context, It.IsAny<CancellationToken>()), Times.Once));
        if (mode == "company") f.Company.Clear();
        else if (mode == "account") f.Auth.Change(new(new ClaimsIdentity()));
        else if (mode == "silent-account") f.Auth.Principal = new(new ClaimsIdentity());
        else if (mode == "field") f.Fields.Raise(x => x.CurrentFieldChanged += null!, "other-field");
        var result = f.Result(1);
        if (mode == "foreign-result") result = result with { Context = f.Context with { Company = new("foreign", "company") } };
        pending.SetResult(result); await click;
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("a")));
        Assert.DoesNotContain("1 overdue assigned actions", page.Markup); Assert.DoesNotContain("0 overdue", page.Markup);
        Assert.Contains(mode is "silent-account" or "foreign-result" ? "could not be confirmed" : "changed", page.Markup);
    }

    [Fact]
    public async Task AssignedActionLandingRefusesUnauthorizedReads()
    {
        using var f = new Fixture(); f.Auth.Principal = new(new ClaimsIdentity());
        var page = f.Ui.RenderComponent<HseAssignedActions>(); await page.Find("button").ClickAsync(new());
        Assert.Contains("could not be confirmed", page.Markup);
        f.Fields.Verify(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()), Times.Never);
        f.Client.Verify(x => x.GetAssignedOverdueActionsAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class Fixture : IDisposable
    {
        public CompanyClientContext Company { get; } = new();
        public FieldSelectionReview Context { get; } = new(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        public Mock<IFieldSelectionContext> Fields { get; } = new();
        public Mock<IHSEServiceClient> Client { get; } = new();
        public Auth Auth { get; } = new(); public TestContext Ui { get; } = new();
        public Fixture()
        {
            Assert.True(Company.TrySelect(Company.Clear(), "actor", new(Context.Company, ["Safety"], ["HSE.View"])));
            Fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = Context });
            Ui.JSInterop.Mode = JSRuntimeMode.Loose; Ui.Services.AddMudServices(); Ui.Services.AddLogging();
            Ui.Services.AddSingleton(Company); Ui.Services.AddSingleton(Fields.Object); Ui.Services.AddSingleton(Client.Object); Ui.Services.AddSingleton<AuthenticationStateProvider>(Auth);
        }
        public CompanyHseAssignedActions Result(int count) => new(Context, DateTime.UtcNow, 1, Enumerable.Range(1, count)
            .Select(i => new CompanyHseAssignedAction("case/شركة?2", i, "Repair " + i, DateTime.UtcNow.AddHours(-i))).ToArray());
        public void Dispose() { Ui.Dispose(); Company.Dispose(); }
    }
    private sealed class Auth : AuthenticationStateProvider
    {
        public ClaimsPrincipal Principal = new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Safety")], "test"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(Principal));
        public void Change(ClaimsPrincipal principal) { Principal = principal; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }
}
