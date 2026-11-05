#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanySelectorIdentityTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        private Task<AuthenticationState> state = For("alice");
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => state;
        public void Change(Task<AuthenticationState> next) { state = next; NotifyAuthenticationStateChanged(next); }
        public static Task<AuthenticationState> For(string actor) => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", actor)], "test"))));
    }
    private sealed class Access : ICompanyAccessClient
    {
        public Func<Task<CompanyScope[]>> Read = () => Task.FromResult(Array.Empty<CompanyScope>());
        public int Calls;
        public Task<CompanyScope[]> ListAsync(CancellationToken token = default) { Calls++; return Read(); }
        public Task<RepositoryCompanyAccess> GetAccessAsync(CompanyScope scope, CancellationToken token = default) => Task.FromResult(new RepositoryCompanyAccess(scope, ["Administrator"], []));
    }
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static CompanyScope[] Companies(IRenderedComponent<CompanySelector> page) => (CompanyScope[])typeof(CompanySelector).GetField("_companies", Flags)!.GetValue(page.Instance)!;
    private static IRenderedComponent<CompanySelector> Render(TestContext ui, Auth auth, Access api, CompanyClientContext company)
    {
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new CompanySelectionService(api, auth, company, p => p.FindFirst("party_id")?.Value));
        ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        return ui.RenderComponent<CompanySelector>();
    }
    [Fact]
    public async Task ExternallySelectedNewCompanyRefreshesAvailableMemberships()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var auth = new Auth();
        var scope = new CompanyScope("tenant", "new-company"); var api = new Access();
        var page = Render(ui, auth, api, company); page.WaitForAssertion(() => Assert.Equal(1, api.Calls));
        api.Read = () => Task.FromResult(new[] { scope });
        await page.InvokeAsync(() => Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Administrator"], []))));
        page.WaitForAssertion(() => Assert.Equal(scope, Assert.Single(Companies(page))));
        Assert.Equal(2, api.Calls);
    }
    [Fact]
    public async Task ExternalSelectionDuringMembershipRefreshTriggersAnotherRead()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var auth = new Auth();
        var scope = new CompanyScope("tenant", "new-company"); var pending = new TaskCompletionSource<CompanyScope[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new Access { Read = () => pending.Task }; var page = Render(ui, auth, api, company);
        page.WaitForAssertion(() => Assert.Equal(1, api.Calls));
        api.Read = () => Task.FromResult(new[] { scope });
        await page.InvokeAsync(() => Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Administrator"], []))));
        pending.SetResult([]);
        page.WaitForAssertion(() => Assert.Equal(scope, Assert.Single(Companies(page))));
        Assert.Equal(2, api.Calls);
    }
    [Fact]
    public async Task AccountChangeHidesOldMembershipsUntilNewAccountIsConfirmed()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var auth = new Auth(); var api = new Access { Read = () => Task.FromResult(new[] { new CompanyScope("t", "alice-company") }) };
        var page = Render(ui, auth, api, company); page.WaitForAssertion(() => Assert.Equal("alice-company", Assert.Single(Companies(page)).BusinessAssociateId));
        var next = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Read = () => Task.FromResult(new[] { new CompanyScope("t", "bob-company") });
        await page.InvokeAsync(() => auth.Change(next.Task)); page.WaitForAssertion(() => Assert.Contains("Checking account", page.Markup));
        next.SetResult(await Auth.For("bob"));
        page.WaitForAssertion(() => Assert.Equal("bob-company", Assert.Single(Companies(page)).BusinessAssociateId));
        Assert.Equal(2, api.Calls);
    }
    [Fact]
    public async Task OldMembershipResponseCannotReplaceNewAccountsChoices()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var auth = new Auth(); var old = new TaskCompletionSource<CompanyScope[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new Access { Read = () => old.Task }; var page = Render(ui, auth, api, company); page.WaitForAssertion(() => Assert.Equal(1, api.Calls));
        api.Read = () => Task.FromResult(new[] { new CompanyScope("t", "bob-company") });
        await page.InvokeAsync(() => auth.Change(Auth.For("bob")));
        page.WaitForAssertion(() => Assert.Equal("bob-company", Assert.Single(Companies(page)).BusinessAssociateId));
        old.SetResult([new("t", "alice-company")]); await page.InvokeAsync(() => Task.CompletedTask);
        Assert.Equal("bob-company", Assert.Single(Companies(page)).BusinessAssociateId);
    }
    [Fact]
    public async Task SameAccountNotificationDoesNotInterruptSelectionOrReloadMemberships()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var auth = new Auth(); var scope = new CompanyScope("t", "company");
        var api = new Access { Read = () => Task.FromResult(new[] { scope }) }; var page = Render(ui, auth, api, company);
        page.WaitForAssertion(() => Assert.Single(Companies(page)));
        await page.InvokeAsync(() => auth.Change(Auth.For("alice")));
        page.WaitForAssertion(() => Assert.DoesNotContain("Checking account", page.Markup));
        Assert.Equal(1, api.Calls); Assert.Equal(scope, Assert.Single(Companies(page)));
    }
}
#endif
